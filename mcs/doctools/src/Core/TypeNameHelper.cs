// TypeNameHelper.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
//
// This file is part of Monodoc, a multilingual API documentation tool.
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

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
		public static string GetName(MemberInfo m) {
			return GetName(m, NamingFlags.None);
		}


		/// <summary>
		/// Given a MemberInfo object, creates a friendly string name.  
		/// </summary>
		/// <param name="m">The MemberInfo to name</param>
		/// <param name="flags">NamingFlags can be combined to alter the output</param>
		/// <returns>A friendly name</returns>
		public static string GetName(MemberInfo m, NamingFlags flags)
		{
			StringBuilder name = new StringBuilder();
			
			// type specifier
			if ((flags & NamingFlags.TypeSpecifier) != 0) {
				// append a type specifier to this name
				string    type = null;

				if (m is ConstructorInfo) {
					type = "C";
				} else if (m is MethodInfo) {
					type = "M";
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
				
				if (parameters.Length > 0 && ((flags & NamingFlags.HideMethodParams) == 0)) {
					bool first = true;
					name.Append("(");

					foreach (ParameterInfo p in parameters) {
						if (!first) name.Append(",");
						first = false;
						name.Append(((flags & NamingFlags.ShortParamTypes) != 0) ? 
							p.ParameterType.Name : p.ParameterType.FullName);
					}

					name.Append(")");
				} else if ((flags & NamingFlags.ForceMethodParams) != 0) {
					name.Append("()");
				}
			}

			return name.ToString();
		}
	}
}
