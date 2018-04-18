/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.,  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

//
// System.DirectoryServices.DirectoryServicesPermissionEntryCollection.cs
//
// Copyright (C) 2004  Novell Inc.
//
// Written by Raja R Harinath <rharinath@novell.com>
//

using System.Collections;
using System.Security.Permissions;

namespace System.DirectoryServices {

	// when deserializing an instance serialized by MS.NET, you get the following 
	// error : Field "owner" not found in class ....
	[Serializable]
	[MonoTODO ("Fix serialization compatibility with MS.NET")]
	public class DirectoryServicesPermissionEntryCollection : CollectionBase {

		private DirectoryServicesPermission owner;

		internal DirectoryServicesPermissionEntryCollection (DirectoryServicesPermission owner)
		{
			this.owner = owner;
			ResourcePermissionBaseEntry[] entries = owner.GetEntries ();
			if (entries.Length > 0) {
				foreach (ResourcePermissionBaseEntry entry in entries) {
					DirectoryServicesPermissionAccess dspa = (DirectoryServicesPermissionAccess) entry.PermissionAccess;
					DirectoryServicesPermissionEntry dspe = new DirectoryServicesPermissionEntry (dspa, entry.PermissionAccessPath [0]);
					// we don't want to add them (again) to the base class
					InnerList.Add (dspe);
				}
			}
		}

		public DirectoryServicesPermissionEntry this [int index] {
			get { return List[index] as DirectoryServicesPermissionEntry; }
			set { List[index] = value; }
		}

		public int Add (DirectoryServicesPermissionEntry value)
		{
			return List.Add (value);
		}

		public void AddRange (DirectoryServicesPermissionEntry[] value)
		{
			foreach (DirectoryServicesPermissionEntry entry in value)
				Add (entry);
		}

		public void AddRange (DirectoryServicesPermissionEntryCollection value)
		{
			foreach (DirectoryServicesPermissionEntry entry in value)
				Add (entry);
		}

		public void CopyTo (DirectoryServicesPermissionEntry[] array, int index)
		{
			foreach (DirectoryServicesPermissionEntry entry in List)
				array[index++] = entry;
		}

		public bool Contains (DirectoryServicesPermissionEntry value)
		{
			return List.Contains (value);
		}

		public int IndexOf (DirectoryServicesPermissionEntry value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, DirectoryServicesPermissionEntry value)
		{
			List.Insert (index, value);
		}

		public void Remove (DirectoryServicesPermissionEntry value)
		{
			List.Remove (value);
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

