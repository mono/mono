//------------------------------------------------------------------------------
// <copyright file="DesignTimeResourceProviderFactoryAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    using System;
    using System.ComponentModel;
    using System.Security.Permissions;


    /// <devdoc>
    /// <para>Allows a ResourceProviderFactory to specify the type of the associated
    ///    DesignTimeResourceProviderFactory.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DesignTimeResourceProviderFactoryAttribute : Attribute {

        private string _factoryTypeName;

        public DesignTimeResourceProviderFactoryAttribute(Type factoryType) {
            _factoryTypeName = factoryType.AssemblyQualifiedName;
        }

        public DesignTimeResourceProviderFactoryAttribute(string factoryTypeName) {
            _factoryTypeName = factoryTypeName;
        }

        public string FactoryTypeName {
            get {
                return _factoryTypeName;
            }
        }

        public override bool IsDefaultAttribute() {
            return (_factoryTypeName == null);
        }
    }
}
