//
// System.ServiceProcess.ServiceProcessInstaller.cs
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
	public class ServiceProcessInstaller : System.Configuration.Install.ComponentInstaller
	{
		public ServiceProcessInstaller ()
		{
		}

		private ServiceAccount account;
		private string password;
		private string username;

		[DefaultValue (ServiceAccount.User)]
#if NET_2_0
		[ServiceProcessDescription ("Indicates the account type under which the service will run.")]
#endif
		public ServiceAccount Account {
			get {
				return account;
			}
			set {
				account = value;
			}
		}

		public override string HelpText {
			get {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		public string Password {
			get {
				return password;
			}
			set {
				password = value;
			}
		}

		[Browsable (false)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string Username {
			get {
				return username;
			}
			set {
				username = value;
			}
		}

		public override void CopyFromComponent (IComponent comp)
		{
			if (!comp.GetType ().IsSubclassOf (typeof (ServiceBase)))
				throw new ArgumentException ();
		}

		public override void Install (IDictionary stateSaver)
		{
			throw new NotImplementedException ();
		}
	
		public override void Rollback (IDictionary savedState)
		{
			throw new NotImplementedException ();
		}
	}
}
