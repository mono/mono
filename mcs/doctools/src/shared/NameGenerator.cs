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
				TypeNames tn   = GetTypeNameForMemberInfo(m);
				string    type = null;

				switch (tn) {
					case TypeNames.Constructor:
						type = "C";
						break;
					case TypeNames.Event:
						type = "E";
						break;
					case TypeNames.Field:
						type = "F";
						break;
					case TypeNames.Method:
						type = "M";
						break;
					case TypeNames.Property:
						type = "P";
						break;
					case TypeNames.Type:
						type = "T";
						break;
					default:
						type = "!";
						break;
				}

				name.Append(type + ":");
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


		/// <summary>
		/// Determines the kind of type a given MemberInfo object is.
		/// </summary>
		/// <param name="m">Determine the kind of this member</param>
		/// <returns>An TypeNames enum specifying the kind of this member</returns>
		public static TypeNames GetTypeNameForMemberInfo(MemberInfo m)
		{
			if      (m is EventInfo)       return TypeNames.Event;
			else if (m is FieldInfo)       return TypeNames.Field;
			else if (m is MethodInfo)      return TypeNames.Method;
			else if (m is ConstructorInfo) return TypeNames.Constructor;
			else if (m is PropertyInfo)    return TypeNames.Property;
			else if (m is Type)            return TypeNames.Type;
			else                           return TypeNames.UNKNOWN;
		}
	}
}
