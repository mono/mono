//
// Mono.ILASM.ExternMethodRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class ExternMethodRef : IMethodRef {

                private ExternTypeRef owner;
                private ITypeRef ret_type;
                private string name;
                private ITypeRef[] param;
                private PEAPI.CallConv call_conv;

                private PEAPI.Method peapi_method;
		private bool is_resolved;

                public ExternMethodRef (ExternTypeRef owner, ITypeRef ret_type,
                        PEAPI.CallConv call_conv, string name, ITypeRef[] param)
                {
                        this.owner = owner;
                        this.ret_type = ret_type;
                        this.name = name;
                        this.param = param;
                        this.call_conv = call_conv;
			
			is_resolved = false;
                }

                public PEAPI.Method PeapiMethod {
                        get { return peapi_method; }
                }

		public PEAPI.CallConv CallConv {
			get { return call_conv; }
			set { call_conv = value; }
		}

                public void Resolve (CodeGen code_gen)
                {
			if (is_resolved)
				return;

                        if ((call_conv & PEAPI.CallConv.Vararg) != 0) {
                                ResolveVararg (code_gen);
                                return;
                        }

                        PEAPI.Type[] param_list = new PEAPI.Type[param.Length];
                        string write_name;

                        ret_type.Resolve (code_gen);

                        int count = 0;
                        foreach (ITypeRef typeref in param) {
                                typeref.Resolve (code_gen);
                                param_list[count++] = typeref.PeapiType;
                        }

                        if (name == "<init>")
                                write_name = ".ctor";
                        else
                                write_name = name;

                        owner.Resolve (code_gen);

                        if (owner.UseTypeSpec) {
                                PEAPI.Type owner_ref = owner.PeapiType;
                                peapi_method = code_gen.PEFile.AddMethodToTypeSpec (owner_ref, write_name,
                                                ret_type.PeapiType, param_list);
                        } else {
                                PEAPI.ClassRef owner_ref;
                                owner_ref = (PEAPI.ClassRef) owner.PeapiType;
                                peapi_method = owner_ref.AddMethod (write_name,
                                                ret_type.PeapiType, param_list);
                        }

			Console.WriteLine ("Adding call conv:   " + call_conv);
                        peapi_method.AddCallConv (call_conv);

			is_resolved = true;
                }

                protected void ResolveVararg (CodeGen code_gen)
                {
			if (is_resolved)
				return;

                        ArrayList param_list = new ArrayList ();
                        ArrayList opt_list = new ArrayList ();
                        bool in_opt = false;
                        string write_name;

                        ret_type.Resolve (code_gen);

                        int count = 0;
                        foreach (ITypeRef typeref in param) {
                                if (in_opt) {
                                        typeref.Resolve (code_gen);
                                        opt_list.Add (typeref.PeapiType);
                                } else if (TypeRef.Ellipsis == typeref) {
                                        in_opt = true;
                                } else {
                                        typeref.Resolve (code_gen);
                                        param_list.Add (typeref.PeapiType);
                                }
                        }

                        if (name == "<init>")
                                write_name = ".ctor";
                        else
                                write_name = name;

                        if (owner.IsArray)
                                throw new NotImplementedException ("Vararg methods on arrays are not supported yet.");

                        owner.Resolve (code_gen);

                        if (owner.UseTypeSpec) {
                                PEAPI.Type owner_ref = owner.PeapiType;
                                peapi_method = code_gen.PEFile.AddVarArgMethodToTypeSpec (owner_ref,
                                                write_name, ret_type.PeapiType,
                                                (PEAPI.Type[]) param_list.ToArray (typeof (PEAPI.Type)),
                                                (PEAPI.Type[]) opt_list.ToArray (typeof (PEAPI.Type)));
                        } else {
                                PEAPI.ClassRef owner_ref;
                                owner_ref = (PEAPI.ClassRef) owner.PeapiType;
                                peapi_method = owner_ref.AddVarArgMethod (write_name,
                                                ret_type.PeapiType,
                                                (PEAPI.Type[]) param_list.ToArray (typeof (PEAPI.Type)),
                                                (PEAPI.Type[]) opt_list.ToArray (typeof (PEAPI.Type)));
                        }


                        peapi_method.AddCallConv (call_conv);
			
			is_resolved = true;
                }
        }

}

