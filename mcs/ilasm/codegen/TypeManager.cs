//
// Mono.ILASM.TypeManager.cs
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using System;
using System.Collections;

namespace Mono.ILASM {

        public class TypeManager {
                private ArrayList type_list;
                private Hashtable type_table;
                private CodeGen code_gen;

                public TypeManager (CodeGen code_gen)
                {
                        this.code_gen = code_gen;
                        type_table = new Hashtable ();
                        type_list = new ArrayList ();
                }

                public TypeDef this[string full_name] {
                        get {
                                return (TypeDef) type_table[full_name];
                        }
                        set {
                                type_table[full_name] = value;
                                type_list.Add(value);
                        }
                }

                public PEAPI.Type GetPeapiType (string full_name)
                {
                        TypeDef type_def = (TypeDef) type_table[full_name];

                        if (type_def == null)
                                return null;

                        if (!type_def.IsDefined)
                                type_def.Define (code_gen);

                        return type_def.PeapiType;
                }

                public void DefineAll ()
                {
                        foreach (TypeDef typedef in type_list) {
                                typedef.Define (code_gen);
                        }
                }

        }

}

