//------------------------------------------------------------------------------
// <copyright file="ExpressionEditorAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class ExpressionEditorAttribute : Attribute {
        private string _editorTypeName;


        public ExpressionEditorAttribute(Type type) : this((type != null) ? type.AssemblyQualifiedName : null) {
        }


        public ExpressionEditorAttribute(string typeName) {
            if (String.IsNullOrEmpty(typeName)) {
                throw new ArgumentNullException("typeName");
            }

            _editorTypeName = typeName;
        }


        public string EditorTypeName {
            get {
                return _editorTypeName;
            }
        }


        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            ExpressionEditorAttribute other = obj as ExpressionEditorAttribute;

            return ((other != null) && (other.EditorTypeName == EditorTypeName));
        }


        public override int GetHashCode() {
            return EditorTypeName.GetHashCode();
        }
    }
}
