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

                public SimpInstr (PEAPI.Op op)
                {
                        this.op = op;
                }

                public void Emit (CodeGen code_gen, PEAPI.CILInstructions cil)
                {
                        cil.Inst (op);
                }

        }

}

