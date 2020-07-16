// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Json;
using System.Collections.Generic;
using Mono.Cecil;

public class PInvokeTableGenerator
{
	public PInvokeTableGenerator () {
	}

	List<AssemblyDefinition> assemblies;
	Dictionary<string, string> modules;
	List<Pinvoke> pinvokes;
	List<PinvokeCallback> callbacks;

	public void Run (List<AssemblyDefinition> assemblies, Dictionary<string, string> modules) {
		this.assemblies = assemblies;
		this.modules = modules;

		CollectPInvokes (assemblies, out pinvokes, out callbacks);

		Console.WriteLine ("// GENERATED FILE, DO NOT MODIFY");
		Console.WriteLine ();

		GenPInvokeTable ();
		GenNativeToInterp ();
	}

	void GenPInvokeTable () {
		var decls = new Dictionary<string, Pinvoke> ();
		foreach (var pinvoke in pinvokes) {
			if (modules.ContainsKey (pinvoke.Module)) {
				pinvoke.CDecl = GenPinvokeDecl (pinvoke);
				if (decls.TryGetValue (pinvoke.EntryPoint, out Pinvoke prev_pinvoke)) {
					if (pinvoke.CDecl != prev_pinvoke.CDecl) {
						Console.Error.WriteLine ($"Warning: PInvoke method '{pinvoke.EntryPoint}' has incompatible declarations.");
						pinvoke.TableEntryPoint = "mono_wasm_pinvoke_vararg_stub";
						prev_pinvoke.TableEntryPoint = "mono_wasm_pinvoke_vararg_stub";
						continue;
					}
				}
				decls [pinvoke.EntryPoint] = pinvoke;
				Console.WriteLine (pinvoke.CDecl);
			}
		}

		foreach (var module in modules.Keys) {
			string symbol = module.Replace (".", "_") + "_imports";
			Console.WriteLine ("static PinvokeImport " + symbol + " [] = {");
			foreach (var pinvoke in pinvokes) {
				if (pinvoke.Module == module)
					Console.WriteLine ("{\"" + pinvoke.EntryPoint + "\", " + pinvoke.TableEntryPoint + "},");
			}
			Console.WriteLine ("{NULL, NULL}");
			Console.WriteLine ("};");
		}
		Console.Write ("static void *pinvoke_tables[] = { ");
		foreach (var module in modules.Keys) {
			string symbol = module.Replace (".", "_") + "_imports";
			Console.Write (symbol + ",");
		}
		Console.WriteLine ("};");
		Console.Write ("static char *pinvoke_names[] = { ");
		foreach (var module in modules.Keys) {
			Console.Write ("\"" + module + "\"" + ",");
		}
		Console.WriteLine ("};");
	}

	void GenNativeToInterp () {
		// Generate native->interp entry functions
		// These are called by native code, so they need to obtain
		// the interp entry function/arg from a global array
		// They also need to have a signature matching what the
		// native code expects, which is the native signature
		// of the delegate invoke in the [MonoPInvokeCallback]
		// attribute.
		// Only blittable parameter/return types are supposed.
		int cb_index = 0;

		// Arguments to interp entry functions in the runtime
		Console.WriteLine ("InterpFtnDesc wasm_native_to_interp_ftndescs[" + callbacks.Count + "];");

		foreach (var cb in callbacks) {
			var method = cb.Method;

			if (!IsBlittable (method.ReturnType))
				Error ("The return type of pinvoke callback method '" + method.FullName + "' needs to be blittable.");
			foreach (var p in method.Parameters) {
				if (!IsBlittable (p.ParameterType))
					Error ("Parameter types of pinvoke callback method '" + method.FullName + "' needs to be blittable.");
			}
		}

		foreach (var cb in callbacks) {
			var sb = new StringBuilder ();
			var method = cb.Method;

			// The signature of the interp entry function
			// This is a gsharedvt_in signature
			sb.Append ("typedef void ");
			sb.Append (" (*WasmInterpEntrySig_" + cb_index + ") (");
			int pindex = 0;
			if (method.ReturnType.Name != "Void") {
				sb.Append ("int");
				pindex ++;
			}
			foreach (var p in method.Parameters) {
				if (pindex > 0)
					sb.Append (",");
				sb.Append ("int");
				pindex ++;
			}
			if (pindex > 0)
				sb.Append (",");
			// Extra arg
			sb.Append ("int");
			sb.Append (");\n");

			bool is_void = method.ReturnType.Name == "Void";

			string module_symbol = method.DeclaringType.Module.Assembly.Name.Name.Replace (".", "_");
			var token = method.MetadataToken.ToUInt32 ();
			string entry_name = $"wasm_native_to_interp_{module_symbol}_{token}";
			cb.EntryName = entry_name;
			sb.Append (WasmTuner.MapType (method.ReturnType));
			sb.Append ($" {entry_name} (");
			pindex = 0;
			foreach (var p in method.Parameters) {
				if (pindex > 0)
					sb.Append (",");
				sb.Append (WasmTuner.MapType (method.Parameters [pindex].ParameterType));
				sb.Append (" arg" + pindex);
				pindex ++;
			}
			sb.Append (") { \n");
			if (!is_void)
				sb.Append (WasmTuner.MapType (method.ReturnType) + " res;\n");
			sb.Append ("((WasmInterpEntrySig_" + cb_index + ")wasm_native_to_interp_ftndescs [" + cb_index + "].func) (");
			pindex = 0;
			if (!is_void) {
				sb.Append ("&res");
				pindex ++;
			}
			int aindex = 0;
			foreach (var p in method.Parameters) {
				if (pindex > 0)
					sb.Append (", ");
				sb.Append ("&arg" + aindex);
				pindex ++;
				aindex ++;
			}
			if (pindex > 0)
				sb.Append (", ");
			sb.Append ($"wasm_native_to_interp_ftndescs [{cb_index}].arg");
			sb.Append (");\n");
			if (!is_void)
				sb.Append ("return res;\n");
			sb.Append ("}");
			Console.WriteLine (sb);
			cb_index ++;
		}

		// Array of function pointers
		Console.Write ("static void *wasm_native_to_interp_funcs[] = { ");
		foreach (var cb in callbacks) {
			Console.Write (cb.EntryName + ",");
		}
		Console.WriteLine ("};");

		// Lookup table from method->interp entry
		// The key is a string of the form <assembly name>_<method token>
		// FIXME: Use a better encoding
		Console.Write ("static const char *wasm_native_to_interp_map[] = { ");
		foreach (var cb in callbacks) {
			var method = cb.Method;
			string module_symbol = method.DeclaringType.Module.Assembly.Name.Name.Replace (".", "_");
			var token = method.MetadataToken.ToUInt32 ();
			Console.WriteLine ($"\"{module_symbol}_{token}\",");
		}
		Console.WriteLine ("};");
	}

