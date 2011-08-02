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
using System.Security;

using Mono.CompilerServices.SymbolWriter;

namespace Mono.ILASM {

        public class MethodDef : ICustomAttrTarget, IDeclSecurityTarget {

                private PEAPI.MethAttr meth_attr;
                private PEAPI.CallConv call_conv;
                private PEAPI.ImplAttr impl_attr;
                private string name;
                private string signature;
                private Hashtable vararg_sig_table;
                private ParamDef ret_param;
                private ArrayList param_list;
                private ArrayList inst_list;
                private ArrayList customattr_list;
                private DeclSecurity decl_sec;
                private Hashtable label_table;
                private Hashtable labelref_table;
                private ArrayList label_list;
                private PEAPI.MethodDef methoddef;
                private bool entry_point;
                private bool zero_init;
                private bool is_resolved;
                private bool is_defined;
                private ArrayList local_list;
                private ArrayList named_local_tables;
                private int current_scope_depth;
                private bool init_locals;
                private int max_stack;
                private bool pinvoke_info;
                private ExternModule pinvoke_mod;
                private string pinvoke_name;
                private PEAPI.PInvokeAttr pinvoke_attr;
		private SourceMethod source;
                private TypeDef type_def;
                private GenericParameters gen_params;
                private Location start;
                private CodeGen codegen;

