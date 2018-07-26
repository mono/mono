using System;
using System.Collections.Generic;
using System.Reflection;

using SimpleJit.Metadata;

namespace Mono.Compiler
{
	public class ClassInfo
	{
		internal TypeInfo type;

		public string Name { get => type.FullName; }

		ClassInfo (TypeInfo type)
		{
			this.type = type;
		}

		public static ClassInfo FromType (Type t)
		{
			var ci = new ClassInfo (t.GetTypeInfo ());
			return ci;
		}

		public MethodInfo GetMethodInfoFor (string methodName)
		{
			/* FIXME: methodName is not enough to uniquely determine a method */
			var m = type.GetMethod (methodName);
			return GetMethodInfoFor (m, methodName);
		}

		MethodInfo GetMethodInfoFor (System.Reflection.MethodInfo m, string methodName)
		{
			var parameters = ParametersFromReflection (m.GetParameters ());
			var srBody = m.GetMethodBody ();
			var bodyBytes = srBody.GetILAsByteArray ();
			var maxStack = srBody.MaxStackSize;
			var initLocals = srBody.InitLocals;
			var localsToken = srBody.LocalSignatureMetadataToken;
			var locals = LocalVariableInfo (srBody.LocalVariables);
			var body = new SimpleJit.Metadata.MethodBody (bodyBytes, maxStack, initLocals, localsToken, locals);
			return new MethodInfo (this, methodName, body, m.MethodHandle, RuntimeInformation.ClrTypeFromType (m.ReturnType), parameters);
		}

		IList<SimpleJit.Metadata.LocalVariableInfo> LocalVariableInfo (IList<System.Reflection.LocalVariableInfo> locals)
		{
			var res = new SimpleJit.Metadata.LocalVariableInfo[locals.Count];
			int i = 0;
			foreach (var l in locals) {
				var t = RuntimeInformation.ClrTypeFromType (l.LocalType);
				res[i++] = new SimpleJit.Metadata.LocalVariableInfo (t, l.LocalIndex);
			}
			return res;
		}

		IReadOnlyList<SimpleJit.Metadata.ParameterInfo> ParametersFromReflection (System.Reflection.ParameterInfo[] ps)
		{
			var res = new SimpleJit.Metadata.ParameterInfo[ps.Length];
			int i = 0;
			foreach  (var p in ps) {
				var t = RuntimeInformation.ClrTypeFromType (p.ParameterType);
				res[i++] = new SimpleJit.Metadata.ParameterInfo (t, p.Position);
			}
			return res;
		}

	}
}
