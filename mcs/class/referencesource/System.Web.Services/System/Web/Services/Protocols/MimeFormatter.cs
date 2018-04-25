//------------------------------------------------------------------------------
// <copyright file="MimeFormatter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.IO;
    using System;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections;
    using System.Web.Services;
    using System.Security.Permissions;

    /// <include file='doc\MimeFormatter.uex' path='docs/doc[@for="MimeFormatter"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class MimeFormatter {
        /// <include file='doc\MimeFormatter.uex' path='docs/doc[@for="MimeFormatter.GetInitializer"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract object GetInitializer(LogicalMethodInfo methodInfo);
        /// <include file='doc\MimeFormatter.uex' path='docs/doc[@for="MimeFormatter.Initialize"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void Initialize(object initializer);

        /// <include file='doc\MimeFormatter.uex' path='docs/doc[@for="MimeFormatter.GetInitializers"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual object[] GetInitializers(LogicalMethodInfo[] methodInfos) {
            object[] initializers = new object[methodInfos.Length];
            for (int i = 0; i < initializers.Length; i++)
                initializers[i] = GetInitializer(methodInfos[i]);
            return initializers;
        }

        /// <include file='doc\MimeFormatter.uex' path='docs/doc[@for="MimeFormatter.GetInitializer1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static object GetInitializer(Type type, LogicalMethodInfo methodInfo) {
            return ((MimeFormatter)Activator.CreateInstance(type)).GetInitializer(methodInfo);
        }

        /// <include file='doc\MimeFormatter.uex' path='docs/doc[@for="MimeFormatter.GetInitializers1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static object[] GetInitializers(Type type, LogicalMethodInfo[] methodInfos) {
            return ((MimeFormatter)Activator.CreateInstance(type)).GetInitializers(methodInfos);
        }

        /// <include file='doc\MimeFormatter.uex' path='docs/doc[@for="MimeFormatter.CreateInstance"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static MimeFormatter CreateInstance(Type type, object initializer) {
            MimeFormatter formatter = (MimeFormatter)Activator.CreateInstance(type);
            formatter.Initialize(initializer);
            return formatter;
        }
    }

}
