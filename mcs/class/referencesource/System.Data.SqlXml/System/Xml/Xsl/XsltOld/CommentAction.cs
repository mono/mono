//------------------------------------------------------------------------------
// <copyright file="CommentAction.cs" company="Microsoft">
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

    internal class CommentAction : ContainerAction {
        internal override void Compile(Compiler compiler) {
            CompileAttributes(compiler);

            if (compiler.Recurse()) {
                CompileTemplate(compiler);
                compiler.ToParent();
            }
        }

        internal override void Execute(Processor processor, ActionFrame frame) {
            Debug.Assert(processor != null && frame != null);

            switch (frame.State) {
            case Initialized:
                if (processor.BeginEvent(XPathNodeType.Comment, string.Empty, string.Empty, string.Empty, false) == false) {
                    // Come back later
                    break;
                }

                processor.PushActionFrame(frame);
                frame.State = ProcessingChildren;
                break;                              // Allow children to run

            case ProcessingChildren:
                if (processor.EndEvent(XPathNodeType.Comment) == false) {
                    break;
                }

                frame.Finished();
                break;

            default:
                Debug.Fail("Invalid IfAction execution state");
    		    break;
            }
        }
    }
}
