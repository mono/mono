using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Metadata;

namespace Mono.Debugger.Soft
{
	public class VirtualMachine : Mirror
	{
		Queue queue;
		object queue_monitor;
		object startup_monitor;
		AppDomainMirror root_domain;
		Dictionary<int, EventRequest> requests;
		ITargetProcess process;

		internal Connection conn;

		VersionInfo version;

		internal VirtualMachine (ITargetProcess process, Connection conn) : base () {
			SetVirtualMachine (this);
			queue = new Queue ();
			queue_monitor = new Object ();
			startup_monitor = new Object ();
			requests = new Dictionary <int, EventRequest> ();
			this.conn = conn;
			this.process = process;
			conn.ErrorHandler += ErrorHandler;
		}

		// The standard output of the process is available normally through Process
		public StreamReader StandardOutput { get; set; }
		public StreamReader StandardError { get; set; }

		
		public Process Process {
			get {
				ProcessWrapper pw = process as ProcessWrapper;
				if (pw == null)
				    throw new InvalidOperationException ("Process instance not available");
				return pw.Process;
			}
		}

		public ITargetProcess TargetProcess {
			get {
				return process;
			}
		}

		public AppDomainMirror RootDomain {
			get {
				return root_domain;
			}
	    }

		public EndPoint EndPoint {
			get {
				return conn.EndPoint;
			}
		}

		public VersionInfo Version {
			get {
				return version;
			}
		}

		EventSet current_es;
		int current_es_index;

		/*
		 * It is impossible to determine when to resume when using this method, since
		 * the debuggee is suspended only once per event-set, not event.
		 */
		[Obsolete ("Use GetNextEventSet () instead")]
		public Event GetNextEvent () {
			lock (queue_monitor) {
				if (current_es == null || current_es_index == current_es.Events.Length) {
					if (queue.Count == 0)
						Monitor.Wait (queue_monitor);
					current_es = (EventSet)queue.Dequeue ();
					current_es_index = 0;
				}
				return current_es.Events [current_es_index ++];
			}
		}

		public Event GetNextEvent (int timeout) {
			throw new NotImplementedException ();
		}

		public EventSet GetNextEventSet () {
			lock (queue_monitor) {
				if (queue.Count == 0)
					Monitor.Wait (queue_monitor);

				current_es = null;
				current_es_index = 0;

				return (EventSet)queue.Dequeue ();
			}
		}

		[Obsolete ("Use GetNextEventSet () instead")]
		public T GetNextEvent<T> () where T : Event {
			return GetNextEvent () as T;
		}

		public void Suspend () {
			conn.VM_Suspend ();
	    }

		public void Resume () {
			try {
				conn.VM_Resume ();
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.NOT_SUSPENDED)
					throw new InvalidOperationException ("The vm is not suspended.");
				else
					throw;
			}
	    }

		public void Exit (int exitCode) {
			conn.VM_Exit (exitCode);
		}

		public void Dispose () {
			conn.VM_Dispose ();
			conn.Close ();
			notify_vm_event (EventType.VMDisconnect, SuspendPolicy.None, 0, 0, null);
		}

		public IList<ThreadMirror> GetThreads () {
			long[] ids = vm.conn.VM_GetThreads ();
			ThreadMirror[] res = new ThreadMirror [ids.Length];
			for (int i = 0; i < ids.Length; ++i)
				res [i] = GetThread (ids [i]);
			return res;
		}

		// Same as the mirrorOf methods in JDI
		public PrimitiveValue CreateValue (object value) {
			if (value == null)
				return new PrimitiveValue (vm, null);

			if (!value.GetType ().IsPrimitive)
				throw new ArgumentException ("value must be of a primitive type instead of '" + value.GetType () + "'", "value");

			return new PrimitiveValue (vm, value);
		}

		public EnumMirror CreateEnumMirror (TypeMirror type, PrimitiveValue value) {
			return new EnumMirror (this, type, value);
		}

