using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Debugger.Soft
{
	public abstract class EventRequest {
		protected int id;
		protected EventType etype;
		protected bool enabled;
		protected VirtualMachine vm;
		protected SuspendPolicy suspend;
		protected int count;
		protected ThreadMirror thread;
		protected IList<AssemblyMirror> assembly_filter;

		internal EventRequest (VirtualMachine vm, EventType etype) {
			this.vm = vm;
			this.etype = etype;
			this.suspend = SuspendPolicy.All;
		}

		internal EventRequest (EventType etype, int id) {
			this.id = id;
			this.etype = etype;
		}

		internal int Id {
			get {
				return id;
			}
			set {
				id = value;
			}
		}

		public int GetId()
		{
			return id;
		}

		public EventType EventType {
			get {
				return etype;
			}
		}

		public bool Enabled {
			get {
				return enabled;
			}
			set {
				if (value != enabled) {
					if (value)
						Enable ();
					else
						Disable ();
				}
			}
		}

		public int Count {
			get {
				return count;
			}
			set {
				CheckDisabled ();
				count = value;
			}
		}

		public ThreadMirror Thread {
			get {
				return thread;
			}
			set {
				CheckDisabled ();
				if (value != null && value.VirtualMachine != vm)
					throw new VMMismatchException ();
				thread = value;
			}
		}

		public IList<AssemblyMirror> AssemblyFilter {
			get {
				return assembly_filter;
			}
			set {
				CheckDisabled ();
				if (value != null) {
					foreach (var ass in value)
						if (ass == null)
							throw new ArgumentException ("one of the elements of the array is null.");
				}
				assembly_filter = value;
			}
		}

		/*
		 * Every time an EventRequest object is enabled, a new JDWP event request
		 * is created, and the event request's id changes.
		 */
		internal void SendReq (List<Modifier> mods) {
			if (!enabled) {
				if (Count > 0)
					mods.Add (new CountModifier () { Count = Count });
				if (Thread != null)
					mods.Add (new ThreadModifier () { Thread = Thread.Id });
				if (AssemblyFilter != null)
					mods.Add (new AssemblyModifier () { Assemblies = AssemblyFilter.Select (x => x.Id ).ToArray () });
				id = vm.conn.EnableEvent (EventType, suspend, mods);
				SetEnabled (id);
			}
		}
				
		public virtual void Enable () {
			SendReq (new List<Modifier> ());
		}

		public void Disable () {
			if (enabled) {
				vm.conn.ClearEventRequest (etype, id);
				enabled = false;
				// FIXME: This causes problems because Events can still reference
				// the old id
				//vm.RemoveRequest (this, id);
				id = -1;
			}
		}

		protected void SetEnabled (int id) {
			this.id = id;
			enabled = true;
			vm.AddRequest (this, id);
		}

		protected void CheckDisabled () {
			if (Enabled)
				throw new InvalidOperationException ("Request objects can only be modified while they are disabled.");
		}

		protected void CheckMirror (VirtualMachine vm, Mirror m) {
			if (vm != m.VirtualMachine)
				throw new VMMismatchException ();
		}
	}
}