// Mono.Util.CorCompare.MissingType
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class method that missing.
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingType 
	{
		// e.g. <class name="System.Byte" status="missing"/>
		protected Type theType;
		public MissingType(Type t) {
			theType = t;
		}

		public override bool Equals(object o) {
			if (o is MissingType) {
				return o.GetHashCode() == this.GetHashCode();
			}
			return false;
		}

		public override int GetHashCode() {
			return theType.GetHashCode();
		}

		public string Name {
			get {
				return theType.Name;
			}
		}

		public string NameSpace {
			get {
				return theType.Namespace;
			}
		}

		public virtual string Status {
			get {
				return "missing";
			}
		}
	}
}
