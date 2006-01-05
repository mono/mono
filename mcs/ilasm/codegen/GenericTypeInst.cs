//
// Mono.ILASM.GenericTypeInst
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//  Ankit Jain (JAnkit@novell.com)
//
// (C) 2003 Latitude Geographics Group, All rights reserved
// (C) 2005 Novell, Inc (http://www.novell.com)
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class GenericTypeInst : ModifiableType, IClassRef {

                private IClassRef class_ref;
                private PEAPI.Type ptype;
                private bool is_valuetypeinst;
                private bool is_resolved;
                private GenericArguments gen_args;
                private string sig_mod;
                private bool is_added; /* Added to PEFile (to TypeSpec table) ? */
                private static Hashtable method_table = new Hashtable ();

                public GenericTypeInst (IClassRef class_ref, GenericArguments gen_args, bool is_valuetypeinst)
                        : this (class_ref, gen_args, is_valuetypeinst, null, null)
                {
                }

                public GenericTypeInst (IClassRef class_ref, GenericArguments gen_args, bool is_valuetypeinst,
                                string sig_mod, ArrayList conv_list)
                {
                        if (class_ref is GenericTypeInst)
                                throw new ArgumentException (String.Format ("Cannot create nested GenericInst, '{0}' '{1}'", class_ref.FullName, gen_args.ToString ()));

                        this.class_ref = class_ref;
                        this.gen_args = gen_args;
                        this.is_valuetypeinst = is_valuetypeinst;
                        this.sig_mod = sig_mod;
                        is_added = false;
                        if (conv_list != null)
                                ConversionList = conv_list;

                        is_resolved = false;
                }

                public PEAPI.Type PeapiType {
                        get { return ptype; }
                }

                public string FullName {
                        get { return class_ref.FullName + gen_args.ToString () + SigMod; }
                }

                public PEAPI.Class PeapiClass {
                        get { return (PEAPI.Class) ptype; }
                }

                public override string SigMod {
                        get { return sig_mod; }
                        set { sig_mod = value; }
                }

                public IClassRef Clone ()
                {
                        //Clone'd instance shares the class_ref and gen_args,
                        //as its basically used to create modified types (arrays etc)
                        return new GenericTypeInst (class_ref, gen_args, is_valuetypeinst, sig_mod, 
                                        (ArrayList) ConversionList.Clone () );
                }

                public void MakeValueClass ()
                {
                        class_ref.MakeValueClass ();
                }

                public GenericTypeInst GetGenericTypeInst (GenericArguments gen_args)
                {
                        throw new Exception (String.Format ("Invalid attempt to create '{0}''{1}'", FullName, gen_args.ToString ()));
                }

                public PEAPI.Type ResolveInstance (CodeGen code_gen, GenericArguments gen_args)
                {
                        throw new Exception (String.Format ("Invalid attempt to create '{0}''{1}'", FullName, gen_args.ToString ()));
                }
                
                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        class_ref.Resolve (code_gen);
                        ptype = class_ref.ResolveInstance (code_gen, gen_args);

                        ptype = Modify (code_gen, ptype);

                        is_resolved = true;
                }

                /* Resolves, AND adds to the TypeSpec table,
		   called from TypeDef.Define for base class and
		   interface implementations.
		   
		   Not required to be called for method/field refs, as
		   PEFile's AddMethodToTypeSpec & AddFieldToTypeSpec is
		   used which adds it to the TypeSpec table.
		 */
                public void ResolveAsClass (CodeGen code_gen)
                {
                        Resolve (code_gen);
                        if (is_added)
                                return;

                        code_gen.PEFile.AddGenericClass ((PEAPI.GenericTypeInst) ptype);
                        is_added = true;
                }

                public IMethodRef GetMethodRef (ITypeRef ret_type, PEAPI.CallConv call_conv,
                                string meth_name, ITypeRef[] param, int gen_param_count)
                {
                        string key = FullName + MethodDef.CreateSignature (ret_type, meth_name, param, gen_param_count);
                        TypeSpecMethodRef mr = method_table [key] as TypeSpecMethodRef;
                        if (mr == null) {         
                                mr = new TypeSpecMethodRef (this, ret_type, call_conv, meth_name, param, gen_param_count);
                                method_table [key] = mr;
                        }

                        return mr;
                }

                public IFieldRef GetFieldRef (ITypeRef ret_type, string field_name)
                {
                        return new TypeSpecFieldRef (this, ret_type, field_name);
                }
        }
}

