//------------------------------------------------------------------------------
// <copyright file="ChooseAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;

    internal class ChooseAction : ContainerAction {
        internal override void Compile(Compiler compiler) {
            CompileAttributes(compiler);

            if (compiler.Recurse()) {
                CompileConditions(compiler);
                compiler.ToParent();
            }
        }

        private void CompileConditions(Compiler compiler) {
            NavigatorInput input = compiler.Input;
            bool when       = false;
            bool otherwise  = false;

            do {
                switch (input.NodeType) {
                case XPathNodeType.Element:
                    compiler.PushNamespaceScope();
                    string nspace = input.NamespaceURI;
                    string name   = input.LocalName;

                    if (Ref.Equal(nspace, input.Atoms.UriXsl)) {
                        IfAction action = null;
                        if (Ref.Equal(name, input.Atoms.When)) {
                            if (otherwise) {
                                throw XsltException.Create(Res.Xslt_WhenAfterOtherwise);
                            }
                            action = compiler.CreateIfAction(IfAction.ConditionType.ConditionWhen);
                            when = true;
                        }
                        else if (Ref.Equal(name, input.Atoms.Otherwise)) {
                            if (otherwise) {
                                throw XsltException.Create(Res.Xslt_DupOtherwise);
                            }
                            action = compiler.CreateIfAction(IfAction.ConditionType.ConditionOtherwise);
                            otherwise = true;
                        }
                        else {
                            throw compiler.UnexpectedKeyword();
                        }
                        AddAction(action);
                    }
                    else {
                        throw compiler.UnexpectedKeyword();
                    }
                    compiler.PopScope();
                    break;

                case XPathNodeType.Comment:
                case XPathNodeType.ProcessingInstruction:
                case XPathNodeType.Whitespace:
                case XPathNodeType.SignificantWhitespace:
                    break;

                default:
                    throw XsltException.Create(Res.Xslt_InvalidContents, "choose");
                }
            }
            while (compiler.Advance());
            if (! when) {
                throw XsltException.Create(Res.Xslt_NoWhen);
            }
        }
    }
}
