//------------------------------------------------------------------------------
// <copyright file="JavaScriptConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Serialization {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    public abstract class JavaScriptConverter {
        public abstract IEnumerable<Type> SupportedTypes {
            get;
        }

        public abstract object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
            Justification = "Cannot change parameter name as would break binary compatibility with legacy apps.")]
        public abstract IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer);
    }
}