	static bool IsBlittable (TypeReference type) {
		switch (type.MetadataType) {
		case MetadataType.Void:
		case MetadataType.Char:
		case MetadataType.Boolean:
		case MetadataType.Byte:
		case MetadataType.SByte:
		case MetadataType.Int16:
		case MetadataType.UInt16:
		case MetadataType.Int32:
		case MetadataType.UInt32:
		case MetadataType.Int64:
		case MetadataType.UInt64:
		case MetadataType.IntPtr:
		case MetadataType.UIntPtr:
		case MetadataType.Single:
		case MetadataType.Double:
		case MetadataType.Pointer:
		case MetadataType.ByReference:
			return true;
		case MetadataType.ValueType: {
			var tdef = type.Resolve ();
			return tdef != null && tdef.IsEnum;
		}
		default:
			return false;
		}
	}

	static void Error (string msg) {
		Console.Error.WriteLine (msg);
		Environment.Exit (1);
	}

	public static void CollectPInvokes (List<AssemblyDefinition> assemblies, out List<Pinvoke> pinvokes, out List<PinvokeCallback> callbacks) {
		pinvokes = new List<Pinvoke> ();
		callbacks = new List<PinvokeCallback> ();
		foreach (var assembly in assemblies) {
			foreach (var type in assembly.MainModule.Types) {
				ProcessTypeForPinvoke (pinvokes, callbacks, type);
				foreach (var nested in type.NestedTypes)
					ProcessTypeForPinvoke (pinvokes, callbacks, nested);
			}
		}
	}

	static void ProcessTypeForPinvoke (List<Pinvoke> pinvokes, List<PinvokeCallback> callbacks, TypeDefinition type) {
		foreach (var method in type.Methods) {
			var info = method.PInvokeInfo;
			if (info != null) {
				pinvokes.Add (new Pinvoke (info.EntryPoint, info.Module.Name, method));
			}

			foreach (var attr in method.CustomAttributes) {
				// The type has no defined namespace
				if (attr.Constructor.DeclaringType.Name == "MonoPInvokeCallbackAttribute") {
					if (!method.IsStatic)
						Error ($"Method '{method.FullName}' decored with [MonoPInvokeCallback] needs to be static.");
					var callback_type = attr.ConstructorArguments [0];
					if (callback_type.Type.Name != "Type" || callback_type.Value == null)
						Error ("[MonoPInvokeCallback] attribute has invalid format.");
					var tref = (TypeReference)callback_type.Value;
					callbacks.Add (new PinvokeCallback (method, tref));
				} else if (attr.Constructor.DeclaringType.Name == "UnmanagedCallersOnlyAttribute") {
					callbacks.Add (new PinvokeCallback (method, null));
				}
			}
		}
	}

	static string GenPinvokeDecl (Pinvoke pinvoke) {
		var sb = new StringBuilder ();
		var method = pinvoke.Method;
		sb.Append (WasmTuner.MapType (method.ReturnType));
		sb.Append ($" {pinvoke.EntryPoint} (");
		int pindex = 0;
		foreach (var p in method.Parameters) {
			if (pindex > 0)
				sb.Append (",");
			sb.Append (WasmTuner.MapType (method.Parameters [pindex].ParameterType));
			pindex ++;
		}
		sb.Append (");");
		return sb.ToString ();
	}
}

public class Pinvoke
{
	public Pinvoke (string entry_point, string module, MethodReference method) {
		EntryPoint = entry_point;
		TableEntryPoint = entry_point;
		Module = module;
		Method = method;
	}

	public string EntryPoint;
	public string TableEntryPoint;
	public string Module;
	public MethodReference Method;
	public string CDecl;
}

public class PinvokeCallback
{
	public PinvokeCallback (MethodReference method, TypeReference callback_type) {
		Method = method;
		CallbackType = callback_type;
	}

	public MethodReference Method;
	public TypeReference CallbackType;
	public string EntryName;
}
