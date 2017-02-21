using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;


public enum Record {
	MustNotHoldAny,
	MustNotHoldOne,
	MustHoldOne,
	LockAcquired,
	LockReleased
}


public struct LockRecord {
	public SimLock lk;
	public string frame;

	public LockRecord (SimLock lk, string frame) {
		this.lk = lk;
		this.frame = frame;
	}
}

public class SimThread
{
	int thread;
	List <LockRecord> locks = new List <LockRecord> ();

	public SimThread (int t)
	{
		this.thread = t;
	}

	public bool HoldsLock (SimLock lk) {
		foreach (var l in locks) {
			if (l.lk == lk)
				return true;
		}
		return false;
	}


	public int HoldCount (SimLock lk) {
		int res = 0;
		foreach (var l in locks)
			if (l.lk == lk)
				++res;
		return res;
	}

	public void Lock (SimLock lk, string frame) {
		foreach (LockRecord lr in locks) {
			if (lk.WarnAbout (this, lr.lk)) 
				Console.WriteLine ("WARNING: tried to acquire lock {0} at {1} while holding {2} at {3}: {4}", lk, frame, lr.lk, lr.frame, lk.GetWarningMessage (this, lr.lk));
			else if (!lk.IsValid (this, lr.lk))
				Console.WriteLine ("ERROR: tried to acquire lock {0} at {1} while holding {2} at {3}: {4}", lk, frame, lr.lk, lr.frame, lk.GetErrorMessage (this, lr.lk));
		}
		locks.Add (new LockRecord (lk, frame));
	}

	public void Release (SimLock lk, string frame) {
		if (locks.Count == 0) {
			Console.WriteLine ("ERROR: released lock {0} at {1} while holding no locks!", lk, frame);
			return;
		}
		LockRecord top = locks [locks.Count - 1];
		if (top.lk != lk && !(lk.IsGlobalLock && HoldCount (lk) > 1)) {
			Console.WriteLine ("WARNING: released lock {0} at {1} out of order with {2} at {3}!", lk, frame, top.lk, top.frame);
		}
		for (int i = locks.Count -1; i >= 0; --i) {
			if (locks [i].lk == lk) {
				locks.RemoveAt (i);
				break;
			}
		}
	}
}

/*
LOCK RULES

Simple locks:
 	Can be acquired at any point regardless of which locks are taken or not.
	No other locks can be acquired or released while holding a simple lock.
	Reentrancy is not recommended. (warning)
	Simple locks are leaf locks on the lock lattice.

Complex locks:
	Must respect locking order, which form a lattice.
	IOW, to take a given lock, only it's parents might have been taken.
	Reentrancy is ok.
	Locks around resources count as separate instances of the hierarchy.

Global locks:
	Must respect locking order.
	Must be the at the botton of the locking lattice.
	Can be taken out-of-order by other locks given that it was previously acquired.
	Adding global locks is not to be taken lightly.

The current lock hierarchy:
loader lock (global)
	domain lock (complex)
		domain jit lock (complex)
		marshal lock
			simple locks

Examples:
	You can take the loader lock without holding a domain lock.
	You can take the domain load while holding the loader lock
	You cannot take the loader lock if only the domain lock is held.
	You cannot take a domain lock while holding the lock to another domain.


TODO:

We have a few known ok violation. We need a way to whitelist them.

Known ok issues:

ERROR: tried to acquire lock DomainLock at mono_domain_code_reserve_align while holding DomainLock at mono_class_create_runtime_vtable: Hierarchy violation.
	This is triggered when building the vtable of a non-root domain and fetching a vtable trampoline for an offset that has not been built. We'll take the root
	domain lock while holding the other one.
	This is ok since we never allow locking to have in the other direction, IOW, the root-domain lock is one level down from the other domain-locks.

WARNING: tried to acquire lock ImageDataLock at mono_image_init_name_cache while holding ImageDataLock at mono_class_from_name
WARNING: tried to acquire lock ImageDataLock at mono_image_init_name_cache while holding ImageDataLock at mono_image_add_to_name_cache
	Both of those happen when filling up the name_cache, as it needs to alloc image memory.
	This one is fixable by splitting mono_image_init_name_cache into a locked and an unlocked variants and calling them appropriately.

*/

