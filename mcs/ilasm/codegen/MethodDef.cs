//
// Mono.ILASM.MethodDef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Text;
using System.Collections;


namespace Mono.ILASM {

        public class MethodDef {

                private PEAPI.MethAttr meth_attr;
                private PEAPI.ImplAttr impl_attr;
                private string name;
                private string signature;
                private ITypeRef ret_type;
                private ArrayList param_list;
                private ArrayList inst_list;
                private PEAPI.MethodDef methoddef;
                private bool is_defined;

                public MethodDef (PEAPI.MethAttr meth_attr, PEAPI.ImplAttr impl_attr,
                                string name, ITypeRef ret_type, ArrayList param_list)
                {
                        this.meth_attr = meth_attr;
                        this.impl_attr = impl_attr;
                        this.name = name;
                        this.ret_type = ret_type;
                        this.param_list = param_list;

                        inst_list = new ArrayList ();
                        is_defined = false;
                        CreateSignature ();
                }

                public string Name {
                        get { return name; }
                }

                public string Signature {
                        get { return signature; }
                }

                public PEAPI.MethodDef PeapiMethodDef {
                        get { return methoddef; }
                }

                /// <summary>
                ///  Define a global method
                /// </summary>
                public void Define (CodeGen code_gen)
                {
                        if (is_defined)
                                return;

                        PEAPI.Param[] param_array = new PEAPI.Param[param_list.Count];
                        int count = 0;
                        ret_type.Resolve (code_gen);

                        foreach (ParamDef paramdef in param_list) {
                                paramdef.Define (code_gen);
                                param_array[count++] = paramdef.PeapiParam;
                        }

                        methoddef = code_gen.PEFile.AddMethod (meth_attr, impl_attr,
                                        name, ret_type.PeapiType, param_array);
                        is_defined = true;
                }

                /// <summary>
                ///  Define a member method
                /// </summary>
                public void Define (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (is_defined)
                                return;

                        PEAPI.Param[] param_array = new PEAPI.Param[param_list.Count];
                        int count = 0;
                        ret_type.Resolve (code_gen);

                        foreach (ParamDef paramdef in param_list) {
                                paramdef.Define (code_gen);
                                param_array[count++] = paramdef.PeapiParam;
                        }

                        methoddef = classdef.AddMethod (meth_attr, impl_attr,
                                        name, ret_type.PeapiType, param_array);

                        if (inst_list.Count > 0) {
                                PEAPI.CILInstructions cil = methoddef.CreateCodeBuffer ();
                                foreach (IInstr instr in inst_list)
                                        instr.Emit (code_gen, cil);
                        }

                        is_defined = true;
                }

                public void AddInstr (IInstr instr)
                {
                        inst_list.Add (instr);
                }

                private void CreateSignature ()
                {
                        StringBuilder builder = new StringBuilder ();

                        builder.Append (ret_type.FullName);
                        builder.Append ('_');
                        builder.Append (name);
                        builder.Append ('(');

                        bool first = true;
                        foreach (ParamDef paramdef in param_list) {
                                if (!first)
                                        builder.Append (',');
                                builder.Append (paramdef.TypeName);
                        }
                        builder.Append (')');


                        signature = builder.ToString ();
                }
        }

}

