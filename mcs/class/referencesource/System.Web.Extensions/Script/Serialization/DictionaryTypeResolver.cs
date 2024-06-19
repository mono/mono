//------------------------------------------------------------------------------
// <copyright file="DictionaryTypeResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

 namespace System.Web.Script.Serialization {
    using System;
    using System.Web;
    using System.Collections.Generic;

     internal class DictionaryTypeResolver : JavaScriptTypeResolver {
        public override Type ResolveType(string id) {
            return typeof(Dictionary<string, object>);
        }

         public override string ResolveTypeId(Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

             return type.AssemblyQualifiedName;
        }
    }
}