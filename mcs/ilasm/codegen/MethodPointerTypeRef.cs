//
// Mono.ILASM.MethodPointerTypeRef
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// Copyright 2004 Novell, Inc (http://www.novell.com)
//


using System;
using System.Text;
using System.Collections;

namespace Mono.ILASM {

        public class MethodPointerTypeRef : BaseTypeRef {

                private PEAPI.CallConv callconv;
                private BaseTypeRef ret;
                private ArrayList param_list;

                public MethodPointerTypeRef (PEAPI.CallConv callconv, BaseTypeRef ret, ArrayList param_list)
                        : this (callconv, ret, param_list, null, String.Empty)
                {
                }

                public MethodPointerTypeRef (PEAPI.CallConv callconv, BaseTypeRef ret, ArrayList param_list, ArrayList conv_list, string sig_mod)
                        : base (String.Empty, conv_list, sig_mod)
                {
                        this.callconv = callconv;
                        this.ret = ret;
                        this.param_list = param_list;

                        // We just need these to not break the interface
                        full_name = BuildTypeName ();
                        //sig_mod = String.Empty;
                }

                private String BuildTypeName ()
                {
                        StringBuilder builder = new StringBuilder ();
                        builder.Append ("method ");
                        if (callconv != PEAPI.CallConv.Default) {
                                builder.Append (callconv.ToString ());
                                builder.Append (' ');
                        }
                        builder.Append (ret.FullName);
                        builder.Append ("*(");
                        if (param_list != null) {
                                bool first = true;
                                foreach (ParamDef paramdef in param_list) {
                                        if (!first)
                                                builder.Append (',');
                                        builder.Append (paramdef.TypeName);
                                        first = false;
                                }
                        }
                        builder.Append (')');
                        return builder.ToString ();
                }

                public override BaseTypeRef Clone ()
                {
                        return new MethodPointerTypeRef (callconv, ret,
                                        param_list == null ? null : (ArrayList) param_list.Clone (),
                                        (ArrayList) ConversionList.Clone (), sig_mod);
                }

                public override void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        PEAPI.Type [] arg_array;
                        PEAPI.Type [] opt_array;
                        bool is_vararg = false;

                        if (param_list != null) {
                                ArrayList opt_list = new ArrayList ();
                                ArrayList arg_list = new ArrayList ();
                                bool in_opt = false;
                                int max = param_list.Count;

                                for (int i = 0; i < max; i++) {
                                        ParamDef param = (ParamDef) param_list [i];

                                        if (param.IsSentinel ()) {
                                                is_vararg = true;
                                                in_opt = true;
                                                param.Type.Resolve (code_gen);
                                        } else if (in_opt) {
                                                param.Type.Resolve (code_gen);
                                                opt_list.Add (param.Type.PeapiType);
                                        } else {
                                                param.Type.Resolve (code_gen);
                                                arg_list.Add (param.Type.PeapiType);
                                        }
                                }

                                arg_array = (PEAPI.Type []) arg_list.ToArray (typeof (PEAPI.Type));
                                opt_array = (PEAPI.Type []) opt_list.ToArray (typeof (PEAPI.Type));
                        } else {
                                arg_array = new PEAPI.Type [0];
                                opt_array = new PEAPI.Type [0];
                        }

                        ret.Resolve (code_gen);

                        type = new PEAPI.MethPtrType (callconv, ret.PeapiType, arg_array, is_vararg, opt_array);
                        type = Modify (code_gen, type);

                        is_resolved = true;
                }

                protected override BaseMethodRef CreateMethodRef (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, BaseTypeRef[] param, int gen_param_count)
                {
                        return new TypeSpecMethodRef (this, call_conv, ret_type, name, param, gen_param_count);
                }

                protected override IFieldRef CreateFieldRef (BaseTypeRef ret_type, string name)
                {
                        return new TypeSpecFieldRef (this, ret_type, name);
                }

        }

}

