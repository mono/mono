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
                private LabelInfo label;
	
                public BranchInstr (PEAPI.BranchOp op, LabelInfo label, Location loc)
			: base (loc)
                {
                        this.op = op;
                        this.label = label;
                }

                public override void Emit (CodeGen code_gen, MethodDef meth,
					   PEAPI.CILInstructions cil)
                {
			cil.Branch (op, label.Label);
                }
        }

}

