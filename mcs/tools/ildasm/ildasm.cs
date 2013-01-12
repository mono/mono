using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono;

public partial class ILDAsm
{
	public static int Main (String[] args) {
		if (args.Length != 1) {
			Console.Error.WriteLine ("Usage: ildasm <assembly file>");
			return 1;
		}

		var inst = new ILDAsm ();
		return inst.Run (args);
	}

	public AssemblyDefinition ad;
	public ModuleDefinition main;
	public TextWriter os;
	public int indent;
	public List<FieldDefinition> fields_with_rva = new List<FieldDefinition> ();

	public void WriteLine () {
		os.WriteLine ();
	}

	public void WriteLine (String s) {
		for (int i = 0; i < indent; ++i)
			os.Write ("  ");
		os.WriteLine (s);
	}

	public int Run (String[] args) {
		ad = AssemblyDefinition.ReadAssembly (args [0]);

		main = ad.MainModule;

		os = Console.Out;

		// Emit assembly references
		EmitAssemblyReferences ();
		EmitAssembly ();
		EmitModule ();
		foreach (var typedef in main.Types) {
			// FIXME:
			if (typedef.Name == "<Module>")
				EmitGlobals (typedef);
			else
				EmitType (typedef);
		}
		EmitData ();

		return 0;
	}

