//
// Mono.ILASM.LdcInstr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class LdcInstr : IInstr {

                private MiscInstr op;
                private double d_operand;
                private long l_operand;

                public LdcInstr (MiscInstr op, double operand)
                {
                        this.op = op;
                        d_operand = operand;
                }

                public LdcInstr (MiscInstr op, long operand)
                {
                        this.op = op;
                        l_operand = operand;
                }

                public void Emit (CodeGen code_gen, PEAPI.CILInstructions cil)
                {
                        switch (op) {
                        case MiscInstr.ldc_r8:
                                cil.ldc_r8 (d_operand);
                                break;
                        case MiscInstr.ldc_r4:
                                cil.ldc_r4 ((float) d_operand);
                                break;
                        case MiscInstr.ldc_i8:
                                cil.ldc_i8 (l_operand);
                                break;
                        }
                }

        }

}

