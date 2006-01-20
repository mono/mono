//
// Mono.ILASM.IMethodRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;


namespace Mono.ILASM {

        public interface IMethodRef {

                PEAPI.Method PeapiMethod {
                        get;
                }

		PEAPI.CallConv CallConv {
			get;
			set;
		}

                void Resolve (CodeGen code_gen);

		BaseTypeRef Owner {
			get;
		}
        }

}



