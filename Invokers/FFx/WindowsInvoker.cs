using System;
using System.Reflection;
using System.Windows.Forms;
using TheInvoker = System.Windows.Forms.Control;

namespace OpenNETCF.IoC
{
    public class WindowsInvoker : IInvoker
    {
        private TheInvoker m_invoker;
        private Delegate m_targetDelegate;
        private MethodInfo m_methodInfo = null;

        public WindowsInvoker()
        {
        }

        public void Initialize(object invokerObject, Delegate targetDelegate)
        {
            m_invoker = (Control)invokerObject;

            // force handle creation under WinForms
            var handleCheck = m_invoker.Handle;

            m_targetDelegate = targetDelegate;
        }

        public void Handler(object source, EventArgs args)
        {
            if (!m_invoker.IsDisposed)
            {
                try
                {
                    m_invoker.Invoke(m_targetDelegate, new object[] { source, args });
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                    // force handle creation under WinForms
                    try
                    {
                        var handleCheck = m_invoker.Handle;
                    }
                    catch
                    {
                        // nop
                    }
                }
            }
        }

        public MethodInfo HandlerMethod
        {
            get
            {
                if (m_methodInfo == null)
                {
                    m_methodInfo = this.GetType().GetMethod("Handler", BindingFlags.Public | BindingFlags.Instance);
                }

                return m_methodInfo;
            }
        }

        public object CreateInvokerObject()
        {
            return new System.Windows.Forms.Control();
        }

    }
}
