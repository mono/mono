//
// System.ServiceProcess.ServiceBase.cs
//
// Authors:
//      Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//      Duncan Mak (duncan@ximian.com)
//      Joerg Rosenkranz (joergr@voelcker.com)
//
// (C) 2003, Ximian Inc and Cesar Octavio Lopez Nataren.
// (C) 2005, Voelcker Informatik AG
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

		public const int MaxNameLength = 80;

		bool hasStarted;
		bool auto_log = true;
		bool can_handle_power_event;
		bool can_pause_and_continue;
		bool can_shutdown;
		bool can_stop = true;
		EventLog event_log;
		string service_name;
#if NET_2_0
		bool can_handle_session_change_event;
#endif

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

#if NET_2_0
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
#endif

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

#if NET_2_0
		[ComVisible (false)]
		public int ExitCode { get; set; }

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected IntPtr ServiceHandle {
			get { throw new NotImplementedException (); }
		}
#endif

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

#if NET_2_0
		protected virtual void OnSessionChange (SessionChangeDescription changeDescription)
		{
		}

		[ComVisible (false)]
		[MonoTODO]
		public void RequestAdditionalTime (int milliseconds)
		{
			throw new NotImplementedException ();
		}

		[ComVisible (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[MonoTODO]
		public void ServiceMainCallback (int argCount, IntPtr argPointer)
		{
			throw new NotImplementedException ();
		}

		public void Stop ()
		{
			OnStop ();
		}
#endif

		public static void Run (ServiceBase service)
		{
			Run (new ServiceBase [] { service });
		}

		public static void Run (ServiceBase [] services)
		{
			if (RunService != null)
				RunService (services);
		}
	}
}
