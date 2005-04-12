/*
 * monod.cs: Mono daemon for running services based on System.ServiceProcess
 *
 * Author:
 *   Joerg Rosenkranz (joergr@voelcker.com)
 *   Miguel de Icaza (miguel@novell.com)
 *
 * (C) 2005 Voelcker Informatik AG
 * (C) 2005 Novell Inc
 */
using System;
using System.IO;
using System.Reflection;
using Mono.Unix;
using System.ServiceProcess;
using System.Threading;
using System.Runtime.InteropServices;

class MonoServiceRunner : MarshalByRefObject
{
	string assembly, name, logname;
	
	static void info (string prefix, string format, params object [] args)
	{
		Syscall.syslog (SyslogLevel.LOG_INFO, String.Format ("{0}: {1}", prefix, String.Format (format, args)));
	}
	
	static void error (string prefix, string format, params object [] args)
	{
		Syscall.syslog (SyslogLevel.LOG_ERR, String.Format ("{0}: {1}", prefix, String.Format (format, args)));
	}
	
	static void Usage ()
	{
		Console.Error.WriteLine (
					 "Usage is:\n" +
					 "mono-service [-d:DIRECTORY] [-l:LOCKFILE] [-n:NAME] [-m:LOGNAME] service.exe\n");
		Environment.Exit (1);
	}

	delegate void sighandler_t (int arg);
	
	AutoResetEvent signal_event;

	[DllImport ("libc")]
	extern static int signal (int signum, sighandler_t handler);

	int signum;
	
	void my_handler (int sig)
	{
		signum = sig;
		signal_event.Set ();
	}

