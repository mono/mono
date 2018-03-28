//------------------------------------------------------------------------------
// <copyright file="GenerateScriptTypeAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Services {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    [
    AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)
    ]
    public sealed class GenerateScriptTypeAttribute : Attribute {

        // Constructors
        public GenerateScriptTypeAttribute(Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            _type = type;
        }

        // Instance Properties
        private Type _type;
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods",
            Justification = "Distinguishable from Object.GetType()")]
        public Type Type {
            get {
                return _type;
            }
        }

        private string _typeId;
        public string ScriptTypeId {
            get {
                return _typeId ?? String.Empty;
            }
            set {
                _typeId = value;
            }
        }
    }
}
