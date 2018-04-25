//------------------------------------------------------------------------------
// <copyright file="newinstructionaction.cs" company="Microsoft">
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

    internal class NewInstructionAction : ContainerAction {
        string name;
        string parent;
        bool fallback;
        
        internal override void Compile(Compiler compiler) {
            XPathNavigator nav = compiler.Input.Navigator.Clone();
            name = nav.Name;
            nav.MoveToParent();
            parent = nav.Name;
            if (compiler.Recurse()) {
                CompileSelectiveTemplate(compiler);
                compiler.ToParent();
            }
        }

        internal void CompileSelectiveTemplate(Compiler compiler){
            NavigatorInput input = compiler.Input;
            do{
                if (Ref.Equal(input.NamespaceURI, input.Atoms.UriXsl) &&
                    Ref.Equal(input.LocalName, input.Atoms.Fallback)){
                    fallback = true;
                    if (compiler.Recurse()){
                        CompileTemplate(compiler);
                        compiler.ToParent();
                    }
                }
            }
            while (compiler.Advance());
        }
        
       internal override void Execute(Processor processor, ActionFrame frame) {
            Debug.Assert(processor != null && frame != null);

            switch (frame.State) {
            case Initialized:
                if (!fallback) {
                    throw XsltException.Create(Res.Xslt_UnknownExtensionElement, this.name);
                }
                if (this.containedActions != null && this.containedActions.Count > 0) {
                    processor.PushActionFrame(frame);
                    frame.State = ProcessingChildren;
                    break;
                }
                else goto case ProcessingChildren;
            case ProcessingChildren:
                frame.Finished();
                break;

            default:
                Debug.Fail("Invalid Container action execution state");
                break;
            }
        }
    }
}
