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

                public LdstrInstr (string operand)
                {
                        this.operand = operand;
                }

                public void Emit (CodeGen code_gen, PEAPI.CILInstructions cil)
                {
                        cil.ldstr (operand);
                }

        }

}

