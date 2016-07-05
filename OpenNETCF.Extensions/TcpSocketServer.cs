#if !PCL
using OpenNETCF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Net.Sockets
{
    public class TcpSocketServer
    {
        public event EventHandler<GenericEventArgs<byte[]>> DataReceived;
        public event EventHandler ClientConnected;

        private Socket m_listener;
        private Socket m_server;

        private int m_port;

        private byte[] m_buffer;

        public TcpSocketServer(int port)
        {
            m_port = port;
            m_buffer = new byte[4096];
        }

        public bool IsConnected
        {
            get 
            {
                if (m_server == null) return false;
                return m_server.Connected; 
            }
        }

        public IPEndPoint ClientAddress
        {
            get
            {
                if (m_server == null) return null;
                if (!m_server.Connected) return null;
                return (IPEndPoint)m_server.RemoteEndPoint;
            }
        }

        public void Start()
        {
            m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var ep = new IPEndPoint(IPAddress.Any, m_port);
            m_listener.Bind(ep);
            m_listener.Listen(100);
            m_listener.BeginAccept(AcceptCallback, null);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            m_server = m_listener.EndAccept(ar);
            m_server.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, ReceiveCallback, null);

            ClientConnected.Fire(this, EventArgs.Empty);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            SocketError error;
            var length = m_server.EndReceive(ar, out error);

            switch (error)
            {
                case SocketError.Success:
                    try
                    {
                        var buffer = new byte[length];
                        Buffer.BlockCopy(m_buffer, 0, buffer, 0, buffer.Length);
                        DataReceived.Fire(this, new GenericEventArgs<byte[]>(buffer));
                    }
                    finally
                    {
                        m_server.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, ReceiveCallback, null);
                    }
                    break;
                case SocketError.ConnectionReset:
                // client disconnected and reconnected?  // re-start?
                default:
                    try
                    {
                        m_server.Close();
                        m_listener.Close();
                        Start();
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        if (Debugger.IsAttached) Debugger.Break();
                    }
                    break;
            }
        }

        public int Send(byte[] data)
        {
            return m_server.Send(data);
        }
    }
}
#endif
