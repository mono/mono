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
                private MethodDef method;
                private string label;

                public BranchInstr (PEAPI.BranchOp op, MethodDef method, string label)
                {
                        this.op = op;
                        this.method = method;
                        this.label = label;
                }

                public void Emit (CodeGen code_gen, PEAPI.CILInstructions cil)
                {
                        cil.Branch (op, method.GetLabelDef (label));
                }
        }

}

