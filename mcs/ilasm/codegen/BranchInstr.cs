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
		private uint offset;
		private bool use_offset;

                public BranchInstr (PEAPI.BranchOp op, string label)
                {
                        this.op = op;
                        this.label = label;
			use_offset = false;
                }

		public BranchInstr (PEAPI.BranchOp op, uint offset)
		{
			this.op = op;
			this.offset = offset;
			use_offset = true;
		}

                public void Emit (CodeGen code_gen, MethodDef meth,
				  PEAPI.CILInstructions cil)
                {
			if (use_offset)
				cil.Branch (op, meth.GetLabelDef (offset));
			else
                        	cil.Branch (op, meth.GetLabelDef (label));
                }
        }

}

