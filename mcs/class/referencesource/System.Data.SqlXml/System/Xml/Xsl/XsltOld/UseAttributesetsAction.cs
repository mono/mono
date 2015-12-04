//------------------------------------------------------------------------------
// <copyright file="UseAttributeSetsAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;
    using System.Collections;

    internal class UseAttributeSetsAction : CompiledAction {
        private XmlQualifiedName[]  useAttributeSets;
        private string              useString;

        private const int  ProcessingSets = 2;

        internal XmlQualifiedName[] UsedSets {
            get { return this.useAttributeSets; }
        }

        internal override void Compile(Compiler compiler) {
            Debug.Assert(Ref.Equal(compiler.Input.LocalName, compiler.Atoms.UseAttributeSets));
            this.useString = compiler.Input.Value;

            Debug.Assert(this.useAttributeSets == null);

            if (this.useString.Length == 0) {
                // Split creates empty node is spliting empty string
                this.useAttributeSets = new XmlQualifiedName[0];
                return;
            }

            string[] qnames = XmlConvert.SplitString(this.useString);

            try {
                this.useAttributeSets = new XmlQualifiedName[qnames.Length]; {
                    for (int i = 0; i < qnames.Length; i++) {
                        this.useAttributeSets[i] = compiler.CreateXPathQName(qnames[i]);
                    }
                }
            }
            catch (XsltException) {
                if (!compiler.ForwardCompatibility) {
                    // Rethrow the exception if we're not in forwards-compatible mode
                    throw;
                }
                // Ignore the whole list in forwards-compatible mode
                this.useAttributeSets = new XmlQualifiedName[0];
            }
        }

        internal override void Execute(Processor processor, ActionFrame frame) {
            switch(frame.State) {
            case Initialized:
                frame.Counter = 0;
                frame.State = ProcessingSets;
                goto case ProcessingSets;

            case ProcessingSets:
                if (frame.Counter < this.useAttributeSets.Length) {
                    AttributeSetAction action = processor.RootAction.GetAttributeSet(this.useAttributeSets[frame.Counter]);
                    frame.IncrementCounter();
                    processor.PushActionFrame(action, frame.NodeSet);
                }
                else {
                    frame.Finished();
                }
                break;

            default:
                Debug.Fail("Invalid Container action execution state");
		        break;
            }
        }
    }
}
