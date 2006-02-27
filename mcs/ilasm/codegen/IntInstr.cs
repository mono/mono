//
// Mono.ILASM.IntInstr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class IntInstr : IInstr {

                private PEAPI.IntOp op;
                private int operand;

                public IntInstr (PEAPI.IntOp op, int operand, Location loc)
			: base (loc)
                {
                        this.op = op;
                        this.operand = operand;
                }

                public override void Emit (CodeGen code_gen, MethodDef meth,
					   PEAPI.CILInstructions cil)
                {
                        cil.IntInst (op, operand);
                }

        }

}

