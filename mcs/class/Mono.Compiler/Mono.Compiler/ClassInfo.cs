using System;
using System.Reflection;

using SimpleJit.Metadata;

namespace Mono.Compiler
{
	public class ClassInfo
	{
		internal Type type;

		public string Name { get => type.FullName; }

		public static ClassInfo FromType (Type t)
		{
			var ci = new ClassInfo ();
			ci.type = t;
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
