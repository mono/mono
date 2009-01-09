//
// System.Reflection/CustomAttributeData.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
//   Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Reflection {

	[ComVisible (true)]
	[Serializable]
	public sealed class CustomAttributeData {
		ConstructorInfo ctorInfo;
		IList<CustomAttributeTypedArgument> ctorArgs;
		IList<CustomAttributeNamedArgument> namedArgs;

		internal CustomAttributeData (ConstructorInfo ctorInfo, object [] ctorArgs, object [] namedArgs)
		{
			this.ctorInfo = ctorInfo;
			
			this.ctorArgs = Array.AsReadOnly<CustomAttributeTypedArgument> 
				(ctorArgs != null ? UnboxValues<CustomAttributeTypedArgument> (ctorArgs) : new CustomAttributeTypedArgument [0]);
			
			this.namedArgs = Array.AsReadOnly<CustomAttributeNamedArgument> 
				(namedArgs != null ? UnboxValues<CustomAttributeNamedArgument> (namedArgs) : new CustomAttributeNamedArgument [0]);
		}

		[ComVisible (true)]
		public ConstructorInfo Constructor {
			get {
				return ctorInfo;
			}
		}

		[ComVisible (true)]
		public IList<CustomAttributeTypedArgument> ConstructorArguments {
			get {
				return ctorArgs;
			}
		}

		public IList<CustomAttributeNamedArgument> NamedArguments {
			get {
				return namedArgs;
			}
		}

		public static IList<CustomAttributeData> GetCustomAttributes (Assembly target) {
			return MonoCustomAttrs.GetCustomAttributesData (target);
		}

		public static IList<CustomAttributeData> GetCustomAttributes (MemberInfo target) {
			return MonoCustomAttrs.GetCustomAttributesData (target);
		}

		public static IList<CustomAttributeData> GetCustomAttributes (Module target) {
			return MonoCustomAttrs.GetCustomAttributesData (target);
		}

		public static IList<CustomAttributeData> GetCustomAttributes (ParameterInfo target) {
			return MonoCustomAttrs.GetCustomAttributesData (target);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("[" + ctorInfo.DeclaringType.FullName + "(");
			for (int i = 0; i < ctorArgs.Count; i++) {
				sb.Append (ctorArgs [i].ToString ());
				if (i + 1 < ctorArgs.Count)
					sb.Append (", ");
			}

			if (namedArgs.Count > 0)
				sb.Append (", ");
			
			for (int j = 0; j < namedArgs.Count; j++) {
				sb.Append (namedArgs [j].ToString ());
				if (j + 1 < namedArgs.Count)
					sb.Append (", ");
			}
			sb.AppendFormat (")]");

			return sb.ToString ();
		}

		static T [] UnboxValues<T> (object [] values)
		{
			T [] retval = new T [values.Length];
			for (int i = 0; i < values.Length; i++)
				retval [i] = (T) values [i];

			return retval;
		}

		public override bool Equals (object obj)
		{
			CustomAttributeData other = obj as CustomAttributeData;
			if (other == null || other.ctorInfo != ctorInfo ||
			    other.ctorArgs.Count != ctorArgs.Count ||
			    other.namedArgs.Count != namedArgs.Count)
				return false;
			for (int i = 0; i < ctorArgs.Count; i++)
				if (ctorArgs [i].Equals (other.ctorArgs [i]))
					return false;
			for (int i = 0; i < namedArgs.Count; i++) {
				bool matched = false;
				for (int j = 0; j < other.namedArgs.Count; j++)
					if (namedArgs [i].Equals (other.namedArgs [j])) {
						matched = true;
						break;
					}
				if (!matched)
					return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			int ret = ctorInfo.GetHashCode () << 16;
			// argument order-dependent
			for (int i = 0; i < ctorArgs.Count; i++)
				ret += ret ^ 7 + ctorArgs [i].GetHashCode () << (i * 4);
			// argument order-independent
			for (int i = 0; i < namedArgs.Count; i++)
				ret += (namedArgs [i].GetHashCode () << 5);
			return ret;
		}
	}

}

#endif

