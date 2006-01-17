//
// Mono.ILASM.TypeInstr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class TypeInstr : IInstr {

                private PEAPI.TypeOp op;
                private BaseTypeRef operand;

                public TypeInstr (PEAPI.TypeOp op, BaseTypeRef operand, Location loc)
			: base (loc)
                {
                        this.op = op;
                        this.operand = operand;
                }

                public override void Emit (CodeGen code_gen, MethodDef meth, 
					   PEAPI.CILInstructions cil)
                {
                        operand.Resolve (code_gen);
                        cil.TypeInst (op, operand.PeapiType);
                }

        }

}

