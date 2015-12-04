//------------------------------------------------------------------------------
// <copyright file="CodeObjectCreateExpression.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
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
    ///       Represents an object create expression.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeObjectCreateExpression : CodeExpression {
        private CodeTypeReference createType;
        private CodeExpressionCollection parameters = new CodeExpressionCollection();

        /// <devdoc>
        ///    <para>
        ///       Initializes a new <see cref='System.CodeDom.CodeObjectCreateExpression'/>.
        ///    </para>
        /// </devdoc>
        public CodeObjectCreateExpression() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new <see cref='System.CodeDom.CodeObjectCreateExpression'/> using the specified type and
        ///       parameters.
        ///    </para>
        /// </devdoc>
        public CodeObjectCreateExpression(CodeTypeReference createType, params CodeExpression[] parameters) {
            CreateType = createType;
            Parameters.AddRange(parameters);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeObjectCreateExpression(string createType, params CodeExpression[] parameters) {
            CreateType = new CodeTypeReference(createType);
            Parameters.AddRange(parameters);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeObjectCreateExpression(Type createType, params CodeExpression[] parameters) {
            CreateType = new CodeTypeReference(createType);
            Parameters.AddRange(parameters);
        }

        /// <devdoc>
        ///    <para>
        ///       The type of the object to create.
        ///    </para>
        /// </devdoc>
        public CodeTypeReference CreateType {
            get {
                if (createType == null) {
                    createType = new CodeTypeReference("");
                }
                return createType;
            }
            set {
                createType = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the parameters to use in creating the
        ///       object.
        ///    </para>
        /// </devdoc>
        public CodeExpressionCollection Parameters {
            get {
                return parameters;
            }
        }
    }
}
