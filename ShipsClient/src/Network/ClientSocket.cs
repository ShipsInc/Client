using ShipsClient.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using ShipsClient.Protocol;

namespace ShipsClient.Network
{
    public class ClientSocket
    {
        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendPacketDone = new ManualResetEvent(false);
        private ManualResetEvent receivePacketDone = new ManualResetEvent(false);

        private byte[] ReadBuffer;

        private Socket Socket { get; set; }

        private string IP { get; set; }
        private int Port { get; set; }

        private static ClientSocket _instance;

        private readonly System.Timers.Timer PacketTimer;
        private readonly Queue<Packet> SendPacketQueue;

        private ClientSocket()
        {
            ReadBuffer = new byte[256];

            PacketTimer = new System.Timers.Timer(50);
            PacketTimer.Elapsed += UpdateSendQueue;
            PacketTimer.Start();

            SendPacketQueue = new Queue<Packet>();
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public static ClientSocket Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ClientSocket();
                return _instance;
            }
        }

        public bool IsOpen()
        {
            return Socket.Connected;
        }

        public void Close()
        {
            Socket.Close();
        }

        public void Connect(string ip = "", int port = 0)
        {
            if (ip != "" && port != 0)
            {
                IP = ip;
                Port = port;
            }

            // Connect to a remote device.
            try
            {
                var remoteEp = new IPEndPoint(IPAddress.Parse(IP), port);
                // Connect to the remote endpoint.
                Socket.BeginConnect(remoteEp, new AsyncCallback(ConnectCallback), null);
                connectDone.WaitOne();

                // Receive the response from the remote device.
                ReceivePacket();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Complete the connection.
                Socket.EndConnect(ar);
                Console.WriteLine($"Socket connected to {Socket.RemoteEndPoint.ToString()}");
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SendPacketCallback(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = Socket.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendPacketDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceivePacket()
        {
            try
            {
                // Begin receiving the data from the remote device.
                Socket.BeginReceive(ReadBuffer, 0, ReadBuffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
                receivePacketDone.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Read data from the remote device.
                int bytesRead = 0;
                try
                {
                    bytesRead = Socket.EndReceive(ar);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }

                Packet packet = ParsePacket(ReadBuffer);
                if (packet != null)
                    Handlers.SelectHandler(packet);

                Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
                Socket.BeginReceive(ReadBuffer, 0, ReadBuffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
                receivePacketDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public Packet ParsePacket(byte[] bytes)
        {
            var buffer = new ByteBuffer(bytes);
            UInt16 opcode = buffer.ReadUInt16();
            UInt16 size = buffer.ReadUInt16();

            if ((Opcodes)opcode >= Opcodes.MAX_OPCODE)
                return null;

            if (size > 1000)
                return null;

            var packet = new Packet(opcode);
            packet.WriteBytes(buffer.GetBytes(size));
            packet.ResetPos();
            return packet;
        }

        public void WriteHeader(Packet packet)
        {
            byte[] bytes = packet.ToArray();

            packet.Clear();
            packet.WriteUInt16((ushort)packet.Opcode);
            packet.WriteUInt16((ushort)bytes.Length);
            packet.WriteBytes(bytes);
        }

        public void SendPacket(Packet packet, bool now = false)
        {
            if (!now)
                SendPacketQueue.Enqueue(packet);
            else
            {
                WriteHeader(packet);
                Socket.BeginSend(packet.ToArray(), 0, (int)packet.Length(), 0, new AsyncCallback(SendPacketCallback), null);
            }
        }

        private void UpdateSendQueue(object sender, ElapsedEventArgs e)
        {
            while (SendPacketQueue.Count != 0)
            {
                var packet = SendPacketQueue.Dequeue();
                WriteHeader(packet);
                Socket.BeginSend(packet.ToArray(), 0, (int)packet.Length(), 0, new AsyncCallback(SendPacketCallback), null);
            }

            sendPacketDone.WaitOne();
        }
    }
}