		//
		// Enable send and receive timeouts on the connection and send a keepalive event
		// every 'keepalive_interval' milliseconds.
		//

		public void SetSocketTimeouts (int send_timeout, int receive_timeout, int keepalive_interval)
		{
			conn.SetSocketTimeouts (send_timeout, receive_timeout, keepalive_interval);
		}

		//
		// Methods to create event request objects
		//
		public BreakpointEventRequest CreateBreakpointRequest (MethodMirror method, long il_offset) {
			return new BreakpointEventRequest (this, method, il_offset);
		}

		public BreakpointEventRequest CreateBreakpointRequest (Location loc) {
			if (loc == null)
				throw new ArgumentNullException ("loc");
			CheckMirror (loc);
			return new BreakpointEventRequest (this, loc.Method, loc.ILOffset);
		}

		public StepEventRequest CreateStepRequest (ThreadMirror thread) {
			return new StepEventRequest (this, thread);
		}

		public MethodEntryEventRequest CreateMethodEntryRequest () {
			return new MethodEntryEventRequest (this);
		}

		public MethodExitEventRequest CreateMethodExitRequest () {
			return new MethodExitEventRequest (this);
		}

		public ExceptionEventRequest CreateExceptionRequest (TypeMirror exc_type) {
			return new ExceptionEventRequest (this, exc_type, true, true);
		}

		public ExceptionEventRequest CreateExceptionRequest (TypeMirror exc_type, bool caught, bool uncaught) {
			return new ExceptionEventRequest (this, exc_type, caught, uncaught);
		}

		public AssemblyLoadEventRequest CreateAssemblyLoadRequest () {
			return new AssemblyLoadEventRequest (this);
		}

		public void EnableEvents (params EventType[] events) {
			foreach (EventType etype in events) {
				if (etype == EventType.Breakpoint)
					throw new ArgumentException ("Breakpoint events cannot be requested using EnableEvents", "events");
				conn.EnableEvent (etype, SuspendPolicy.All, null);
			}
		}

		public BreakpointEventRequest SetBreakpoint (MethodMirror method, long il_offset) {
			BreakpointEventRequest req = CreateBreakpointRequest (method, il_offset);

			req.Enable ();

			return req;
		}

		public void ClearAllBreakpoints () {
			conn.ClearAllBreakpoints ();
		}
		
		public void Disconnect () {
			conn.Close ();
		}
		
		internal void queue_event_set (EventSet es) {
			lock (queue_monitor) {
				queue.Enqueue (es);
				Monitor.Pulse (queue_monitor);
			}
		}

		internal void ErrorHandler (object sender, ErrorHandlerEventArgs args) {
			switch (args.ErrorCode) {
			case ErrorCode.INVALID_OBJECT:
				throw new ObjectCollectedException ();
			case ErrorCode.INVALID_FRAMEID:
				throw new InvalidStackFrameException ();
			case ErrorCode.NOT_SUSPENDED:
				throw new InvalidOperationException ("The vm is not suspended.");
			case ErrorCode.NOT_IMPLEMENTED:
				throw new NotSupportedException ("This request is not supported by the protocol version implemented by the debuggee.");
			case ErrorCode.ABSENT_INFORMATION:
				throw new AbsentInformationException ();
			case ErrorCode.NO_SEQ_POINT_AT_IL_OFFSET:
				throw new ArgumentException ("Cannot set breakpoint on the specified IL offset.");
			default:
				throw new CommandException (args.ErrorCode);
			}
		}

		/* Wait for the debuggee to start up and connect to it */
		internal void connect () {
			conn.Connect ();

			// Test the connection
			version = conn.Version;
			if (version.MajorVersion != Connection.MAJOR_VERSION)
				throw new NotSupportedException (String.Format ("The debuggee implements protocol version {0}.{1}, while {2}.{3} is required.", version.MajorVersion, version.MinorVersion, Connection.MAJOR_VERSION, Connection.MINOR_VERSION));

			long root_domain_id = conn.RootDomain;
			root_domain = GetDomain (root_domain_id);
		}

