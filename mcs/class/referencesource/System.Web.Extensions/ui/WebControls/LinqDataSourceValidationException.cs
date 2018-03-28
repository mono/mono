//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceValidationException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// NOTE: Suppress messages for CA2126 and CA2114 work around FxCop bugs that are resolved in the latest FxCop release.
namespace System.Web.UI.WebControls {
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Web.Resources;
    using System.Web.DynamicData;
    using System;    

    [SuppressMessage("Microsoft.Security", "CA2126:TypeLinkDemandsRequireInheritanceDemands", Justification="Workaround for FxCop Bug")]
    [Serializable]
    public class LinqDataSourceValidationException : Exception, IDynamicValidatorException, ISerializable {

        private IDictionary<string, Exception> _innerExceptions;

        public LinqDataSourceValidationException() : base(AtlasWeb.LinqDataSourceValidationException_ValidationFailed) {
        }

        public LinqDataSourceValidationException(string message) : base(message) {
        }

        public LinqDataSourceValidationException(string message, Exception innerException)
            : base(message, innerException) {
        }

        public LinqDataSourceValidationException(string message, IDictionary<string, Exception> innerExceptions)
            : this(message) {
            _innerExceptions = innerExceptions;
        }

        protected LinqDataSourceValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            _innerExceptions = (IDictionary<string, Exception>)
                info.GetValue("InnerExceptions", typeof(IDictionary<string, Exception>));
        }

        public IDictionary<string, Exception> InnerExceptions {
            get {
                if (_innerExceptions == null) {
                    _innerExceptions = new Dictionary<string, Exception>(StringComparer.OrdinalIgnoreCase);
                }
                return _innerExceptions;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2114:MethodSecurityShouldBeASupersetOfType", Justification = "Workaround for FxCop Bug")]
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase",
            Justification="Base exception doesn't declare the AspNetHostingPermission link demand required by this class")]
        // Transparency
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("InnerExceptions", InnerExceptions, typeof(IDictionary<string, Exception>));
        }

    }

}
