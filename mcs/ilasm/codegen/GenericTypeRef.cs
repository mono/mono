//
// Mono.ILASM.GenericTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class GenericTypeRef : PeapiTypeRef, ITypeRef {

                public GenericTypeRef (PEAPI.GenericTypeSpec gen_type,
                                string full_name) : base (gen_type, full_name)
                {

                }

                public IClassRef AsClassRef (CodeGen code_gen)
                {
                        throw new NotImplementedException ("Can not create classrefs from generic types.");
                }

        }

}

