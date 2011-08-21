using System;
using System.Reflection;
using System.Threading;

public class Container
{
	public static int Main (string[] args) 
	{
		var thism = MethodBase.GetCurrentMethod ();
		foreach (var m in typeof (Container).GetMethods(BindingFlags.Static | BindingFlags.Public)) {
			if (m != thism) {
				if (!((bool) m.Invoke (null, null))) {
					int li = m.Name.LastIndexOf ('_');
					return int.Parse (m.Name.Substring (li + 1, m.Name.Length - li - 1));
				}
			}
		}

		return 0;
	}

	public static bool owership_remains_after_interrupt_12 () 
	{
		var theLock = new object ();
		
		Monitor.Enter (theLock);
		try {
			var t = Thread.CurrentThread;
			ThreadPool.QueueUserWorkItem(_ => t.Interrupt ());
			Monitor.Wait (theLock);
		} catch (ThreadInterruptedException) {
			Monitor.Exit (theLock);
			return true;
		}
		
		return false;
	}

	public static bool owership_remains_after_abort_11 () 
	{
		var theLock = new object ();
		
		Monitor.Enter (theLock);
		try {
			var t = Thread.CurrentThread;
			ThreadPool.QueueUserWorkItem(_ => t.Abort ());
			Monitor.Wait (theLock);
		} catch (ThreadAbortException) {
			Thread.ResetAbort ();
			Monitor.Exit (theLock);
			return true;
		}
		
		return false;
	}

	public static bool getting_hash_code_when_infated_10 () 
	{
		var theLock = new object ();
		
		lock (theLock) Monitor.Pulse (theLock);
		
		theLock.GetHashCode ();
		return true;
	}
	
	public static bool inflation_due_to_presence_of_hash_code_9 () 
	{
		var theLock = new object ();
		
		var hashCode = theLock.GetHashCode ();		
		
		lock (theLock) { }
		
		return true;
	}
	
	public static bool inflation_due_to_getting_hash_code_8 () 
	{
		var theLock = new object ();
			
		lock (theLock) { theLock.GetHashCode (); }
		
		return true;
	}
	
	public static bool inflation_due_to_nesting_overflow_7 () 
	{
		var theLock = new object ();

		for (int i = 0; i < (1 << 10); ++i) {
			Monitor.Enter (theLock);
		}
		
		// Lock is now inflated.
		
		for (int i = 0; i < (1 << 10); ++i) {
			Monitor.Exit (theLock);
		}
		return true;
	}

	public static bool inflation_when_owned_6 () 
	{
		var theLock = new object ();
		lock (theLock) { Monitor.Pulse (theLock); }
		return true;
	}

	public static bool normal_acquisition_5 () 
	{
		var theLock = new object ();
		Monitor.Enter (theLock);
		Monitor.Exit (theLock);
		return true;
	}

	public static bool sync_exception_on_wait_4 () 
	{
		var theLock = new object ();
		lock (theLock) Monitor.Pulse (theLock);
		
		try {
			Monitor.Wait (theLock);
		} catch (SynchronizationLockException) {
			return true;
		}
		return false;
	}

	public static bool sync_exception_on_pulse_3 () 
	{
		var theLock = new object ();

		try {
			Monitor.Pulse (theLock);
		} catch (SynchronizationLockException) {
			return true;
		}
		return false;
	}
	
	public static bool sync_exception_on_pulse_all_2 () 
	{
		var theLock = new object ();

		try {
			Monitor.PulseAll (theLock);
		} catch (SynchronizationLockException) {
			return true;
		}
		return false;
	}

	public static bool sync_exception_on_exit_1 () 
	{
		var theLock = new object ();

		try {
			Monitor.Exit (theLock);
		} catch (SynchronizationLockException) {
			return true;
		}
		return false;
	}
}
