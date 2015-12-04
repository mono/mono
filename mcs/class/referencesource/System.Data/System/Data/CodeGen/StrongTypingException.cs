//------------------------------------------------------------------------------
// <copyright file="StrongTypingException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Collections;
    using System.Data;
    using System.Runtime.Serialization;

    /// <devdoc>
    ///    <para>DEV: The exception that is throwing from strong typed DataSet when user access to DBNull value.</para>
    /// </devdoc>
    [Serializable]
    public class StrongTypingException : DataException {
        protected StrongTypingException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public StrongTypingException() : base() {
            HResult = HResults.StrongTyping;
        }

        public StrongTypingException(string message)  : base(message) {
            HResult = HResults.StrongTyping;
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public StrongTypingException(string s, Exception innerException) : base(s, innerException) {
            HResult = HResults.StrongTyping;
        }
    }


    /// <devdoc>
    ///    <para>DEV: The exception that is throwing in generating strong typed DataSet when name conflict happens.</para>
    /// </devdoc>
    [Serializable]
    public class TypedDataSetGeneratorException : DataException {
        private ArrayList errorList;
        private string KEY_ARRAYCOUNT = "KEY_ARRAYCOUNT";
        private string KEY_ARRAYVALUES = "KEY_ARRAYVALUES";

        protected TypedDataSetGeneratorException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
            int count = (int) info.GetValue(KEY_ARRAYCOUNT, typeof(System.Int32));
            if (count > 0) {
                errorList = new ArrayList();
                for (int i = 0; i < count; i++) {
                    errorList.Add(info.GetValue(KEY_ARRAYVALUES + i, typeof(System.String)));
                }
            }
            else
                errorList = null;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public TypedDataSetGeneratorException() : base() {
            errorList = null;
            HResult = HResults.StrongTyping;
        }

        public TypedDataSetGeneratorException(string message)  : base(message) {
            HResult = HResults.StrongTyping;
        }

        public TypedDataSetGeneratorException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.StrongTyping;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public TypedDataSetGeneratorException(ArrayList list) : this() {
            errorList = list;
            HResult = HResults.StrongTyping;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ArrayList ErrorList {
            get {
                return errorList;
            }
        }
        
        [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags=System.Security.Permissions.SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (errorList != null) {
                info.AddValue(KEY_ARRAYCOUNT, errorList.Count);
                for (int i = 0; i < errorList.Count; i++) {
                    info.AddValue(KEY_ARRAYVALUES + i, errorList[i].ToString());
                }
            }
            else {
                info.AddValue(KEY_ARRAYCOUNT, 0);
            }
        }
    }
}
