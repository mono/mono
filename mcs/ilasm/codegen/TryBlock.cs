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

                private MethodDef method;
                private string from_label;
                private string to_label;
                private ArrayList clause_list;

                public TryBlock (string from_label, string to_label)
                {
                        this.method = method;
                        this.from_label = from_label;
                        this.to_label = to_label;

                        clause_list = new ArrayList ();
                }

                public void SetMethod (MethodDef method)
                {
                        this.method = method;
                }

                public void AddSehClause (ISehClause clause)
                {
                        clause_list.Add (clause);
                }

                public void Emit (CodeGen code_gen, PEAPI.CILInstructions cil)
                {
                        PEAPI.CILLabel from = method.GetLabelDef (from_label);
                        PEAPI.CILLabel to = method.GetLabelDef (to_label);
                        PEAPI.TryBlock try_block = new PEAPI.TryBlock (from, to);

                        foreach (ISehClause clause in clause_list)
                                try_block.AddHandler (clause.Resolve (code_gen, method));

                        cil.AddTryBlock (try_block);
                }

        }

}

