using ShipsClient.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShipsClient.Network
{
    class ClientSocket
    {
        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendPacketDone = new ManualResetEvent(false);
        private ManualResetEvent packetReceivi = new ManualResetEvent(false);

        private ByteBuffer WriteBuffer { get; set; }
        private ByteBuffer ReadBuffer { get; set; }

        private Socket Socket { get; set; }

        public string IP { get; private set; }
        public int Port { get; private set; }

        private static ClientSocket instance;
   
        private ClientSocket()
        {
            WriteBuffer = new ByteBuffer(256);
            ReadBuffer = new ByteBuffer(256);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public static ClientSocket Instance
        {
            get
            {
                if (instance == null)
                    instance = new ClientSocket();
                return instance;
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
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(IP), port);
                // Connect to the remote endpoint.
                Socket.BeginConnect(remoteEP,  new AsyncCallback(ConnectCallback), null);
                connectDone.WaitOne();

                // Release the socket.
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();

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
                Console.WriteLine("Socket connected to {0}", Socket.RemoteEndPoint.ToString());
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendPacket(Packet packet)
        {
            WriteHeader(packet);
            WriteBuffer.WriteBytes(packet.ToArray());
            Socket.BeginSend(WriteBuffer.ToArray(), 0, WriteBuffer.Count, 0,  new AsyncCallback(SendPacketCallback), null);
            sendPacketDone.WaitOne();
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

        private void WriteHeader(Packet packet)
        {
            WriteBuffer.Clear();
            WriteBuffer.Reset();
            WriteBuffer.WriteInt16((short)packet.Opcode);
            WriteBuffer.WriteInt16((short)packet.Length);
        }
    }
}