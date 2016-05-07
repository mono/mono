//------------------------------------------------------------------------------
// <copyright file="IfAction.cs" company="Microsoft">
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

    internal class IfAction : ContainerAction {
        internal enum ConditionType {
            ConditionIf,
            ConditionWhen,
            ConditionOtherwise
        }

        private ConditionType   type;
        private int             testKey = Compiler.InvalidQueryKey;

        internal IfAction(ConditionType type) {
            this.type = type;
        }

        internal override void Compile(Compiler compiler) {
            CompileAttributes(compiler);
            if (this.type != ConditionType.ConditionOtherwise) {
                CheckRequiredAttribute(compiler, this.testKey != Compiler.InvalidQueryKey, "test");
            }

            if (compiler.Recurse()) {
                CompileTemplate(compiler);
                compiler.ToParent();
            }
        }

        internal override bool CompileAttribute(Compiler compiler) {
            string name   = compiler.Input.LocalName;
            string value  = compiler.Input.Value;
            if (Ref.Equal(name, compiler.Atoms.Test)) {
                if (this.type == ConditionType.ConditionOtherwise) {
                    return false;
                }
                this.testKey = compiler.AddBooleanQuery(value);
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
                if (this.type == ConditionType.ConditionIf || this.type == ConditionType.ConditionWhen) {
                    Debug.Assert(this.testKey != Compiler.InvalidQueryKey);
                    bool value = processor.EvaluateBoolean(frame, this.testKey);
                    if (value == false) {
                        frame.Finished();
                        break;
                    }
                }

                processor.PushActionFrame(frame);
                frame.State = ProcessingChildren;
                break;                              // Allow children to run

            case ProcessingChildren:
                if (this.type == ConditionType.ConditionWhen ||this.type == ConditionType.ConditionOtherwise) {
                    Debug.Assert(frame.Container != null);
                    frame.Exit();
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
