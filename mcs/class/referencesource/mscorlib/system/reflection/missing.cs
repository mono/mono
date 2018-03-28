// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

namespace System.Reflection 
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Diagnostics.Contracts;

    // This is not serializable because it is a reflection command.
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class Missing: ISerializable
    {
        public static readonly Missing Value = new Missing();

        #region Constructor
        private Missing() { }
        #endregion

#if FEATURE_SERIALIZATION
        #region ISerializable
        [System.Security.SecurityCritical]  // auto-generated_required
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            Contract.EndContractBlock();

            UnitySerializationHolder.GetUnitySerializationInfo(info, this);
        }
        #endregion
#endif
    }
}
