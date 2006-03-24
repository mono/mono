//
// Mono.ILASM.ExternFieldRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        public class ExternFieldRef : IFieldRef {

                private ExternTypeRef owner;
                private BaseTypeRef type;
                private string name;

		private bool is_resolved;
                private PEAPI.FieldRef peapi_field;

                public ExternFieldRef (ExternTypeRef owner, BaseTypeRef type, string name)
                {
                        this.owner = owner;
                        this.type = type;
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

                        owner.Resolve (code_gen);

                        if (owner.UseTypeSpec) {
                                PEAPI.Type owner_ref = owner.PeapiType;
                                code_gen.PEFile.AddFieldToTypeSpec (owner_ref, name,
                                                type.PeapiType);
                        } else {
                                PEAPI.ClassRef owner_ref;
                                owner_ref = (PEAPI.ClassRef) owner.PeapiType;
                                type.Resolve (code_gen);
                                peapi_field = owner_ref.AddField (name, type.PeapiType);
                        }

			is_resolved = true;
                }
        }

}