		internal void notify_vm_event (EventType evtype, SuspendPolicy spolicy, int req_id, long thread_id, string vm_uri) {
			//Console.WriteLine ("Event: " + evtype + "(" + vm_uri + ")");

			switch (evtype) {
			case EventType.VMStart:
				/* Notify the main thread that the debuggee started up */
				lock (startup_monitor) {
					Monitor.Pulse (startup_monitor);
				}
				queue_event_set (new EventSet (this, spolicy, new Event[] { new VMStartEvent (vm, req_id, thread_id) }));
				break;
			case EventType.VMDeath:
				queue_event_set (new EventSet (this, spolicy, new Event[] { new VMDeathEvent (vm, req_id) }));
				break;
			case EventType.VMDisconnect:
				queue_event_set (new EventSet (this, spolicy, new Event[] { new VMDisconnectEvent (vm, req_id) }));
				break;
			default:
				throw new Exception ();
			}
		}

		//
		// Methods to create instances of mirror objects
		//

		/*
		class MirrorCache<T> {
			static Dictionary <long, T> mirrors;
			static object mirror_lock = new object ();

			internal static T GetMirror (VirtualMachine vm, long id) {
				lock (mirror_lock) {
				if (mirrors == null)
					mirrors = new Dictionary <long, T> ();
				T obj;
				if (!mirrors.TryGetValue (id, out obj)) {
					obj = CreateMirror (vm, id);
					mirrors [id] = obj;
				}
				return obj;
				}
			}

			internal static T CreateMirror (VirtualMachine vm, long id) {
			}
		}
		*/

		// FIXME: When to remove items from the cache ?

		Dictionary <long, MethodMirror> methods;
		object methods_lock = new object ();

		internal MethodMirror GetMethod (long id) {
			lock (methods_lock) {
				if (methods == null)
					methods = new Dictionary <long, MethodMirror> ();
				MethodMirror obj;
				if (id == 0)
					return null;
				if (!methods.TryGetValue (id, out obj)) {
					obj = new MethodMirror (this, id);
					methods [id] = obj;
				}
				return obj;
			}
	    }

		Dictionary <long, AssemblyMirror> assemblies;
		object assemblies_lock = new object ();

		internal AssemblyMirror GetAssembly (long id) {
			lock (assemblies_lock) {
				if (assemblies == null)
					assemblies = new Dictionary <long, AssemblyMirror> ();
				AssemblyMirror obj;
				if (id == 0)
					return null;
				if (!assemblies.TryGetValue (id, out obj)) {
					obj = new AssemblyMirror (this, id);
					assemblies [id] = obj;
				}
				return obj;
			}
	    }

		Dictionary <long, ModuleMirror> modules;
		object modules_lock = new object ();

		internal ModuleMirror GetModule (long id) {
			lock (modules_lock) {
				if (modules == null)
					modules = new Dictionary <long, ModuleMirror> ();
				ModuleMirror obj;
				if (id == 0)
					return null;
				if (!modules.TryGetValue (id, out obj)) {
					obj = new ModuleMirror (this, id);
					modules [id] = obj;
				}
				return obj;
			}
	    }

		Dictionary <long, AppDomainMirror> domains;
		object domains_lock = new object ();

		internal AppDomainMirror GetDomain (long id) {
			lock (domains_lock) {
				if (domains == null)
					domains = new Dictionary <long, AppDomainMirror> ();
				AppDomainMirror obj;
				if (id == 0)
					return null;
				if (!domains.TryGetValue (id, out obj)) {
					obj = new AppDomainMirror (this, id);
					domains [id] = obj;
				}
				return obj;
			}
	    }

