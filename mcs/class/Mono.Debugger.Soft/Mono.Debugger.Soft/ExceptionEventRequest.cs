using System;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	public sealed class ExceptionEventRequest : EventRequest {

		TypeMirror exc_type;
		bool caught, uncaught, subclasses, not_filtered_feature, everything_else;
		
		internal ExceptionEventRequest (VirtualMachine vm, TypeMirror exc_type, bool caught, bool uncaught, bool not_filtered_feature = false, bool everything_else = false) : base (vm, EventType.Exception) {
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
			this.not_filtered_feature = not_filtered_feature;
			this.everything_else = everything_else;
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
			mods.Add (new ExceptionModifier () { Type = exc_type != null ? exc_type.Id : 0, Caught = caught, Uncaught = uncaught, Subclasses = subclasses, NotFilteredFeature = not_filtered_feature, EverythingElse = everything_else });
			SendReq (mods);
		}
	}
}