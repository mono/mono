//
// Mono.ILASM.CatchBlock
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;


namespace Mono.ILASM {

        public class CatchBlock : ISehClause {

                private BaseTypeRef type_ref;
                private HandlerBlock handler_block;

                public CatchBlock (BaseTypeRef type_ref)
                {
                        this.type_ref = type_ref;
                }

                public void SetHandlerBlock (HandlerBlock hb)
                {
                        handler_block = hb;
                }

                public PEAPI.HandlerBlock Resolve (CodeGen code_gen, MethodDef method)
                {
                        PEAPI.CILLabel from = handler_block.GetFromLabel (code_gen, method);
                        PEAPI.CILLabel to = handler_block.GetToLabel (code_gen, method);
                        PEAPI.Catch katch;

                        type_ref.Resolve (code_gen);

                        katch = new PEAPI.Catch (type_ref.PeapiType, from, to);

                        return katch;
                }
        }

}
