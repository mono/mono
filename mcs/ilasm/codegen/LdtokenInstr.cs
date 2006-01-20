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
                private BaseMethodRef method_ref;
                private BaseTypeRef type_ref;

                public LdtokenInstr (IFieldRef field_ref, Location loc)
			: base (loc)
                {
                        this.field_ref = field_ref;
                }

                public LdtokenInstr (BaseMethodRef method_ref, Location loc)
			: base (loc)
                {
                        this.method_ref = method_ref;
                }

                public LdtokenInstr (BaseTypeRef type_ref, Location loc)
			: base (loc)
                {
                        this.type_ref = type_ref;
                }

                public override void Emit (CodeGen code_gen, MethodDef meth,
					   PEAPI.CILInstructions cil)
                {
                        if (field_ref != null) {
                                field_ref.Resolve (code_gen);
                                cil.FieldInst (PEAPI.FieldOp.ldtoken,
                                                field_ref.PeapiField);
                        } else if (method_ref != null) {
                                method_ref.Resolve (code_gen);
                                cil.MethInst (PEAPI.MethodOp.ldtoken,
                                                method_ref.PeapiMethod);
                        } else if (type_ref != null) {
                                type_ref.Resolve (code_gen);
                                cil.TypeInst (PEAPI.TypeOp.ldtoken,
                                                type_ref.PeapiType);
                        }
                }

        }

}

