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

namespace System.ServiceProcess
{
	public class ServiceInstaller : System.Configuration.Install.ComponentInstaller
	{
		public ServiceInstaller () {}

		private string display_name;
		private string service_name;
		private string[] services_depended_on;
		private ServiceStartMode start_type;

		public string DisplayName {
			get {
				return display_name;
			}
			set {
				display_name = value;
			}
		}

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

		public string[] ServicesDependedOn {
			get {
				return services_depended_on;
			}
			set {
				services_depended_on = value;
			}
		}

		public ServiceStartMode StartType {
			get {
				return start_type;
			}
			set {
				start_type = value;
			}
		}

		public override void CopyFromComponent (IComponent component) {
			if (!component.GetType ().IsSubclassOf (typeof (ServiceBase)))
				throw new ArgumentException ();
		}

		public override void Install (IDictionary stateSaver) {
			throw new NotImplementedException ();
		}
	
		public override bool IsEquivalentInstaller (ComponentInstaller otherInstaller) {
			throw new NotImplementedException ();
		}

		public override void Rollback (IDictionary savedState) {
			throw new NotImplementedException ();
		}

		public override void Uninstall (IDictionary savedState) {
			throw new NotImplementedException ();
		}
	}
}
