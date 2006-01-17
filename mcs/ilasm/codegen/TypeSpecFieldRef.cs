//
// Mono.ILASM.TypeSpecFieldRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 JacksonHarper, All rights reserved
//


using System;


namespace Mono.ILASM {

        public class TypeSpecFieldRef : IFieldRef {

                private BaseTypeRef owner;
                private BaseTypeRef type;
                private string name;

                private PEAPI.FieldRef peapi_field;
		private bool is_resolved;

                public TypeSpecFieldRef (BaseTypeRef owner, BaseTypeRef type, string name)
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

                        type.Resolve (code_gen);
                        peapi_field = code_gen.PEFile.AddFieldToTypeSpec (owner.PeapiType, name, type.PeapiType);
		
			is_resolved = true;
                }

        }

}

