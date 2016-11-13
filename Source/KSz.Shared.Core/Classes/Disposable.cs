using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    
    public class DisposableObject : ObservableObject, IDisposable
    {
            
        /// <summary>
        /// Releases all resources used by the instance (managed and unamanaged)
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // https://msdn.microsoft.com/en-us/library/b1yfkh5e.aspx
            // ✓ DO implement the IDisposable interface by simply calling Dispose(true) followed by GC.SuppressFinalize(this).
            // The call to SuppressFinalize should only occur if Dispose(true) executes successfully.

            // See also CA1816: Call GC.SuppressFinalize correctly
            // https://msdn.microsoft.com/en-us/library/ms182269.aspx

            // https://msdn.microsoft.com/en-us/library/system.gc.suppressfinalize.aspx
            // If obj does not have a finalizer, the call to the SuppressFinalize method has no effect
            GC.SuppressFinalize(this);
        }


        private bool disposed = false;// To detect redundant calls
        /// <summary>Releases the unmanaged resources used by the instance and optionally releases the managed resources. </summary>
        /// <param name="disposing">
        /// Indicates wheater to release managed resources. 
        /// True to release both managed and unmanaged resources - called from Dispose() method; 
        /// False to release nly unmanaged resources - called from Dispose() method and by the garbage collector (GC) via finalizer ~DisposableObject(). 
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // This part of code is claed only from Dispose() mehtod - out of  garbage collector (GC) and out of ~DisposableObject() finalizer.
                    // Dispose managed resources.
                    OnDisposing();
                }

                // This part of code is called from Dispose() method and by the garbage collector (GC) via finalizer ~DisposableObject() 
                // Free unmanaged resources (unmanaged objects)
                OnFinalizing();
                disposed = true;
            }
        }


        /// <summary>
        /// Dispose managed resources, and set large fields to null, eg. 
        /// if (this.resource != null) 
        ///   this.resource.Dispose();
        /// </summary>
        protected virtual void OnDisposing()
        {
        }


        /// <summary>
        /// Free unmanaged resources (unmanaged objects), eg. 
        /// if (handle != IntPtr.Zero)  
        ///   CloseHandle(handle)); 
        /// </summary>
        protected virtual void OnFinalizing()
        {
        }


        // Objects with finalizers are more expensive to create - the CLR keeps tabs on objects with a finalizer ~DisposableObject() when they are created. 
        //~DisposableObject()
        //{
        //    Dispose(false);
        //}
    }
}
