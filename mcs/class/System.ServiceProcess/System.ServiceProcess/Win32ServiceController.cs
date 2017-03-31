//
// System.ServiceProcess.Win32ServiceController
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.ServiceProcess
{
	internal class Win32ServiceController : ServiceControllerImpl
	{
		SERVICE_STATUS_PROCESS _status;

		public Win32ServiceController (ServiceController serviceController)
			: base (serviceController)
		{
		}

		public override bool CanPauseAndContinue {
			get {
				if ((int) _status.dwServiceType == 0)
					_status = GetServiceStatus (ServiceController.ServiceName,
						ServiceController.MachineName);
				return (_status.dwControlsAccepted & SERVICE_CONTROL_ACCEPTED.SERVICE_ACCEPT_PAUSE_CONTINUE) != 0;
			}
		}

		public override bool CanShutdown {
			get {
				if ((int) _status.dwServiceType == 0)
					_status = GetServiceStatus (ServiceController.ServiceName,
						ServiceController.MachineName);
				return (_status.dwControlsAccepted & SERVICE_CONTROL_ACCEPTED.SERVICE_ACCEPT_SHUTDOWN) != 0;
			}
		}

		public override bool CanStop {
			get {
				if ((int) _status.dwServiceType == 0)
					_status = GetServiceStatus (ServiceController.ServiceName,
						ServiceController.MachineName);
				return (_status.dwControlsAccepted & SERVICE_CONTROL_ACCEPTED.SERVICE_ACCEPT_STOP) != 0;
			}
		}

		public override ServiceController [] DependentServices {
			get {
				return GetDependentServices (ServiceController.ServiceName,
					ServiceController.MachineName);
			}
		}

		public override string DisplayName {
			get {
				string lookupName = ServiceController.ServiceName;

				IntPtr scHandle = IntPtr.Zero;
				try {
					scHandle = OpenServiceControlManager (ServiceController.MachineName,
						SERVICE_MANAGER_RIGHTS.SC_MANAGER_CONNECT);
					if (lookupName.Length == 0) {
						// if the service name is not available, then
						// assume the specified name is in fact already a display
						// name
						try {
							string serviceName = GetServiceName (scHandle, 
								lookupName);
							ServiceController.InternalServiceName = serviceName;
							ServiceController.Name = string.Empty;
							return lookupName;
						} catch (Win32Exception) {
						}
					}

					if (ServiceController.InternalDisplayName.Length == 0)
						return GetServiceDisplayName (scHandle, lookupName,
							ServiceController.MachineName);
					return ServiceController.InternalDisplayName;
				} finally {
					if (scHandle != IntPtr.Zero)
						CloseServiceHandle (scHandle);
				}
			}
		}

		public override string ServiceName {
			get {
				string lookupName = ServiceController.Name;
				if (lookupName.Length == 0)
					lookupName = ServiceController.InternalDisplayName;

				IntPtr scHandle = IntPtr.Zero;
				try {
					scHandle = OpenServiceControlManager (ServiceController.MachineName,
						SERVICE_MANAGER_RIGHTS.SC_MANAGER_CONNECT);

					// assume the specified name is in fact a display name
					try {
						string serviceName = GetServiceName (scHandle, lookupName);
						ServiceController.InternalDisplayName = lookupName;
						ServiceController.Name = string.Empty;
						return serviceName;
					} catch (Win32Exception) {
					}

					// instead of opening the service to verify whether it exists,
					// we'll try to get its displayname and hereby avoid looking
					// this up separately
					string displayName = GetServiceDisplayName (scHandle,
						lookupName, ServiceController.MachineName);
					ServiceController.InternalDisplayName = displayName;
					ServiceController.Name = string.Empty;
					return lookupName;
				} finally {
					if (scHandle != IntPtr.Zero)
						CloseServiceHandle (scHandle);
				}
			}
		}

		public override ServiceController [] ServicesDependedOn {
			get {
				return GetServiceDependencies (ServiceController.ServiceName,
					ServiceController.MachineName);
			}
		}

		public override ServiceType ServiceType {
			get {
				if ((int) _status.dwServiceType == 0)
					_status = GetServiceStatus (ServiceController.ServiceName,
						ServiceController.MachineName);
				return _status.dwServiceType;
			}
		}

		public override ServiceControllerStatus Status {
			get {
				if ((int) _status.dwServiceType == 0)
					_status = GetServiceStatus (ServiceController.ServiceName,
						ServiceController.MachineName);
				return _status.dwCurrentState;
			}
		}

		public override void Close ()
		{
			// clear status cache
			_status.dwServiceType = 0;
		}

		public override void Continue ()
		{
			string serviceName = ServiceController.ServiceName;
			string machineName = ServiceController.MachineName;
			IntPtr scHandle = IntPtr.Zero;
			IntPtr svcHandle = IntPtr.Zero;

			try {
				scHandle = OpenServiceControlManager (machineName,
					SERVICE_MANAGER_RIGHTS.SC_MANAGER_CONNECT);

				svcHandle = OpenService (scHandle, serviceName, SERVICE_RIGHTS.SERVICE_PAUSE_CONTINUE);
				if (svcHandle == IntPtr.Zero)
					throw CreateCannotOpenServiceException (serviceName,
						machineName);

				SERVICE_STATUS status = new SERVICE_STATUS ();
				if (!ControlService (svcHandle, SERVICE_CONTROL_TYPE.SERVICE_CONTROL_CONTINUE, ref status))
					throw new InvalidOperationException (string.Format (
						CultureInfo.CurrentCulture, "Cannot resume {0} service"
						+ " on computer '{1}'.", serviceName, machineName),
						new Win32Exception ());
			} finally {
				if (svcHandle != IntPtr.Zero)
					CloseServiceHandle (svcHandle);
				if (scHandle != IntPtr.Zero)
					CloseServiceHandle (scHandle);
			}
		}

		public override void Dispose (bool disposing)
		{
			// we're not keeping any handles open
		}

		public override void ExecuteCommand (int command)
		{
			string serviceName = ServiceController.ServiceName;
			string machineName = ServiceController.MachineName;
			IntPtr scHandle = IntPtr.Zero;
			IntPtr svcHandle = IntPtr.Zero;

			try {
				scHandle = OpenServiceControlManager (machineName,
					SERVICE_MANAGER_RIGHTS.SC_MANAGER_CONNECT);

				// MSDN: the hService handle must have the SERVICE_USER_DEFINED_CONTROL
				// access right
				svcHandle = OpenService (scHandle, serviceName, SERVICE_RIGHTS.SERVICE_USER_DEFINED_CONTROL);
				if (svcHandle == IntPtr.Zero)
					throw CreateCannotOpenServiceException (serviceName,
						machineName);

				SERVICE_STATUS status = new SERVICE_STATUS ();
				if (!ControlService (svcHandle, (SERVICE_CONTROL_TYPE) command, ref status))
					throw new InvalidOperationException (string.Format (
						CultureInfo.CurrentCulture, "Cannot control {0} service"
						+ " on computer '{1}'.", serviceName, machineName),
						new Win32Exception ());
			} finally {
				if (svcHandle != IntPtr.Zero)
					CloseServiceHandle (svcHandle);
				if (scHandle != IntPtr.Zero)
					CloseServiceHandle (scHandle);
			}
		}

		public override ServiceController [] GetDevices ()
		{
			return GetServices (ServiceController.MachineName,
				SERVICE_TYPE.SERVICE_DRIVER, null);
		}

		public override ServiceController [] GetServices ()
		{
			return GetServices (ServiceController.MachineName,
				SERVICE_TYPE.SERVICE_WIN32, null);
		}

		public override void Pause ()
		{
			string serviceName = ServiceController.ServiceName;
			string machineName = ServiceController.MachineName;
			IntPtr scHandle = IntPtr.Zero;
			IntPtr svcHandle = IntPtr.Zero;

			try {
				scHandle = OpenServiceControlManager (machineName,
					SERVICE_MANAGER_RIGHTS.SC_MANAGER_CONNECT);

				svcHandle = OpenService (scHandle, serviceName, SERVICE_RIGHTS.SERVICE_PAUSE_CONTINUE);
				if (svcHandle == IntPtr.Zero)
					throw CreateCannotOpenServiceException (serviceName,
						machineName);

				SERVICE_STATUS status = new SERVICE_STATUS ();
				if (!ControlService (svcHandle, SERVICE_CONTROL_TYPE.SERVICE_CONTROL_PAUSE, ref status))
					throw new InvalidOperationException (string.Format (
						CultureInfo.CurrentCulture, "Cannot pause {0} service"
						+ " on computer '{1}'.", serviceName, machineName),
						new Win32Exception ());
			} finally {
				if (svcHandle != IntPtr.Zero)
					CloseServiceHandle (svcHandle);
				if (scHandle != IntPtr.Zero)
					CloseServiceHandle (scHandle);
			}
		}

		public override void Refresh ()
		{
			// force refresh of status
			_status.dwServiceType = 0;
		}

		public override void Start (string [] args)
		{
			string serviceName = ServiceController.ServiceName;
			string machineName = ServiceController.MachineName;
			IntPtr scHandle = IntPtr.Zero;
			IntPtr svcHandle = IntPtr.Zero;
			IntPtr [] arguments = new IntPtr [args.Length];

			try {
				scHandle = OpenServiceControlManager (machineName,
					SERVICE_MANAGER_RIGHTS.SC_MANAGER_CONNECT);

				svcHandle = OpenService (scHandle, serviceName, SERVICE_RIGHTS.SERVICE_START);
				if (svcHandle == IntPtr.Zero)
					throw CreateCannotOpenServiceException (serviceName,
						machineName);

				for (int i = 0; i < args.Length; i++) {
					string argument = args [i];
					arguments [i] = Marshal.StringToHGlobalAnsi (argument);
				}

				if (!StartService (svcHandle, arguments.Length, arguments))
					throw new InvalidOperationException (string.Format (
						CultureInfo.CurrentCulture, "Cannot start {0} service"
						+ " on computer '{1}'.", serviceName, machineName),
						new Win32Exception ());
			} finally {
				for (int i = 0; i < arguments.Length; i++)
					Marshal.FreeHGlobal (arguments [i]);
				if (svcHandle != IntPtr.Zero)
					CloseServiceHandle (svcHandle);
				if (scHandle != IntPtr.Zero)
					CloseServiceHandle (scHandle);
			}

		}

		public override void Stop ()
		{
			string serviceName = ServiceController.ServiceName;
			string machineName = ServiceController.MachineName;
			IntPtr scHandle = IntPtr.Zero;
			IntPtr svcHandle = IntPtr.Zero;

			try {
				scHandle = OpenServiceControlManager (machineName,
					SERVICE_MANAGER_RIGHTS.SC_MANAGER_CONNECT);

				svcHandle = OpenService (scHandle, serviceName, SERVICE_RIGHTS.SERVICE_STOP);
				if (svcHandle == IntPtr.Zero)
					throw CreateCannotOpenServiceException (serviceName,
						machineName);

				SERVICE_STATUS status = new SERVICE_STATUS ();
				if (!ControlService (svcHandle, SERVICE_CONTROL_TYPE.SERVICE_CONTROL_STOP, ref status))
					throw new InvalidOperationException (string.Format (
						CultureInfo.CurrentCulture, "Cannot stop {0} service"
						+ " on computer '{1}'.", serviceName, machineName),
						new Win32Exception ());
			} finally {
				if (svcHandle != IntPtr.Zero)
					CloseServiceHandle (svcHandle);
				if (scHandle != IntPtr.Zero)
					CloseServiceHandle (scHandle);
			}
		}

		private static ServiceController [] GetDependentServices (string serviceName, string machineName)
		{
			IntPtr scHandle = IntPtr.Zero;
			IntPtr svcHandle = IntPtr.Zero;
			IntPtr buffer = IntPtr.Zero;

			try {
				scHandle = OpenServiceControlManager (machineName,
					SERVICE_MANAGER_RIGHTS.SC_MANAGER_CONNECT);

				svcHandle = OpenService (scHandle, serviceName, SERVICE_RIGHTS.SERVICE_ENUMERATE_DEPENDENTS);
				if (svcHandle == IntPtr.Zero)
					throw CreateCannotOpenServiceException (serviceName, machineName);

				uint bufferSize = 0;
				uint bytesNeeded = 0;
				uint servicesReturned = 0;

				ServiceController [] services;

				while (true) {
					if (!EnumDependentServices (svcHandle, SERVICE_STATE_REQUEST.SERVICE_STATE_ALL, buffer, bufferSize, out bytesNeeded, out servicesReturned)) {
						int err = Marshal.GetLastWin32Error ();
						if (err == ERROR_MORE_DATA) {
							buffer = Marshal.AllocHGlobal ((int) bytesNeeded);
							bufferSize = bytesNeeded;
						} else {
							throw new Win32Exception (err);
						}
					} else {
						IntPtr iPtr = buffer;

						services = new ServiceController [servicesReturned];
						for (int i = 0; i < servicesReturned; i++) {
							ENUM_SERVICE_STATUS serviceStatus = (ENUM_SERVICE_STATUS) Marshal.PtrToStructure (
								iPtr, typeof (ENUM_SERVICE_STATUS));
							// TODO: use internal ctor that takes displayname too ?
							services [i] = new ServiceController (serviceStatus.pServiceName,
								machineName);
							// move on to the next services
							iPtr = IntPtr.Add(iPtr, ENUM_SERVICE_STATUS.SizeOf);
						}

						// we're done, so exit the loop
						break;
					}
				}

				return services;
			} finally {
				if (scHandle != IntPtr.Zero)
					CloseServiceHandle (scHandle);
				if (svcHandle != IntPtr.Zero)
					CloseServiceHandle (svcHandle);
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal (buffer);
			}
		}

		private static ServiceController [] GetServiceDependencies (string serviceName, string machineName)
		{
			IntPtr scHandle = IntPtr.Zero;
			IntPtr svcHandle = IntPtr.Zero;
			IntPtr buffer = IntPtr.Zero;

			try {
				scHandle = OpenServiceControlManager (machineName,
					SERVICE_MANAGER_RIGHTS.SC_MANAGER_CONNECT);

				svcHandle = OpenService (scHandle, serviceName, SERVICE_RIGHTS.SERVICE_QUERY_CONFIG);
				if (svcHandle == IntPtr.Zero)
					throw CreateCannotOpenServiceException (serviceName, machineName);

				uint bufferSize = 0;
				uint bytesNeeded = 0;

				ServiceController [] services;

				while (true) {
					if (!QueryServiceConfig (svcHandle, buffer, bufferSize, out bytesNeeded)) {
						int err = Marshal.GetLastWin32Error ();
						if (err == ERROR_INSUFFICIENT_BUFFER) {
							buffer = Marshal.AllocHGlobal ((int) bytesNeeded);
							bufferSize = bytesNeeded;
						} else {
							throw new Win32Exception (err);
						}
					} else {
						QUERY_SERVICE_CONFIG config = (QUERY_SERVICE_CONFIG) Marshal.PtrToStructure (
							buffer, typeof (QUERY_SERVICE_CONFIG));

						Hashtable depServices = new Hashtable ();
						IntPtr iPtr = config.lpDependencies;
						StringBuilder sb = new StringBuilder ();
						string currentChar = Marshal.PtrToStringUni (iPtr, 1);
						while (currentChar != "\0") {
							sb.Append (currentChar);
							iPtr = new IntPtr (iPtr.ToInt64 () + Marshal.SystemDefaultCharSize);
							currentChar = Marshal.PtrToStringUni (iPtr, 1);
							if (currentChar != "\0") {
								continue;
							}
							iPtr = new IntPtr (iPtr.ToInt64 () + Marshal.SystemDefaultCharSize);
							currentChar = Marshal.PtrToStringUni (iPtr, 1);
							string dependency = sb.ToString ();
							if (dependency [0] == SC_GROUP_IDENTIFIER) {
								ServiceController [] groupServices = GetServices (
									machineName, SERVICE_TYPE.SERVICE_WIN32,
									dependency.Substring (1));
								foreach (ServiceController sc in groupServices) {
									if (!depServices.Contains (sc.ServiceName))
										depServices.Add (sc.ServiceName, sc);
								}
							} else if (!depServices.Contains (dependency)) {
								depServices.Add (dependency, new ServiceController (dependency, machineName));
							}
							sb.Length = 0;
						}

						services = new ServiceController [depServices.Count];
						depServices.Values.CopyTo (services, 0);
						break;
					}
				}

				return services;
			} finally {
				if (scHandle != IntPtr.Zero)
					CloseServiceHandle (scHandle);
				if (svcHandle != IntPtr.Zero)
					CloseServiceHandle (svcHandle);
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal (buffer);
			}
		}

		private static string GetServiceDisplayName (IntPtr scHandle, string serviceName, string machineName)
		{
			StringBuilder buffer = new StringBuilder ();

			uint bufferSize = (uint) buffer.Capacity;

			while (true) {
				if (!GetServiceDisplayName (scHandle, serviceName, buffer, ref bufferSize)) {
					int err = Marshal.GetLastWin32Error ();
					if (err == ERROR_INSUFFICIENT_BUFFER) {
						// allocate additional byte for terminating null char
						buffer = new StringBuilder ((int) bufferSize + 1);
						bufferSize = (uint) buffer.Capacity;
					} else {
						throw new InvalidOperationException (string.Format (
							CultureInfo.CurrentCulture, "Service {0} was not"
							+ " found on computer '{1}'.", serviceName,
							machineName), new Win32Exception ());
					}
				} else {
					return buffer.ToString ();
				}
			}
		}

		private static string GetServiceName (IntPtr scHandle, string displayName)
		{
			StringBuilder buffer = new StringBuilder ();

			uint bufferSize = (uint) buffer.Capacity;

			while (true) {
				if (!GetServiceKeyName (scHandle, displayName, buffer, ref bufferSize)) {
					int err = Marshal.GetLastWin32Error ();
					if (err == ERROR_INSUFFICIENT_BUFFER) {
						// allocate additional byte for terminating null char
						buffer = new StringBuilder ((int) bufferSize + 1);
						bufferSize = (uint) buffer.Capacity;
					} else {
						throw new Win32Exception ();
					}
				} else {
					return buffer.ToString ();
				}
			}
		}

		private static SERVICE_STATUS_PROCESS GetServiceStatus (string serviceName, string machineName)
		{
			IntPtr scHandle = IntPtr.Zero;
			IntPtr svcHandle = IntPtr.Zero;
			IntPtr buffer = IntPtr.Zero;

			try {
				scHandle = OpenServiceControlManager (machineName,
					SERVICE_MANAGER_RIGHTS.SC_MANAGER_CONNECT);

				svcHandle = OpenService (scHandle, serviceName, SERVICE_RIGHTS.SERVICE_QUERY_STATUS);
				if (svcHandle == IntPtr.Zero)
					throw CreateCannotOpenServiceException (serviceName, machineName);

				int bufferSize = 0;
				int bytesNeeded = 0;

				while (true) {
					if (!QueryServiceStatusEx (svcHandle, SC_STATUS_PROCESS_INFO, buffer, bufferSize, out bytesNeeded)) {
						int err = Marshal.GetLastWin32Error ();
						if (err == ERROR_INSUFFICIENT_BUFFER) {
							buffer = Marshal.AllocHGlobal (bytesNeeded);
							bufferSize = bytesNeeded;
						} else {
							throw new Win32Exception (err);
						}
					} else {
						SERVICE_STATUS_PROCESS serviceStatus = (SERVICE_STATUS_PROCESS) Marshal.PtrToStructure (
							buffer, typeof (SERVICE_STATUS_PROCESS));
						return serviceStatus;
					}
				}
			} finally {
				if (scHandle != IntPtr.Zero)
					CloseServiceHandle (scHandle);
				if (svcHandle != IntPtr.Zero)
					CloseServiceHandle (svcHandle);
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal (buffer);
			}
		}

		private static ServiceController [] GetServices (string machineName, SERVICE_TYPE serviceType, string group)
		{
			IntPtr scHandle = IntPtr.Zero;
			IntPtr buffer = IntPtr.Zero;

			try {
				scHandle = OpenServiceControlManager (machineName, 
					SERVICE_MANAGER_RIGHTS.SC_MANAGER_ENUMERATE_SERVICE);

				uint bufferSize = 0;
				uint bytesNeeded = 0;
				uint servicesReturned = 0;
				uint resumeHandle = 0;

				ServiceController [] services;

				while (true) {
					if (!EnumServicesStatusEx (scHandle, SC_ENUM_PROCESS_INFO, serviceType, SERVICE_STATE_REQUEST.SERVICE_STATE_ALL, buffer, bufferSize, out bytesNeeded, out servicesReturned, ref resumeHandle, group)) {
						int err = Marshal.GetLastWin32Error ();
						if (err == ERROR_MORE_DATA) {
							buffer = Marshal.AllocHGlobal ((int) bytesNeeded);
							bufferSize = bytesNeeded;
						} else {
							throw new Win32Exception (err);
						}
					} else {
						IntPtr iPtr = buffer;

						services = new ServiceController [servicesReturned];
						for (int i = 0; i < servicesReturned; i++) {
							ENUM_SERVICE_STATUS_PROCESS serviceStatus = (ENUM_SERVICE_STATUS_PROCESS) Marshal.PtrToStructure (
								iPtr, typeof (ENUM_SERVICE_STATUS_PROCESS));
							// TODO: use internal ctor that takes displayname too
							services [i] = new ServiceController (serviceStatus.pServiceName,
								machineName);
							// move on to the next services
							iPtr = IntPtr.Add(iPtr, ENUM_SERVICE_STATUS_PROCESS.SizeOf);
						}

						// we're done, so exit the loop
						break;
					}
				}

				return services;
			} finally {
				if (scHandle != IntPtr.Zero)
					CloseServiceHandle (scHandle);
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal (buffer);
			}
		}

		private static IntPtr OpenServiceControlManager (string machineName, SERVICE_MANAGER_RIGHTS rights)
		{
			return OpenServiceControlManager (machineName, rights, false);
		}

		private static IntPtr OpenServiceControlManager (string machineName, SERVICE_MANAGER_RIGHTS rights, bool ignoreWin32Error)
		{
				IntPtr scHandle = OpenSCManager (machineName, SERVICES_ACTIVE_DATABASE,
					rights);
				if (scHandle == IntPtr.Zero) {
					string msg = string.Format (CultureInfo.CurrentCulture,
						"Cannot open Service Control Manager on computer '{0}'."
						+ " This operation might require other priviliges.",
						machineName);
					if (ignoreWin32Error)
						throw new InvalidOperationException (msg);
					throw new InvalidOperationException (msg, new Win32Exception ());
				}
				return scHandle;
		}

		private static InvalidOperationException CreateCannotOpenServiceException (string serviceName, string machineName)
		{
			return new InvalidOperationException (string.Format (CultureInfo.CurrentCulture,
				"Cannot open {0} service on computer '{1}'.", serviceName, machineName),
				new Win32Exception ());
		}

		#region PInvoke declaration

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern void CloseServiceHandle (IntPtr SCHANDLE);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool ControlService (
			IntPtr hService,
			SERVICE_CONTROL_TYPE dwControl,
			ref SERVICE_STATUS lpServiceStatus);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool EnumDependentServices (
			IntPtr hService,
			SERVICE_STATE_REQUEST dwServiceState,
			IntPtr lpServices,
			uint cbBufSize,
			out uint pcbBytesNeeded,
			out uint lpServicesReturned);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool EnumServicesStatusEx (
			IntPtr hSCManager,
			int InfoLevel,
			SERVICE_TYPE dwServiceType,
			SERVICE_STATE_REQUEST dwServiceState,
			IntPtr lpServices,
			uint cbBufSize,
			out uint pcbBytesNeeded,
			out uint lpServicesReturned,
			ref uint lpResumeHandle,
			string pszGroupName);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool GetServiceDisplayName (
			IntPtr hSCManager,
			string lpServiceName,
			StringBuilder lpDisplayName,
			ref uint lpcchBuffer);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool GetServiceKeyName (
			IntPtr hSCManager,
			string lpDisplayName,
			StringBuilder lpServiceName,
			ref uint lpcchBuffer);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr OpenSCManager (
			string lpMachineName,
			string lpSCDB,
			SERVICE_MANAGER_RIGHTS scParameter);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr OpenService (
			IntPtr SCHANDLE,
			string lpSvcName,
			SERVICE_RIGHTS dwNumServiceArgs);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool QueryServiceConfig (
			IntPtr hService,
			IntPtr lpServiceConfig,
			uint cbBufSize,
			out uint pcbBytesNeeded);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool QueryServiceStatusEx (
			IntPtr serviceHandle,
			int InfoLevel,
			IntPtr lpBuffer,
			int cbBufSize,
			out int pcbBytesNeeded);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool StartService (
			IntPtr SVHANDLE,
			int dwNumServiceArgs,
			IntPtr [] lpServiceArgVectors);

		private const int SC_ENUM_PROCESS_INFO = 0;
		private const char SC_GROUP_IDENTIFIER = '+';
		private const int SC_STATUS_PROCESS_INFO = 0;
		private const int SERVICE_NO_CHANGE = -1;
		private const int ERROR_MORE_DATA = 234;
		private const int ERROR_INSUFFICIENT_BUFFER = 122;
		private const int STANDARD_RIGHTS_REQUIRED = 0xf0000;
		private const string SERVICES_ACTIVE_DATABASE = "ServicesActive";

		internal struct QUERY_SERVICE_CONFIG
		{
			public int dwServiceType;
			public int dwStartType;
			public int dwErrorControl;
			public IntPtr lpBinaryPathName;
			public IntPtr lpLoadOrderGroup;
			public int dwTagId;
			public IntPtr lpDependencies;
			public IntPtr lpServiceStartName;
			public IntPtr lpDisplayName;
		}

		[Flags]
		private enum SERVICE_RIGHTS
		{
			SERVICE_QUERY_CONFIG = 1,
			SERVICE_CHANGE_CONFIG = 2,
			SERVICE_QUERY_STATUS = 4,
			SERVICE_ENUMERATE_DEPENDENTS = 8,
			SERVICE_START = 16,
			SERVICE_STOP = 32,
			SERVICE_PAUSE_CONTINUE = 64,
			SERVICE_INTERROGATE = 128,
			SERVICE_USER_DEFINED_CONTROL = 256,
			SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SERVICE_QUERY_CONFIG |
				SERVICE_CHANGE_CONFIG | SERVICE_QUERY_STATUS |
				SERVICE_ENUMERATE_DEPENDENTS | SERVICE_START | SERVICE_STOP |
				SERVICE_PAUSE_CONTINUE | SERVICE_INTERROGATE |
				SERVICE_USER_DEFINED_CONTROL)
		}

		private enum SERVICE_MANAGER_RIGHTS : uint
		{
			STANDARD_RIGHTS_READ = 0x20000,
			STANDARD_RIGHTS_WRITE = 0x20000,
			STANDARD_RIGHTS_EXECUTE = 0x20000,
			STANDARD_RIGHTS_ALL = 0x1F0000,

			SC_MANAGER_ALL_ACCESS = 0xf003f,
			SC_MANAGER_CONNECT = 1,
			SC_MANAGER_CREATE_SERVICE = 2,
			SC_MANAGER_ENUMERATE_SERVICE = 4,
			SC_MANAGER_LOCK = 8,
			SC_MANAGER_QUERY_LOCK_STATUS = 16,
			SC_MANAGER_MODIFY_BOOT_CONFIG = 32,

			GENERIC_READ = 0x80000000,
			GENERIC_WRITE = 0x40000000,
			GENERIC_EXECUTE = 0x20000000,
			GENERIC_ALL = 0x10000000
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct ENUM_SERVICE_STATUS_PROCESS
		{
			public static readonly int SizeOf = Marshal.SizeOf (typeof (ENUM_SERVICE_STATUS_PROCESS));

			[MarshalAs (UnmanagedType.LPWStr)]
			public string pServiceName;

			[MarshalAs (UnmanagedType.LPWStr)]
			public string pDisplayName;

			public SERVICE_STATUS_PROCESS ServiceStatus;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct ENUM_SERVICE_STATUS
		{
			public static readonly int SizeOf = Marshal.SizeOf (typeof (ENUM_SERVICE_STATUS));

			[MarshalAs (UnmanagedType.LPWStr)]
			public string pServiceName;
			[MarshalAs (UnmanagedType.LPWStr)]
			public string pDisplayName;
			public SERVICE_STATUS ServiceStatus;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct SERVICE_STATUS
		{
			public ServiceType dwServiceType;
			public ServiceControllerStatus dwCurrentState;
			public SERVICE_CONTROL_ACCEPTED dwControlsAccepted;
			public int dwWin32ExitCode;
			public int dwServiceSpecificExitCode;
			public uint dwCheckPoint;
			public uint dwWaitHint;
		}

		[StructLayout (LayoutKind.Sequential, Pack = 1)]
		private struct SERVICE_STATUS_PROCESS
		{
			public static readonly int SizeOf = Marshal.SizeOf (typeof (SERVICE_STATUS_PROCESS));

			public ServiceType dwServiceType;
			public ServiceControllerStatus dwCurrentState;
			public SERVICE_CONTROL_ACCEPTED dwControlsAccepted;
			public int dwWin32ExitCode;
			public int dwServiceSpecificExitCode;
			public int dwCheckPoint;
			public int dwWaitHint;
			public int dwProcessId;
			public int dwServiceFlags;
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

		private enum SERVICE_START_TYPE
		{
			SERVICE_BOOT_START = 0x0,
			SERVICE_SYSTEM_START = 0x1,
			SERVICE_AUTO_START = 0x2,
			SERVICE_DEMAND_START = 0x3,
			SERVICE_DISABLED = 0x4,
			SERVICESTARTTYPE_NO_CHANGE = SERVICE_NO_CHANGE
		}

		private enum SERVICE_ERROR_CONTROL
		{
			SERVICE_ERROR_IGNORE = 0x0,
			SERVICE_ERROR_NORMAL = 0x1,
			SERVICE_ERROR_SEVERE = 0x2,
			SERVICE_ERROR_CRITICAL = 0x3,
			msidbServiceInstallErrorControlVital = 0x8000,
			SERVICEERRORCONTROL_NO_CHANGE = SERVICE_NO_CHANGE
		}

		private enum SERVICE_STATE_REQUEST
		{
			SERVICE_ACTIVE = 0x1,
			SERVICE_INACTIVE = 0x2,
			SERVICE_STATE_ALL = (SERVICE_ACTIVE | SERVICE_INACTIVE)
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

		#endregion PInvoke declaration
	}
}
