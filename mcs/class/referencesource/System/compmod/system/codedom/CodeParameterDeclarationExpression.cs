//------------------------------------------------------------------------------
// <copyright file="CodeParameterDeclarationExpression.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom {

    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using System.Collections;
    using System.Runtime.InteropServices;

    /// <devdoc>
    ///    <para>
    ///       Represents a parameter declaration for method, constructor, or property arguments.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeParameterDeclarationExpression : CodeExpression {
        private CodeTypeReference type;
        private string name;
        private CodeAttributeDeclarationCollection customAttributes = null;
        private FieldDirection dir = FieldDirection.In;


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeParameterDeclarationExpression'/>.
        ///    </para>
        /// </devdoc>
        public CodeParameterDeclarationExpression() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeParameterDeclarationExpression'/> using the specified type and name.
        ///    </para>
        /// </devdoc>
        public CodeParameterDeclarationExpression(CodeTypeReference type, string name) {
            Type = type;
            Name = name;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeParameterDeclarationExpression(string type, string name) {
            Type = new CodeTypeReference(type);
            Name = name;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeParameterDeclarationExpression(Type type, string name) {
            Type = new CodeTypeReference(type);
            Name = name;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the custom attributes for the parameter declaration.
        ///    </para>
        /// </devdoc>
        public CodeAttributeDeclarationCollection CustomAttributes {
            get {
                if (customAttributes == null) {
                    customAttributes = new CodeAttributeDeclarationCollection();
                }
                return customAttributes;
            }
            set {
                customAttributes = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the direction of the field.
        ///    </para>
        /// </devdoc>
        public FieldDirection Direction {
            get {
                return dir;
            }
            set {
                dir = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the type of the parameter.
        ///    </para>
        /// </devdoc>
        public CodeTypeReference Type {
            get {
                if (type == null) {
                    type = new CodeTypeReference("");
                }
                return type;
            }
            set {
                type = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the name of the parameter.
        ///    </para>
        /// </devdoc>
        public string Name {
            get {
                return (name == null) ? string.Empty : name;
            }
            set {
                name = value;
            }
        }
    }
}