	static void call (object o, string method, object [] arg)
	{
		MethodInfo m = o.GetType ().GetMethod (method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		if (arg != null)
			m.Invoke (o, new object [1] { arg });
		else
			m.Invoke (o, null);
	}
	
	static int Main (string [] args)
	{
		string assembly = null;
		string directory = null;
		string lockfile = null;
		string name = null;
		string logname = null;

		foreach (string s in args){
			if (s.Length > 3 && s [0] == '-' && s [2] == ':'){
				string arg = s.Substring (3);

				switch (Char.ToLower (s [1])){
				case 'd': directory = arg; break;
				case 'l': lockfile = arg; break;
				case 'n': name = arg; break;
				case 'm': logname = arg; break;
				default: Usage (); break;
				}
			} else {
				if (assembly != null)
					Usage ();
				
				assembly = s;
			}
		}

		if (logname == null)
			logname = assembly;

		if (assembly == null){
			error (logname, "Assembly name is missing");
			Usage ();
		}
		
		if (directory != null){
			if (Syscall.chdir (directory) != 0){
				error (logname, "Could not change to directory {0}", directory);
				return 1;
			}
		}
		
		// Use lockfile to allow only one instance
		if (lockfile == null)
			lockfile = String.Format ("/tmp/{0}.lock", Path.GetFileName (assembly));

		int lfp = Syscall.open (lockfile, OpenFlags.O_RDWR|OpenFlags.O_CREAT, 
			FilePermissions.S_IRUSR|FilePermissions.S_IWUSR|FilePermissions.S_IRGRP);
	
		if (lfp<0)  {
			error (logname, "Cannot open lock file.");
			return 1;
		}
	
		if (Syscall.lockf(lfp, LockFlags.F_TLOCK,0)<0)  {
			info (logname, "Daemon is already running.");
			return 0;
		}
		
		// Write pid to lock file
		string pid = Syscall.getpid ().ToString () + Environment.NewLine;
		IntPtr buf = Marshal.StringToCoTaskMemAnsi (pid);
		Syscall.write (lfp, buf, (ulong)pid.Length);
		Marshal.FreeCoTaskMem (buf);

		// Create new AppDomain to run service
		AppDomainSetup setup = new AppDomainSetup ();
		setup.ApplicationBase = Environment.CurrentDirectory;
		setup.ConfigurationFile = Path.Combine (Environment.CurrentDirectory, assembly + ".config");
		setup.ApplicationName = logname;
		
		AppDomain newDomain = AppDomain.CreateDomain (logname, AppDomain.CurrentDomain.Evidence, setup);
		MonoServiceRunner rnr = newDomain.CreateInstanceAndUnwrap(
            typeof (MonoServiceRunner).Assembly.FullName,
            typeof (MonoServiceRunner).FullName,
            true,
            BindingFlags.Default,
            null,
            new object [] {assembly, name, logname},
            null, null, null) as MonoServiceRunner;
			
		if (rnr == null) {
			error (logname, "Internal Mono Error: Could not create MonoServiceRunner.");
			return 1;
		}

		return rnr.StartService ();
	}
	
	public MonoServiceRunner (string assembly, string name, string logname)
	{
		this.assembly = assembly;
		this.name = name;
		this.logname = logname;
	}
	
	public int StartService ()
	{
		try	{
			//
			// Setup signals
			//
			signal_event = new AutoResetEvent (false);
	
			// Invoke all the code used in the signal handler, so the JIT does
			// not kick-in inside the signal handler
			signal_event.Set ();
			signal_event.Reset ();
	
			// Hook up 
			signal (UnixConvert.FromSignum (Signum.SIGTERM), new sighandler_t (my_handler));
			signal (UnixConvert.FromSignum (Signum.SIGUSR1), new sighandler_t (my_handler));
			signal (UnixConvert.FromSignum (Signum.SIGUSR2), new sighandler_t (my_handler));
	
			// Load service assembly
			Assembly a = null;
			
			try {
				a = Assembly.LoadFrom (assembly);
			} catch (FileNotFoundException) {
				error (logname, "Could not find assembly {0}", assembly);
				return 1;
			} catch (BadImageFormatException){
				error (logname, "File {0} is not a valid assembly", assembly);
				return 1;
			} catch { }
			
			if (a == null){
				error (logname, "Could not load assembly {0}", assembly);
				return 1;
			}
	
			// Hook up RunService callback
			Type cbType = Type.GetType ("System.ServiceProcess.ServiceBase+RunServiceCallback, System.ServiceProcess");
			if (cbType == null){
				error (logname, "Internal Mono Error: Could not find RunServiceCallback in ServiceBase");
				return 1;			
			}
			
			FieldInfo fi = typeof (ServiceBase).GetField ("RunService", BindingFlags.Static | BindingFlags.NonPublic);
			if (fi == null){
				error (logname, "Internal Mono Error: Could not find RunService in ServiceBase");
				return 1;
			}
			fi.SetValue (null, Delegate.CreateDelegate(cbType, this, "MainLoop"));
			
			// And run its Main. Our RunService handler is invoked from 
			// ServiceBase.Run.
			MethodInfo entry = a.EntryPoint;
			if (entry == null){
				error (logname, "Entry point not defined in service");
				return 1;
			}
	
			string [] service_args = new string [0];
			entry.Invoke (null, service_args);
			
			return 0;
			
		} catch ( Exception ex ) {
			for (Exception e = ex; e != null; e = e.InnerException)
				error (logname, e.Message);
			
			return 1;
		}
	}
	
	// The main service loop
	private void MainLoop (ServiceBase [] services)
	{
		try {
			ServiceBase service;
	
			if (services == null || services.Length == 0){
				error (logname, "No services were registered by this service");
				return;
			}
			
			// Start up the service.
			service = null;
			
			if (name != null){
				foreach (ServiceBase svc in services){
					if (svc.ServiceName == name){
						service = svc;
						break;
					}
				}
			} else {
				service = services [0];
			}
	
			call (service, "OnStart", new string [0]);
			info (logname, "Service {0} started", service.ServiceName);
	
			for (bool running = true; running; ){
				signal_event.WaitOne ();
				Signum v;
				
				if (UnixConvert.TryToSignum (signum, out v)){
					signum = 0;
					
					switch (v){
					case Signum.SIGTERM:
						if (service.CanStop) {
							info (logname, "Stopping service {0}", service.ServiceName);
							call (service, "OnStop", null);
							running = false;
						}
						break;
					case Signum.SIGUSR1:
						if (service.CanPauseAndContinue) {
							info (logname, "Pausing service {0}", service.ServiceName);
							call (service, "OnPause", null);
						}
						break;
					case Signum.SIGUSR2:
						if (service.CanPauseAndContinue) {
							info (logname, "Continuing service {0}", service.ServiceName);
							call (service, "OnContinue", null);
						}
						break;
					}
				}
			}
		} finally {
			// Clean up
			foreach (ServiceBase svc in services){
				svc.Dispose ();
			}
		}
	}
}
