//------------------------------------------------------------------------------
// <copyright file="LicenseException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Represents the exception thrown when a component cannot be granted a license.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")] // must not, a Type is required in all constructors.
    [Serializable]
    public class LicenseException : SystemException {
        private Type type;
        private object instance;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.LicenseException'/> class for the 
        ///    specified type.</para>
        /// </devdoc>
        public LicenseException(Type type) : this(type, null, SR.GetString(SR.LicExceptionTypeOnly, type.FullName)) {
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.LicenseException'/> class for the 
        ///    specified type and instance.</para>
        /// </devdoc>
        public LicenseException(Type type, object instance) : this(type, null, SR.GetString(SR.LicExceptionTypeAndInstance, type.FullName, instance.GetType().FullName))  {
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.LicenseException'/> class for the 
        ///    specified type and instance with the specified message.</para>
        /// </devdoc>
        public LicenseException(Type type, object instance, string message) : base(message) {
            this.type = type;
            this.instance = instance;
            HResult = HResults.License;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.LicenseException'/> class for the 
        ///    specified innerException, type and instance with the specified message.</para>
        /// </devdoc>
        public LicenseException(Type type, object instance, string message, Exception innerException) : base(message, innerException) {
            this.type = type;
            this.instance = instance;
            HResult = HResults.License;
        }
   
        /// <devdoc>
        ///     Need this constructor since Exception implements ISerializable. 
        /// </devdoc>
        protected LicenseException(SerializationInfo info, StreamingContext context) : base (info, context) {
            type = (Type) info.GetValue("type", typeof(Type));
            instance = info.GetValue("instance", typeof(object));
        }

        /// <devdoc>
        ///    <para>Gets the type of the component that was not granted a license.</para>
        /// </devdoc>
        public Type LicensedType {
            get {
                return type;
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

            info.AddValue("type", type);
            info.AddValue("instance", instance);

            base.GetObjectData(info, context);
        }
    }
}
