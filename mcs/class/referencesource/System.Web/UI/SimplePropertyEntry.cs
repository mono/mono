//------------------------------------------------------------------------------
// <copyright file="SimplePropertyEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System.CodeDom;
    using System.Web.Compilation;
    using Debug=System.Web.Util.Debug;

    /// <devdoc>
    /// PropertyEntry for simple attributes
    /// </devdoc>
    public class SimplePropertyEntry : PropertyEntry {
        private string _persistedValue;
        private bool _useSetAttribute;
        private object _value;

        internal SimplePropertyEntry() {
        }


        /// <devdoc>
        /// </devdoc>
        // 
        public string PersistedValue {
            get {
                return _persistedValue;
            }
            set {
                _persistedValue = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public bool UseSetAttribute {
            get {
                return _useSetAttribute;
            }
            set {
                _useSetAttribute = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public object Value {
            get {
                return _value;
            }
            set {
                _value = value;
            }
        }

        // Build the statement that assigns this property
        internal CodeStatement GetCodeStatement(BaseTemplateCodeDomTreeGenerator generator,
            CodeExpression ctrlRefExpr) {

            // If we don't have a type, use IAttributeAccessor.SetAttribute
            if (UseSetAttribute) {
                // e.g. ((IAttributeAccessor)__ctrl).SetAttribute("{{_name}}", "{{_value}}");
                CodeMethodInvokeExpression methCallExpression = new CodeMethodInvokeExpression(
                    new CodeCastExpression(typeof(IAttributeAccessor), ctrlRefExpr),
                        "SetAttribute");

                methCallExpression.Parameters.Add(new CodePrimitiveExpression(Name));
                methCallExpression.Parameters.Add(new CodePrimitiveExpression(Value));
                return new CodeExpressionStatement(methCallExpression);
            }

            CodeExpression leftExpr, rightExpr = null;

            if (PropertyInfo != null) {
                leftExpr = CodeDomUtility.BuildPropertyReferenceExpression(ctrlRefExpr, Name);
            }
            else {
                // In case of a field, there should only be one (unlike properties)
                Debug.Assert(Name.IndexOf('.') < 0, "_name.IndexOf('.') < 0");
                leftExpr = new CodeFieldReferenceExpression(ctrlRefExpr, Name);
            }

            if (Type == typeof(string)) {
                rightExpr = generator.BuildStringPropertyExpression(this);
            }
            else {
                rightExpr = CodeDomUtility.GenerateExpressionForValue(PropertyInfo, Value, Type);
            }

            // Now that we have both side, add the assignment
            return new CodeAssignStatement(leftExpr, rightExpr);
        }
    }

}


