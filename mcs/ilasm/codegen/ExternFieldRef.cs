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
                private ITypeRef type;
                private string name;

                private PEAPI.FieldRef peapi_field;

                public ExternFieldRef (ExternTypeRef owner, ITypeRef type, string name)
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
                        PEAPI.ClassRef owner_ref;

                        owner.Resolve (code_gen);
                        owner_ref = owner.PeapiClassRef;

                        type.Resolve (code_gen);
                        peapi_field = owner_ref.AddField (name, type.PeapiType);
                }
        }

}

