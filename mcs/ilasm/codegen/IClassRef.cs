//
// Mono.ILASM.IClassRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using System;

namespace Mono.ILASM {

        public interface IClassRef : ITypeRef {

                PEAPI.Class PeapiClass { get; }

                IMethodRef GetMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, ITypeRef[] param);

                IFieldRef GetFieldRef (ITypeRef ret_type, string name);
	
		void MakeValueClass ();
        }

}

