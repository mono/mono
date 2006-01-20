//
// Mono.ILASM.MethodInstr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;


namespace Mono.ILASM {

        public class MethodInstr : IInstr {

                private PEAPI.MethodOp op;
                private BaseMethodRef operand;

                public MethodInstr (PEAPI.MethodOp op, BaseMethodRef operand, Location loc)
			: base (loc)
                {
                        this.op = op;
                        this.operand = operand;

                        if (op == PEAPI.MethodOp.newobj || op == PEAPI.MethodOp.callvirt)
                                operand.CallConv |= PEAPI.CallConv.Instance;
                }

                public override void Emit (CodeGen code_gen, MethodDef meth,
					   PEAPI.CILInstructions cil)
                {
                        operand.Resolve (code_gen);
                        cil.MethInst (op, operand.PeapiMethod);
                }
        }

}


