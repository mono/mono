//------------------------------------------------------------------------------
// <copyright file="Warning.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {    
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    
    /// <devdoc>
    ///    <para>Specifies an exception that is handled as a warning instead of an error.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [Serializable]
    public class WarningException : SystemException {
        private readonly string helpUrl;
        private readonly string helpTopic;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.Win32Exception'/> class with the last Win32 error 
        ///    that occured.</para>
        /// </devdoc>
        public WarningException() : this(null, null, null) {
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.WarningException'/> class with 
        ///    the specified message and no Help file.</para>
        /// </devdoc>
        public WarningException(string message) : this(message, null, null) {
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.WarningException'/> class with 
        ///    the specified message, and with access to the specified Help file.</para>
        /// </devdoc>
        public WarningException(string message, string helpUrl) : this(message, helpUrl, null) {
        }

        /// <devdoc>
        ///     Initializes a new instance of the Exception class with a specified error message and a 
        ///     reference to the inner exception that is the cause of this exception.
        ///     FxCop CA1032: Multiple constructors are required to correctly implement a custom exception.
        /// </devdoc>
        public WarningException( string message, Exception innerException ) 
            : base(message, innerException) {
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.WarningException'/> class with the 
        ///    specified message, and with access to the specified Help file and topic.</para>
        /// </devdoc>
        public WarningException(string message, string helpUrl, string helpTopic)
            : base(message) {
            this.helpUrl = helpUrl;
            this.helpTopic = helpTopic;
        }

        /// <devdoc>
        ///     Need this constructor since Exception implements ISerializable. 
        /// </devdoc>
        protected WarningException(SerializationInfo info, StreamingContext context) : base (info, context) {
            helpUrl = (string) info.GetValue("helpUrl", typeof(string));
            helpTopic = (string) info.GetValue("helpTopic", typeof(string));
        }

        /// <devdoc>
        ///    <para> Specifies the Help file associated with the 
        ///       warning. This field is read-only.</para>
        /// </devdoc>
        public string HelpUrl {
            get {
                return helpUrl;
            }
        }

        /// <devdoc>
        ///    <para> Specifies the 
        ///       Help topic associated with the warning. This field is read-only. </para>
        /// </devdoc>
        public string HelpTopic {
            get {
                return helpTopic;
            }
        }

        /// <devdoc>
        ///     Need this since Exception implements ISerializable and we have fields to save out.
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            info.AddValue("helpUrl", helpUrl);
            info.AddValue("helpTopic", helpTopic);

            base.GetObjectData(info, context);
        }
    }
}
