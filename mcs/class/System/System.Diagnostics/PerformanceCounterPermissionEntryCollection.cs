//
// System.Diagnostics.PerformanceCounterPermissionEntryCollection.cs
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

using System.Collections;
using System.Globalization;
using System.Security.Permissions;

namespace System.Diagnostics {

	[Serializable]
	public class PerformanceCounterPermissionEntryCollection : CollectionBase {

		private PerformanceCounterPermission owner;

		internal PerformanceCounterPermissionEntryCollection (PerformanceCounterPermission owner)
		{
			this.owner = owner;
			ResourcePermissionBaseEntry[] entries = owner.GetEntries ();
			if (entries.Length > 0) {
				foreach (ResourcePermissionBaseEntry entry in entries) {
					PerformanceCounterPermissionAccess elpa = (PerformanceCounterPermissionAccess) entry.PermissionAccess;
					string machine = entry.PermissionAccessPath [0];
					string category = entry.PermissionAccessPath [1];
					PerformanceCounterPermissionEntry elpe = new PerformanceCounterPermissionEntry (elpa, machine, category);
					// we don't want to add them (again) to the base class
					InnerList.Add (elpe);
				}
			}
		}

		internal PerformanceCounterPermissionEntryCollection (ResourcePermissionBaseEntry[] entries)
		{
			foreach (ResourcePermissionBaseEntry entry in entries) {
				List.Add (new PerformanceCounterPermissionEntry ((PerformanceCounterPermissionAccess) entry.PermissionAccess, entry.PermissionAccessPath[0], entry.PermissionAccessPath[1]));
			}	
		}

		public PerformanceCounterPermissionEntry this [int index] {
			get { return (PerformanceCounterPermissionEntry) InnerList[index]; }
			set { InnerList[index] = value; }
		}

		public int Add (PerformanceCounterPermissionEntry value)
		{
			return List.Add (value);
		}

		public void AddRange (PerformanceCounterPermissionEntry[] value)
		{
			foreach (PerformanceCounterPermissionEntry e in value)
				List.Add (e);
		}

		public void AddRange (PerformanceCounterPermissionEntryCollection value)
		{
			foreach (PerformanceCounterPermissionEntry e in value)
				List.Add (e);
		}

		public bool Contains (PerformanceCounterPermissionEntry value)
		{
			return List.Contains (value);
		}

		public void CopyTo (PerformanceCounterPermissionEntry[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (PerformanceCounterPermissionEntry value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, PerformanceCounterPermissionEntry value)
		{
			List.Insert (index, value);
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

		public void Remove (PerformanceCounterPermissionEntry value)
		{
			List.Remove (value);
		}
	}
}
