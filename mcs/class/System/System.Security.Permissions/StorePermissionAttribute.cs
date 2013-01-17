//
// System.Security.Permissions.StorePermissionAttribute class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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


using System.Globalization;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
		AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, 
		AllowMultiple = true, Inherited = false)]
	[Serializable]
	public sealed class StorePermissionAttribute : CodeAccessSecurityAttribute {

		private StorePermissionFlags _flags;

		public StorePermissionAttribute (SecurityAction action)
			: base (action) 
		{
			_flags = StorePermissionFlags.NoFlags;
		}


		public StorePermissionFlags Flags {
			get { return _flags; }
			set {
				if ((value & StorePermissionFlags.AllFlags) != value) {
					string msg = String.Format (Locale.GetText ("Invalid flags {0}"), value);
					throw new ArgumentException (msg, "StorePermissionFlags");
				}

				_flags = value;
			}
		}

		public bool AddToStore {
			get { return ((_flags & StorePermissionFlags.AddToStore) != 0); }
			set {
				if (value) {
					_flags |= StorePermissionFlags.AddToStore;
				}
				else {
					_flags &= ~StorePermissionFlags.AddToStore;
				}
			}
		}

		public bool CreateStore {
			get { return ((_flags & StorePermissionFlags.CreateStore) != 0); }
			set {
				if (value) {
					_flags |= StorePermissionFlags.CreateStore;
				}
				else {
					_flags &= ~StorePermissionFlags.CreateStore;
				}
			}
		}

		public bool DeleteStore {
			get { return ((_flags & StorePermissionFlags.DeleteStore) != 0); }
			set {
				if (value) {
					_flags |= StorePermissionFlags.DeleteStore;
				}
				else {
					_flags &= ~StorePermissionFlags.DeleteStore;
				}
			}
		}

		public bool EnumerateCertificates {
			get { return ((_flags & StorePermissionFlags.EnumerateCertificates) != 0); }
			set {
				if (value) {
					_flags |= StorePermissionFlags.EnumerateCertificates;
				}
				else {
					_flags &= ~StorePermissionFlags.EnumerateCertificates;
				}
			}
		}

		public bool EnumerateStores {
			get { return ((_flags & StorePermissionFlags.EnumerateStores) != 0); }
			set {
				if (value) {
					_flags |= StorePermissionFlags.EnumerateStores;
				}
				else {
					_flags &= ~StorePermissionFlags.EnumerateStores;
				}
			}
		}

		public bool OpenStore {
			get { return ((_flags & StorePermissionFlags.OpenStore) != 0); }
			set {
				if (value) {
					_flags |= StorePermissionFlags.OpenStore;
				}
				else {
					_flags &= ~StorePermissionFlags.OpenStore;
				}
			}
		}

		public bool RemoveFromStore {
			get { return ((_flags & StorePermissionFlags.RemoveFromStore) != 0); }
			set {
				if (value) {
					_flags |= StorePermissionFlags.RemoveFromStore;
				}
				else {
					_flags &= ~StorePermissionFlags.RemoveFromStore;
				}
			}
		}


		public override IPermission CreatePermission ()
		{
			StorePermission perm = null;
			if (this.Unrestricted)
				perm = new StorePermission (PermissionState.Unrestricted);
			else
				perm = new StorePermission (_flags);
			return perm;
		}
	}
}

