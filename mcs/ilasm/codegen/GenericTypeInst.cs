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

        public class GenericTypeInst : BaseGenericTypeRef {

                private BaseClassRef class_ref;
                private PEAPI.GenericTypeInst p_gen_inst;
                const bool is_valuetypeinst = false;
                private GenericArguments gen_args;
                private bool is_added; /* Added to PEFile (to TypeSpec table) ? */
                /* Note: Using static hashtable here as GenericTypeInsts is not cached */
                private static Hashtable s_method_table = new Hashtable ();
                private static Hashtable s_field_table = new Hashtable ();

                public GenericTypeInst (BaseClassRef class_ref, GenericArguments gen_args, bool is_valuetypeinst)
                        : this (class_ref, gen_args, is_valuetypeinst, null, null)
                {
                }

                public GenericTypeInst (BaseClassRef class_ref, GenericArguments gen_args, bool is_valuetypeinst,
                                string sig_mod, ArrayList conv_list)
                        : base ("", is_valuetypeinst, conv_list, sig_mod)
                {
                        if (class_ref is GenericTypeInst)
                                throw new InternalErrorException ("Cannot create nested GenericInst, '" +
                                                        class_ref.FullName + "' '" + gen_args.ToString () + "'");

                        this.class_ref = class_ref;
                        this.gen_args = gen_args;
                        is_added = false;
                }

                public override string FullName {
                        get { return class_ref.FullName + gen_args.ToString () + SigMod; }
                }

                public override BaseTypeRef Clone ()
                {
                        //Clone'd instance shares the class_ref and gen_args,
                        //as its basically used to create modified types (arrays etc)
                        return new GenericTypeInst (class_ref, gen_args, is_valuetypeinst, sig_mod, 
                                        (ArrayList) ConversionList.Clone () );
                }

                public override void MakeValueClass ()
                {
                        class_ref.MakeValueClass ();
                }

                public override void ResolveNoTypeSpec (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        class_ref.Resolve (code_gen);
                        p_gen_inst = (PEAPI.GenericTypeInst) class_ref.ResolveInstance (code_gen, gen_args);

                        type = Modify (code_gen, p_gen_inst);

                        is_resolved = true;
                }

                public override void Resolve (CodeGen code_gen)
                {
                        ResolveNoTypeSpec (code_gen);
                        if (is_added)
                                return;

                        code_gen.PEFile.AddGenericClass ((PEAPI.GenericTypeInst) p_gen_inst);
                        is_added = true;
                }

                public override void Resolve (GenericParameters type_gen_params, GenericParameters method_gen_params)
                {
                        gen_args.Resolve (type_gen_params, method_gen_params);
                }

                protected override BaseMethodRef CreateMethodRef (BaseTypeRef ret_type,
                        PEAPI.CallConv call_conv, string name, BaseTypeRef[] param, int gen_param_count)
                {
                        throw new InternalErrorException ("Should not be called");
                }

                public override BaseMethodRef GetMethodRef (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                                string meth_name, BaseTypeRef[] param, int gen_param_count)
                {
			/* Note: Using FullName here as we are caching in a static hashtable */
                        string key = FullName + MethodDef.CreateSignature (ret_type, call_conv, meth_name, param, gen_param_count, true);
                        TypeSpecMethodRef mr = s_method_table [key] as TypeSpecMethodRef;
                        if (mr == null) {         
                                mr = new TypeSpecMethodRef (this, call_conv, ret_type, meth_name, param, gen_param_count);
                                s_method_table [key] = mr;
                        }

                        return mr;
                }

                protected override IFieldRef CreateFieldRef (BaseTypeRef ret_type, string field_name)
                {
			/* Note: Using FullName here as we are caching in a static hashtable */
                        string key = FullName + ret_type.FullName + field_name;

                        IFieldRef fr = (IFieldRef) s_field_table [key];

                        if (fr == null) {
                                fr = new TypeSpecFieldRef (this, ret_type, field_name);
                                s_field_table [key] = fr;
                        }

                        return fr;
                }
        }
}

