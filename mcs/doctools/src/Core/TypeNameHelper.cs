// TypeNameHelper.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

using System;
using System.Reflection;
using System.Text;

namespace Mono.Doc.Core
{
	/// <summary>
	/// Generates friendly names for types. 
	/// </summary>
	public sealed class TypeNameHelper
	{
		private TypeNameHelper()
		{
			// can't instantiate this class
		}


		/// <summary>
		/// Given a MemberInfo object, creates a friendly string name without
		/// full qualifiers or a type prefix.
		/// </summary>
		/// <param name="m">The MemberInfo to name</param>
		/// <returns>A friendly name</returns>
		public static string GetNameForMemberInfo(MemberInfo m) {
			return GetNameForMemberInfo(m, NamingFlags.None);
		}


		/// <summary>
		/// Given a MemberInfo object, creates a friendly string name.  
		/// </summary>
		/// <param name="m">The MemberInfo to name</param>
		/// <param name="flags">NamingFlags can be combined to alter the output</param>
		/// <returns>A friendly name</returns>
		public static string GetNameForMemberInfo(MemberInfo m, NamingFlags flags)
		{
			StringBuilder name = new StringBuilder();
			
			// type specifier
			if ((flags & NamingFlags.TypeSpecifier) != 0) {
				// append a type specifier to this name
				string    type = null;

				if (m is ConstructorInfo) {
					type = "C";
				} else if (m is EventInfo) {
					type = "E";
				} else if (m is FieldInfo) {
					type = "F";
				} else if (m is PropertyInfo) {
					type = "P";
				} else if (m is Type) {
					type = "T";
				} else {
					type = "!";
				}

				name.Append(type + ":");
			}

			// first-class types
			if (m.DeclaringType == null && m is Type) {
				return name.Append((m as Type).FullName).ToString();
			}

			// full name
			if (((flags & NamingFlags.FullName) != 0) && m.DeclaringType != null) {
				name.Append(m.DeclaringType.FullName + ".");
			}

			// normal name
			name.Append(m.Name.Replace(".", "#")); // for #ctor
		
			// for methods and constructors, params are part of the name
			if (m is MethodBase) {
				MethodBase      method     = m as MethodBase;
				ParameterInfo[] parameters = method.GetParameters();
				
				if (parameters.Length > 0) {
					bool first = true;
					name.Append("(");

					foreach (ParameterInfo p in parameters) {
						if (!first) name.Append(",");
						first = false;
						name.Append(p.ParameterType.FullName);
					}

					name.Append(")");
				} else if ((flags & NamingFlags.ForceMethodParams) != 0) {
					name.Append("()");
				}
			}

			return name.ToString();
		}


		public static string GetKindNameForType(Type t)
		{
			string kind = null;

			if (t.IsClass) {
				kind = "class";
			} else if (t.IsInterface) {
				kind = "interface";
			} else if (t.IsValueType) {
				if (t.BaseType.FullName == "System.Enum") {
					kind = "enum";
				} else {
					kind = "struct";
				}
			} else {
				kind = "UNKNOWN";
			}

			return kind;
		}
	}
}
