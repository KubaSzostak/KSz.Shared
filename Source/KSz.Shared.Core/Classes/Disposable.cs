using System;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public class DisposableBase : IDisposable
    {
        public DisposableBase()
        {

        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    OnDisposing();
                }
                // if there are unmanaged resources to release, they need to be released here.
                disposed = true;
            }
            //base.Dispose(disposing);
        }

        /// <summary>
        /// Dispose managed resources: if (resource != null) resource.Dispose();
        /// </summary>
        protected virtual void OnDisposing()
        {

        }

        ~DisposableBase()
        {
            Dispose(false);
        }

    }
}
