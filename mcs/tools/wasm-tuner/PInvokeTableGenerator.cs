// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

	List<Pinvoke> pinvokes;
	List<PinvokeCallback> callbacks;

	public void Run (List<AssemblyDefinition> assemblies, Dictionary<string, string> modules) {
		CollectPInvokes (assemblies, out pinvokes, out callbacks);

		Console.WriteLine ("// GENERATED FILE, DO NOT MODIFY");
		Console.WriteLine ();

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
