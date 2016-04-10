//
// System.Diagnostics.StackTraceHelper.cs
//
// Author:
//      Marcos Henrich (marcos.henrich@xamarin.com)
//
// Copyright (C) Xamarin, Inc (http://www.xamarin.com)
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

namespace System.Diagnostics {

	using System.Text;
#if INSIDE_CORLIB
	using System.Reflection;
#else
	using IKVM.Reflection;
	using IKVM.Reflection.Reader;
	using Type = IKVM.Reflection.Type;
#endif
	// This class exists so tools such as mono-symbolicate can use it directly.
	class StackTraceHelper {
		
		public static void GetFullNameForStackTrace (StringBuilder sb, MethodBase mi)
		{
			var declaringType = mi.DeclaringType;
			if (declaringType.IsGenericType && !declaringType.IsGenericTypeDefinition)
				declaringType = declaringType.GetGenericTypeDefinition ();

			// Get generic definition
			var bindingflags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			foreach (var m in declaringType.GetMethods (bindingflags)) {
				if (m.MetadataToken == mi.MetadataToken) {
					mi = m;
					break;
				}
			}

			sb.Append (declaringType.ToString ());

			sb.Append (".");
			sb.Append (mi.Name);

			if (mi.IsGenericMethod) {
				Type[] gen_params = mi.GetGenericArguments ();
				sb.Append ("[");
				for (int j = 0; j < gen_params.Length; j++) {
					if (j > 0)
						sb.Append (",");
					sb.Append (gen_params [j].Name);
				}
				sb.Append ("]");
			}

			ParameterInfo[] p = mi.GetParameters ();

			sb.Append (" (");
			for (int i = 0; i < p.Length; ++i) {
				if (i > 0)
					sb.Append (", ");

				Type pt = p[i].ParameterType;
				if (pt.IsGenericType && ! pt.IsGenericTypeDefinition)
					pt = pt.GetGenericTypeDefinition ();

				sb.Append (pt.ToString());

				if (p [i].Name != null) {
					sb.Append (" ");
					sb.Append (p [i].Name);
				}
			}
			sb.Append (")");
		}
	}
}
