//
// System.Diagnostics.PerformanceCounterPermission.cs
//
// Authors:
//	Jonathan Pryor (jonpryor@vt.edu)
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002
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
	public sealed class PerformanceCounterPermission : ResourcePermissionBase {

		PerformanceCounterPermissionEntryCollection innerCollection;

		public PerformanceCounterPermission ()
		{
			SetUp ();
		}

		public PerformanceCounterPermission (PerformanceCounterPermissionEntry[] permissionAccessEntries)
		{
			if (permissionAccessEntries == null)
				throw new ArgumentNullException("permissionAccessEntries");

			SetUp ();
			innerCollection = new PerformanceCounterPermissionEntryCollection (this);
			innerCollection.AddRange (permissionAccessEntries);
		}

		public PerformanceCounterPermission (PermissionState state)
			: base (state)
		{
			SetUp ();
		}

		public PerformanceCounterPermission (PerformanceCounterPermissionAccess permissionAccess, string machineName, string categoryName)
		{
			SetUp ();
			innerCollection = new PerformanceCounterPermissionEntryCollection (this);
			innerCollection.Add (new PerformanceCounterPermissionEntry (permissionAccess, machineName, categoryName));
		}

		public PerformanceCounterPermissionEntryCollection PermissionEntries {
			get {
				if (innerCollection == null) {
					// must be here to work with XML deserialization
					innerCollection = new PerformanceCounterPermissionEntryCollection (this);
				}
				return innerCollection;
			}
		}

		// private stuff

		private void SetUp () 
		{
			TagNames = new string [2] { "Machine", "Category" };
			PermissionAccessType = typeof (PerformanceCounterPermissionAccess);
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
			PerformanceCounterPermissionEntry pcpe = (obj as PerformanceCounterPermissionEntry);
			base.AddPermissionAccess (pcpe.CreateResourcePermissionBaseEntry ());
		}

		internal void Remove (object obj) 
		{
			PerformanceCounterPermissionEntry pcpe = (obj as PerformanceCounterPermissionEntry);
			base.RemovePermissionAccess (pcpe.CreateResourcePermissionBaseEntry ());
		}
	}
}

