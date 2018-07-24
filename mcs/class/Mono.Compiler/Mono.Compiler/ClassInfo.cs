using System;
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
			var bodyBytes = m.GetMethodBody ().GetILAsByteArray ();
			var body = new SimpleJit.Metadata.MethodBody (bodyBytes);
			return new MethodInfo (this, methodName, body);
		}

	}
}
