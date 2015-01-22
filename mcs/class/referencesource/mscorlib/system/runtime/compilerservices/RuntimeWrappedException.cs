// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: RuntimeWrappedException
**
**
** Purpose: The exception class uses to wrap all non-CLS compliant exceptions.
**
**
=============================================================================*/

namespace System.Runtime.CompilerServices {
    using System;
    using System.Runtime.Serialization;
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System.Diagnostics.Contracts;
        
    [Serializable]
    public sealed class RuntimeWrappedException : Exception
    {
        private RuntimeWrappedException(Object thrownObject)
            : base(Environment.GetResourceString("RuntimeWrappedException")) {
            SetErrorCode(System.__HResults.COR_E_RUNTIMEWRAPPED);
            m_wrappedException = thrownObject;
        }
    
        public Object WrappedException {
            get { return m_wrappedException; }
        }

        private Object m_wrappedException;

        [System.Security.SecurityCritical]  // auto-generated_required
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info==null) {
                throw new ArgumentNullException("info");
            }
            Contract.EndContractBlock();
            base.GetObjectData(info, context);
            info.AddValue("WrappedException", m_wrappedException, typeof(Object));
        }

        internal RuntimeWrappedException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            m_wrappedException = info.GetValue("WrappedException", typeof(Object));
        }
    }
}

