//
// Mono.ILASM.SentinelTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using System;

namespace Mono.ILASM {

        public class SentinelTypeRef : BaseTypeRef {

                public SentinelTypeRef ()
			: base ("...")
                {
                }

                public override void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        type = new PEAPI.Sentinel ();
                        type = Modify (code_gen, type);

                        is_resolved = true;
                }

                public override IMethodRef GetMethodRef (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, BaseTypeRef[] param, int gen_param_count)
                {
                        return new TypeSpecMethodRef (this, call_conv, ret_type, name, param, gen_param_count);
                }

                public override IFieldRef GetFieldRef (BaseTypeRef ret_type, string name)
                {
                        return new TypeSpecFieldRef (this, ret_type, name);
                }

                public override string ToString ()
                {
                        return "Sentinel  " + full_name;
                }
        }

}

