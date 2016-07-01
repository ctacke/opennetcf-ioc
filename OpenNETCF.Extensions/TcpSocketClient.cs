#if !XAMARIN
using OpenNETCF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Net.Sockets
{
#if WindowsCE
    public enum SocketError
    {
        None = 0,
        ConnectionRefused = 10061,
        ConnectionReset = 10054,

        // TODO: fill this out further
    }
#endif

    public class TcpSocketClient
    {
        public event EventHandler<GenericEventArgs<byte[]>> DataReceived;
        public event EventHandler Connected;
        public event EventHandler<GenericEventArgs<SocketError>> Error;

        private Socket m_socket;
        private byte[] m_buffer;

        public TcpSocketClient()
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_buffer = new byte[4096];
        }

        public bool IsConnected
        {
            get
            {
                if (m_socket == null) return false;
                return m_socket.Connected;
            }
        }

        public IPEndPoint ServerAddress
        {
            get
            {
                if (m_socket == null) return null;
                if (!m_socket.Connected) return null;
                return (IPEndPoint)m_socket.RemoteEndPoint;
            }
        }

        public void BeginConnect(IPAddress host, int port)
        {
            var endpoint = new IPEndPoint(host, port);

            try
            {
                m_socket.BeginConnect(endpoint, ConnectCallback, null);
            }
            catch (SocketException e)
            {
                Error.Fire(this, new GenericEventArgs<SocketError>(e.SocketErrorCode));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (Debugger.IsAttached) Debugger.Break();
            }
        }

        public void BeginConnect(string host, int port)
        {
            var address = Dns.GetHostEntry(host).AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
            var endpoint = new IPEndPoint(address, port);

            try
            {
                m_socket.BeginConnect(endpoint, ConnectCallback, null);
            }
            catch (SocketException e)
            {
                Error.Fire(this, new GenericEventArgs<SocketError>(e.SocketErrorCode));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (Debugger.IsAttached) Debugger.Break();
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                m_socket.EndConnect(ar);

                m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, ReceiveCallback, null);

                Connected.Fire(this, EventArgs.Empty);
            }
            catch (SocketException e)
            {
                Error.Fire(this, new GenericEventArgs<SocketError>(e.SocketErrorCode));
            }
        }

#if WindowsCE
        private void ReceiveCallback(IAsyncResult ar)
        {
            SocketError error;

            try
            {
                var length = m_socket.EndReceive(ar);
                try
                {
                    var buffer = new byte[length];
                    Buffer.BlockCopy(m_buffer, 0, buffer, 0, buffer.Length);
                    DataReceived.Fire(this, new GenericEventArgs<byte[]>(buffer));
                }
                finally
                {
                    m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, ReceiveCallback, null);
                }
            }
            catch (SocketException ex)
            {
                switch ((SocketError)ex.ErrorCode)
                {
                    case SocketError.ConnectionReset:
                    // server stopped - attempt reconnect?
                    default:
                        Error.Fire(this, new GenericEventArgs<SocketError>((SocketError)ex.ErrorCode));

                        // TODO: retry connection

                        break;
                }            }
        }
#else
        private void ReceiveCallback(IAsyncResult ar)
        {
            SocketError error;

            var length = m_socket.EndReceive(ar, out error);

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
                        m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, ReceiveCallback, null);
                    }
                    break;
                case SocketError.ConnectionReset:
                // server stopped - attempt reconnect?
                default:
                    Error.Fire(this, new GenericEventArgs<SocketError>(error));

                    // TODO: retry connection

                    break;
            }
        }
#endif
        public int Send(byte[] data)
        {
            try
            {
                return m_socket.Send(data);
            }
            catch (SocketException e)
            {
#if WindowsCE
                Error.Fire(this, new GenericEventArgs<SocketError>((SocketError)e.ErrorCode));
#else
                Error.Fire(this, new GenericEventArgs<SocketError>(e.SocketErrorCode));
#endif
                return 0;
            }
        }
    }
}
#endif