public enum Lock {
	Invalid,
	LoaderLock,
	ImageDataLock,
	DomainLock,
	DomainAssembliesLock,
	DomainJitCodeHashLock,
	IcallLock,
	AssemblyBindingLock,
	MarshalLock,
	ClassesLock,
	LoaderGlobalDataLock,
	ThreadsLock,
}

public class SimLock
{
	Lock kind;
	int id;

	public SimLock (Lock kind, int id) {
		this.kind = kind;
		this.id = id;
	}

	static int GetLockOrder (Lock kind) {
		switch (kind) {
			case Lock.LoaderLock:
				return 0;
			case Lock.DomainLock:
				return 1;
			case Lock.DomainJitCodeHashLock:
			case Lock.MarshalLock:
				return 2;
			default:
				return 3;
		}
	}

	bool IsParent (SimLock other) {
		return GetLockOrder (kind) > GetLockOrder (other.kind);
	}

	public bool IsSimpleLock {
		get { return GetLockOrder (kind) == 3; }
	}

	public bool IsGlobalLock {
		get { return kind == Lock.LoaderLock; }
	}

	public bool IsResursiveLock {
		get { return kind == Lock.LoaderLock || kind == Lock.DomainLock; }
	}

	/*locked is already owned by the thread, 'this' is the new one*/
	bool Compare (SimThread thread, SimLock locked, out bool isWarning, out string msg)
	{
		isWarning = false;
		msg = null;

		if (locked != this) {
			if (!IsParent (locked)) {
				if (IsGlobalLock) { /*acquiring a global lock*/
					if (!thread.HoldsLock (this)) { /*does the thread alread hold it?*/
						msg = "Acquired a global lock after a regular lock without having it before.";
						return false;
					}
				} else {
					msg = "Hierarchy violation.";
					return false;
				}
			}
		} else if (IsSimpleLock) {
			msg = "Avoid taking simple locks recursively";
			isWarning = true;
			return false;
		}

		return true;
	}

	public bool IsValid (SimThread thread, SimLock locked) {
		bool warn;
		string msg;
		return Compare (thread, locked, out warn, out msg);
	}

	public bool WarnAbout (SimThread thread, SimLock locked) {
		bool warn;
		string msg;
		Compare (thread, locked, out warn, out msg);
		return warn;
	}

	public string GetWarningMessage (SimThread thread, SimLock locked) {
		bool warn;
		string msg;
		Compare (thread, locked, out warn, out msg);
		return warn ? msg : null;
	}

	public string GetErrorMessage (SimThread thread, SimLock locked) {
		bool warn;
		string msg;
		bool res = Compare (thread, locked, out warn, out msg);
		return !res && !warn ? msg : null;
	}

	public override string ToString () {
		switch (kind) {
		case Lock.LoaderLock:
		case Lock.IcallLock:
		case Lock.AssemblyBindingLock:
		case Lock.MarshalLock:
		case Lock.ClassesLock:
		case Lock.LoaderGlobalDataLock:
		case Lock.ThreadsLock:
			return String.Format ("{0}", kind);

		case Lock.ImageDataLock:
		case Lock.DomainLock:
		case Lock.DomainAssembliesLock:
		case Lock.DomainJitCodeHashLock:
			return String.Format ("{0}[{1}]", kind, id);
		default:
			return String.Format ("Unknown({0})[{1}]", kind, id);
		}
	}
}

public class LockSimulator
{
	static Dictionary <int, SimThread> threads = new Dictionary <int, SimThread> ();
	static Dictionary <int, SimLock> locks = new Dictionary <int, SimLock> ();

	SymbolTable syms;

	public LockSimulator (SymbolTable s) { this.syms = s; }

	SimLock GetLock (Trace t)  {
		if (locks.ContainsKey (t.lockPtr))
			return locks [t.lockPtr];
		else {
			return locks [t.lockPtr] = new SimLock (t.lockKind, t.lockPtr);
		}
	}

