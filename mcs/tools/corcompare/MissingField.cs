// Mono.Util.CorCompare.MissingField
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class event that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/24/2002 10:43:57 PM
	/// </remarks>
	class MissingField : IMissingMember {
		// e.g. <method name="Equals" status="missing"/>
		MemberInfo mInfo;

		public MissingField(MemberInfo info) {
			mInfo = info;
		}

		public string Name {
			get {
				return mInfo.Name;
			}
		}

		public virtual string Status {
			get {
				return "missing";
			}
		}

		public string Type {
			get {
				return "field";
			}
		}
	}
}
