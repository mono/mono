//
// Mono.ILASM.ExternTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        /// <summary>
        /// A reference to a type in another assembly
        /// </summary>
        public class ExternTypeRef : ModifiableType, IClassRef {

                private PEAPI.Type type;
                private string assembly_name;
                private string full_name;
                private string sig_mod;
                private bool is_valuetype;

                private bool is_resolved;

                private Hashtable method_table;
                private Hashtable field_table;
                
                public ExternTypeRef (string assembly_name, string full_name, bool is_valuetype)
                {
                        this.assembly_name = assembly_name;
                        this.full_name = full_name;
                        this.is_valuetype = is_valuetype;
                        sig_mod = String.Empty;

                        method_table = new Hashtable ();
                        field_table = new Hashtable ();
                        
                        is_resolved = false;
                }

                public PEAPI.Type PeapiType {
                        get { return type; }
                }

                public PEAPI.Class PeapiClass {
                        get { return type as PEAPI.Class; }
                }

                public string FullName {
                        get { return full_name + sig_mod; }
                }


                public override string SigMod {
                        get { return sig_mod; }
                        set { sig_mod = value; }
                }

                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        if (is_valuetype)
                                type = code_gen.ExternTable.GetValueClass (assembly_name, full_name);
                        else
                                type = code_gen.ExternTable.GetClass (assembly_name, full_name);

                        type = Modify (code_gen, type);

                        is_resolved = true;
                }

                public void MakeValueClass ()
                {
                        is_valuetype = true;
                }

                public IMethodRef GetMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, ITypeRef[] param)
                {
                        string sig = MethodDef.CreateSignature (name, param);
                        ExternMethodRef mr = method_table [sig] as ExternMethodRef;
                        
                        if (mr == null) {
                                mr = new ExternMethodRef (this, ret_type, call_conv, name, param);
                                method_table [sig] = mr;
                        }

                        return mr;
                }

                public IFieldRef GetFieldRef (ITypeRef ret_type, string name)
                {
                        ExternFieldRef fr = field_table [name] as ExternFieldRef;

                        if (fr == null) {
                                fr = new ExternFieldRef (this, ret_type, name);
                                field_table [name] = fr;
                        }

                        return fr;
                }
        }

}

