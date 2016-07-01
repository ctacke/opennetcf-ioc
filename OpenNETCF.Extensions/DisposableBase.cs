using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public abstract class DisposableBase : IDisposable
    {
        public bool IsDisposed { get; protected set; }

        public void Dispose()
        {
            ReleaseManagedResources();
            ReleaseNativeResources();
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        protected virtual void ReleaseManagedResources()
        {
        }
        protected virtual void ReleaseNativeResources()
        {
        }

        ~DisposableBase()
        {
            ReleaseNativeResources();
        }

    }
}
