using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Mono.Cecil;

//
// This class generates the icall_trampoline_dispatch () function used by the interpreter to call native code on WASM.
// It should be kept in sync with mono_wasm_interp_to_native_trampoline () in the runtime.
//

class InterpToNativeGenerator {
	// Default set of signatures
	static string[] cookies = new string[] {
		"V",
		"VI",
		"VII",
		"VIII",
		"VIIII",
		"VIIIII",
		"VIIIIII",
		"VIIIIIII",
		"VIIIIIIII",
		"VIIIIIIIII",
		"VIIIIIIIIII",
		"VIIIIIIIIIII",
		"VIIIIIIIIIIII",
		"VIIIIIIIIIIIII",
		"VIIIIIIIIIIIIII",
		"VIIIIIIIIIIIIIII",
		"I",
		"II",
		"III",
		"IIII",
		"IIIII",
		"IIIIII",
		"IIIIIII",
		"IIIIIIII",
		"IIIIIIIII",
		"IIIIIIIIII",
		"IIIIIIIIIII",
		"IIIIIIIIIIII",
		"IIIIIIIIIIIII",
		"IIIIIIIIIIIIII",
		"IILIIII",
		"IIIL",
		"IF",
		"ID",
		"IIF",
		"IIFI",
		"IIFF",
		"IFFII",
		"IIFII",		
		"IIFFI",
		"IIFFF",
		"IIFFFI",
		"IIFFII",
		"IIFIII",
		"IIFFFFI",
		"IIFFFFII",
		"IIIF",
		"IIIFI",
		"IIIFII",
		"IIIFIII",		
		"IIIIF",
		"IIIIFI",
		"IIIIFII",
		"IIIIFIII",
		"IIIFFFF",
		"IIIFFFFF",
		"IIFFFFFF",		
		"IIIFFFFFF",
		"IIIIIIIF",		
		"IIIIIIIFF",
		"IIFFFFFFFF",		
		"IIIFFFFFFFF",
		"IIIIIIFII",
		"IIIFFFFFFFFIII",
		"IIIIIFFFFIIII",
		"IFFFFFFI",
		"IIFFIII",
		"ILI",
		"IILLI",
		"L",
		"LL",
		"LI",
		"LIL",
		"LILI",
		"LILII",
		"DD",
		"DDI",
		"DDD",
		"DDDD",
		"VF",
		"VFF",
		"VFFF",
		"VFFFF",
		"VFFFFF",
		"VFFFFFF",
		"VFFFFFFF",
		"VFFFFFFFF",
		"VFI",
		"VIF",
		"VIFF",
		"VIFFFF",
		"VIFFFFF",
		"VIFFFFFF",
		"VIFFFFFI",
		"VIIFFI",
		"VIIF",
		"VIIFFF",
		"VIIFI",
		"FF",
		"FFI",
		"FFF",
		"FFFF",
		"DI",
		"FI",
		"IIL",
		"IILI",
		"IILIIIL",
		"IILLLI",
		"IDIII",
		"LII",
		"VID",
		"VILLI",
		"DID",
		"DIDD",
		"FIF",
		"FIFF",
		"LILL",
		"VL",
		"VIL",
		"VIIL",
		"FIFFF",
		"FII",
		"FIII",
		"FIIIIII",
		"IFFFFIIII",
		"IFFI",
		"IFFIF",
		"IFFIFI",
		"IFI",
		"IFIII",
		"IIFIFIIIII",
		"IIFIFIIIIII",
		"IIFIIIII",
		"IIFIIIIII",
		"IIIFFFII",
		"IIIFFIFFFII",
		"IIIFFIFFII",
		"IIIFFII",
		"IIIFFIIIII",
		"IIIIIF",
		"IIIIIFII",
		"IIIIIIFFI",
		"IIIIIIIFFI",
		"VIFFF",
		"VIFFFFI",
		"VIFFFI",
		"VIFFFIIFF",
		"VIFFI",
		"VIFI",
		"VIIFF",
		"VIIFFFF",
		"VIIFFII",
		"VIIIF",
		"VIIIFFII",
		"VIIIFFIII",
		"VIIIFII",
		"VIIIFIII",
		"VIIIIF",
	};
 
	static string TypeToSigType (char c) {
		switch (c) {
		case 'V': return "void";
		case 'I': return "int";
		case 'L': return "gint64";
		case 'F': return "float";
		case 'D': return "double";
		default:
			throw new Exception ("Can't handle " + c);
		}
	}

	List<string> extra_signatures;

	public InterpToNativeGenerator () {
		extra_signatures = new List<string> ();
	}

	static char typeToChar (TypeReference type) {
		switch (type.MetadataType) {
		case MetadataType.Void:
			return 'V';
		case MetadataType.Boolean:
		case MetadataType.Char:
		case MetadataType.SByte:
		case MetadataType.Byte:
		case MetadataType.Int16:
		case MetadataType.UInt16:
		case MetadataType.Int32:
		case MetadataType.UInt32:
		case MetadataType.IntPtr:
		case MetadataType.UIntPtr:
			return 'I';
		case MetadataType.Int64:
		case MetadataType.UInt64:
			return 'L';
		case MetadataType.Single:
			return 'F';
		case MetadataType.Double:
			return 'D';
		case MetadataType.String:
		case MetadataType.Pointer:
		case MetadataType.ByReference:
		case MetadataType.Object:
		case MetadataType.Class:
		case MetadataType.Array:
			return 'I';
		case MetadataType.ValueType:
			return 'I';
		default:
			throw new Exception ("Can't handle type '" + type.FullName + "' (" + type.MetadataType + ").");
		}
	}

