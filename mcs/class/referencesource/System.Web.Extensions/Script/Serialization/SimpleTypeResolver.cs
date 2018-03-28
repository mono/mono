//------------------------------------------------------------------------------
// <copyright file="SimpleTypeResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Serialization {
    using System;
    using System.Web;

    public class SimpleTypeResolver : JavaScriptTypeResolver {
        public override Type ResolveType(string id) {
            return Type.GetType(id);
        }

        public override string ResolveTypeId(Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            return type.AssemblyQualifiedName;
        }
    }
}
