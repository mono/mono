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

                protected class LabelInfo : IComparable {

                        public readonly string Name;
                        public readonly int Pos;
                        public PEAPI.CILLabel Label;

                        public LabelInfo (string name, int pos)
                        {
                                Name = name;
                                Pos = pos;
                                Label = null;
                        }

                        public void Define (PEAPI.CILLabel label)
                        {
                                Label = label;
                        }

                        public int CompareTo (object obj)
                        {
                                LabelInfo other = obj as LabelInfo;

                                if(other != null)
                                        return Pos.CompareTo(other.Pos);

                                throw new ArgumentException ("object is not a LabelInfo");
                        }
                }

                private PEAPI.MethAttr meth_attr;
                private PEAPI.CallConv call_conv;
                private PEAPI.ImplAttr impl_attr;
                private string name;
                private string signature;
                private ITypeRef ret_type;
                private ArrayList param_list;
                private Hashtable named_param_table;
                private ArrayList inst_list;
                private ArrayList customattr_list;
                private Hashtable label_table;
                private PEAPI.MethodDef methoddef;
                private bool entry_point;
                private bool is_resolved;
                private bool is_defined;
                private ArrayList local_list;
                private Hashtable named_local_table;
                private bool init_locals;
                private int max_stack;
                private Random label_random;

                public MethodDef (PEAPI.MethAttr meth_attr, PEAPI.CallConv call_conv,
                                PEAPI.ImplAttr impl_attr, string name,
                                ITypeRef ret_type, ArrayList param_list)
                {
                        this.meth_attr = meth_attr;
                        this.call_conv = call_conv;
                        this.impl_attr = impl_attr;
                        this.name = name;
                        this.ret_type = ret_type;
                        this.param_list = param_list;

                        inst_list = new ArrayList ();
                        customattr_list = new ArrayList ();
                        label_table = new Hashtable ();
                        local_list = new ArrayList ();
                        named_local_table = new Hashtable ();
                        named_param_table = new Hashtable ();
                        label_random = new Random ();
                        entry_point = false;
                        init_locals = false;
                        max_stack = -1;

                        is_defined = false;
                        is_resolved = false;
                        CreateSignature ();
                        CreateNamedParamTable ();
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

                public bool IsVararg {
                        get { return (call_conv & PEAPI.CallConv.Vararg) != 0; }
                }

                public void AddLocals (ArrayList local_list)
                {
                        int slot_pos = this.local_list.Count;

                        foreach (Local local in local_list) {
                                if (local.Slot == -1) {
                                        local.Slot = slot_pos;
                                }
                                slot_pos++;
                                if (local.Name == null)
                                        continue;
                                named_local_table.Add (local.Name, local);
                        }

                        this.local_list.AddRange (local_list);
                }

                public Local GetNamedLocal (string name)
                {
                        return (Local) named_local_table[name];
                }

                public int GetNamedLocalSlot (string name)
                {
                        Local local = (Local) named_local_table[name];

                        return local.Slot;
                }

                public int GetNamedParamPos (string name)
                {
                        int pos = (int) named_param_table[name];

                        return pos;
                }

                public void InitLocals ()
                {
                        init_locals = true;
                }

                public void EntryPoint ()
                {
                        entry_point = true;
                }

                public void SetMaxStack (int max_stack)
                {
                        this.max_stack = max_stack;
                }

                public void AddCustomAttr (CustomAttr customattr)
                {
                        customattr_list.Add (customattr);
                }

                public PEAPI.MethodDef Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return methoddef;

                        PEAPI.Param[] param_array;

                        if (param_list != null) {
                                int param_count = param_list.Count;
				if (IsVararg && param_list[param_count-1] == ParamDef.Ellipsis)
					param_count--;
                                param_array = new PEAPI.Param[param_count];
                                int count = 0;

                                foreach (ParamDef paramdef in param_list) {
                                        if (paramdef == ParamDef.Ellipsis)
                                                break;
                                        paramdef.Define (code_gen);
                                        param_array[count++] = paramdef.PeapiParam;
                                }
                        } else {
                                param_array = new PEAPI.Param[0];
                        }

                        FixAttributes ();
                        ret_type.Resolve (code_gen);

                        methoddef = code_gen.PEFile.AddMethod (meth_attr, impl_attr,
                                        name, ret_type.PeapiType, param_array);

                        methoddef.AddCallConv (call_conv);
                        is_resolved = true;

                        return methoddef;
                }

                public PEAPI.MethodDef Resolve (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (is_resolved)
                                return methoddef;

                        PEAPI.Param[] param_array;

                        if (param_list != null) {
                                int param_count = param_list.Count;
				if (IsVararg && param_list[param_count-1] == ParamDef.Ellipsis)		
					param_count--;
                                param_array = new PEAPI.Param[param_count];
                                int count = 0;
				
                                foreach (ParamDef paramdef in param_list) {
                                        if (paramdef == ParamDef.Ellipsis)
                                                break;
                                        paramdef.Define (code_gen);
                                        param_array[count++] = paramdef.PeapiParam;
                                }
                        } else {
                                param_array = new PEAPI.Param[0];
                        }

                        FixAttributes ();
                        ret_type.Resolve (code_gen);

                        methoddef = classdef.AddMethod (meth_attr, impl_attr,
                                        name, ret_type.PeapiType, param_array);

                        methoddef.AddCallConv (call_conv);
                        is_resolved = true;

                        return methoddef;
                }

                public PEAPI.MethodRef GetVarargSig (PEAPI.Type[] opt)
                {
                        if (!is_resolved)
                                throw new Exception ("Methods must be resolved before a vararg sig can be created.");

                        return methoddef.MakeVarArgSignature (opt);
                }

                /// <summary>
                ///  Define a global method
                /// </summary>
                public void Define (CodeGen code_gen)
                {
                        if (is_defined)
                                return;

                        Resolve (code_gen);

                        WriteCode (code_gen, methoddef);

                        is_defined = true;
                }

                /// <summary>
                ///  Define a member method
                /// </summary>
                public void Define (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (is_defined)
                                return;

                        Resolve (code_gen, classdef);

                        WriteCode (code_gen, methoddef);

                        is_defined = true;
                }

                public void AddInstr (IInstr instr)
                {
                        inst_list.Add (instr);
                }

                protected void WriteCode (CodeGen code_gen, PEAPI.MethodDef methoddef)
                {
                        if (entry_point)
                                methoddef.DeclareEntryPoint ();

                        if (local_list != null) {
                                PEAPI.Local[] local_array = new PEAPI.Local[local_list.Count];
                                int i = 0;

                                foreach (Local local in local_list)
                                        local_array[local.Slot]  = local.GetPeapiLocal (code_gen);

                                methoddef.AddLocals (local_array, init_locals);
                        }

                        /// Nothing seems to work if maxstack is not set,
                        /// i need to find out if this NEEDs to be set
                        /// and what its default value should be
                        if (max_stack < 0)
                                max_stack = 8;
                        methoddef.SetMaxStack (max_stack);

                        /// Add the custrom attributes to this method
                        foreach (CustomAttr customattr in customattr_list)
                                customattr.AddTo (code_gen, methoddef);

                        if (inst_list.Count < 1)
                                return;

                        PEAPI.CILInstructions cil = methoddef.CreateCodeBuffer ();
                        /// Create all the labels
                        /// TODO: Most labels don't actually need to be created so we could
                        /// probably only create the ones that need to be
                        LabelInfo[] label_info = new LabelInfo[label_table.Count];
                        label_table.Values.CopyTo (label_info, 0);
                        int previous_pos = -1;
                        LabelInfo previous_label = null;
                        Array.Sort (label_info);

                        foreach (LabelInfo label in label_info) {
                                if (label.Pos == previous_pos)
                                        label.Label = previous_label.Label;
                                else
                                        label.Define (cil.NewLabel ());
                                previous_label = label;
                                previous_pos = label.Pos;
                        }

                        int label_pos = 0;
                        int next_label_pos = (label_info.Length > 0 ? label_info[0].Pos : -1);

                        for (int i=0; i<inst_list.Count; i++) {
                                IInstr instr = (IInstr) inst_list[i];
                                if (next_label_pos == i) {
                                        cil.CodeLabel (label_info[label_pos].Label);
                                        if (label_pos < label_info.Length) {
                                                while (next_label_pos == i && ++label_pos < label_info.Length)
                                                        next_label_pos = label_info[label_pos].Pos;
                                        }
                                        if (label_pos >= label_info.Length)
                                                next_label_pos = -1;
                                }
                                instr.Emit (code_gen, this, cil);
                        }

                }

                public void AddLabel (string name)
                {
                        LabelInfo label_info = new LabelInfo (name, inst_list.Count);

                        label_table.Add (name, label_info);
                }

                /// TODO: This whole process is kinda a hack.
                public string RandomLabel ()
                {
                        int rand = label_random.Next ();
                        string name = rand.ToString ();
                        LabelInfo label_info = new LabelInfo (name, inst_list.Count);

                        label_table.Add (name, label_info);
                        return name;
                }

                public PEAPI.CILLabel GetLabelDef (string name)
                {
                        LabelInfo label_info = (LabelInfo) label_table[name];

                        return label_info.Label;
                }

                private void CreateSignature ()
                {
                        signature = CreateSignature (name, param_list);
                }

                public static string CreateSignature (string name, IList param_list)
                {
                        StringBuilder builder = new StringBuilder ();

                        builder.Append (name);
                        builder.Append ('(');

                        if (param_list != null) {
                                bool first = true;
                                foreach (ParamDef paramdef in param_list) {
                                        if (ParamDef.Ellipsis == paramdef)
                                                break;
                                        if (!first)
                                                builder.Append (',');
                                        builder.Append (paramdef.TypeName);
                                        first = false;
                                }
                        }
                        builder.Append (')');

                        return builder.ToString ();
                }

                public static string CreateSignature (string name, ITypeRef[] param_list)
                {
                        StringBuilder builder = new StringBuilder ();

                        builder.Append (name);
                        builder.Append ('(');

                        if (param_list != null) {
                                bool first = true;
                                foreach (ITypeRef param in param_list) {
                                        if (param == TypeRef.Ellipsis)
                                                break;
                                        if (!first)
                                                builder.Append (',');
                                        builder.Append (param.FullName);
                                        first = false;
                                }
                        }
                        builder.Append (')');

                        return builder.ToString ();
                }

                private void CreateNamedParamTable ()
                {
                        if (param_list == null)
                                return;

                        int count = 0;
                        foreach (ParamDef param in param_list) {
                                if (param.Name != null)
                                        named_param_table.Add (param.Name, count);
                                count++;
                        }
                }

                private void FixAttributes ()
                {
                        if (name == ".ctor" || name == ".cctor")
                                meth_attr |= PEAPI.MethAttr.SpecialName | PEAPI.MethAttr.RTSpecialName;
                        // If methods aren't flagged as static they are instance
                        if ((PEAPI.MethAttr.Static & meth_attr) == 0)
                                call_conv |= PEAPI.CallConv.Instance;
                }

        }

}

