//
// Mono.ILASM.ISehClause
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


namespace Mono.ILASM {

        public interface ISehClause {

                PEAPI.HandlerBlock Resolve (CodeGen code_gen, MethodDef method);

                void SetHandlerBlock (HandlerBlock hb);
        }

}

