// Mono.Util.CorCompare.MissingMethod
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
	/// 	Represents a class method that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingMethod : MissingMember 
	{
		// e.g. <method name="Equals" status="missing"/>
		public MissingMethod (MemberInfo infoMono, MemberInfo infoMS) : base (infoMono, infoMS) {}

		public override string Name {
			get {
				string s = Info.ToString();
				int index = s.IndexOf(' ');
				return s.Substring(index + 1);
			}
		}

		public override string Type {
			get {
				return "method";
			}
		}
	}

}
