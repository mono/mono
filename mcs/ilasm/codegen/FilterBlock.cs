//
// Mono.ILASM.FilterBlock
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;


namespace Mono.ILASM {

        public class FilterBlock : ISehClause {

                private HandlerBlock this_block;
		private HandlerBlock handler_block;

                public FilterBlock (HandlerBlock this_block)
                {
                        this.this_block = this_block;
                }

                public void SetHandlerBlock (HandlerBlock hb)
                {
                        handler_block = hb;
                }

                public PEAPI.HandlerBlock Resolve (CodeGen code_gen, MethodDef method)
                {
                        PEAPI.CILLabel label = this_block.GetFromLabel (code_gen, method);
                        PEAPI.CILLabel from = handler_block.GetFromLabel (code_gen, method);
                        PEAPI.CILLabel to = handler_block.GetToLabel (code_gen, method);
                        PEAPI.Filter filter = new PEAPI.Filter (label, from, to);

                        return filter;
                }
        }

}

