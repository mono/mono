// Mono.Util.CorCompare.MissingProperty
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;
using System.Text;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a missing property from a class
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingProperty : IMissingMember 
	{
		// e.g. <property name="Length" status="missing"/>
		MemberInfo info;

		public MissingProperty(MemberInfo pInfo) {
			info = pInfo;
		}

		public string Name {
			get {
				StringBuilder retVal = new StringBuilder(info.Name + "{");
				if (this.NeedsGet) {
					retVal.Append(" get;");
				}
				if (this.NeedsSet) {
					retVal.Append(" set;");
				}

				retVal.Append(" }");
				return retVal.ToString();
			}
		}
		public virtual string Status {
			get {
				return "missing";
			}
		}

		public string Type {
			get {
				return "property";
			}
		}

		public bool NeedsGet = false;
		public bool NeedsSet = false;
	}
}
