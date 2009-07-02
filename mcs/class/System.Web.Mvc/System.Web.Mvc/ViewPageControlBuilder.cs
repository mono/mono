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
    using System.CodeDom;
    using System.Web.UI;

    internal sealed class ViewPageControlBuilder : FileLevelPageControlBuilder {
        public string PageBaseType {
            get;
            set;
        }

        public override void ProcessGeneratedCode(
            CodeCompileUnit codeCompileUnit,
            CodeTypeDeclaration baseType,
            CodeTypeDeclaration derivedType,
            CodeMemberMethod buildMethod,
            CodeMemberMethod dataBindingMethod) {

            // If we find got a base class string, use it
            if (PageBaseType != null) {
                derivedType.BaseTypes[0] = new CodeTypeReference(PageBaseType);
            }
        }
    }
}
