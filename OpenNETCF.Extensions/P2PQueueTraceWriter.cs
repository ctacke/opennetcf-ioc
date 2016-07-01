#if WindowsCE
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using OpenNETCF.WindowsCE.Messaging;
using System.Runtime.Serialization;

namespace System.Diagnostics
{
    public class TraceMessage : Message
    {
        private BasicBinarySerializer m_serializer;

        public string Category { get; set; }
        public string Message { get; set; }

        public TraceMessage()
        {
        }

        public TraceMessage(BasicBinarySerializer serializer)
        {
            m_serializer = serializer;
        }

        public TraceMessage(BasicBinarySerializer serializer, string message, string category)
        {
            m_serializer = serializer;
            Message = message;
            Category = category;
        }

        [DoNotSerialize]
        public override byte[] MessageBytes
        {
            get { return m_serializer.Serialize(this); }
            set
            {
                var msg = m_serializer.Deserialize<TraceMessage>(value);
                this.Message = msg.Message;
                this.Category = msg.Category;
            }
        }
    }

    public class P2PQueueTraceWriter : TraceListener
    {
        private P2PMessageQueue m_queue;
        private BasicBinarySerializer m_serializer;

        public P2PQueueTraceWriter()
        {
            m_queue = new P2PMessageQueue(false, "OpenNETCF.P2PTrace");
            m_serializer = new BasicBinarySerializer();
        }

        public override void Write(string message, string category)
        {
            Send(message, category);
        }

        public override void WriteLine(string message, string category)
        {
            Send(message, category);
        }

        public override void Write(string message)
        {
            Send(message, null);
        }

        public override void WriteLine(string message)
        {
            Send(message, null);
        }

        private void Send(string message, string category)
        {
            var msg = new TraceMessage(m_serializer, message, category);
            m_queue.Send(msg, 1000);
        }
    }
}
#endif
