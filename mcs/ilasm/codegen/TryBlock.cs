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

                private string from_label;
                private string to_label;
                private ArrayList clause_list;

                public TryBlock (string from_label, string to_label)
                {
                        this.from_label = from_label;
                        this.to_label = to_label;

                        clause_list = new ArrayList ();
                }

                public TryBlock (HandlerBlock block) :
                        this (block.from_label, block.to_label)
                {

                }

                public void AddSehClause (ISehClause clause)
                {
                        clause_list.Add (clause);
                }

                public void Emit (CodeGen code_gen, MethodDef meth,
				  PEAPI.CILInstructions cil)
                {
                        PEAPI.CILLabel from = meth.GetLabelDef (from_label);
                        PEAPI.CILLabel to = meth.GetLabelDef (to_label);
                        PEAPI.TryBlock try_block = new PEAPI.TryBlock (from, to);

                        foreach (ISehClause clause in clause_list)
                                try_block.AddHandler (clause.Resolve (code_gen, meth));

                        cil.AddTryBlock (try_block);
                }

        }

}

