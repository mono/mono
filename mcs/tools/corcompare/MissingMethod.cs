// Mono.Util.CorCompare.MissingMethod
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class method that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingMethod 
	{
		// e.g. <method name="Equals" status="missing"/>
		MethodInfo mInfo;

		public MissingMethod(MethodInfo info) {
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
	}
}
