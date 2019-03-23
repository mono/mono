using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace System.Threading
{
    partial class Semaphore
    {
        const int MAX_PATH = 260;

        Semaphore(SafeWaitHandle handle) => this.SafeWaitHandle = handle;

        int ReleaseCore (int releaseCount)
        {
            int previousCount;
            if (!ReleaseSemaphore_internal (SafeWaitHandle.DangerousGetHandle (), releaseCount, out previousCount))
                throw new SemaphoreFullException ();
            return previousCount;
        }

        static OpenExistingResult OpenExistingWorker(string name, out Semaphore result)
        {
            if (name == null)
                throw new ArgumentNullException (nameof (name));

            if (name.Length  == 0)
                throw new ArgumentException (SR.Argument_StringZeroLength, nameof (name));

            if (null != name && MAX_PATH < name.Length)
                throw new ArgumentException (SR.Argument_WaitHandleNameTooLong);

            result = null;
            MonoIOError errorCode;
            var myHandle = new SafeWaitHandle (OpenSemaphore_internal (name, 
                /*SemaphoreRights.Modify | SemaphoreRights.Synchronize*/ 0x000002 | 0x100000, 
                out errorCode), true);
            
            if (myHandle.IsInvalid)
            {
                if (MonoIOError.ERROR_FILE_NOT_FOUND == errorCode || MonoIOError.ERROR_INVALID_NAME == errorCode)
                    return OpenExistingResult.NameNotFound;
                if (MonoIOError.ERROR_PATH_NOT_FOUND == errorCode)
                    return OpenExistingResult.PathNotFound;
                if (null != name && 0 != name.Length && MonoIOError.ERROR_INVALID_HANDLE == errorCode)
                    return OpenExistingResult.NameInvalid;
                //this is for passed through NativeMethods Errors
                throw new IOException ($"Unknown Error '{errorCode}'");
            }
            result = new Semaphore (myHandle);
            return OpenExistingResult.Success;
        }

        void CreateSemaphoreCore (int initialCount, int maximumCount, string name, out bool createdNew)
        {
            if(null != name && MAX_PATH < name.Length)
                throw new ArgumentException (SR.Argument_WaitHandleNameTooLong);

            MonoIOError errorCode;
            var myHandle = new SafeWaitHandle (CreateSemaphore_internal (initialCount, maximumCount, name, out errorCode), true);
            
            if (myHandle.IsInvalid)
            {
                if(null != name && 0 != name.Length && MonoIOError.ERROR_INVALID_HANDLE == errorCode)
                    throw new WaitHandleCannotBeOpenedException (SR.GetString (SR.Threading_WaitHandleCannotBeOpenedException_InvalidHandle, name));

                throw new IOException ($"Unknown Error '{errorCode}'");
            }
            this.SafeWaitHandle = myHandle;
            createdNew = errorCode != MonoIOError.ERROR_ALREADY_EXISTS;
        }

        internal unsafe static IntPtr CreateSemaphore_internal (int initialCount, int maximumCount, string name, out MonoIOError errorCode)
        {
            // FIXME Check for embedded nuls in name.
            fixed (char *fixed_name = name)
                return CreateSemaphore_icall (initialCount, maximumCount,
                    fixed_name, name?.Length ?? 0, out errorCode);
        }

        unsafe static IntPtr OpenSemaphore_internal (string name, int rights, out MonoIOError errorCode)
        {
            // FIXME Check for embedded nuls in name.
            fixed (char *fixed_name = name)
                return OpenSemaphore_icall (fixed_name, name?.Length ?? 0, rights, out errorCode);
        }

        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        unsafe static extern IntPtr CreateSemaphore_icall (int initialCount, int maximumCount, char *name, int name_length, out MonoIOError errorCode);

        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        unsafe static extern IntPtr OpenSemaphore_icall (char *name, int name_length, int rights, out MonoIOError errorCode);

        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        static extern bool ReleaseSemaphore_internal (IntPtr handle, int releaseCount, out int previousCount);
    }
}
