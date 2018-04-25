//------------------------------------------------------------------------------
// <copyright file="PreApplicationStartMethodAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class PreApplicationStartMethodAttribute : Attribute {

        private readonly Type _type;
        private readonly string _methodName;

        public PreApplicationStartMethodAttribute(Type type, string methodName) {
            _type = type;
            _methodName = methodName;
        }

        public Type Type {
            get {
                return _type;
            }
        }

        public string MethodName {
            get {
                return _methodName;
            }
        }
    }
}
