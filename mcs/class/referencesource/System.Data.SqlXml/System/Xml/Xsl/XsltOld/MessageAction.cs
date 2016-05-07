//------------------------------------------------------------------------------
// <copyright file="MessageAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.IO;
    using System.Globalization;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;

    internal class MessageAction : ContainerAction {
        bool _Terminate;

        internal override void Compile(Compiler compiler) {
            CompileAttributes(compiler);

            if (compiler.Recurse()) {
                CompileTemplate(compiler);
                compiler.ToParent();
            }
        }

        internal override bool CompileAttribute(Compiler compiler) {
            string name   = compiler.Input.LocalName;
            string value  = compiler.Input.Value;
            if (Ref.Equal(name, compiler.Atoms.Terminate)) {
                _Terminate = compiler.GetYesNo(value);
            }
            else {
                return false;
            }

            return true;
        }

        internal override void Execute(Processor processor, ActionFrame frame) {
            Debug.Assert(processor != null && frame != null);
            switch (frame.State) {
            case Initialized:
                TextOnlyOutput output = new TextOnlyOutput(processor, new StringWriter(CultureInfo.InvariantCulture));
                processor.PushOutput(output);
                processor.PushActionFrame(frame);
                frame.State = ProcessingChildren;
                break;

            case ProcessingChildren:
                TextOnlyOutput recOutput = processor.PopOutput() as TextOnlyOutput;
                Debug.Assert(recOutput != null);
                Console.WriteLine(recOutput.Writer.ToString());

                if (_Terminate) {
                    throw XsltException.Create(Res.Xslt_Terminate, recOutput.Writer.ToString());
                }
                frame.Finished();
                break;
            
            default:
                Debug.Fail("Invalid MessageAction execution state");
                break;
            }
        }
    }
}
