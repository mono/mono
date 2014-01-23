//
// System.ServiceProcess.ServiceInstaller.cs
//
// Authors:
//	Geoff Norton (gnorton@customerdna.com)
//
// (C) 2005, Geoff Norton
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
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.ServiceProcess
{
	[MonoTODO]
	public class ServiceInstaller : ComponentInstaller
	{
		public ServiceInstaller ()
		{
		}
		
		private string display_name;
		private string service_name;
		private string[] services_depended_on;
		private ServiceStartMode start_type;
#if NET_2_0
		private string description;
#endif
#if NET_4_0
		private bool delayedAutoStart;
#endif

#if NET_4_0
		[DefaultValue(false)]
		[ServiceProcessDescription("Indicates that the service's start should be delayed after other automatically started services have started.")]
		public bool DelayedAutoStart {
			get {
				return delayedAutoStart;
			}
			set {
				delayedAutoStart = value;
			}
		}
#endif

#if NET_2_0
		[ComVisible (false)]
		[DefaultValue ("")]
		[ServiceProcessDescription ("Indicates the service's description (a brief comment that explains the purpose of the service). ")]
		public string Description {
			get {
				return description;
			}
			set {
				description = value;
			}
		}
#endif

		[DefaultValue("")]
#if NET_2_0
		[ServiceProcessDescription ("Indicates the friendly name that identifies the service to the user.")]
#endif
		public string DisplayName {
			get {
				return display_name;
			}
			set {
				display_name = value;
			}
		}

		[DefaultValue("")]
#if NET_2_0
		[ServiceProcessDescription ("Indicates the name used by the system to identify this service.")]
#endif
		[TypeConverter("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string ServiceName {
			get {
				return service_name;
			}
			set {
				if (value == null || value.Length == 0 || value.Length > 256)
					throw new ArgumentException ();
				service_name = value;
			}
		}

#if NET_2_0
		[ServiceProcessDescription ("Indicates the services that must be running in order for this service to run.")]
#endif
		public string[] ServicesDependedOn {
			get {
				return services_depended_on;
			}
			set {
				services_depended_on = value;
			}
		}

		[DefaultValue (ServiceStartMode.Manual)]
#if NET_2_0
		[ServiceProcessDescription ("Indicates how and when this service is started.")]
#endif
		public ServiceStartMode StartType {
			get {
				return start_type;
			}
			set {
				start_type = value;
			}
		}

		public override void CopyFromComponent (IComponent component)
		{
			if (!component.GetType ().IsSubclassOf (typeof (ServiceBase)))
				throw new ArgumentException ();
		}

		public override void Install (IDictionary stateSaver)
		{
			throw new NotImplementedException ();
		}
	
		public override bool IsEquivalentInstaller (ComponentInstaller otherInstaller)
		{
			throw new NotImplementedException ();
		}

		public override void Rollback (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}

		public override void Uninstall (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}
	}
}
