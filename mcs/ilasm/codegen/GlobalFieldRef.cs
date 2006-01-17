//
// Mono.ILASM.GlobalFieldRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {


        public class GlobalFieldRef : IFieldRef {

                private BaseTypeRef ret_type;
                private string name;

                private PEAPI.Field peapi_field;
		private bool is_resolved;

                public GlobalFieldRef (BaseTypeRef ret_type, string name)
                {
                        this.ret_type = ret_type;
                        this.name = name;

			is_resolved = false;
                }

                public PEAPI.Field PeapiField {
                        get { return peapi_field; }
                }

                public void Resolve (CodeGen code_gen)
                {
			if (is_resolved)
				return;

                        peapi_field = code_gen.ResolveField (name, ret_type.FullName);

			is_resolved = true;
                }
        }
}

