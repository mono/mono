// Mono.Util.CorCompare.ToDoProperty
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class's property that is marked with a MonoTODO
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class ToDoProperty : MissingProperty 
	{
		// e.g. <property name="Count" status="todo" note="another note"/>
		string todoNote = "";

		public ToDoProperty(MemberInfo pInfo) : base(pInfo) {
		}
		public ToDoProperty(MemberInfo pInfo, string note) :base(pInfo) {
			todoNote = note;
		}
		public string Note {
			get {
				return todoNote;
			}
		}
		public override string Status {
			get {
				return "todo";
			}
		}
	}
}
