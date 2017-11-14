// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: MissingMethodException
**
**
** Purpose: The exception class for class loading failures.
**
**
=============================================================================*/

namespace System {
    
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Runtime.CompilerServices;
    using System.Globalization;
[System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public class MissingMethodException : MissingMemberException, ISerializable {
        public MissingMethodException() 
            : base(Environment.GetResourceString("Arg_MissingMethodException")) {
            SetErrorCode(__HResults.COR_E_MISSINGMETHOD);
        }
    
        public MissingMethodException(String message) 
            : base(message) {
            SetErrorCode(__HResults.COR_E_MISSINGMETHOD);
        }
    
        public MissingMethodException(String message, Exception inner) 
            : base(message, inner) {
            SetErrorCode(__HResults.COR_E_MISSINGMETHOD);
        }

        protected MissingMethodException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    
        public override String Message
        {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                if (ClassName == null) {
                    return base.Message;
                } else {
#if MONO
                    string res = ClassName + "." + MemberName;
                    if (!string.IsNullOrEmpty(signature))
                        res = string.Format (CultureInfo.InvariantCulture, signature, res);
                    if (!string.IsNullOrEmpty(_message))
                        res += " Due to: " + _message;
                    return res;
#else
                    // do any desired fixups to classname here.
                    return Environment.GetResourceString("MissingMethod_Name",
                                                                       ClassName + "." + MemberName +
                                                                       (Signature != null ? " " + FormatSignature(Signature) : ""));
#endif
                }
            }
        }
    
        // Called from the EE
        private MissingMethodException(String className, String methodName, byte[] signature)
        {
            ClassName   = className;
            MemberName  = methodName;
            Signature   = signature;
        }
    
        public MissingMethodException(String className, String methodName)
        {
            ClassName   = className;
            MemberName  = methodName;
        }
    
        // If ClassName != null, Message will construct on the fly using it
        // and the other variables. This allows customization of the
        // format depending on the language environment.
#if MONO
        // Called from the EE
        private MissingMethodException(String className, String methodName, String signature, String message) : base (message)
        {
            ClassName   = className;
            MemberName  = methodName;
            this.signature = signature;
        }

		[NonSerialized]
        string signature;
#endif
    }
}
