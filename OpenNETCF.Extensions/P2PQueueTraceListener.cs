#if WindowsCE
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using OpenNETCF.WindowsCE.Messaging;
using System.Runtime.Serialization;
using System.Threading;
using OpenNETCF;

namespace System.Diagnostics
{
    public class P2PQueueTraceListener
    {
        public event EventHandler<GenericEventArgs< TraceMessage>> OnMessage;

        private P2PMessageQueue m_queue;
        private BasicBinarySerializer m_serializer;

        public P2PQueueTraceListener()
        {
            m_queue = new P2PMessageQueue(true, "OpenNETCF.P2PTrace");
            m_queue.DataOnQueueChanged += new EventHandler(m_queue_DataOnQueueChanged);
            
            m_serializer = new BasicBinarySerializer();
        }

        void m_queue_DataOnQueueChanged(object sender, EventArgs e)
        {
            var msg = new TraceMessage(m_serializer);

            try
            {
                Thread.Sleep(100);
                var result = m_queue.Receive(msg);

                if (result == ReadWriteResult.OK)
                {
                    OnMessage.Fire(this, new GenericEventArgs<TraceMessage>(msg));
                }
            }
            catch (Exception ex)
            { 
            }
        }
    }
}
#endif
