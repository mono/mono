//
// Mono.ILASM.SimpInstr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class SimpInstr : IInstr {

                private PEAPI.Op op;

                public SimpInstr (PEAPI.Op op, Location loc)
			: base (loc)
                {
                        this.op = op;
                }

                public override void Emit (CodeGen code_gen, MethodDef meth, 
					   PEAPI.CILInstructions cil)
                {
                        cil.Inst (op);
                }

        }

}

