using System;
using System.Collections.Generic;

namespace Mono.Debugger
{
	public sealed class ExceptionEventRequest : EventRequest {

		TypeMirror exc_type;
		
		internal ExceptionEventRequest (VirtualMachine vm, TypeMirror exc_type) : base (vm, EventType.Exception) {
			if (exc_type != null) {
				CheckMirror (vm, exc_type);
				TypeMirror exception_type = vm.RootDomain.Corlib.GetType ("System.Exception", false, false);
				if (!exception_type.IsAssignableFrom (exc_type))
					throw new ArgumentException ("The exception type does not inherit from System.Exception", "exc_type");
			}
			this.exc_type = exc_type;
		}

		public TypeMirror ExceptionType {
			get {
				return exc_type;
			}
		}

		public override void Enable () {
			var mods = new List <Modifier> ();
			mods.Add (new ExceptionModifier () { Type = exc_type != null ? exc_type.Id : 0 });
			SendReq (mods);
		}
	}
}