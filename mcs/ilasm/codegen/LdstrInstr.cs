//
// Mono.ILASM.LdstrInstr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class LdstrInstr : IInstr {

                private string operand;
                private byte[] b_operand;
                
                public LdstrInstr (string operand, Location loc)
			: base (loc)
                {
                        this.operand = operand;
                }

                public LdstrInstr (byte[] b_operand, Location loc)
			: base (loc)
                {
                        this.b_operand = b_operand;
                }
                
                public override void Emit (CodeGen code_gen, MethodDef meth,
					   PEAPI.CILInstructions cil)
                {
                        if (operand != null)
                                cil.ldstr (operand);
                        else
                                cil.ldstr (b_operand);
                }

        }

}

