//
// System.ServiceProcess.ServiceBase.cs
//
// Authors:
//      Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc and Cesar Octavio Lopez Nataren.
//


using System;
using System.Globalization;
using System.Diagnostics;

namespace System.ServiceProcess
{
	public class ServiceBase : System.ComponentModel.Component
	{
		public ServiceBase() { }

                public const int MaxNameLength = 80;

                bool hasStarted;
                
                bool auto_log;
                bool can_handle_power_event;
                bool can_pause_and_continue;
                bool can_shutdown;
                bool can_stop;
                EventLog event_log;
                string service_name;

                public bool AutoLog {

                        get { return auto_log; }

                        set { auto_log = value; }
                }

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
                
                public virtual EventLog EventLog {
                        get { return event_log; }
                }

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
                                
		protected override void Dispose (bool disposing) { }

		protected virtual void OnStart (string [] args) { }

		protected virtual void OnStop () { }

                public static void Run (ServiceBase service) { }

		public static void Run (ServiceBase [] ServicesToRun) { }

	}
}
