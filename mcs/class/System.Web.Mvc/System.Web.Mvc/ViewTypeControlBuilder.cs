/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Web.UI;

    internal sealed class ViewTypeControlBuilder : ControlBuilder {
        private string _typeName;

        public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, IDictionary attribs) {
            base.Init(parser, parentBuilder, type, tagName, id, attribs);

            _typeName = (string)attribs["typename"];
        }

        public override void ProcessGeneratedCode(
            CodeCompileUnit codeCompileUnit,
            CodeTypeDeclaration baseType,
            CodeTypeDeclaration derivedType,
            CodeMemberMethod buildMethod,
            CodeMemberMethod dataBindingMethod) {

            // Override the view's base type with the explicit base type
            derivedType.BaseTypes[0] = new CodeTypeReference(_typeName);
        }
    }
}