	string EscapeName (string s) {
		bool escape = false;

		if (s.Contains ("/")) {
			string[] parts = s.Split ('/');
			var sb = new StringBuilder ();
			for (int i = 0; i < parts.Length; ++i) {
				if (i > 0)
					sb.Append ("/");
				sb.Append (EscapeName (parts [i]));
			}
			return sb.ToString ();
		}
			
		foreach (char c in s) {
			if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || (c == '_') || (c == '.') || (c == '`')))
				escape = true;
		}
		if (!escape && keyword_table.ContainsKey (s))
			escape = true;
		if (escape)
			return "'" + s + "'";
		else
			return s;
	}

	string EscapeString (string s) {
		return s.Replace ("\\", "\\\\").Replace ("\"", "\\\"");
	}

	void EmitScope (IMetadataScope s, StringBuilder sb) {
		if (s is AssemblyNameReference) {
			AssemblyNameReference aname = (s as AssemblyNameReference);
			sb.Append ("[" + EscapeName (aname.Name) + "]");
		} else if (s is ModuleDefinition) {
			if (s != main)
				throw new NotImplementedException ();
		} else {
			throw new NotImplementedException (s.GetType ().ToString ());
		}
	}

	string StringifyTypeRef (TypeReference t) {
		switch (t.MetadataType) {
		case MetadataType.Void:
			return "void";
		case MetadataType.Boolean:
			return "bool";
		case MetadataType.Char:
			return "char";
		case MetadataType.SByte:
			return "int8";
		case MetadataType.Byte:
			return "unsigned int8";
		case MetadataType.Int16:
			return "int16";
		case MetadataType.UInt16:
			return "unsigned int16";
		case MetadataType.Int32:
			return "int32";
		case MetadataType.UInt32:
			return "unsigned int32";
		case MetadataType.Int64:
			return "int64";
		case MetadataType.UInt64:
			return "unsigned int64";
		case MetadataType.Single:
			return "float32";
		case MetadataType.Double:
			return "float64";
		case MetadataType.String:
			return "string";
		case MetadataType.IntPtr:
			return "native int";
		case MetadataType.UIntPtr:
			//return "unsigned native int";
			return "[mscorlib]System.UIntPtr";
		case MetadataType.TypedByReference:
			return "typedref";
		case MetadataType.Class:
		case MetadataType.Object:
		case MetadataType.ValueType: {
			var sb = new StringBuilder ();
			IMetadataScope s = t.Scope;
			if (t.MetadataType == MetadataType.ValueType)
				sb.Append ("valuetype ");
			else
				sb.Append ("class ");
			EmitScope (s, sb);
			sb.Append (EscapeName (t.FullName));
			return sb.ToString ();
		}
		case MetadataType.Array: {
			ArrayType at = (t as ArrayType);
			
			if (at.IsVector)
				return StringifyTypeRef (at.ElementType) + "[]";

			var suffix = new StringBuilder ();
			suffix.Append ("[");
			for (int i = 0; i < at.Dimensions.Count; i++) {
				if (i > 0)
					suffix.Append (",");

				suffix.Append (at.Dimensions [i].ToString ());
			}
			suffix.Append ("]");

			return StringifyTypeRef (at.ElementType) + suffix;
		}
		case MetadataType.Pointer:
			return StringifyTypeRef ((t as TypeSpecification).ElementType) + "*";
		case MetadataType.ByReference:
			return StringifyTypeRef ((t as TypeSpecification).ElementType) + "&";
		case MetadataType.Pinned:
			return StringifyTypeRef ((t as TypeSpecification).ElementType) + " pinned";
		case MetadataType.GenericInstance: {
			var sb = new StringBuilder ();
			var inst = (t as GenericInstanceType);
			sb.Append (StringifyTypeRef (inst.ElementType));
			sb.Append ("<");
			int aindex = 0;
			foreach (TypeReference arg in inst.GenericArguments) {
				if (aindex > 0)
					sb.Append (", ");
				sb.Append (StringifyTypeRef (arg));
				aindex ++;
			}
			sb.Append (">");
			return sb.ToString ();
		}
		case MetadataType.Var:
			return "!" + (t as GenericParameter).Position;
		case MetadataType.MVar:
			return "!!" + (t as GenericParameter).Position;
		case MetadataType.Sentinel:
			return "..., " + StringifyTypeRef ((t as SentinelType).ElementType);
		case MetadataType.RequiredModifier: {
			var mod = (t as RequiredModifierType);
			if (mod.ModifierType.MetadataType != MetadataType.Class)
				throw new NotImplementedException ();
			var sb = new StringBuilder ();
			sb.Append (StringifyTypeRef (mod.ElementType));
			sb.Append (" modreq (");
			EmitScope (mod.ModifierType.Scope, sb);
			sb.Append (EscapeName (mod.ModifierType.FullName));
			sb.Append (")");
			return sb.ToString ();
		}
		default:
			throw new NotImplementedException ("" + t.MetadataType + " " + t.ToString ());
		}
	}

	// Same as StringifyTypeRef, but emit primitive types as [mscorlib]...
	string StringifyTypeRefNoPrim (TypeReference t) {
		switch (t.MetadataType) {
		case MetadataType.Void:
		case MetadataType.Boolean:
		case MetadataType.Char:
		case MetadataType.SByte:
		case MetadataType.Byte:
		case MetadataType.Int16:
		case MetadataType.UInt16:
		case MetadataType.Int32:
		case MetadataType.UInt32:
		case MetadataType.Int64:
		case MetadataType.UInt64:
		case MetadataType.Single:
		case MetadataType.Double:
		case MetadataType.String:
		case MetadataType.IntPtr:
		case MetadataType.UIntPtr:
		case MetadataType.TypedByReference:
			return "[mscorlib]" + t.FullName;
		default:
			return StringifyTypeRef (t);
		}
	}

	string StringifyMethodRef (MethodReference method) {
		var sb = new StringBuilder ();
		if (method.CallingConvention == MethodCallingConvention.VarArg)
			sb.Append ("vararg ");
		if (method.HasThis)
			sb.Append ("instance ");
		sb.Append (StringifyTypeRef (method.ReturnType));
		sb.Append (' ');
		sb.Append (StringifyTypeRefNoPrim (method.DeclaringType));
		sb.Append ("::");
		sb.Append (EscapeName (method.Name));
		if (method is GenericInstanceMethod) {
			sb.Append ("<");
			int idx = 0;
			foreach (var gpar in (method as GenericInstanceMethod).GenericArguments) {
				if (idx > 0)
					sb.Append (", ");
				sb.Append (StringifyTypeRef (gpar));
				idx ++;
			}
			sb.Append (">");
		}
		sb.Append ('(');
		int par_index = 0;
		foreach (ParameterReference par in method.Parameters) {
			if (par_index > 0)
				sb.Append (", ");
			sb.Append (StringifyTypeRef (par.ParameterType));
			par_index ++;
		}
		sb.Append (")");
		return sb.ToString ();
	}

	string StringifyFieldRef (FieldReference field) {
		var sb = new StringBuilder ();
		sb.Append (StringifyTypeRef (field.FieldType));
		sb.Append (' ');
		sb.Append (StringifyTypeRefNoPrim (field.DeclaringType));
		sb.Append ("::");
		sb.Append (EscapeName (field.Name));
		return sb.ToString ();
	}

	void WriteBlob (byte[] blob) {
		int idx = 0;
		while (idx < blob.Length) {
			int len = idx + 16 < blob.Length ? 16 : blob.Length - idx;
			var sb = new StringBuilder ();
			var sb2 = new StringBuilder ();
			for (int i = idx; i < idx + len; ++i) {
				sb.Append (String.Format ("{0:X2} ", blob [i]));
				if (Char.IsLetterOrDigit ((char)blob [i]))
					sb2.Append ((char)blob [i]);
				else
					sb2.Append ('.');
			}
			for (int i = 0; i < 16 - len; ++i)
				sb.Append ("   ");
			if (len < 16 || idx + 16 == blob.Length)
				sb.Append (')');
			else
				sb.Append (' ');
			WriteLine (String.Format ("{0} // {1}", sb.ToString (), sb2.ToString ()));
			idx += 16;
		}
	}

	string Map (Dictionary <uint, string> map, uint val) {
		string s;

		if (map.TryGetValue (val, out s))
			return s;
		else
			throw new NotImplementedException ("Value '" + val + "' not supported.");
	}

	string MapFlags (Dictionary <uint, string> map, uint val) {
		var sb = new StringBuilder ();
		foreach (var flag in map.Keys)
			if ((val & flag) != 0)
				sb.Append (map [flag]);
		return sb.ToString ();
	}

	void EmitAssemblyReferences () {
		foreach (var aname in main.AssemblyReferences) {
			os.WriteLine (".assembly extern " + EscapeName (aname.Name));
			os.WriteLine ("{");
			indent ++;
			Version v = aname.Version;
			WriteLine (String.Format (".ver {0}:{1}:{2}:{3}", v.Major, v.Minor, v.Build, v.Revision));
			byte [] token = aname.PublicKeyToken;
			if (token.Length > 0) {
				StringBuilder sb = new StringBuilder ();
				StringBuilder sb2 = new StringBuilder ();
				for (int i = 0; i < token.Length; ++i) {
					if (i > 0)
						sb.Append (" ");
					sb.Append (String.Format ("{0:X2}", token [i]));
					if (Char.IsLetterOrDigit ((char)token [i]))
						sb2.Append ((char)token [i]);
					else
						sb2.Append ('.');
				}
				WriteLine (String.Format (".publickeytoken = ({0}) // {1}", sb, sb2));
			}
			indent --;
			WriteLine ("}");
		}
	}

	void EmitCattrs (ICustomAttributeProvider prov) {
		foreach (var cattr in prov.CustomAttributes) {
			WriteLine (String.Format (".custom {0} = (", StringifyMethodRef (cattr.Constructor)));
			indent += 3;
			byte[] blob = cattr.GetBlob ();
			WriteBlob (blob);
			indent -= 3;
		}
	}

	void EmitSecDeclarations (ISecurityDeclarationProvider prov) {
		foreach (var sec in prov.SecurityDeclarations) {
			string act_str = null;
			if (!sec_action_to_string.TryGetValue (sec.Action, out act_str))
				throw new NotImplementedException (sec.Action.ToString ());
			WriteLine (".permissionset " + act_str + " = (");
			WriteBlob (sec.GetBlob ());
		}
	}

	void EmitAssembly () {
		AssemblyNameDefinition aname = ad.Name;

		WriteLine (".assembly " + EscapeName (aname.Name));
		WriteLine ("{");
		indent ++;
		EmitCattrs (ad);
		EmitSecDeclarations (ad);
		WriteLine (String.Format (".hash algorithm 0x{0:X8}", (int)aname.HashAlgorithm));
		Version v = aname.Version;
		WriteLine (String.Format (".ver {0}:{1}:{2}:{3}", v.Major, v.Minor, v.Build, v.Revision));
		byte[] token = aname.PublicKey;
		if (token != null && token.Length > 0) {
			StringBuilder sb = new StringBuilder ();
			StringBuilder sb2 = new StringBuilder ();
			for (int i = 0; i < token.Length; ++i) {
				if (i > 0)
					sb.Append (" ");
				sb.Append (String.Format ("{0:X2}", token [i]));
				if (Char.IsLetterOrDigit ((char)token [i]))
					sb2.Append ((char)token [i]);
				else
					sb2.Append ('.');
			}
			WriteLine (String.Format (".publickey = ({0}) // {1}", sb, sb2));
		}
		indent --;
		WriteLine ("}");
	}

	void EmitModule () {
		WriteLine (".module " + EscapeName (main.Name) + " // GUID = " + "{" + main.Mvid.ToString ().ToUpper () + "}");
		EmitCattrs (main);
	}

	string StringifyTypeAttrs (TypeAttributes attrs) {
		var sb = new StringBuilder ();

		sb.Append (Map (type_sem_map, (uint)(attrs & TypeAttributes.ClassSemanticMask)));
		sb.Append (Map (type_access_map, (uint)(attrs & TypeAttributes.VisibilityMask)));
		sb.Append (Map (type_layout_map, (uint)(attrs & TypeAttributes.LayoutMask)));
		sb.Append (Map (type_string_format_map, (uint)(attrs & TypeAttributes.StringFormatMask)));
		sb.Append (MapFlags (type_flag_map, (uint)(attrs)));

		return sb.ToString ();
	}

	void EmitGenParams (IGenericParameterProvider prov, StringBuilder sb) {
		sb.Append ("<");
		int idx = 0;
		foreach (var gpar in prov.GenericParameters) {
			if (idx > 0)
				sb.Append (", ");
			if (gpar.HasDefaultConstructorConstraint)
				sb.Append (".ctor ");
			if (gpar.HasNotNullableValueTypeConstraint)
				sb.Append ("valuetype ");
			if (gpar.HasReferenceTypeConstraint)
				sb.Append ("class ");
			if (gpar.HasConstraints) {
				int idx2 = 0;
				sb.Append ("(");
				foreach (var c in gpar.Constraints) {
					if (idx2 > 0)
						sb.Append (", ");
					sb.Append (StringifyTypeRef (c));
					idx2 ++;
				}
				sb.Append (")");
			}
			sb.Append (EscapeName (gpar.Name));
			idx ++;
		}
		sb.Append (">");
	}

	void EmitGenParamCattrs (IGenericParameterProvider prov) {
		foreach (var gpar in prov.GenericParameters) {
			if (gpar.HasCustomAttributes) {
				WriteLine (".param type " + gpar.Name);
				EmitCattrs (gpar);
			}
		}
	}

	void EmitGlobals (TypeDefinition td) {
		foreach (var md in td.Methods)
			EmitMethod (md);
	}

	void EmitType (TypeDefinition td) {
		WriteLine ("");

		if (td.MetadataType != MetadataType.Class && td.MetadataType != MetadataType.ValueType)
			throw new NotImplementedException (td.MetadataType.ToString ());

		// FIXME: Group types by namespaces
		if (!td.IsNested && td.Namespace != null && td.Namespace != String.Empty) {
			WriteLine (".namespace " + EscapeName (td.Namespace));
			WriteLine ("{");
			indent ++;
		}

		var sb = new StringBuilder ();
		sb.Append (".class ");
		sb.Append (StringifyTypeAttrs (td.Attributes));
		sb.Append (EscapeName (td.Name));
		if (td.HasGenericParameters)
			EmitGenParams (td, sb);
		WriteLine (sb.ToString ());
		indent ++;
		if (td.BaseType != null)
			WriteLine ("extends " + StringifyTypeRef (td.BaseType));
		if (td.HasInterfaces) {
			int idx = 0;
			sb = new StringBuilder ();
			foreach (TypeReference t in td.Interfaces) {
				if (idx > 0)
					sb.Append (", ");
				sb.Append (StringifyTypeRef (t));
				idx ++;
			}
			WriteLine (String.Format ("implements {0}", sb.ToString ()));
		}
		indent --;
		WriteLine ("{");
		indent ++;
		if (td.PackingSize != -1)
			WriteLine (".pack " + td.PackingSize);
		if (td.ClassSize != -1)
			WriteLine (".size " + td.ClassSize);
		EmitCattrs (td);
		EmitGenParamCattrs (td);
		EmitSecDeclarations (td);
		foreach (var fd in td.Fields)
			EmitField (fd);
		foreach (var md in td.Methods)
			EmitMethod (md);
		foreach (var p in td.Properties)
			EmitProperty (p);
		foreach (var e in td.Events)
			EmitEvent (e);
		foreach (var t in td.NestedTypes)
			EmitType (t);
		indent --;
		WriteLine ("}");

		if (!td.IsNested && td.Namespace != null && td.Namespace != String.Empty) {
			WriteLine ("}");
			indent --;
		}
	}

	string StringifyFieldAttributes (FieldAttributes attrs) {
		var sb = new StringBuilder ();
		sb.Append (Map (field_access_map, (uint)(attrs & FieldAttributes.FieldAccessMask)));
		sb.Append (MapFlags (field_flag_map, (uint)(attrs)));
		return sb.ToString ();
	}

	void EmitField (FieldDefinition fd) {
		var sb = new StringBuilder ();
		sb.Append (".field ");
		if (fd.Offset != -1)
			sb.Append ("[" + fd.Offset + "] ");
		sb.Append (StringifyFieldAttributes (fd.Attributes));
		if (fd.HasMarshalInfo) {
			sb.Append (" ");
			sb.Append (StringifyMarshalInfo (fd.MarshalInfo));
		}
		sb.Append (" ");
		sb.Append (StringifyTypeRef (fd.FieldType));
		sb.Append (" ");
		sb.Append (EscapeName (fd.Name));
		if (fd.HasConstant)
			EmitConstant (fd.Constant, sb);
		if (fd.RVA != 0) {
			sb.Append (String.Format (" at D_{0:x8}", fd.RVA));
			fields_with_rva.Add (fd);
		}
		WriteLine (sb.ToString ());
		EmitCattrs (fd);
	}

	string StringifyMethodAttributes (MethodAttributes attrs) {
		var sb = new StringBuilder ();
		sb.Append (Map (method_access_map, (uint)(attrs & MethodAttributes.MemberAccessMask)));
		sb.Append (MapFlags (method_flag_map, (uint)(attrs)));
		return sb.ToString ();
	}

	string StringifyMethodImplAttributes (MethodImplAttributes attrs) {
		var sb = new StringBuilder ();
		sb.Append (Map (method_impl_map, (uint)(attrs & MethodImplAttributes.CodeTypeMask)));
		sb.Append (Map (method_managed_map, (uint)(attrs & MethodImplAttributes.ManagedMask)));
		sb.Append (MapFlags (method_impl_flag_map, (uint)(attrs)));

		return sb.ToString ();
	}

	string StringifyTypeNameReflection (TypeReference t) {
		if (t.MetadataType != MetadataType.Class)
			throw new NotImplementedException ();
		IMetadataScope s = t.Scope;
		if (!(s is ModuleDefinition))
			throw new NotImplementedException ();
		return t.FullName.Replace ("/", "+");
	}

	string StringifyMarshalInfo (MarshalInfo mi) {
		var sb = new StringBuilder ();

		sb.Append ("marshal (");

		string s = null;
		switch (mi.NativeType) {
		case NativeType.Array: {
			var ami = (mi as ArrayMarshalInfo);
			if (native_type_to_str.TryGetValue (ami.ElementType, out s)) {
				sb.Append (s);
			}
			sb.Append ("[");
			//Console.WriteLine ("// XXX: " + ami.Size + " " + ami.SizeParameterIndex + " " + ami.SizeParameterMultiplier);

			/*
			 * Comments in metadata.c:
			 * So if (param_num == 0) && (num_elem > 0), then
			 * elem_mult == 0 -> the array size is num_elem
			 * elem_mult == 1 -> the array size is @param_num + num_elem
			 */
			if (ami.Size != -1 && ami.Size != 0)
				sb.Append (ami.Size.ToString ());
			if (ami.SizeParameterMultiplier != 0 && ami.SizeParameterIndex != -1)
				sb.Append ("+" + ami.SizeParameterIndex.ToString ());
			sb.Append ("]");
			break;
		}
		case NativeType.FixedArray: {
			var ami = (mi as FixedArrayMarshalInfo);
			/*
			if (native_type_to_str.TryGetValue (ami.ElementType, out s)) {
				sb.Append (s);
			}
			*/
			sb.Append ("fixed array [" + ami.Size + "]");
			break;
		}
		case NativeType.FixedSysString: {
			var ami = (mi as FixedSysStringMarshalInfo);
			sb.Append ("fixed sysstring [" + ami.Size + "]");
			break;
		}
		case NativeType.SafeArray: {
			var sami = (mi as SafeArrayMarshalInfo);
			sb.Append ("safearray ");
			switch (sami.ElementType) {
			case VariantType.Variant:
				sb.Append ("variant");
				break;
			default:
				throw new NotImplementedException ();
			}
			break;
		}
		case NativeType.CustomMarshaler: {
			var cmi = (mi as CustomMarshalInfo);

			if (cmi.Guid != Guid.Empty || cmi.UnmanagedType != String.Empty)
				throw new NotImplementedException ();
			sb.Append ("custom (\"" + StringifyTypeNameReflection (cmi.ManagedType) + "\", \"" + EscapeString (cmi.Cookie) + "\")");
			break;
		}
		default:
			if (native_type_to_str.TryGetValue (mi.NativeType, out s))
				sb.Append (s);
			else
				throw new NotImplementedException (mi.NativeType.ToString ());
			break;
		}
		sb.Append (")");
		return sb.ToString ();
	}

	string StringifySignature (MethodDefinition md) {
		var sb = new StringBuilder ();
		int pindex = 0;
		foreach (var par in md.Parameters) {
			if (pindex > 0)
				sb.Append (", ");
			if (par.IsIn)
				sb.Append ("[in] ");
			if (par.IsOut)
				sb.Append ("[out] ");
			if (par.IsOptional)
				sb.Append ("[opt] ");
			sb.Append (StringifyTypeRef (par.ParameterType));
			if (par.HasMarshalInfo) {
				sb.Append (" ");
				sb.Append (StringifyMarshalInfo (par.MarshalInfo));
			}
			sb.Append (" ");
			sb.Append (EscapeName (par.Name));
			pindex ++;
		}

		return sb.ToString ();
	}

	string StringifyPInvokeAttrs (PInvokeAttributes attrs) {
		var sb = new StringBuilder ();
		sb.Append (Map (pinvoke_char_set_map, (uint)(attrs & PInvokeAttributes.CharSetMask)));
		sb.Append (Map (pinvoke_cconv_map, (uint)(attrs & PInvokeAttributes.CallConvMask)));
		sb.Append (MapFlags (pinvoke_flags_map, (uint)(attrs)));
		return sb.ToString ();
	}		

	string StringifyCallingConvention (MethodCallingConvention cconv) {
		switch (cconv) {
		case MethodCallingConvention.Default:
			return "default";
		case MethodCallingConvention.VarArg:
			return "vararg";
		default:
			throw new NotImplementedException (cconv.ToString ());
		}
	}

	void EmitMethod (MethodDefinition md) {
		int idx;

		WriteLine ();
		var pinvoke_sb = new StringBuilder ();
		if ((uint)(md.Attributes & MethodAttributes.PInvokeImpl) != 0) {
			var pinvoke = md.PInvokeInfo;
			pinvoke_sb.Append ("pinvokeimpl (\"" + pinvoke.Module.Name + "\" as \"" + pinvoke.EntryPoint + "\" " + StringifyPInvokeAttrs (pinvoke.Attributes) + " )");
		}
		WriteLine (String.Format (".method {0}{1}", StringifyMethodAttributes (md.Attributes), pinvoke_sb));

		var sb = new StringBuilder ();
		sb.Append ("       ");
		if (!md.IsStatic)
			sb.Append ("instance ");		
		// CallingConvention seems to be 32
		sb.Append (StringifyCallingConvention ((MethodCallingConvention)((uint)md.CallingConvention & 0xf)));
		sb.Append (" ");
		sb.Append (StringifyTypeRef (md.ReturnType));
		sb.Append (" ");
		if (md.MethodReturnType.HasMarshalInfo) {
			sb.Append (StringifyMarshalInfo (md.MethodReturnType.MarshalInfo));
			sb.Append (" ");
		}
		sb.Append (EscapeName (md.Name));
		if (md.HasGenericParameters)
			EmitGenParams (md, sb);
		WriteLine (String.Format ("{0} ({1}) {2}", sb, StringifySignature (md), StringifyMethodImplAttributes (md.ImplAttributes)));
		WriteLine ("{");
		indent ++;
		foreach (var ov in md.Overrides)
			WriteLine (".override method " + StringifyMethodRef (ov));
		EmitCattrs (md);
		EmitGenParamCattrs (md);
		EmitSecDeclarations (md);
		idx = 0;
		foreach (var par in md.Parameters) {
			if (par.HasCustomAttributes) {
				WriteLine (".param [" + (idx + 1) + "]");
				EmitCattrs (par);
			}
			if (par.HasConstant) {
				sb = new StringBuilder ();
				EmitConstant (par.Constant, sb);
				WriteLine (".param [" + (idx + 1) + "]" + sb);
			}
			idx ++;
		}
		// FIXME: Print RVA, code size
		if (md == main.EntryPoint)
			WriteLine (".entrypoint");
		if (md.HasBody) {
			MethodBody body = md.Body;
			WriteLine (".maxstack " + body.MaxStackSize);
			if (body.InitLocals)
				WriteLine (".locals init (");
			else
				WriteLine (".locals (");
			indent ++;
			int vindex = 0;
			foreach (var v in body.Variables) {
				WriteLine (StringifyTypeRef (v.VariableType) + " " + (v.Name == "" ? "V_" + v.Index : v.Name) + (vindex + 1 < body.Variables.Count ? ", " : ""));
				vindex ++;
			}
			indent --;
			WriteLine (")");

			List<ExceptionHandler> handlers = body.ExceptionHandlers.ToList ();
			List<ExceptionHandler> reverse_handlers = body.ExceptionHandlers.Reverse ().ToList ();

			var trys = new Dictionary<ExceptionHandler, bool> ();
			if (handlers.Count > 0)
				trys [handlers [0]] = true;
			for (int i = 1; i < handlers.Count; ++i) {
				trys [handlers [i]] = true;
				for (int j = 0; j < i; ++j) {
					if (handlers [i].TryStart == handlers [j].TryStart && handlers [i].TryEnd == handlers [j].TryEnd) {
						trys [handlers [i]] = false;
						break;
					}
				}
			}

			idx = 0;
			var handler_to_idx = new Dictionary<ExceptionHandler, int> ();
			foreach (ExceptionHandler eh in body.ExceptionHandlers) {
				handler_to_idx [eh] = idx;
				idx ++;
			}

			foreach (var ins in body.Instructions) {
				foreach (var eh in handlers) {
					if (eh.TryEnd == ins && trys [eh]) {
						indent --;
						WriteLine ("} // end try " + handler_to_idx [eh]);
					}

					if (eh.HandlerEnd == ins) {
						indent --;
						WriteLine ("} // end handler " + handler_to_idx [eh]);
					}
				}			

				foreach (var eh in reverse_handlers) {
					if (eh.TryStart == ins && trys [eh]) {
						WriteLine (".try { // " + handler_to_idx [eh]);
						indent ++;
					}
					if (eh.HandlerStart == ins) {
						string type_str = null;
						switch (eh.HandlerType) {
						case ExceptionHandlerType.Catch:
							type_str = "catch";
							break;
						case ExceptionHandlerType.Finally:
							type_str = "finally";
							break;
						default:
							throw new NotImplementedException (eh.HandlerType.ToString ());
						}
						if (eh.CatchType == null)
							WriteLine (type_str + " { // " + handler_to_idx [eh]);
						else
							WriteLine (type_str + " " + StringifyTypeRef (eh.CatchType) + " { // " + handler_to_idx [eh]);
						indent ++;
					}
				}

				WriteLine (StringifyIns (ins));
			}
		}
		indent --;
		WriteLine ("}");
	}

	// Based on Instruction:ToString ()
	public string StringifyIns (Instruction ins) {
		var sb = new StringBuilder ();

		AppendLabel (sb, ins);
		sb.Append (':');
		sb.Append ("  ");
		sb.Append (ins.OpCode.Name);

		if (ins.Operand == null)
			return sb.ToString ();

		sb.Append (' ');

		object operand = ins.Operand;
		switch (ins.OpCode.OperandType) {
		case OperandType.ShortInlineBrTarget:
		case OperandType.InlineBrTarget:
			AppendLabel (sb, (Instruction) operand);
			break;
		case OperandType.InlineSwitch:
			var labels = (Instruction []) operand;
			sb.Append ("(");
			for (int i = 0; i < labels.Length; i++) {
				if (i > 0)
					sb.Append (',');

				AppendLabel (sb, labels [i]);
			}
			sb.Append (")");
			break;
			case OperandType.InlineString:
				sb.Append ('\"');
				sb.Append (EscapeString ((string)operand));
				sb.Append ('\"');
				break;
		default:
			if (operand is MethodReference) {
				if (ins.OpCode == OpCodes.Ldtoken)
					sb.Append ("method ");
				sb.Append (StringifyMethodRef ((MethodReference)operand));
			} else if (operand is TypeReference)
				sb.Append (StringifyTypeRef ((TypeReference)operand));
			else if (operand is VariableDefinition)
				sb.Append (operand.ToString ());
			else if (operand is FieldReference) {
				if (ins.OpCode == OpCodes.Ldtoken)
					sb.Append ("field ");
				sb.Append (StringifyFieldRef ((FieldReference)operand));
			} else if (operand is ParameterDefinition) {
				var pd = (operand as ParameterDefinition);
				sb.Append (pd.Index + (pd.Method.HasThis ? 1 : 0));
			}
			else {
				EmitConstantOperand (operand, sb);
			}
			break;
			}

		return sb.ToString ();
	}

	static void AppendLabel (StringBuilder builder, Instruction instruction) {
		builder.Append ("IL_");
		builder.Append (instruction.Offset.ToString ("x4"));
	}

	void EmitProperty (PropertyDefinition p) {
		// FIXME: attributes

		var sb = new StringBuilder ();
		sb.Append (".property ");
		if (p.HasThis)
			sb.Append ("instance ");
		sb.Append (StringifyTypeRef (p.PropertyType));
		sb.Append (" ");
		sb.Append (EscapeName (p.Name));
		sb.Append ("(");
		int idx = 0;
		foreach (var par in p.Parameters) {
			if (idx > 0)
				sb.Append (", ");
			sb.Append (StringifyTypeRef (par.ParameterType));
			idx ++;
		}
		sb.Append (")");
		WriteLine (sb.ToString ());
		WriteLine ("{");
		indent ++;
		EmitCattrs (p);
		if (p.SetMethod != null)
			WriteLine (".set " + StringifyMethodRef (p.SetMethod));
		if (p.GetMethod != null)
			WriteLine (".get " + StringifyMethodRef (p.GetMethod));
		if (p.HasOtherMethods)
			throw new NotImplementedException ();
		indent --;
		WriteLine ("}");
	}

	void EmitEvent (EventDefinition e) {
		WriteLine (".event " + StringifyTypeRef (e.EventType) + " " + EscapeName (e.Name));
		WriteLine ("{");
		indent ++;
		if (e.AddMethod != null)
			WriteLine (".addon " + StringifyMethodRef (e.AddMethod));
		if (e.RemoveMethod != null)
			WriteLine (".removeon " + StringifyMethodRef (e.RemoveMethod));
		foreach (var m in e.OtherMethods)
			WriteLine (".other " + StringifyMethodRef (m));
		indent --;
		WriteLine ("}");
	}

	void EmitData () {
		foreach (var fd in fields_with_rva) {
			WriteLine (String.Format (".data D_{0:x8} = bytearray (", fd.RVA));
			WriteBlob (fd.InitialValue);
		}
	}

	void EmitConstantOperand (object operand, StringBuilder sb) {
		if (operand is double) {
			double d = (double)operand;
			// FIXME:
			//if (Double.IsNaN (d) || Double.IsInfinity (d)) {
			{
				byte[] b = DataConverter.GetBytesLE (d);
				sb.Append ("(");
				int index = 0;
				for (int i = 0; i < b.Length; ++i) {
					if (index > 0)
						sb.Append (" ");
					sb.Append (String.Format ("{0:x2}", b [i]));
					index ++;
				}
				sb.Append (")");
			}
		} else if (operand is float) {
			float d = (float)operand;
			// FIXME:
			//if (Single.IsNaN (d) || Single.IsInfinity (d)) {
			{
				byte[] b = DataConverter.GetBytesLE (d);
				sb.Append ("(");
				int index = 0;
				for (int i = 0; i < b.Length; ++i) {
					if (index > 0)
						sb.Append (" ");
					sb.Append (String.Format ("{0:x2}", b [i]));
					index ++;
				}
				sb.Append (")");
			}
		} else if (operand.GetType ().Assembly == typeof (int).Assembly)
			sb.Append (operand.ToString ());
		else
			throw new NotImplementedException (operand.GetType ().ToString ());
	}

	void EmitConstant (object o, StringBuilder sb) {
		if (o is byte)
			sb.Append (String.Format (" = int8(0x{0:x2})", (byte)o));
		else if (o is sbyte)
			sb.Append (String.Format (" = int8(0x{0:x2})", (sbyte)o));
		else if (o is short)
			sb.Append (String.Format (" = int16(0x{0:x4})", (short)o));
		else if (o is ushort)
			sb.Append (String.Format (" = int16(0x{0:x4})", (ushort)o));
		else if (o is int)
			sb.Append (String.Format (" = int32(0x{0:x8})", (int)o));
		else if (o is uint)
			sb.Append (String.Format (" = int32(0x{0:x8})", (uint)o));
		else if (o is long)
			sb.Append (String.Format (" = int64(0x{0:x8})", (long)o));
		else if (o is ulong)
			sb.Append (String.Format (" = int64(0x{0:x8})", (ulong)o));
		else if (o is string)
			sb.Append (String.Format (" = \"{0}\"", EscapeString ((string)o)));
		else if (o is bool)
			sb.Append (String.Format (" = bool({0})", (bool)o ? "true" : " false"));
		else if (o is char)
			sb.Append (String.Format (" = char(0x{0:x4})", (int)(char)o));
		else if (o is double)
			// FIXME:
			sb.Append (String.Format (" = float64({0:f})", (double)o));
		else if (o is float)
			// FIXME:
			sb.Append (String.Format (" = float32({0:f})", (float)o));
		else if (o == null)
			sb.Append ("= nullref");
		else
			throw new NotImplementedException ("" + o.GetType ().ToString () + " " + o.ToString ());
	}
}