	SimThread GetThread (Trace t) {
		if (threads.ContainsKey (t.thread))
			return threads [t.thread];
		else
			return threads [t.thread] = new SimThread (t.thread);		
	}

	public void PlayBack (IEnumerable<Trace> traces) {
		foreach (var t in traces) {
			SimThread thread = GetThread (t);
			SimLock lk = GetLock (t);
			string frame = t.GetUsefullTopTrace (this.syms);

			switch (t.record) {
			case Record.MustNotHoldAny:
			case Record.MustNotHoldOne:
			case Record.MustHoldOne:
				throw new Exception ("not supported");
			case Record.LockAcquired:
				thread.Lock (lk, frame);
				break;
			case Record.LockReleased:
				thread.Release (lk, frame);
				break;
			default:
				throw new Exception ("Invalid trace record: "+t.record);
			}
		}
	}
}

public class Trace {
	public int thread;
	public Record record;
	public Lock lockKind;
	public int lockPtr;
	int[] frames;

	static readonly string[] BAD_FRAME_METHODS = new string[] {
		"mono_loader_lock",
		"mono_loader_unlock",
		"mono_image_lock",
		"mono_image_unlock",
		"mono_icall_lock",
		"mono_icall_unlock",
		"add_record",
		"mono_locks_lock_acquired",
		"mono_locks_lock_released",
		"mono_threads_lock",
		"mono_threads_unlock",
		"mono_domain_lock",
		"mono_domain_unlock",
	};

	public Trace (string[] fields) {
		thread = fields [0].ParseHex ();
		record = (Record)fields [1].ParseDec ();
		lockKind = (Lock)fields [2].ParseDec ();
		lockPtr = fields [3].ParseHex ();
		frames = new int [fields.Length - 4];
		for (int i = 0; i < frames.Length; ++i)
			frames [i] = fields [i + 4].ParseHex ();
	}

	public void Dump (SymbolTable table) {
		Console.WriteLine ("{0:x} {1} {2} {3:x}", thread, record, lockKind, lockPtr);
		for (int i = 0; i < frames.Length; ++i)
			Console.WriteLine ("\t{0}", table.Translate (frames [i]));
	}

	public string GetUsefullTopTrace (SymbolTable syms) {
		for (int i = 0; i < frames.Length; ++i) {
			string str = syms.Translate (frames [i]);
			bool ok = true;
			for (int j = 0; j < BAD_FRAME_METHODS.Length; ++j) {
				if (str.IndexOf (BAD_FRAME_METHODS [j]) >= 0) {
					ok = false;
					break;
				}
			}
			if (ok)
				return str;
		}
		return "[unknown]";
	}
}

public class Symbol : IComparable<Symbol>
{
	public int offset;
	public int size;
	public string name;

	public Symbol (int o, int size, string n) {
		this.offset = o;
		this.size = size;
		this.name = n;
	}

	public int CompareTo(Symbol other) {
		return offset - other.offset;
	}

	public void AdjustSize (Symbol next) {
		size = next.offset - this.offset;
	}
}

public interface SymbolTable {
	string Translate (int offset);
}

public class OsxSymbolTable : SymbolTable
{
	Symbol[] table;

	const int MAX_FUNC_SIZE = 0x20000;

	public OsxSymbolTable (string binary) {
		Load (binary);
	}

	void Load (string binary) {
		ProcessStartInfo psi = new ProcessStartInfo ("gobjdump", "-t "+binary);
		psi.UseShellExecute = false;
		psi.RedirectStandardOutput = true;

		var proc = Process.Start (psi);
		var list = new List<Symbol> ();
		string line;
		while ((line = proc.StandardOutput.ReadLine ()) != null) {
			string[] fields = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
			if (fields.Length < 7)
				continue;

			if (!fields [3].Equals ("FUN"))
				continue;

			int offset = fields [0].ParseHex ();
			string name = fields [6];
			if (name.StartsWith ("_"))
				name = name.Substring (1);

			if (offset != 0)
				list.Add (new Symbol (offset, 0, name));
		}
		table = new Symbol [list.Count];
		list.CopyTo (table, 0);
		Array.Sort (table);
		for (int i = 1; i < table.Length; ++i) {
			table [i - 1].AdjustSize (table [i]);
		}
	}