		Dictionary <long, TypeMirror> types;
		object types_lock = new object ();

		internal TypeMirror GetType (long id) {
			lock (types_lock) {
				if (types == null)
					types = new Dictionary <long, TypeMirror> ();
				TypeMirror obj;
				if (id == 0)
					return null;
				if (!types.TryGetValue (id, out obj)) {
					obj = new TypeMirror (this, id);
					types [id] = obj;
				}
				return obj;
			}
	    }

		Dictionary <long, ObjectMirror> objects;
		object objects_lock = new object ();

		internal T GetObject<T> (long id, long domain_id, long type_id) where T : ObjectMirror {
			lock (objects_lock) {
				if (objects == null)
					objects = new Dictionary <long, ObjectMirror> ();
				ObjectMirror obj;
				if (!objects.TryGetValue (id, out obj)) {
					/*
					 * Obtain the domain/type of the object to determine the type of
					 * object we need to create.
					 */
					if (domain_id == 0)
						domain_id = conn.Object_GetDomain (id);
					AppDomainMirror d = GetDomain (domain_id);

					if (type_id == 0)
						type_id = conn.Object_GetType (id);
					TypeMirror t = GetType (type_id);

					if (t.Assembly == d.Corlib && t.Namespace == "System.Threading" && t.Name == "Thread")
						obj = new ThreadMirror (this, id);
					else if (t.Assembly == d.Corlib && t.Namespace == "System" && t.Name == "String")
						obj = new StringMirror (this, id);
					else if (typeof (T) == typeof (ArrayMirror))
						obj = new ArrayMirror (this, id);
					else
						obj = new ObjectMirror (this, id);
					objects [id] = obj;
				}
				return (T)obj;
			}
	    }

		internal T GetObject<T> (long id) where T : ObjectMirror {
			return GetObject<T> (id, 0, 0);
		}

		internal ObjectMirror GetObject (long objid) {
			return GetObject<ObjectMirror> (objid);
		}

		internal ThreadMirror GetThread (long id) {
			return GetObject <ThreadMirror> (id);
		}

		object requests_lock = new object ();

		internal void AddRequest (EventRequest req, int id) {
			lock (requests_lock) {
				requests [id] = req;
			}
		}

		internal void RemoveRequest (EventRequest req, int id) {
			lock (requests_lock) {
				requests.Remove (id);
			}
		}

		internal EventRequest GetRequest (int id) {
			lock (requests_lock) {
				return requests [id];
			}
		}

		internal Value DecodeValue (ValueImpl v) {
			if (v.Value != null)
				return new PrimitiveValue (this, v.Value);

			switch (v.Type) {
			case ElementType.Void:
				return null;
			case ElementType.SzArray:
			case ElementType.Array:
				return GetObject<ArrayMirror> (v.Objid);
			case ElementType.String:
				return GetObject<StringMirror> (v.Objid);
			case ElementType.Class:
			case ElementType.Object:
				return GetObject (v.Objid);
			case ElementType.ValueType:
				if (v.IsEnum)
					return new EnumMirror (this, GetType (v.Klass), DecodeValues (v.Fields));
				else
					return new StructMirror (this, GetType (v.Klass), DecodeValues (v.Fields));
			case (ElementType)ValueTypeId.VALUE_TYPE_ID_NULL:
				return new PrimitiveValue (this, null);
			default:
				throw new NotImplementedException ("" + v.Type);
			}
		}

		internal Value[] DecodeValues (ValueImpl[] values) {
			Value[] res = new Value [values.Length];
			for (int i = 0; i < values.Length; ++i)
				res [i] = DecodeValue (values [i]);
			return res;
		}

