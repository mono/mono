//
// System.Diagnostics.EventLogPermission.cs
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

namespace System.Diagnostics {

	[Serializable]
	public sealed class EventLogPermission : ResourcePermissionBase {

		EventLogPermissionEntryCollection innerCollection;

		public EventLogPermission ()
		{
			SetUp ();
		}

		public EventLogPermission (EventLogPermissionEntry[] permissionAccessEntries)
		{
			if (permissionAccessEntries == null)
				throw new ArgumentNullException ("permissionAccessEntries");

			SetUp ();
			innerCollection = new EventLogPermissionEntryCollection (this);
			innerCollection.AddRange (permissionAccessEntries);
		}

		public EventLogPermission (PermissionState state)
			: base (state)
		{
			SetUp ();
		}

		public EventLogPermission (EventLogPermissionAccess permissionAccess, string machineName)
		{
			SetUp ();
			innerCollection = new EventLogPermissionEntryCollection (this);
			innerCollection.Add (new EventLogPermissionEntry (permissionAccess, machineName));
		}

		public EventLogPermissionEntryCollection PermissionEntries {
			get {
				if (innerCollection == null) {
					// must be here to work with XML deserialization
					innerCollection = new EventLogPermissionEntryCollection (this);
				}
				return innerCollection;
			}
		}

		// private stuff

		private void SetUp () 
		{
			TagNames = new string [1] { "Machine" };
			PermissionAccessType = typeof (EventLogPermissionAccess);
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
			EventLogPermissionEntry elpe = (obj as EventLogPermissionEntry);
			base.AddPermissionAccess (elpe.CreateResourcePermissionBaseEntry ());
		}

		internal void Remove (object obj) 
		{
			EventLogPermissionEntry elpe = (obj as EventLogPermissionEntry);
			base.RemovePermissionAccess (elpe.CreateResourcePermissionBaseEntry ());
		}
	}
}