	public string Translate (int offset) {
		Symbol sym = null;
		int res = Array.BinarySearch (table, new Symbol (offset, 0, null));
		if (res >= 0)
			return table [res].name;
		res = ~res;

		if (res >= table.Length)
			sym = table [table.Length - 1];
		else if (res != 0)
			sym = table [res - 1];

		
		if (sym != null) {
			int size = Math.Max (sym.size, 10);
			if (offset - sym.offset < size)
				return sym.name;
		}
		return String.Format ("[{0:x}]", offset);
	}
}

public class LinuxSymbolTable : SymbolTable
{
	Symbol[] table;

	const int MAX_FUNC_SIZE = 0x20000;

	public LinuxSymbolTable (string binary) {
		Load (binary);
	}

	void Load (string binary) {
		ProcessStartInfo psi = new ProcessStartInfo ("objdump", "-t "+binary);
		psi.UseShellExecute = false;
		psi.RedirectStandardOutput = true;

		var proc = Process.Start (psi);
		var list = new List<Symbol> ();
		string line;
		while ((line = proc.StandardOutput.ReadLine ()) != null) {
			string[] fields = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);

			if (fields.Length < 6)
				continue;
			if (fields [3] != ".text" || fields [2] != "F")
				continue;

			int offset = fields [0].ParseHex ();
			int size = fields [4].ParseHex ();
			string name = fields [fields.Length - 1];
			if (offset != 0)
				list.Add (new Symbol (offset, size, name));
		}
		table = new Symbol [list.Count];
		list.CopyTo (table, 0);
		Array.Sort (table);
	}

	public string Translate (int offset) {
		Symbol sym = null;
		int res = Array.BinarySearch (table, new Symbol (offset, 0, null));
		if (res >= 0)
			return table [res].name;
		res = ~res;

		if (res >= table.Length)
			sym = table [table.Length - 1];
		else if (res != 0)
			sym = table [res - 1];

		if (sym != null && offset - sym.offset < MAX_FUNC_SIZE)
			return sym.name;
		return String.Format ("[{0:x}]", offset);
	}
}

public class TraceDecoder
{
	string file;

	public TraceDecoder (string file) {
		this.file = file;
	}

	public IEnumerable<Trace> GetTraces () {
		using (StreamReader reader = new StreamReader (file)) {
			string line;
			while ((line = reader.ReadLine ()) != null) {
				string[] fields = line.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
				if (fields.Length >= 7) {
					yield return new Trace (fields);
				}
			}
		}
	}
}

public class Driver
{
	[DllImport ("libc")]
	static extern int uname (IntPtr buf);

	static bool IsOSX ()
	{
		bool isOsx = false;
		IntPtr buf = Marshal.AllocHGlobal (8192);
		if (uname (buf) == 0) {
			string os = Marshal.PtrToStringAnsi (buf);
			isOsx = os == "Darwin";
		}

		Marshal.FreeHGlobal (buf);
		return isOsx;
	}


	static void Main (string[] args) {
		SymbolTable syms;
		if (args.Length != 2) {
			Console.WriteLine ("usage: LockTracerDecoder.exe /path/to/mono /path/to/locks.pid");
			return;
		}
		if (IsOSX ())
			syms = new OsxSymbolTable (args [0]);
		else
			syms = new LinuxSymbolTable (args [0]);

		var decoder = new TraceDecoder (args [1]);
		var sim = new LockSimulator (syms);
		sim.PlayBack (decoder.GetTraces ());
	}
}

public static class Utils
{
	public static int ParseHex (this string number) {
		while (number.Length > 1 && (number [0] == '0' || number [0] == 'x' || number [0] == 'X'))
			number = number.Substring (1);
		return int.Parse (number, NumberStyles.HexNumber);
	}

	public static int ParseDec (this string number) {
		while (number.Length > 1 && number [0] == '0')
			number = number.Substring (1);
		return int.Parse (number);
	}
}
