using System;
using System.Reflection;
using System.Text;

namespace Mono.Doc.Utils
{
	/// <summary>
	/// Generates friendly names for types. 
	/// </summary>
	public sealed class NameGenerator
	{
		private NameGenerator()
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
