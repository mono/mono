//
// Mono.ILASM.LdtokenInstr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class LdtokenInstr : IInstr {

                private IFieldRef field_ref;
                private IMethodRef method_ref;

                public LdtokenInstr (IFieldRef field_ref)
                {
                        this.field_ref = field_ref;
                }

                public LdtokenInstr (IMethodRef method_ref)
                {
                        this.method_ref = method_ref;
                }

                public void Emit (CodeGen code_gen, PEAPI.CILInstructions cil)
                {
                        if (field_ref != null) {
                                field_ref.Resolve (code_gen);
                                cil.FieldInst (PEAPI.FieldOp.ldtoken,
                                                field_ref.PeapiField);
                        } else if (method_ref != null) {
                                method_ref.Resolve (code_gen);
                                cil.MethInst (PEAPI.MethodOp.ldtoken,
                                                method_ref.PeapiMethod);
                        }
                }

        }

}