		internal ValueImpl EncodeValue (Value v) {
			if (v is PrimitiveValue) {
				object val = (v as PrimitiveValue).Value;
				if (val == null)
					return new ValueImpl { Type = (ElementType)ValueTypeId.VALUE_TYPE_ID_NULL, Objid = 0 };
				else
					return new ValueImpl { Value = val };
			} else if (v is ObjectMirror) {
				return new ValueImpl { Type = ElementType.Object, Objid = (v as ObjectMirror).Id };
			} else if (v is StructMirror) {
				return new ValueImpl { Type = ElementType.ValueType, Klass = (v as StructMirror).Type.Id, Fields = EncodeValues ((v as StructMirror).Fields) };
			} else {
				throw new NotSupportedException ();
			}
		}

		internal ValueImpl[] EncodeValues (IList<Value> values) {
			ValueImpl[] res = new ValueImpl [values.Count];
			for (int i = 0; i < values.Count; ++i)
				res [i] = EncodeValue (values [i]);
			return res;
		}
    }

	class EventHandler : MarshalByRefObject, IEventHandler
	{		
		VirtualMachine vm;

		public EventHandler (VirtualMachine vm) {
			this.vm = vm;
		}

		public void Events (SuspendPolicy suspend_policy, EventInfo[] events) {
			var l = new List<Event> ();

			for (int i = 0; i < events.Length; ++i) {
				EventInfo ei = events [i];
				int req_id = ei.ReqId;
				long thread_id = ei.ThreadId;
				long id = ei.Id;
				long loc = ei.Location;

				switch (ei.EventType) {
				case EventType.VMStart:
					vm.notify_vm_event (EventType.VMStart, suspend_policy, req_id, thread_id, null);
					break;
				case EventType.VMDeath:
					vm.notify_vm_event (EventType.VMDeath, suspend_policy, req_id, thread_id, null);
					break;
				case EventType.ThreadStart:
					l.Add (new ThreadStartEvent (vm, req_id, id));
					break;
				case EventType.ThreadDeath:
					l.Add (new ThreadDeathEvent (vm, req_id, id));
					break;
				case EventType.AssemblyLoad:
					l.Add (new AssemblyLoadEvent (vm, req_id, thread_id, id));
					break;
				case EventType.AssemblyUnload:
					l.Add (new AssemblyUnloadEvent (vm, req_id, thread_id, id));
					break;
				case EventType.TypeLoad:
					l.Add (new TypeLoadEvent (vm, req_id, thread_id, id));
					break;
				case EventType.MethodEntry:
					l.Add (new MethodEntryEvent (vm, req_id, thread_id, id));
					break;
				case EventType.MethodExit:
					l.Add (new MethodExitEvent (vm, req_id, thread_id, id));
					break;
				case EventType.Breakpoint:
					l.Add (new BreakpointEvent (vm, req_id, thread_id, id, loc));
					break;
				case EventType.Step:
					l.Add (new StepEvent (vm, req_id, thread_id, id, loc));
					break;
				case EventType.Exception:
					l.Add (new ExceptionEvent (vm, req_id, thread_id, id, loc));
					break;
				case EventType.AppDomainCreate:
					l.Add (new AppDomainCreateEvent (vm, req_id, thread_id, id));
					break;
				case EventType.AppDomainUnload:
					l.Add (new AppDomainUnloadEvent (vm, req_id, thread_id, id));
					break;
				case EventType.UserBreak:
					l.Add (new UserBreakEvent (vm, req_id, thread_id));
					break;
				default:
					break;
				}
			}
			
			if (l.Count > 0)
				vm.queue_event_set (new EventSet (vm, suspend_policy, l.ToArray ()));
		}

		public void VMDisconnect (int req_id, long thread_id, string vm_uri) {
			vm.notify_vm_event (EventType.VMDisconnect, SuspendPolicy.None, req_id, thread_id, vm_uri);
        }
    }

	internal class CommandException : Exception {

		public CommandException (ErrorCode error_code) : base ("Debuggee returned error code " + error_code + ".") {
			ErrorCode = error_code;
		}

		public ErrorCode ErrorCode {
			get; set;
		}
	}
}
