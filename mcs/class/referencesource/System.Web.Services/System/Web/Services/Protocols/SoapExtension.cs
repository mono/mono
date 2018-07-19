//------------------------------------------------------------------------------
// <copyright file="SoapExtension.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Web.Services;
    using System.Xml.Serialization;
    using System;
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.ComponentModel;

    /// <include file='doc\SoapExtension.uex' path='docs/doc[@for="SoapExtension"]/*' />
    public abstract class SoapExtension {

        /// <include file='doc\SoapExtension.uex' path='docs/doc[@for="SoapExtension.GetInitializer"]/*' />
        public abstract object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute);

        /// <include file='doc\SoapExtension.uex' path='docs/doc[@for="SoapExtension.GetInitializer1"]/*' />
        public abstract object GetInitializer(Type serviceType);

        /// <include file='doc\SoapExtension.uex' path='docs/doc[@for="SoapExtension.Initialize"]/*' />
        public abstract void Initialize(object initializer);

        /// <include file='doc\SoapExtension.uex' path='docs/doc[@for="SoapExtension.ProcessMessage"]/*' />
        public abstract void ProcessMessage(SoapMessage message);
        
        /// <include file='doc\SoapExtension.uex' path='docs/doc[@for="SoapExtension.ChainStream"]/*' />
        public virtual Stream ChainStream(Stream stream) {
            return stream;
        }
    }

}
