//
// System.ServiceProcess.ServiceBase.cs
//
// Authors:
//      Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//      Duncan Mak (duncan@ximian.com)
//      Joerg Rosenkranz (joergr@voelcker.com)
//      Vincent Povirk (madewokherd@gmail.com)
//
// (C) 2003, Ximian Inc and Cesar Octavio Lopez Nataren.
// (C) 2005, Voelcker Informatik AG
// (C) 2014, CodeWeavers Inc.
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.ServiceProcess
{
#if ONLY_1_1
	[Designer ("Microsoft.VisualStudio.Install.UserNTServiceDesigner, " + Consts.AssemblyMicrosoft_VisualStudio, "System.ComponentModel.Design.IRootDesigner")]
#endif
	[InstallerType (typeof (ServiceProcessInstaller))]
	public class ServiceBase : Component
	{
		internal delegate void RunServiceCallback (ServiceBase [] services);

		// This member is used for interoperation with mono-service
		internal static RunServiceCallback RunService;

		internal delegate void NotifyStatusCallback (ServiceBase service, ServiceControllerStatus status);
		internal static NotifyStatusCallback NotifyStatus;

		public const int MaxNameLength = 80;

		bool hasStarted;
		bool auto_log = true;
		bool can_handle_power_event;
		bool can_pause_and_continue;
		bool can_shutdown;
		bool can_stop = true;
		EventLog event_log;
		string service_name;
		bool can_handle_session_change_event;
		IntPtr service_handle;
		ManualResetEvent stop_event;
		static bool share_process;

		public ServiceBase ()
		{
		}

		[DefaultValue (true)]
		[ServiceProcessDescription ("Whether the service should automatically write to the event log on common events such as Install and Start.")]
		public bool AutoLog {
			get { return auto_log; }
			set { auto_log = value; }
		}

		[DefaultValue (false)]
		[MonoTODO]
		public bool CanHandlePowerEvent {
			get { return can_handle_power_event; }
			set {
				if (hasStarted)
					throw new InvalidOperationException (
							Locale.GetText ("Cannot modify this property " +
											"after the service has started."));

				can_handle_power_event = value;
			}
		}

		[DefaultValue (false)]
		[MonoTODO]
		[ComVisible (false)]
		public bool CanHandleSessionChangeEvent {
			get { return can_handle_session_change_event; }
			set {
				if (hasStarted)
					throw new InvalidOperationException (
							Locale.GetText ("Cannot modify this property " +
											"after the service has started."));

				can_handle_session_change_event = value;
			}
		}

		[DefaultValue (false)]
		public bool CanPauseAndContinue {
			get { return can_pause_and_continue; }
			set {
				if (hasStarted)
					throw new InvalidOperationException (
							Locale.GetText ("Cannot modify this property " +
											"after the service has started."));

				can_pause_and_continue = value;
			}
		}

		[DefaultValue (false)]
		public bool CanShutdown {
			get { return can_shutdown; }
			set {
				if (hasStarted)
					throw new InvalidOperationException (
							Locale.GetText ("Cannot modify this property " +
											"after the service has started."));

				can_shutdown = value;
			}
		}

		[DefaultValue (true)]
		public bool CanStop {
			get { return can_stop; }
			set {
				if (hasStarted)
					throw new InvalidOperationException (
							Locale.GetText ("Cannot modify this property " +
											"after the service has started."));

				can_stop = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual EventLog EventLog {
			get {
				if (event_log == null)
					event_log = new EventLog ("Application", ".", service_name);
				return event_log;
			}
		}

		[ComVisible (false)]
		public int ExitCode { get; set; }

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected IntPtr ServiceHandle {
			get { return service_handle; }
		}

		[ServiceProcessDescription ("The name by which the service is identified to the system.")]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string ServiceName {
			get { return service_name; }
			set {
				if (hasStarted)
					throw new InvalidOperationException (
							Locale.GetText ("Cannot modify this property " +
											"after the service has started."));

				service_name = value;
			}
		}

		protected override void Dispose (bool disposing)
		{
		}

		protected virtual void OnStart (string [] args)
		{
		}

		protected virtual void OnStop ()
		{
		}

		protected virtual void OnContinue ()
		{
		}

		protected virtual void OnCustomCommand (int command)
		{
		}

		protected virtual void OnPause ()
		{
		}

		protected virtual bool OnPowerEvent (PowerBroadcastStatus powerStatus)
		{
			return true;
		}

		protected virtual void OnShutdown ()
		{
		}

		protected virtual void OnSessionChange (SessionChangeDescription changeDescription)
		{
		}

		[ComVisible (false)]
		[MonoTODO]
		public void RequestAdditionalTime (int milliseconds)
		{
			throw new NotImplementedException ();
		}

		public void Stop ()
		{
			if (stop_event != null)
				stop_event.Set ();
			else
				OnStop ();
		}

		private void SetStatus (ServiceControllerStatus status)
		{
			if (!hasStarted && status != ServiceControllerStatus.Stopped)
				hasStarted = true;
			if (NotifyStatus != null)
				NotifyStatus (this, status);
		}

		#region Win32 implementation

		private const int NO_ERROR = 0;
		private const int ERROR_CALL_NOT_IMPLEMENTED = 120;
		private const int SERVICE_NO_CHANGE = -1;

		[Flags]
		private enum SERVICE_CONTROL_ACCEPTED
		{
			SERVICE_ACCEPT_NONE = 0x0,
			SERVICE_ACCEPT_STOP = 0x1,
			SERVICE_ACCEPT_PAUSE_CONTINUE = 0x2,
			SERVICE_ACCEPT_SHUTDOWN = 0x4,
			SERVICE_ACCEPT_PARAMCHANGE = 0x8,
			SERVICE_ACCEPT_NETBINDCHANGE = 0x10,
			SERVICE_ACCEPT_HARDWAREPROFILECHANGE = 0x20,
			SERVICE_ACCEPT_POWEREVENT = 0x40,
			SERVICE_ACCEPT_SESSIONCHANGE = 0x80
		}

		private enum SERVICE_CONTROL_TYPE
		{
			SERVICE_CONTROL_STOP = 0x1,
			SERVICE_CONTROL_PAUSE = 0x2,
			SERVICE_CONTROL_CONTINUE = 0x3,
			SERVICE_CONTROL_INTERROGATE = 0x4,
			SERVICE_CONTROL_SHUTDOWN = 0x5,
			SERVICE_CONTROL_PARAMCHANGE = 0x6,
			SERVICE_CONTROL_NETBINDADD = 0x7,
			SERVICE_CONTROL_NETBINDREMOVE = 0x8,
			SERVICE_CONTROL_NETBINDENABLE = 0x9,
			SERVICE_CONTROL_NETBINDDISABLE = 0xA,
			SERVICE_CONTROL_DEVICEEVENT = 0xB,
			SERVICE_CONTROL_HARDWAREPROFILECHANGE = 0xC,
			SERVICE_CONTROL_POWEREVENT = 0xD,
			SERVICE_CONTROL_SESSIONCHANGE = 0xE
		}

		private enum SERVICE_TYPE
		{
			SERVICE_KERNEL_DRIVER = 0x1,
			SERVICE_FILE_SYSTEM_DRIVER = 0x2,
			SERVICE_ADAPTER = 0x4,
			SERVICE_RECOGNIZER_DRIVER = 0x8,
			SERVICE_DRIVER = (SERVICE_KERNEL_DRIVER | SERVICE_FILE_SYSTEM_DRIVER | SERVICE_RECOGNIZER_DRIVER),
			SERVICE_WIN32_OWN_PROCESS = 0x10,
			SERVICE_WIN32_SHARE_PROCESS = 0x20,
			SERVICE_INTERACTIVE_PROCESS = 0x100,
			SERVICETYPE_NO_CHANGE = SERVICE_NO_CHANGE,
			SERVICE_WIN32 = (SERVICE_WIN32_OWN_PROCESS | SERVICE_WIN32_SHARE_PROCESS),
			SERVICE_TYPE_ALL = (SERVICE_WIN32 | SERVICE_ADAPTER | SERVICE_DRIVER | SERVICE_INTERACTIVE_PROCESS)
		}

		[UnmanagedFunctionPointerAttribute (CallingConvention.StdCall)]
		private delegate int LPHANDLER_FUNCTION_EX(int dwControl, int dwEventType, IntPtr lpEventData, IntPtr lpContext);

		[UnmanagedFunctionPointerAttribute (CallingConvention.StdCall)]
		private delegate void LPSERVICE_MAIN_FUNCTION(int dwArgc, IntPtr lpszArgv);

		[StructLayout (LayoutKind.Sequential, Pack = 1)]
		private struct SERVICE_STATUS
		{
			public int dwServiceType;
			public int dwCurrentState;
			public int dwControlsAccepted;
			public int dwWin32ExitCode;
			public int dwServiceSpecificErrorCode;
			public int dwCheckPoint;
			public int dwWaitHint;
		}

		[StructLayout (LayoutKind.Sequential, Pack = 1)]
		private struct SERVICE_TABLE_ENTRY
		{
			[MarshalAs (UnmanagedType.LPWStr)]
			public string lpServiceName;
			public LPSERVICE_MAIN_FUNCTION lpServiceProc;
		}

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr RegisterServiceCtrlHandlerEx (
			string lpServiceName,
			LPHANDLER_FUNCTION_EX lpHandlerProc,
			IntPtr lpContext);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool SetServiceStatus (
			IntPtr hServiceStatus,
			[MarshalAs (UnmanagedType.LPStruct)] SERVICE_STATUS status);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool StartServiceCtrlDispatcher (
			[MarshalAs (UnmanagedType.LPArray)] SERVICE_TABLE_ENTRY[] lpServiceTable);

		private static void Win32NotifyStatus (ServiceBase service, ServiceControllerStatus status)
		{
			SERVICE_STATUS service_status = new SERVICE_STATUS ();

			service_status.dwServiceType = share_process ? (int)SERVICE_TYPE.SERVICE_WIN32_SHARE_PROCESS : (int)SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS;

			service_status.dwCurrentState = (int)status;

			if (status != ServiceControllerStatus.StartPending)
			{
				if (service.can_stop)
					service_status.dwControlsAccepted |= (int)SERVICE_CONTROL_ACCEPTED.SERVICE_ACCEPT_STOP;

				if (service.can_pause_and_continue)
					service_status.dwControlsAccepted |= (int)SERVICE_CONTROL_ACCEPTED.SERVICE_ACCEPT_PAUSE_CONTINUE;

				if (service.can_handle_power_event)
					service_status.dwControlsAccepted |= (int)SERVICE_CONTROL_ACCEPTED.SERVICE_ACCEPT_POWEREVENT;

				if (service.can_handle_session_change_event)
					service_status.dwControlsAccepted |= (int)SERVICE_CONTROL_ACCEPTED.SERVICE_ACCEPT_SESSIONCHANGE;

				if (service.can_shutdown)
					service_status.dwControlsAccepted |= (int)SERVICE_CONTROL_ACCEPTED.SERVICE_ACCEPT_SHUTDOWN;
			}

			service_status.dwWin32ExitCode = service.ExitCode;
			service_status.dwWaitHint = 5000;

			SetServiceStatus (service.service_handle, service_status);
		}

		private int Win32HandlerFn (int dwControl, int dwEventType, IntPtr lpEventData, IntPtr lpContext)
		{
			switch ((SERVICE_CONTROL_TYPE)dwControl)
			{
			case SERVICE_CONTROL_TYPE.SERVICE_CONTROL_STOP:
				if (can_stop)
				{
					Stop ();
					return NO_ERROR;
				}
				break;
			case SERVICE_CONTROL_TYPE.SERVICE_CONTROL_PAUSE:
				if (can_pause_and_continue)
				{
					SetStatus (ServiceControllerStatus.PausePending);
					OnPause ();
					SetStatus (ServiceControllerStatus.Paused);
					return NO_ERROR;
				}
				break;
			case SERVICE_CONTROL_TYPE.SERVICE_CONTROL_CONTINUE:
				if (can_pause_and_continue)
				{
					SetStatus (ServiceControllerStatus.ContinuePending);
					OnContinue ();
					SetStatus (ServiceControllerStatus.Running);
					return NO_ERROR;
				}
				break;
			case SERVICE_CONTROL_TYPE.SERVICE_CONTROL_INTERROGATE:
				return NO_ERROR;
			case SERVICE_CONTROL_TYPE.SERVICE_CONTROL_SHUTDOWN:
				if (can_shutdown)
				{
					OnShutdown ();
					return NO_ERROR;
				}
				break;
			default:
				break;
			}
			return ERROR_CALL_NOT_IMPLEMENTED;
		}

		[ComVisible (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[MonoTODO ("This only makes sense on Windows")]
		public void ServiceMainCallback (int argCount, IntPtr argPointer)
		{
			LPHANDLER_FUNCTION_EX handler = new LPHANDLER_FUNCTION_EX (Win32HandlerFn);
			// handler needs to last until the service stops

			service_handle = RegisterServiceCtrlHandlerEx (ServiceName ?? "", handler, IntPtr.Zero);

			if (service_handle != IntPtr.Zero)
			{
				SetStatus (ServiceControllerStatus.StartPending);
				
				stop_event = new ManualResetEvent (false);

				string[] args = new string[argCount];
				for (int i=0; i<argCount; i++)
				{
					IntPtr arg = Marshal.ReadIntPtr (argPointer, IntPtr.Size * i);
					args[i] = Marshal.PtrToStringUni (arg);
				}

				OnStart (args);

				SetStatus (ServiceControllerStatus.Running);

				stop_event.WaitOne ();

				SetStatus (ServiceControllerStatus.StopPending);

				OnStop ();

				SetStatus (ServiceControllerStatus.Stopped);
			}
		}

		private static void Win32RunService (ServiceBase [] services)
		{
			SERVICE_TABLE_ENTRY[] table = new SERVICE_TABLE_ENTRY[services.Length + 1];

			NotifyStatus = new NotifyStatusCallback (Win32NotifyStatus);

			for (int i = 0; i<services.Length; i++)
			{
				table[i].lpServiceName = services[i].ServiceName ?? "";
				table[i].lpServiceProc = new LPSERVICE_MAIN_FUNCTION (services[i].ServiceMainCallback);
			}

			// table[services.Length] is a NULL terminator

			share_process = (services.Length > 1);

			if (!StartServiceCtrlDispatcher (table))
				throw new Win32Exception ();
		}

		#endregion Win32 implementation

		public static void Run (ServiceBase service)
		{
			Run (new ServiceBase [] { service });
		}

		public static void Run (ServiceBase [] services)
		{
			int p = (int) Environment.OSVersion.Platform;

			if (RunService != null)
				RunService (services);
			else if (!(p == 4 || p == 128 || p == 6))
				Win32RunService (services);
			else
				Console.Error.WriteLine("Use mono-service to start service processes");
		}
	}
}
