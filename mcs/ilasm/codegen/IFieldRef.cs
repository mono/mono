//
// Mono.ILASM.IFieldRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public interface IFieldRef {

                PEAPI.Field PeapiField {
                        get;
                }

                void Resolve (CodeGen code_gen);
        }
}


