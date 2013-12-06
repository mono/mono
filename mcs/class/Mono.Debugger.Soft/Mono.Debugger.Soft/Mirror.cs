using System;

namespace Mono.Debugger.Soft
{
	public abstract class Mirror : IMirror
	{
		protected VirtualMachine vm;
		protected long id; // The id used in the protocol

		internal Mirror (VirtualMachine vm, long id) {
			this.vm = vm;
			this.id = id;
		}

		internal Mirror () {
		}

		public VirtualMachine VirtualMachine {
			get {
				return vm;
			}
		}

		internal long Id {
			get {
				return id;
			}
		}

		protected void SetVirtualMachine (VirtualMachine vm) {
			this.vm = vm;
		}

		protected void CheckMirror (Mirror m) {
			if (vm != m.VirtualMachine)
				throw new VMMismatchException ();
		}

		public override bool Equals (object obj)
		{
			var mirror = obj as Mirror;
			if (mirror == null)
				return false;

			return id == mirror.Id && vm == mirror.VirtualMachine;
		}

		public override int GetHashCode ()
		{
			return (int) id;
		}
	}
}
