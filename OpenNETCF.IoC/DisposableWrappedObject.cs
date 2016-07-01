// LICENSE
// -------
// This software was originally authored by Christopher Tacke of OpenNETCF Consulting, LLC
// On March 10, 2009 is was placed in the public domain, meaning that all copyright has been disclaimed.
//
// You may use this code for any purpose, commercial or non-commercial, free or proprietary with no legal 
// obligation to acknowledge the use, copying or modification of the source.
//
// OpenNETCF will maintain an "official" version of this software at www.opennetcf.com and public 
// submissions of changes, fixes or updates are welcomed but not required
//
using System;

namespace OpenNETCF.IoC
{
    public class DisposableWrappedObject<T> : IDisposable
        where T : class, IDisposable
    {
        public bool Disposed { get; private set; }
        public T Instance { get; private set; }

        internal event EventHandler<GenericEventArgs<IDisposable>> Disposing;

        internal DisposableWrappedObject(T disposableObject)
        {
            if (disposableObject == null) throw new ArgumentNullException();

            Instance = disposableObject;
        }

        ~DisposableWrappedObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock(this)
            {
                if(Disposed) return;

                EventHandler<GenericEventArgs<IDisposable>> handler = Disposing;
                if(handler != null)
                {
                    Disposing(this, new GenericEventArgs<IDisposable>(Instance));
                }

                Instance.Dispose();

                Disposed = true;
            }
        }
    }
}
