//
// Mono.ILASM.BranchInstr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;


namespace Mono.ILASM {

        public class BranchInstr : IInstr {

                private PEAPI.BranchOp op;
                private string label;

                public BranchInstr (PEAPI.BranchOp op, string label)
                {
                        this.op = op;
                        this.label = label;
                }

                public void Emit (CodeGen code_gen, MethodDef meth,
				  PEAPI.CILInstructions cil)
                {
                        cil.Branch (op, meth.GetLabelDef (label));
                }
        }

}

