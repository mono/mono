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

                protected class GenericInfo {
                        public string Id;
                        public ArrayList ConstraintList;
                }

                private PEAPI.MethAttr meth_attr;
                private PEAPI.CallConv call_conv;
                private PEAPI.ImplAttr impl_attr;
                private string name;
                private string signature;
                private Hashtable vararg_sig_table;
                private ITypeRef ret_type;
                private ArrayList typar_list;
                private ArrayList param_list;
                private Hashtable named_param_table;
                private ArrayList inst_list;
                private ArrayList customattr_list;
                private Hashtable label_table;
                private Hashtable labelref_table;
                private ArrayList label_list;
                private PEAPI.MethodDef methoddef;
                private bool entry_point;
                private bool zero_init;
                private bool is_resolved;
                private bool is_defined;
                private ArrayList local_list;
                private Hashtable named_local_table;
                private bool init_locals;
                private int max_stack;
                private bool pinvoke_info;
                private ExternModule pinvoke_mod;
                private string pinvoke_name;
                private PEAPI.PInvokeAttr pinvoke_attr;
		private SourceMethod source;

                public MethodDef (CodeGen codegen, PEAPI.MethAttr meth_attr,
				  PEAPI.CallConv call_conv, PEAPI.ImplAttr impl_attr,
				  string name, ITypeRef ret_type, ArrayList param_list,
				  Location start)
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
                        labelref_table = new Hashtable ();
                        label_list = new ArrayList ();
                        local_list = new ArrayList ();
                        named_local_table = new Hashtable ();
                        named_param_table = new Hashtable ();

                        entry_point = false;
                        zero_init = false;
                        init_locals = false;
                        max_stack = -1;
                        pinvoke_info = false;

                        is_defined = false;
                        is_resolved = false;
                        CreateSignature ();
                        CreateNamedParamTable ();

			codegen.BeginMethodDef (this);

			if (codegen.SymbolWriter != null)
				source = codegen.SymbolWriter.BeginMethod (this, start);
                }

                public string Name {
                        get { return name; }
                }

                public string Signature {
                        get { return signature; }
                }

                public ITypeRef RetType {
                        get { return ret_type; }
                }

                public PEAPI.CallConv CallConv {
                        get { return call_conv; }
                }

                public PEAPI.MethodDef PeapiMethodDef {
                        get { return methoddef; }
                }

                public bool IsVararg {
                        get { return (call_conv & PEAPI.CallConv.Vararg) != 0; }
                }

                public bool IsStatic {
                        get { return (meth_attr & PEAPI.MethAttr.Static) != 0; }
                }

                public ITypeRef[] ParamTypeList () {

                        if (param_list == null)
                                return new ITypeRef[0];
                        int count = 0;
                        ITypeRef[] type_list = new ITypeRef[param_list.Count];
                        foreach (ParamDef param in param_list) {
                                type_list[count++] = param.Type;
                        }
                        return type_list;
                }

                public void AddPInvokeInfo (PEAPI.PInvokeAttr pinvoke_attr, ExternModule pinvoke_mod,
                                string pinvoke_name)
                {
                        this.pinvoke_attr = pinvoke_attr;
                        this.pinvoke_mod = pinvoke_mod;
                        this.pinvoke_name = pinvoke_name;
                        pinvoke_info = true;
                }

                public void AddGenericParam (string id)
                {
                        if (typar_list == null)
                                typar_list = new ArrayList ();

                        GenericInfo gi = new GenericInfo ();
                        gi.Id = id;

                        typar_list.Add (gi);
                }

                public void AddGenericConstraint (int index, ITypeRef constraint)
                {
                        GenericInfo gi = (GenericInfo) typar_list[index];

                        if (gi.ConstraintList == null)
                                gi.ConstraintList = new ArrayList ();
                        gi.ConstraintList.Add (constraint);
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

                public void ZeroInit ()
                {
                        zero_init = true;
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

                        PEAPI.Param [] param_array = GenerateParams (code_gen);
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

                        PEAPI.Param [] param_array = GenerateParams (code_gen);
                        FixAttributes ();
                        ret_type.Resolve (code_gen);

                        methoddef = classdef.AddMethod (meth_attr, impl_attr,
                                        name, ret_type.PeapiType, param_array);

                        methoddef.AddCallConv (call_conv);
                        is_resolved = true;

                        return methoddef;
                }

                private PEAPI.Param [] GenerateParams (CodeGen code_gen)
                {
                        PEAPI.Param[] param_array;

                        if (param_list != null && param_list.Count > 0) {
                                 int param_count = param_list.Count;

                                 // Remove the last param if its the sentinel, not sure what
                                // should happen with more then one sentinel
                                ParamDef last = (ParamDef) param_list [param_count-1];
                                if (last.IsSentinel ())
                                        param_count--;

                                param_array = new PEAPI.Param [param_count];
                                for (int i = 0; i < param_count; i++) {
                                        ParamDef paramdef = (ParamDef) param_list [i];
                                        paramdef.Define (code_gen);
                                        param_array [i] = paramdef.PeapiParam;
                                }

                        } else {
                                param_array = new PEAPI.Param [0];
                        }

                        return param_array;
                }

                public PEAPI.MethodRef GetVarargSig (PEAPI.Type[] opt)
                {
                        if (!is_resolved)
                                throw new Exception ("Methods must be resolved before a vararg sig can be created.");

                        PEAPI.MethodRef methref = null;
                        StringBuilder sigbuilder = new StringBuilder ();
                        string sig;
                        foreach (PEAPI.Type t in opt)
                                sigbuilder.Append (opt + ", ");
                        sig = sigbuilder.ToString ();

                        if (vararg_sig_table == null) {
                                vararg_sig_table = new Hashtable ();                                
                        } else {
                                methref = vararg_sig_table [sig] as PEAPI.MethodRef;
                        }

                        if (methref == null) {
                                methref = methoddef.MakeVarArgSignature (opt);
                                vararg_sig_table [sig] = methref;
                        }

                        return methref;
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

                        //code_gen.Report.Message (String.Format ("Assembled method '<Module>'::{0}", name));
                        is_defined = true;
                }

                /// <summary>
                ///  Define a member method
                /// </summary>
                public void Define (CodeGen code_gen, TypeDef typedef)
                {
                        if (is_defined)
                                return;

                        Resolve (code_gen, (PEAPI.ClassDef) typedef.ClassDef);
                        WriteCode (code_gen, methoddef);

                        //code_gen.Report.Message (String.Format ("Assembled method {0}::{1}", typedef.FullName, name));                        is_defined = true;
                }

                public void AddInstr (IInstr instr)
                {
                        inst_list.Add (instr);
                }

                protected void WriteCode (CodeGen code_gen, PEAPI.MethodDef methoddef)
                {
                        if (entry_point)
                                methoddef.DeclareEntryPoint ();

                        if (local_list.Count > 0) {
                                int ec = code_gen.Report.ErrorCount;
                                PEAPI.Local[] local_array = new PEAPI.Local[local_list.Count];
                                int i = 0;

                                foreach (Local local in local_list)
                                        local_array[local.Slot]  = local.GetPeapiLocal (code_gen);

                                if (code_gen.Report.ErrorCount > ec)
                                        return;

                                if (zero_init)
                                        init_locals = true;
                                
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

                        if (pinvoke_info) {
                                methoddef.AddPInvokeInfo (pinvoke_mod.ModuleRef,
                                                (pinvoke_name != null ? pinvoke_name : name), pinvoke_attr);

                        }

                        if (inst_list.Count < 1)
                                return;

                        PEAPI.CILInstructions cil = methoddef.CreateCodeBuffer ();
                        /// Create all the labels
                        /// TODO: Most labels don't actually need to be created so we could
                        /// probably only create the ones that need to be
                        LabelInfo[] label_info = new LabelInfo[label_table.Count + label_list.Count];
                        label_table.Values.CopyTo (label_info, 0);
                        label_list.CopyTo (label_info, label_table.Count);
                        int previous_pos = -1;
                        LabelInfo previous_label = null;
                        Array.Sort (label_info);

                        foreach (LabelInfo label in label_info) {
                                if (label.UseOffset) {
                                        label.Define (new PEAPI.CILLabel (label.Offset));
                                        continue;
                                }
                                if (label.Pos == previous_pos)
                                        label.Label = previous_label.Label;
                                else
                                        label.Define (cil.NewLabel ());

                                previous_label = label;
                                previous_pos = label.Pos;
                        }

                        // Set all the label refs
                        foreach (LabelInfo label in labelref_table.Values) {
                                LabelInfo def = (LabelInfo) label_table[label.Name];
                                if (def == null) {
                                        code_gen.Report.Error ("Undefined Label:  " + label);
                                        return;
                                }
                                label.Label = def.Label;
                        }

                        int label_pos = 0;
                        int next_label_pos = (label_info.Length > 0 ? label_info[0].Pos : -1);

                        for (int i=0; i<inst_list.Count; i++) {
                                IInstr instr = (IInstr) inst_list[i];
                                if (next_label_pos == i) {
                                        cil.CodeLabel (label_info[label_pos].Label);
                                        if (label_pos < label_info.Length) {
                                                while (next_label_pos == i && ++label_pos < label_info.Length) {
                                                        if (label_info[label_pos].UseOffset)
                                                                cil.CodeLabel (label_info[label_pos].Label);
                                                       next_label_pos = label_info[label_pos].Pos;
                                                }
                                        }
                                        if (label_pos >= label_info.Length)
                                                next_label_pos = -1;
                                }
				if (source != null)
					source.MarkLocation (instr.Location.line, cil.Offset);
                                instr.Emit (code_gen, this, cil);
                        }

			if (source != null)
				source.MarkLocation (source.EndLine, cil.Offset);

                        // Generic type parameters
                        if (typar_list != null) {
                                short index = 0;
                                foreach (GenericInfo gi in typar_list) {
                                        PEAPI.GenericParameter gp = methoddef.AddGenericParameter (index++, gi.Id);
                                        if (gi.ConstraintList != null) {
                                                foreach (ITypeRef cnst in gi.ConstraintList) {
                                                        cnst.Resolve (code_gen);
                                                        gp.AddConstraint (cnst.PeapiType);
                                                }
                                        }
                                }
                        }
                }

                public LabelInfo AddLabel (string name)
                {
                        LabelInfo label_info = (LabelInfo) label_table[name];
                        if (label_info != null)
                                return label_info;
                        label_info = new LabelInfo (name, inst_list.Count);
                        label_table.Add (name, label_info);
                        return label_info;
                }

                public LabelInfo AddLabelRef (string name)
                {
                        LabelInfo label_info = (LabelInfo) label_table[name];
                        if (label_info != null)
                                return label_info;
                        label_info = (LabelInfo) labelref_table[name];
                        if (label_info != null)
                                return label_info;
                        label_info = new LabelInfo (name, -1);
                        labelref_table.Add (name, label_info);
                        return label_info;
                }

                public LabelInfo AddLabel (int offset)
                {
                        // We go pos + 1 so this line is not counted
                        LabelInfo label_info = new LabelInfo (null, inst_list.Count+1, (uint) offset);
                        label_list.Add (label_info);
                        return label_info;
                }

                public LabelInfo AddLabel ()
                {
                        int pos = inst_list.Count;
                        LabelInfo label_info = new LabelInfo (null, inst_list.Count);
                        label_list.Add (label_info);
                        return label_info;
                }

                public PEAPI.CILLabel GetLabelDef (string name)
                {
                        LabelInfo label_info = (LabelInfo) label_table[name];

                        return label_info.Label;
                }

                public PEAPI.CILLabel GetLabelDef (int pos)
                {
                        foreach (LabelInfo li in label_list) {
                                if (li.Pos == pos)
                                        return li.Label;
                        }
                        return null;
                }

                private void CreateSignature ()
                {
                        if (IsVararg)
                                signature = CreateVarargSignature (name, param_list);
                        else
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
                                        if (!first)
                                                builder.Append (',');
                                        builder.Append (paramdef.TypeName);
                                        first = false;
                                }
                        }
                        builder.Append (')');

                        return builder.ToString ();
                }

                public static string CreateVarargSignature (string name, IList param_list)
                {
                        StringBuilder builder = new StringBuilder ();
                        ParamDef last = null;

                        builder.Append (name);
                        builder.Append ('(');

                        bool first = true;
                        if (param_list != null) {
                                foreach (ParamDef paramdef in param_list) {
                                        if (!first)
                                                builder.Append (',');
                                        builder.Append (paramdef.TypeName);
                                        first = false;
                                }
                                last = (ParamDef) param_list[param_list.Count - 1];
                        }

                        
                        if (last == null || !last.IsSentinel ()) {
                                if (!first)
                                        builder.Append (',');
                                builder.Append ("...");
                        }

                        builder.Append (')');

                        return builder.ToString ();
                }

                public static string CreateVarargSignature (string name, ITypeRef [] param_list)
                {
                        StringBuilder builder = new StringBuilder ();
                        ITypeRef last = null;

                        builder.Append (name);
                        builder.Append ('(');

                        bool first = true;
                        if (param_list != null && param_list.Length > 0) {
                                foreach (ITypeRef param in param_list) {
                                        if (!first)
                                                builder.Append (',');
                                        builder.Append (param.FullName);
                                        first = false;
                                        last = param;
                                        if (param is SentinelTypeRef)
                                                break;
                                }
                                
                        }
                        
                        if (last == null || !(last is SentinelTypeRef)) {
                                if (!first)
                                        builder.Append (',');
                                builder.Append ("...");
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
                                        if (!first)
                                                builder.Append (',');
                                        builder.Append (param.FullName);
                                        first = false;
                                        if (param is SentinelTypeRef)
                                                break;
                                }
                        }
                        builder.Append (')');

                        return builder.ToString ();
                }

                private void CreateNamedParamTable ()
                {
                        if (param_list == null)
                                return;

                        int count = (IsStatic ? 0 : 1);
                        
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

