namespace System
{
    internal interface IRuntimeMethodInfo
    {
        RuntimeMethodHandleInternal Value
        {
            get;
        }
    }

    internal struct RuntimeMethodHandleInternal
    {
        internal static RuntimeMethodHandleInternal EmptyHandle
        {
            get
            {
                return new RuntimeMethodHandleInternal();
            }
        }

        internal bool IsNullHandle()
        {
            return m_handle.IsNull();
        }

        internal IntPtr Value
        {
            get
            {
                return m_handle;
            }
        }

        internal RuntimeMethodHandleInternal(IntPtr value)
        {
            m_handle = value;
        }
      
        internal IntPtr m_handle;
    }
}