//
// System.ServiceProcess.ServiceControllerPermissionEntry.cs
//
// Authors:
//      Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003, Ximian Inc.
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

using System.Collections;
using System.Security.Permissions;

namespace System.ServiceProcess {

	[Serializable]
	public class ServiceControllerPermissionEntryCollection : CollectionBase {

		private ServiceControllerPermission owner;

		internal ServiceControllerPermissionEntryCollection (ServiceControllerPermission owner)
		{
			this.owner = owner;
			ResourcePermissionBaseEntry[] entries = owner.GetEntries ();
			if (entries.Length > 0) {
				foreach (ResourcePermissionBaseEntry entry in entries) {
					ServiceControllerPermissionAccess scpa = (ServiceControllerPermissionAccess) entry.PermissionAccess;
					string machine = entry.PermissionAccessPath [0];
					string service = entry.PermissionAccessPath [1];
					ServiceControllerPermissionEntry scpe = new ServiceControllerPermissionEntry (scpa, machine, service);
					// we don't want to add them (again) to the base class
					InnerList.Add (scpe);
				}
			}
		}

		public ServiceControllerPermissionEntry this [int index] {
			get { return base.List [index] as ServiceControllerPermissionEntry; }
			set { base.List [index] = value; }
		}

		public int Add (ServiceControllerPermissionEntry value)
		{
			return base.List.Add (value);
		}

		public void AddRange (ServiceControllerPermissionEntry [] value)
		{
			foreach (ServiceControllerPermissionEntry entry in value)
				base.List.Add (entry);
		}

		public void AddRange (ServiceControllerPermissionEntryCollection value)
		{
			foreach (ServiceControllerPermissionEntry entry in value)
				base.List.Add (entry);
		}

		public bool Contains (ServiceControllerPermissionEntry value)
		{
			return base.List.Contains (value);
		}

		public void CopyTo (ServiceControllerPermissionEntry [] array, int index)
		{
			base.List.CopyTo (array, index);
		}

		public int IndexOf (ServiceControllerPermissionEntry value)
		{
			return base.List.IndexOf (value);
		}

		public void Insert (int index, ServiceControllerPermissionEntry value)
		{
			base.List.Insert (index, value);
		}

		public void Remove (ServiceControllerPermissionEntry value)
		{
			base.List.Remove (value);
		}

		protected override void OnClear ()
		{
			owner.ClearEntries ();
		}

		protected override void OnInsert (int index, object value)
		{
			owner.Add (value);
		}

		protected override void OnRemove (int index, object value)
		{
			owner.Remove (value);
		}

		protected override void OnSet (int index, object oldValue, object newValue)
		{
			owner.Remove (oldValue);
			owner.Add (newValue);
		}
	}
}