	// Add an extra signature for a pinvoke method
	public void AddSignature (MethodReference Method) {
		var sb = new StringBuilder ();
		sb.Append (typeToChar (Method.ReturnType));
		foreach (var par in Method.Parameters)
			sb.Append (typeToChar (par.ParameterType));
		extra_signatures.Add (sb.ToString ());
	}		

	public void Emit (StreamWriter w) {
		w.WriteLine ("/*");
		w.WriteLine ("* GENERATED FILE, DON'T EDIT");
		w.WriteLine ("* Generated by wasm-tuner.exe --gen-interp-to-native");
		w.WriteLine ("*/");

		var added = new HashSet<string> ();

		var l = new List<string> ();
		foreach (var c in cookies) {
			l.Add (c);
			added.Add (c);
		}
		foreach (var c in extra_signatures) {
			if (!added.Contains (c)) {
				l.Add (c);
				added.Add (c);
			}
		}
		var signatures = l.ToArray ();

		foreach (var c in signatures) {
			w.WriteLine ("static void");
			w.WriteLine ($"wasm_invoke_{c.ToLower ()} (void *target_func, InterpMethodArguments *margs)");
			w.WriteLine ("{");

			w.Write ($"\ttypedef {TypeToSigType (c [0])} (*T)(");
			for (int i = 1; i < c.Length; ++i) {
				char p = c [i];
				if (i > 1)
					w.Write (", ");
				w.Write ($"{TypeToSigType (p)} arg_{i - 1}");
			}
			if (c.Length == 1)
				w.Write ("void");

			w.WriteLine (");\n\tT func = (T)target_func;");

			var ctx = new EmitCtx ();

			w.Write ("\t");
			if (c [0] != 'V')
				w.Write ($"{TypeToSigType (c [0])} res = ");

			w.Write ("func (");
			for (int i = 1; i < c.Length; ++i) {
				char p = c [i];
				if (i > 1)
					w.Write (", ");
				w.Write (ctx.Emit (p));
			}
			w.WriteLine (");");

			if (c [0] != 'V')
				w.WriteLine ($"\t*({TypeToSigType (c [0])}*)margs->retval = res;");

			w.WriteLine ("\n}\n");
		}

		w.WriteLine ("static void\nicall_trampoline_dispatch (const char *cookie, void *target_func, InterpMethodArguments *margs)");
		w.WriteLine ("{");

		/*
		if (args.Any(a => a == "--flat")) {
			for (int i = 0; i < cookies.Length; ++i) {
				var c = cookies [i];
				w.Write ("\t");
				if (i > 0)
					w.Write ("else ");
				w.WriteLine ($"if (!strcmp (\"{c}\", cookie))");
				w.WriteLine ($"\t\twasm_invoke_{c.ToLower ()} (target_func, margs);");
			}
			w.WriteLine ("\telse {");
			w.WriteLine ("\t\tg_error (\"CANNOT HANDLE COOKIE %s\\n\", cookie);");
			w.WriteLine ("\t}");
		} else {
		*/
		Array.Sort (signatures);
		WritePartition (w, signatures, 0);
		w.WriteLine ("\tg_error (\"CANNOT HANDLE COOKIE %s\\n\", cookie);");

		w.WriteLine ("}");
	}
	
	static void WritePartition (StreamWriter w, IEnumerable<string> set, int pos = 0, int depth = 0) {
		var prefix = "\t";
		for (var c = 0; c < pos; c++)
			prefix += "\t";

		var checks = 0;
		
		var groups = set.OrderBy (s => -s.Length).Where (s => s.Length > pos).GroupBy (s => s.Skip(pos).First().ToString());
		foreach (var g in groups) {
			w.WriteLine ($"{prefix}{Elif (checks)} (cookie[{pos}] == '{g.Key}') {{");
			WritePartition (w, g.ToList (), pos + 1, checks + depth);
			w.WriteLine ($"{prefix}}}");
			checks++;
		}

		var hits = set.Where (s => s.Length == pos);
		if (hits.Any ()) {
			w.WriteLine ($"{prefix}{Elif (checks++)} (cookie[{pos}] == '\\0') {{");

			var h = hits.First ();
			w.WriteLine ($"{prefix}\t// found: {h} depth {pos + checks + depth}");
			w.WriteLine ($"{prefix}\twasm_invoke_{h.ToLower ()} (target_func, margs);");
			w.WriteLine ($"{prefix}\treturn;");
			w.WriteLine ($"{prefix}}}");
		}

		string Elif (int c) {
			return c == 0 ? "if" : "else if";
		}
	}

	class EmitCtx
	{
		int iarg, farg;

		public string Emit (char c) {
			switch (c) {
			case 'I':
				iarg += 1;
				return $"(int)(gssize)margs->iargs [{iarg - 1}]";
			case 'F':
				farg += 1;
				return $"*(float*)&margs->fargs [FIDX ({farg - 1})]";
			case 'L':
				iarg += 2;
				return $"get_long_arg (margs, {iarg - 2})";
			case 'D':
				farg += 1;
				return $"margs->fargs [FIDX ({farg - 1})]";
			default:
				throw new Exception ("IDK how to handle " + c);
			}
		}
	}
}
