using System;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	public class ExceptionEventRequest : EventRequest {

		protected TypeMirror exc_type;
		protected bool caught, uncaught, subclasses;
		
		internal ExceptionEventRequest (VirtualMachine vm, TypeMirror exc_type, bool caught, bool uncaught) : base (vm, EventType.Exception) {
			if (exc_type != null) {
				CheckMirror (vm, exc_type);
				TypeMirror exception_type = vm.RootDomain.Corlib.GetType ("System.Exception", false, false);
				if (!exception_type.IsAssignableFrom (exc_type))
					throw new ArgumentException ("The exception type does not inherit from System.Exception", "exc_type");
			}
			this.exc_type = exc_type;
			this.caught = caught;
			this.uncaught = uncaught;
			this.subclasses = true;
		}

		public TypeMirror ExceptionType {
			get {
				return exc_type;
			}
		}

		// Defaults to true
		// Supported since protocol version 2.25
		public bool IncludeSubclasses {
			get {
				return subclasses;
			}
			set {
				vm.CheckProtocolVersion (2, 25);
				subclasses = value;
			}
		}

		public override void Enable () {
			var mods = new List <Modifier> ();
			mods.Add (new ExceptionModifier () { Type = exc_type != null ? exc_type.Id : 0, Caught = caught, Uncaught = uncaught, Subclasses = subclasses });
			SendReq (mods);
		}
	}

	public sealed class ExceptionEventRequest2 : ExceptionEventRequest {

		bool everything_else;
		
		internal ExceptionEventRequest2 (VirtualMachine vm, TypeMirror exc_type, bool caught, bool uncaught, bool everything_else) : base (vm, exc_type, caught, uncaught) {
			this.everything_else = everything_else;
		}

		public override void Enable () {
			var mods = new List <Modifier> ();
			mods.Add (new ExceptionModifier2 () { Type = exc_type != null ? exc_type.Id : 0, Caught = caught, Uncaught = uncaught, Subclasses = subclasses, EverythingElse = everything_else });
			SendReq (mods);
		}
	}
}