                public MethodDef (CodeGen codegen, PEAPI.MethAttr meth_attr,
				  PEAPI.CallConv call_conv, PEAPI.ImplAttr impl_attr,
				  string name, BaseTypeRef ret_type, ArrayList param_list,
				  Location start, GenericParameters gen_params, TypeDef type_def)
                {
                        this.codegen = codegen;
                        this.meth_attr = meth_attr;
                        this.call_conv = call_conv;
                        this.impl_attr = impl_attr;
                        this.name = name;
                        this.param_list = param_list;
                        this.type_def = type_def;
                        this.gen_params = gen_params;
                        this.ret_param = new ParamDef (PEAPI.ParamAttr.Default, "", ret_type);
                        this.start = (Location) start.Clone ();

                        inst_list = new ArrayList ();
                        label_table = new Hashtable ();
                        labelref_table = new Hashtable ();
                        label_list = new ArrayList ();
                        local_list = new ArrayList ();
                        named_local_tables = new ArrayList ();
                        named_local_tables.Add (new Hashtable ());
                        current_scope_depth = 0;

                        entry_point = false;
                        zero_init = false;
                        init_locals = false;
                        max_stack = -1;
                        pinvoke_info = false;

                        is_defined = false;
                        is_resolved = false;
                        ResolveGenParams ();
                        CreateSignature ();

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

                public BaseTypeRef RetType {
                        get { return ret_param.Type; }
                }

                public PEAPI.CallConv CallConv {
                        get { return call_conv; }
                }

                public PEAPI.MethodDef PeapiMethodDef {
                        get { return methoddef; }
                }

                public PEAPI.MethAttr Attributes {
                        get { return meth_attr; }
                        set { meth_attr = value; }
                }

                public bool IsVararg {
                        get { return (call_conv & PEAPI.CallConv.Vararg) != 0; }
                }

                public bool IsStatic {
                        get { return (meth_attr & PEAPI.MethAttr.Static) != 0; }
                }

                public bool IsVirtual {
                        get { return (meth_attr & PEAPI.MethAttr.Virtual) != 0; }
                }

                public bool IsAbstract {
                        get { return (meth_attr & PEAPI.MethAttr.Abstract) != 0; }
                }

                public Location StartLocation {
                        get { return start; }
                }

                public DeclSecurity DeclSecurity {
                        get {
                                if (decl_sec == null)
                                        decl_sec = new DeclSecurity ();
                                return decl_sec;
                        }
                }

                public string FullName { 
                        get {
                                if (type_def == null)
                                        return Name;
                                return type_def.FullName + "." + Name;
                        }
                }

                public BaseTypeRef[] ParamTypeList () {

                        if (param_list == null)
                                return new BaseTypeRef[0];
                        int count = 0;
                        BaseTypeRef[] type_list = new BaseTypeRef[param_list.Count];
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

                public int GenParamCount {
                        get { return (gen_params != null ? gen_params.Count : 0); }
                }

                public GenericParameter GetGenericParam (string id)
                {
                        if (gen_params == null)
                                return null;
                        
                        return gen_params.GetGenericParam (id);
                }

                public GenericParameter GetGenericParam (int index)
                {
                        if (gen_params == null || index < 0 || index >= gen_params.Count)
                                return null;
                        
                        return gen_params [index];
                }

                public int GetGenericParamNum (string id)
                {
                        if (gen_params == null)
                                return -1;
                        
                        return gen_params.GetGenericParamNum (id);
                }

                public void AddCustomAttribute (CustomAttr customattr)
                {
                        if (customattr_list == null)
                                customattr_list = new ArrayList ();

                        customattr_list.Add (customattr);
                }

                public void AddRetTypeMarshalInfo (PEAPI.NativeType native_type)
                {
                        this.ret_param.AddMarshalInfo (native_type);
                }

                //try/catch scope, used to scope local vars
                public void BeginLocalsScope ()
                {
                        current_scope_depth ++;
                        named_local_tables.Add (new Hashtable ());
                }

                public void EndLocalsScope ()
                {
                        named_local_tables.RemoveAt (current_scope_depth);
                        current_scope_depth --;
                }

                public void AddLocals (ArrayList local_list)
                {
                        int slot_pos = this.local_list.Count;

                        Hashtable current_named_table = null;
                        current_named_table = (Hashtable) named_local_tables [current_scope_depth];

                        foreach (Local local in local_list) {
                                if (local.Slot == -1) {
                                        local.Slot = slot_pos;
                                }
                                slot_pos++;
                                if (local.Name == null)
                                        continue;

                                if (!current_named_table.Contains (local.Name))
                                        current_named_table.Add (local.Name, local);
                         }

                        this.local_list.AddRange (local_list);
                }

                public Local GetNamedLocal (string name)
                {
                        Local ret = null;
                        int i = current_scope_depth;
                        while (ret == null && i >= 0) {
                                Hashtable current_named_table = (Hashtable) named_local_tables [i];
                                ret = (Local) current_named_table [name];

                                i --;
                        }

                        return ret;
                }

                public int GetNamedLocalSlot (string name)
                {
                        Local local = GetNamedLocal (name);
                        if (local == null)
                                return -1;

                        return local.Slot;
                }

                public int GetNamedParamPos (string name)
                {
                        int pos = -1;
                        if (param_list == null)
                                return -1;

                        if (!IsStatic)
                                pos ++;
                        foreach (ParamDef param in param_list) {
                                pos ++;
                                if (param.Name.CompareTo (name) == 0)
                                        return pos;
                        }

                        return pos;
                }

                public LocalVariableEntry[] GetLocalVars()
                {
                        ArrayList named_locals = new ArrayList ();
                        foreach (Local local in local_list) {
                                if (local.Name != null) {  // only named variables
                                        named_locals.Add (new LocalVariableEntry(local.Slot, local.Name, 0));
                                }
                        }
                        return (LocalVariableEntry []) named_locals.ToArray (typeof (LocalVariableEntry));
                }


                /* index - 0: return type
                 *         1: params start from this
                 */
                public ParamDef GetParam (int index)
                {
                        if (index == 0)
                                return ret_param;

                        if ((param_list == null) || (index < 0) || (index > param_list.Count))
                                return null;

                        index --; /* param_list has params zero-based */

                        if (param_list [index] != null)
                                return (ParamDef)param_list [index];
                        else
                                return null;
                }

                public void InitLocals ()
                {
                        init_locals = true;
                }

                public void EntryPoint ()
                {
                        if (!IsStatic)
                                Report.Error ("Non-static method as entrypoint.");
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

		public void ResolveGenParam (PEAPI.GenParam gpar)
		{
			if (gpar.Index != -1)
				return;
	
			if (gpar.Type == PEAPI.GenParamType.MVar)
				gpar.Index = GetGenericParamNum (gpar.Name); 
			else
				gpar.Index = type_def.GetGenericParamNum (gpar.Name);

			if (gpar.Index < 0)
				Report.Error (String.Format ("Invalid {0}type parameter '{1}'", 
							(gpar.Type == PEAPI.GenParamType.MVar ? "method " : ""),
							 gpar.Name));
		}

                public void ResolveGenParams ()
                {
			GenericParameters type_params = (type_def != null) ? type_def.TypeParameters : null;

			if (gen_params == null && type_params == null)
				return;

			if (gen_params != null)
				gen_params.ResolveConstraints (type_params, gen_params);
			
			BaseGenericTypeRef gtr = RetType as BaseGenericTypeRef;
			if (gtr != null)
				gtr.Resolve (type_params, gen_params);

			if (param_list == null)
				return;

			foreach (ParamDef param in param_list) {
				gtr = param.Type as BaseGenericTypeRef;
 				if (gtr != null)
					gtr.Resolve (type_params, gen_params);
                        }        
                }

                public PEAPI.MethodDef Resolve (CodeGen code_gen)
                {
                        return Resolve (code_gen, null);
                }

                public PEAPI.MethodDef Resolve (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (is_resolved)
                                return methoddef;

                        PEAPI.Param [] param_array = GenerateParams (code_gen);
                        FixAttributes ();
                        ret_param.Define (code_gen);

                        if (classdef == null)
                                methoddef = code_gen.PEFile.AddMethod (meth_attr, impl_attr,
                                                name, ret_param.PeapiParam, param_array);
                        else			
                                methoddef = classdef.AddMethod (meth_attr, impl_attr,
                                                name, ret_param.PeapiParam, param_array);

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

                public PEAPI.MethodRef GetVarargSig (PEAPI.Type[] opt, string full_signature)
                {
                        if (!is_resolved)
                                throw new InternalErrorException ("Methods must be resolved before a vararg sig can be created.");

                        PEAPI.MethodRef methref = null;
                        if (vararg_sig_table == null) {
                                vararg_sig_table = new Hashtable ();                                
                        } else {
                                methref = vararg_sig_table [full_signature] as PEAPI.MethodRef;
                        }

                        if (methref == null) {
                                methref = methoddef.MakeVarArgSignature (opt);
                                vararg_sig_table [full_signature] = methref;
                        }

                        return methref;
                }

                /// <summary>
                ///  Define a member method
                /// </summary>
                public void Define (CodeGen code_gen)
                {
                        if (is_defined)
                                return;

                        if (type_def == null)
                                /* Global method */
                                Resolve (code_gen, null);
                        else
                                Resolve (code_gen, (PEAPI.ClassDef) type_def.ClassDef);
                                
                        WriteCode (code_gen, methoddef);

                        //code_gen.Report.Message (String.Format ("Assembled method {0}::{1}", typedef.FullName, name));
			is_defined = true;
                }

                public void AddInstr (IInstr instr)
                {
                        inst_list.Add (instr);
                }

                protected void WriteCode (CodeGen code_gen, PEAPI.MethodDef methoddef)
                {
                        /// Add the custrom attributes to this method
                        if (customattr_list != null)
                                foreach (CustomAttr customattr in customattr_list) {
                                        customattr.AddTo (code_gen, methoddef);
                                        if (customattr.IsSuppressUnmanaged (code_gen))
                                                methoddef.AddMethAttribute (PEAPI.MethAttr.HasSecurity);
				}

                        /// Add declarative security to this method
			if (decl_sec != null) {
				decl_sec.AddTo (code_gen, methoddef);
                                methoddef.AddMethAttribute (PEAPI.MethAttr.HasSecurity);
                        }        

                        // Generic type parameters
                        if (gen_params != null)
                                gen_params.Resolve (code_gen, methoddef);

                        if (type_def == null) {
                                //Global method
                                meth_attr &= ~PEAPI.MethAttr.Abstract;
                                meth_attr |= PEAPI.MethAttr.Static;
                        } else {
                                if ((inst_list.Count > 0) && type_def.IsInterface && !IsStatic)
                                        Report.Error (start, "Method cannot have body if it is non-static declared in an interface");
                                
                                if (IsAbstract) {
                                        if (!type_def.IsAbstract)
                                                Report.Error (start, String.Format ("Abstract method '{0}' in non-abstract class '{1}'", 
                                                                        Name, type_def.FullName));
                                        if (inst_list.Count > 0)
                                                Report.Error (start, "Method cannot have body if it is abstract.");
                                        return;
                                }
                        }

                        if (entry_point)
                                methoddef.DeclareEntryPoint ();

                        if (local_list.Count > 0) {
                                int ec = Report.ErrorCount;
                                PEAPI.Local[] local_array = new PEAPI.Local[local_list.Count];

                                foreach (Local local in local_list)
                                        local_array[local.Slot]  = local.GetPeapiLocal (code_gen);

                                if (Report.ErrorCount > ec)
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

                        if (pinvoke_info) {
                                methoddef.AddPInvokeInfo (pinvoke_mod.ModuleRef,
                                                (pinvoke_name != null ? pinvoke_name : name), pinvoke_attr);
                        }

                        if ((impl_attr & PEAPI.ImplAttr.Runtime) == PEAPI.ImplAttr.Runtime) {
                                if (inst_list.Count > 0)
                                        Report.Error (start, String.Format ("Method cannot have body if it is non-IL runtime-supplied, '{0}'", 
                                                                FullName));
                        } else {
                                if (((impl_attr & PEAPI.ImplAttr.Native) != 0) ||
                                        ((impl_attr & PEAPI.ImplAttr.Unmanaged) != 0))
                                        Report.Error (start, String.Format ("Cannot compile native/unmanaged method, '{0}'", 
                                                                FullName));
                        }

                        if (inst_list.Count > 0) {
                                /* Has body */
                                if ((impl_attr & PEAPI.ImplAttr.InternalCall) != 0)
                                        Report.Error (start, String.Format ("Method cannot have body if it is an internal call, '{0}'", 
                                                                FullName));

                                if (pinvoke_info)
                                        Report.Error (start, String.Format ("Method cannot have body if it is pinvoke, '{0}'",
                                                                FullName));
                        } else {
                                if (pinvoke_info ||
                                        ((impl_attr & PEAPI.ImplAttr.Runtime) != 0) ||
                                        ((impl_attr & PEAPI.ImplAttr.InternalCall) != 0))
                                        /* No body required */
                                        return;

                                Report.Warning (start, "Method has no body, 'ret' emitted.");
                                AddInstr (new SimpInstr (PEAPI.Op.ret, start));
                        }

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
                                        label.Define (new PEAPI.CILLabel (label.Offset, true));
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
                                        Report.Error ("Undefined Label:  " + label);
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
                }

                public LabelInfo AddLabel (string name)
                {
                        LabelInfo label_info = (LabelInfo) label_table[name];
                        if (label_info != null)
                                Report.Error ("Duplicate label '" + name + "'");

                        label_info = new LabelInfo (name, inst_list.Count);
                        label_table [name] = label_info;
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
                                signature = CreateVarargSignature (RetType, name, param_list);
                        else
                                signature = CreateSignature (RetType, name, param_list, GenParamCount);
                }

                static string CreateSignature (BaseTypeRef RetType, string name, IList param_list, int gen_param_count)
                {
                        StringBuilder builder = new StringBuilder ();

			builder.Append (RetType.FullName);
			builder.Append (" ");
                        builder.Append (name);
                        if (gen_param_count > 0)
                                builder.AppendFormat ("`{0}", gen_param_count);
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

                static string CreateVarargSignature (BaseTypeRef RetType, string name, IList param_list)
                {
                        StringBuilder builder = new StringBuilder ();
                        ParamDef last = null;

			builder.Append (RetType.FullName);
			builder.Append (" ");
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

                // @include_optional: include optional parameters for vararg methods
                // This will be true mostly for *Ref use, eg. methodrefs at call sites
                // and false for *Def (include only the required params)
                public static string CreateSignature (BaseTypeRef RetType, PEAPI.CallConv call_conv, string name,
                                BaseTypeRef[] param_list, int gen_param_count, bool include_optional)
                {
                        if ((call_conv & PEAPI.CallConv.Vararg) != 0)
                                return CreateVarargSignature (RetType, name, param_list, include_optional);
                        else
                                return CreateSignature (RetType, name, param_list, gen_param_count, include_optional);
                }

                static string CreateVarargSignature (BaseTypeRef RetType, string name, BaseTypeRef [] param_list, bool include_optional)
                {
                        StringBuilder builder = new StringBuilder ();
                        BaseTypeRef last = null;

			builder.Append (RetType.FullName);
			builder.Append (" ");
                        builder.Append (name);
                        builder.Append ('(');

                        bool first = true;
                        if (param_list != null && param_list.Length > 0) {
                                foreach (BaseTypeRef param in param_list) {
                                        if (!first)
                                                builder.Append (',');
                                        builder.Append (param.FullName);
                                        first = false;
                                        last = param;
                                        if (!include_optional && param is SentinelTypeRef)
                                                break;
                                }
                                
                        }
                        
                        if (!include_optional && (last == null || !(last is SentinelTypeRef))) {
                                if (!first)
                                        builder.Append (',');
                                builder.Append ("...");
                        }

                        builder.Append (')');

                        return builder.ToString ();
                }

                static string CreateSignature (BaseTypeRef RetType, string name, BaseTypeRef[] param_list, int gen_param_count, bool include_optional)
                {
                        StringBuilder builder = new StringBuilder ();

			builder.Append (RetType.FullName);
			builder.Append (" ");
                        builder.Append (name);
                        if (gen_param_count > 0)
                                builder.AppendFormat ("`{0}", gen_param_count);
                        builder.Append ('(');

                        if (param_list != null) {
                                bool first = true;
                                foreach (BaseTypeRef param in param_list) {
                                        if (!first)
                                                builder.Append (',');
                                        builder.Append (param.FullName);
                                        first = false;
                                        if (!include_optional && param is SentinelTypeRef)
                                                break;
                                }
                        }
                        builder.Append (')');

                        return builder.ToString ();
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

