// Mono.Util.CorCompare.ToDoMethod
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class method that is marked with MonoTODO
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class ToDoMethod : MissingMethod 
	{
		// e.g. <method name="ToString" status="todo" note="this is the note from MonoTODO"/>
		string todoNote = "";

		public ToDoMethod(MemberInfo info) : base(info) {
		}
		public ToDoMethod(MemberInfo info, string note) :base(info) {
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
