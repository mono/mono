// Mono.Util.CorCompare.MissingConstructor
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using Mono.Cecil;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class event that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/24/2002 10:43:57 PM
	/// </remarks>
	class MissingConstructor : MissingMethod {
		// e.g. <method name="Equals" status="missing"/>
		public MissingConstructor (MethodDefinition infoMono, MethodDefinition infoMS) : base (infoMono, infoMS) { }

		public override string Type {
			get {
				return "constructor";
			}
		}
	}
}
