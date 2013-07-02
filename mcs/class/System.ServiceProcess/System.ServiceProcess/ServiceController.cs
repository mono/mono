//
// System.ServiceProcess.ServiceController 
//
// Authors:
//	Marek Safar (marek.safar@seznam.cz)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005, Marek Safar
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
// TODO: check if there's more information to cache (eg. status)
// Start / Stop / ...

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ServiceProcess.Design;
using System.Threading;

namespace System.ServiceProcess
{
	[Designer("System.ServiceProcess.Design.ServiceControllerDesigner, " + Consts.AssemblySystem_Design)]
	[MonoTODO ("No unix implementation")]
	[ServiceProcessDescription ("Provides the ability to connect to, query, and manipulate running or stopped Windows services.")]
	public class ServiceController : Component
	{
		private string _name;
		private string _serviceName = string.Empty;
		private string _machineName;
		private string _displayName = string.Empty;
		private readonly ServiceControllerImpl _impl;
		private ServiceController [] _dependentServices;
		private ServiceController [] _servicesDependedOn;

		public ServiceController ()
		{
			_machineName = ".";
			_name = string.Empty;
			_impl = CreateServiceControllerImpl (this);
		}

		public ServiceController (string name) : this (name, ".")
		{
		}

		public ServiceController (string name, string machineName)
		{
			if (name == null || name.Length == 0)
				throw new ArgumentException (string.Format (CultureInfo.CurrentCulture,
					"Invalid value {0} for parameter name.", name));

			ValidateMachineName (machineName);

			_machineName = machineName;
			_name = name;
			_impl = CreateServiceControllerImpl (this);
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("Whether this service recognizes the Pause and Continue commands.")]
		public bool CanPauseAndContinue {
			get {
				ValidateServiceName (ServiceName);
				return _impl.CanPauseAndContinue;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("Whether this service can respond to a system shutdown.")]
		public bool CanShutdown {
			get
			{
				ValidateServiceName (ServiceName);
				return _impl.CanShutdown;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("Whether this service can be stopped.")]
		public bool CanStop {
			get
			{
				ValidateServiceName (ServiceName);
				return _impl.CanStop;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("The services that depend on this service in order to run.")]
		public ServiceController [] DependentServices {
			get
			{
				ValidateServiceName (ServiceName);
				if (_dependentServices == null)
					_dependentServices = _impl.DependentServices;
				return _dependentServices;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[ServiceProcessDescription ("The descriptive name of the service.")]
		public string DisplayName {
			get {
				if (_displayName.Length == 0 && (_serviceName.Length > 0 || _name.Length > 0))
					_displayName = _impl.DisplayName;
				return _displayName;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (_displayName == value)
					return;

				_displayName = value;

				// if display name is modified, then we also need to force a
				// new lookup of the corresponding service name
				_serviceName = string.Empty;

				// you'd expect the DependentServices and ServiceDependedOn cache
				// to be cleared too, but the MS implementation doesn't do this
				//
				// categorized as by design:
				// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762

				// release any handles and clear cache
				Close ();
			}
		}

		[Browsable (false)]
		[DefaultValue (".")]
		[RecommendedAsConfigurable (true)]
		[ServiceProcessDescription ("The name of the machine on which this service resides.")]
		public string MachineName {
			get {
				return _machineName;
			}
			set {
				ValidateMachineName (value);

				if (_machineName == value)
					return;

				_machineName = value;

				// you'd expect the DependentServices and ServiceDependedOn cache
				// to be cleared too, but the MS implementation doesn't do this
				//
				// categorized as by design:
				// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762

				// release any handles and clear cache
				Close ();
			}
		}

		[DefaultValue ("")]
		[ReadOnly (true)]
		[RecommendedAsConfigurable (true)]
		[ServiceProcessDescription ("The short name of the service.")]
		[TypeConverter (typeof (ServiceNameConverter))]
		public string ServiceName {
			get {
				if (_serviceName.Length == 0 && (_displayName.Length > 0 || _name.Length > 0))
					_serviceName = _impl.ServiceName;
				return _serviceName;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (_serviceName == value)
					return;

				ValidateServiceName (value);

				_serviceName = value;

				// if service name is modified, then we also need to force a
				// new lookup of the corresponding display name
				_displayName = string.Empty;

				// you'd expect the DependentServices and ServiceDependedOn cache
				// to be cleared too, but the MS implementation doesn't do this
				//
				// categorized as by design:
				// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762

				// release any handles and clear cache
				Close ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("Services that must be started in order for this one to start.")]
		public ServiceController [] ServicesDependedOn {
			get
			{
				ValidateServiceName (ServiceName);
				if (_servicesDependedOn == null)
					_servicesDependedOn = _impl.ServicesDependedOn;
				return _servicesDependedOn;
			}
		}

		[MonoTODO]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SafeHandle ServiceHandle 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("The type of this service.")]
		public ServiceType ServiceType {
			get
			{
				ValidateServiceName (ServiceName);
				return _impl.ServiceType;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("The status of the service, e.g., Running or Stopped.")]
		public ServiceControllerStatus Status {
			get
			{
				ValidateServiceName (ServiceName);
				return _impl.Status;
			}
		}

		public void Close () 
		{
			_impl.Close ();
		}

		public void Continue ()
		{
			ValidateServiceName (ServiceName);
			_impl.Continue ();
		}

		protected override void Dispose (bool disposing)
		{
			_impl.Dispose (disposing);
			base.Dispose (disposing);
		}

		public void ExecuteCommand (int command)
		{
			ValidateServiceName (ServiceName);
			_impl.ExecuteCommand (command);
		}

		public static ServiceController[] GetDevices ()
		{
			return GetDevices (".");
		}

		public static ServiceController[] GetDevices (string machineName)
		{
			ValidateMachineName (machineName);

			using (ServiceController sc = new ServiceController ("dummy", machineName)) {
				ServiceControllerImpl impl = CreateServiceControllerImpl (sc);
				return impl.GetDevices ();
			}
		}

		public static ServiceController[] GetServices ()
		{
			return GetServices (".");
		}

		public static ServiceController[] GetServices (string machineName)
		{
			ValidateMachineName (machineName);

			using (ServiceController sc = new ServiceController ("dummy", machineName)) {
				ServiceControllerImpl impl = CreateServiceControllerImpl (sc);
				return impl.GetServices ();
			}
		}

		public void Pause ()
		{
			ValidateServiceName (ServiceName);
			_impl.Pause ();
		}

		public void Refresh ()
		{
			// MSDN: this method also sets the  ServicesDependedOn and 
			// DependentServices properties to a null reference
			//
			// I assume they wanted to say that the cache for these properties
			// is cleared. Verified by unit tests.
			_dependentServices = null;
			_servicesDependedOn = null;
			_impl.Refresh ();
		}

		public void Start () 
		{
			Start (new string [0]);
		}

		public void Start (string [] args)
		{
			ValidateServiceName (ServiceName);
			_impl.Start (args);
		}

		public void Stop ()
		{
			ValidateServiceName (ServiceName);
			_impl.Stop ();
		}

		public void WaitForStatus (ServiceControllerStatus desiredStatus)
		{
			WaitForStatus (desiredStatus, TimeSpan.MaxValue);
		}

		public void WaitForStatus (ServiceControllerStatus desiredStatus, TimeSpan timeout)
		{
			ValidateServiceName (ServiceName);

			DateTime start = DateTime.Now;
			while (Status != desiredStatus) {
				if (timeout  < (DateTime.Now - start))
					throw new TimeoutException ("Time out has expired and the"
						+ " operation has not been completed.");
				Thread.Sleep (100);
				// force refresh of status
				Refresh ();
			}
		}

		internal string Name {
			get {
				return _name;
			}
			set {
				_name = value;
			}
		}

		internal string InternalDisplayName {
			get {
				return _displayName;
			}
			set {
				_displayName = value;
			}
		}

		internal string InternalServiceName {
			get {
				return _serviceName;
			}
			set {
				_serviceName = value;
			}
		}

		private static void ValidateServiceName (string serviceName)
		{
			if (serviceName.Length == 0 || serviceName.Length > 80)
				throw new ArgumentException (string.Format (CultureInfo.CurrentCulture,
					"Service name {0} contains invalid characters, is empty"
					+ " or is too long (max length = 80).", serviceName));
		}

		private static void ValidateMachineName (string machineName)
		{
			if (machineName == null || machineName.Length == 0)
				throw new ArgumentException (string.Format (CultureInfo.CurrentCulture,
					"MachineName value {0} is invalid.", machineName));
		}

		private static ServiceControllerImpl CreateServiceControllerImpl (ServiceController serviceController)
		{
			int p = (int) Environment.OSVersion.Platform;

			if (p == 4 || p == 128 || p == 6){
				return new UnixServiceController (serviceController);
			} else {
				return new Win32ServiceController (serviceController);
			}
		}
	}
}
