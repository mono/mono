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
                private ITypeRef operand;

                public TypeInstr (PEAPI.TypeOp op, ITypeRef operand)
                {
                        this.op = op;
                        this.operand = operand;
                }

                public void Emit (CodeGen code_gen, MethodDef meth, 
				  PEAPI.CILInstructions cil)
                {
                        operand.Resolve (code_gen);
                        cil.TypeInst (op, operand.PeapiType);
                }

        }

}

