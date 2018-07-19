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
// System.DirectoryServices.DirectoryEntry.cs
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Authors
//	Raja R Harinath <rharinath@novell.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//

using System.Security.Permissions;

namespace System.DirectoryServices {

	[Serializable]
	public sealed class DirectoryServicesPermission : ResourcePermissionBase {

		DirectoryServicesPermissionEntryCollection innerCollection;

		public DirectoryServicesPermission ()
		{
			SetUp ();
		}

		public DirectoryServicesPermission (DirectoryServicesPermissionEntry[] permissionAccessEntries)
		{
			SetUp ();
			innerCollection = new DirectoryServicesPermissionEntryCollection (this);
			innerCollection.AddRange (permissionAccessEntries);
		}

 		public DirectoryServicesPermission (PermissionState state)
			: base (state)
 		{
			SetUp ();
		}

		public DirectoryServicesPermission (DirectoryServicesPermissionAccess permissionAccess, string path)
		{
			SetUp ();
			innerCollection = new DirectoryServicesPermissionEntryCollection (this);
			innerCollection.Add (new DirectoryServicesPermissionEntry (permissionAccess, path));
		}

		public DirectoryServicesPermissionEntryCollection PermissionEntries {
			get {
				if (innerCollection == null) {
					// must be here to work with XML deserialization
					innerCollection = new DirectoryServicesPermissionEntryCollection (this);
				}
				return innerCollection;
			}
		}

		// helpers

		private void SetUp ()
		{
			PermissionAccessType = typeof (DirectoryServicesPermissionAccess);
			TagNames = new string[1] { "Path" };
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
			DirectoryServicesPermissionEntry dspe = (obj as DirectoryServicesPermissionEntry);
			base.AddPermissionAccess (dspe.GetBaseEntry ());
		}

		internal void Remove (object obj) 
		{
			DirectoryServicesPermissionEntry dspe = (obj as DirectoryServicesPermissionEntry);
			base.RemovePermissionAccess (dspe.GetBaseEntry ());
		}
	}
}
