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

                private ITypeRef owner;
                private ITypeRef type;
                private string name;

                private PEAPI.FieldRef peapi_field;

                public TypeSpecFieldRef (ITypeRef owner, ITypeRef type, string name)
                {
                        this.owner = owner;
                        this.type = type;
                        this.name = name;
                }

                public PEAPI.Field PeapiField {
                        get { return peapi_field; }
                }

                public void Resolve (CodeGen code_gen)
                {
                        owner.Resolve (code_gen);

                        type.Resolve (code_gen);
                        peapi_field = code_gen.PEFile.AddFieldToTypeSpec (owner.PeapiType, name, type.PeapiType);
                }

        }

}

