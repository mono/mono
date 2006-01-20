//
// Mono.ILASM.TryBlock
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class TryBlock : IInstr {

                private HandlerBlock block;
                private ArrayList clause_list;

                public TryBlock (HandlerBlock block, Location loc)
			: base (loc)
                {
			this.block = block;
			clause_list = new ArrayList ();
                }

                public void AddSehClause (ISehClause clause)
                {
                        clause_list.Add (clause);
                }

                public override void Emit (CodeGen code_gen, MethodDef meth,
					   PEAPI.CILInstructions cil)
                {
                        PEAPI.CILLabel from = block.GetFromLabel (code_gen, meth);
                        PEAPI.CILLabel to = block.GetToLabel (code_gen, meth);
                        PEAPI.TryBlock try_block = new PEAPI.TryBlock (from, to);

                        foreach (ISehClause clause in clause_list)
                                try_block.AddHandler (clause.Resolve (code_gen, meth));
			
                        cil.AddTryBlock (try_block);
                }

        }

}

