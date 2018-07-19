//------------------------------------------------------------------------------
// <copyright file="CodeStatementBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System.CodeDom;

    /// <summary>
    /// A ControlBuilder implementation that generates Code DOM statements
    /// </summary>
    public abstract class CodeStatementBuilder : ControlBuilder {

        /// <summary>
        /// Build a CodeStatement for a generated Render method.
        /// </summary>
        public abstract CodeStatement BuildStatement(CodeArgumentReferenceExpression writerReferenceExpression);
    }
}
