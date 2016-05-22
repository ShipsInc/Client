using ShipsClient.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Windows;
using ShipsClient.Protocol;
using ShipsClient.Protocol.Parser;

namespace ShipsClient.Network
{
    public class TCPSocket
    {
        // ManualResetEvent instances signal completion.
        private readonly ManualResetEvent _connectDone = new ManualResetEvent(false);
        private readonly ManualResetEvent _sendPacketDone = new ManualResetEvent(false);
        private readonly ManualResetEvent _receivePacketDone = new ManualResetEvent(false);

        private readonly byte[] _readBuffer;

        private Socket Socket { get; set; }

        private string Ip
        {
            get { return _ip; }
            set { _ip = value; }
        }

        private int Port { get; set; }

        private static TCPSocket _instance;

        private readonly Queue<Packet> _sendPacketQueue;
        private string _ip;

        private TCPSocket()
        {
            _readBuffer = new byte[Constants.BUFFER_SIZE];

            var packetTimer = new System.Timers.Timer(50);
            packetTimer.Elapsed += UpdateSendQueue;
            packetTimer.Start();

            _sendPacketQueue = new Queue<Packet>();
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public static TCPSocket Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TCPSocket();
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
                Ip = ip;
                Port = port;
            }

            // Connect to a remote device.
            try
            {
                var remoteEp = new IPEndPoint(IPAddress.Parse(Ip), port);
                // Connect to the remote endpoint.
                Socket.BeginConnect(remoteEp, ConnectCallback, null);
                _connectDone.WaitOne();

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
                _connectDone.Set();
            }
            catch (SocketException sockErr)
            {
                OnSocketError(sockErr.SocketErrorCode);
            }
        }

        private void SendPacketCallback(IAsyncResult ar)
        {
            try
            {
                // Signal that all bytes have been sent.
                _sendPacketDone.Set();
            }
            catch (SocketException sockErr)
            {
                OnSocketError(sockErr.SocketErrorCode);
            }
        }

        private void ReceivePacket()
        {
            try
            {
                // Begin receiving the data from the remote device.
                Socket.BeginReceive(_readBuffer, 0, _readBuffer.Length, 0, ReceiveCallback, null);
                _receivePacketDone.WaitOne();
            }
            catch (SocketException sockErr)
            {
                OnSocketError(sockErr.SocketErrorCode);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                try
                {
                    var bytes = Socket.EndReceive(ar);
                    byte[] temp = _readBuffer;
                    Array.Resize(ref temp, bytes);
                    byte[] decryptBytes = Cryptography.Decrypt(temp);

                    var packet = ParsePacket(decryptBytes);
                    if (packet != null)
                        Handler.SelectHandler(packet);

                    Array.Clear(_readBuffer, 0, _readBuffer.Length);

                    Socket.BeginReceive(_readBuffer, 0, _readBuffer.Length, 0, ReceiveCallback, null);
                }
                catch (SocketException sockErr)
                {
                    OnSocketError(sockErr.SocketErrorCode);
                    return;
                }
                _receivePacketDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private Packet ParsePacket(byte[] bytes)
        {
            var buffer = new ByteBuffer(bytes);
            Opcode opcode = (Opcode)buffer.ReadUInt16();
            var size = buffer.ReadUInt16();

            if (opcode >= Opcode.MAX_OPCODE)
                return null;

            if (size > 1000)
                return null;

            var packet = new Packet(opcode);
            packet.WriteBytes(buffer.GetBytes(size));
            packet.ResetPos();
            return packet;
        }

        private void WriteHeader(Packet packet)
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
                _sendPacketQueue.Enqueue(packet);
            else
            {
                try
                {
                    WriteHeader(packet);
                    byte[] cryptBytes = Cryptography.Encrypt(packet.ToArray());
                    Socket.BeginSend(cryptBytes, 0, cryptBytes.Length, 0, SendPacketCallback, null);
                }
                catch (SocketException sockErr)
                {
                    OnSocketError(sockErr.SocketErrorCode);
                    return;
                }
            }
        }

        private void UpdateSendQueue(object sender, ElapsedEventArgs e)
        {
            while (_sendPacketQueue.Count != 0)
            {
                var packet = _sendPacketQueue.Dequeue();
                WriteHeader(packet);
                byte[] cryptBytes = Cryptography.Encrypt(packet.ToArray());
                try
                {
                    Socket.BeginSend(cryptBytes, 0, cryptBytes.Length, 0, SendPacketCallback, null);
                }
                catch (SocketException sockErr)
                {
                    OnSocketError(sockErr.SocketErrorCode);
                    return;
                }
            }

            _sendPacketDone.WaitOne();
        }

        private void OnSocketError(SocketError err)
        {
            string title, text;
            switch (err)
            {
                case SocketError.ConnectionRefused:
                {
                    title = "Соединение с сервером";
                    text = "Не удалось установить соединение с сервером...";
                    break;
                }
                case SocketError.ConnectionReset:
                {
                    title = "Соединение с сервером";
                    text = "Соединение с сервером было разорвано";
                    break;
                }
                default:
                    Console.WriteLine(err);
                    Environment.Exit(0);
                    return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                new NotificationWindow.NotificationWindow(title, text).ShowDialog();
            });

            Environment.Exit(0);
        }
    }
}