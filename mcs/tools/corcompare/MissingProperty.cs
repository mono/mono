// Mono.Util.CorCompare.MissingProperty
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a missing property from a class
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingProperty 
	{
		// e.g. <property name="Length" status="missing"/>
		MemberInfo info;

		public MissingProperty(MemberInfo pInfo) {
			info = pInfo;
		}

		public string Name {
			get {
				return info.Name;
			}
		}
		public virtual string Status {
			get {
				return "missing";
			}
		}
	}
}
