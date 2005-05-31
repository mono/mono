//
// System.ServiceProcess.ServiceControllerPermission.cs
//	(based on System.Diagnostics.EventLogPermission.cs)
//
// Authors:
//	Jonathan Pryor (jonpryor@vt.edu)
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Jonathan Pryor
// (C) 2003 Andreas Nahr
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Security.Permissions;

namespace System.ServiceProcess {

	[Serializable]
	public sealed class ServiceControllerPermission : ResourcePermissionBase {

		ServiceControllerPermissionEntryCollection innerCollection;

		public ServiceControllerPermission ()
		{
			SetUp ();
		}

		public ServiceControllerPermission (ServiceControllerPermissionEntry[] permissionAccessEntries)
		{
			if (permissionAccessEntries == null)
				throw new ArgumentNullException ("permissionAccessEntries");

			SetUp ();
			innerCollection = new ServiceControllerPermissionEntryCollection (this);
			innerCollection.AddRange (permissionAccessEntries);
		}

		public ServiceControllerPermission (PermissionState state)
			: base (state)
		{
			SetUp ();
		}

		public ServiceControllerPermission (ServiceControllerPermissionAccess permissionAccess, string machineName, string serviceName)
		{
			SetUp ();
			ServiceControllerPermissionEntry scpe = new ServiceControllerPermissionEntry (permissionAccess, machineName, serviceName);
			innerCollection = new ServiceControllerPermissionEntryCollection (this);
			innerCollection.Add (scpe);
		}

		public ServiceControllerPermissionEntryCollection PermissionEntries {
			get {
				if (innerCollection == null) {
					// must be here to work with XML deserialization
					innerCollection = new ServiceControllerPermissionEntryCollection (this);
				}
				return innerCollection;
			}
		}

		// private stuff

		private void SetUp () 
		{
			TagNames = new string [2] { "Machine", "Service" };
			PermissionAccessType = typeof (ServiceControllerPermissionAccess);
		}

		internal ResourcePermissionBaseEntry[] GetEntries ()
		{
			return base.GetPermissionEntries ();
		}

		internal void ClearEntries ()
		{
			base.Clear ();
		}

		internal void Add (object obj) 
		{
			ServiceControllerPermissionEntry cspe = (obj as ServiceControllerPermissionEntry);
			base.AddPermissionAccess (cspe.GetBaseEntry ());
		}

		internal void Remove (object obj) 
		{
			ServiceControllerPermissionEntry cspe = (obj as ServiceControllerPermissionEntry);
			base.RemovePermissionAccess (cspe.GetBaseEntry ());
		}

		// static helpers

		private static char[] invalidChars = new char[] { '\t', '\n', '\v', '\f', '\r', ' ', '\\', '\x160' };

		internal static void ValidateMachineName (string name)
		{
			// FIXME: maybe other checks are required (but not documented)
			if ((name == null) || (name.Length == 0) || (name.IndexOfAny (invalidChars) != -1)) {
				string msg = Locale.GetText ("Invalid machine name '{0}'.");
				if (name == null)
					name = "(null)";
				msg = String.Format (msg, name);
				throw new ArgumentException (msg, "MachineName");
			}
		}

#if NET_2_0
		private static char[] invalidServiceNameChars = new char[] { '/', '\\' };

		internal static void ValidateServiceName (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("ServiceName");
			// FIXME: maybe other checks are required (but not documented)
			if ((name.Length == 0) || (name.IndexOfAny (invalidServiceNameChars) != -1)) {
				string msg = String.Format (Locale.GetText ("Invalid service name '{0}'."), name);
				throw new ArgumentException (msg, "ServiceName");
			}
		}
#else
		internal static void ValidateServiceName (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("ServiceName");
			// FIXME: maybe other checks are required (but not documented)
		}
#endif
	}
}
