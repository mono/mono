//------------------------------------------------------------------------------
// <copyright file="ValueOfAction.cs" company="Microsoft">
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

    internal class ValueOfAction : CompiledAction {
        private const int ResultStored = 2;

        private int    selectKey = Compiler.InvalidQueryKey;
        private bool   disableOutputEscaping;

        private static Action s_BuiltInRule = new BuiltInRuleTextAction();

        internal static Action BuiltInRule() {
            Debug.Assert(s_BuiltInRule != null);
            return s_BuiltInRule;
        }

        internal override void Compile(Compiler compiler) {
            CompileAttributes(compiler);
            CheckRequiredAttribute(compiler, selectKey != Compiler.InvalidQueryKey, "select");
            CheckEmpty(compiler);
        }

        internal override bool CompileAttribute(Compiler compiler) {
            string name   = compiler.Input.LocalName;
            string value  = compiler.Input.Value;

            if (Ref.Equal(name, compiler.Atoms.Select)) {
                this.selectKey = compiler.AddQuery(value);
            }
            else if (Ref.Equal(name, compiler.Atoms.DisableOutputEscaping)) {
                this.disableOutputEscaping = compiler.GetYesNo(value);
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
                Debug.Assert(frame != null);
                Debug.Assert(frame.NodeSet != null);

                string value = processor.ValueOf(frame, this.selectKey);

                if (processor.TextEvent(value, disableOutputEscaping)) {
                    frame.Finished();
                }
                else {
                    frame.StoredOutput = value;
                    frame.State        = ResultStored;
                }
                break;

            case ResultStored:
                Debug.Assert(frame.StoredOutput != null);
                processor.TextEvent(frame.StoredOutput);
                frame.Finished();
                break;

            default:
                Debug.Fail("Invalid ValueOfAction execution state");
                break;
            }
        }
    }

    internal class BuiltInRuleTextAction : Action {
        private const int ResultStored = 2;
        internal override void Execute(Processor processor, ActionFrame frame) {
            Debug.Assert(processor != null && frame != null);

            switch (frame.State) {
            case Initialized:
                Debug.Assert(frame != null);
                Debug.Assert(frame.NodeSet != null);

                string value = processor.ValueOf(frame.NodeSet.Current);

                if (processor.TextEvent(value, /*disableOutputEscaping:*/false)) {
                    frame.Finished();
                }
                else {
                    frame.StoredOutput = value;
                    frame.State        = ResultStored;
                }
                break;

            case ResultStored:
                Debug.Assert(frame.StoredOutput != null);
                processor.TextEvent(frame.StoredOutput);
                frame.Finished();
                break;

            default:
                Debug.Fail("Invalid BuiltInRuleTextAction execution state");
                break;
            }
        }
    }    
}
