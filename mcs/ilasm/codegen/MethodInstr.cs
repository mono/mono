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
                private IMethodRef operand;

                public MethodInstr (PEAPI.MethodOp op, IMethodRef operand)
                {
                        this.op = op;
                        this.operand = operand;
                }

                public void Emit (CodeGen code_gen, MethodDef meth, 
				  PEAPI.CILInstructions cil)
                {
                        operand.Resolve (code_gen);
                        cil.MethInst (op, operand.PeapiMethod);
                }
        }

}


