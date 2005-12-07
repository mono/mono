//
// System.Web.UI.PersistChildrenAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
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

using System.Security.Permissions;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class PersistChildrenAttribute : Attribute
	{
		bool persist;
#if NET_2_0
		bool usesCustomPersistence;
#endif		
		
		public PersistChildrenAttribute (bool persist)
		{
			this.persist = persist;
		}

#if NET_2_0
		public PersistChildrenAttribute (bool persist,
						 bool usesCustomPersistence)
		{
			this.persist = persist;
			this.usesCustomPersistence = usesCustomPersistence;
		}
#endif		
		
		public static readonly PersistChildrenAttribute Default = new PersistChildrenAttribute (true);
		public static readonly PersistChildrenAttribute Yes = new PersistChildrenAttribute (true);
		public static readonly PersistChildrenAttribute No = new PersistChildrenAttribute (false);

		public bool Persist {
			get { return persist; }
		}

#if NET_2_0
		public bool UsesCustomPersistence 
		{
			get {
				return (usesCustomPersistence);
			}
		}
#endif
		
		public override bool Equals (object obj)
		{
			PersistChildrenAttribute pobj = (obj as PersistChildrenAttribute);
			if (pobj == null)
				return false;

			return (pobj.persist == persist
#if NET_2_0
				&& pobj.usesCustomPersistence == usesCustomPersistence
#endif
				);
		}

		public override int GetHashCode ()
		{
			return persist ? 1 : 0;
		}

		public override bool IsDefaultAttribute ()
		{
			/* No idea what the usesCustomPersistence
			 * default is (I assume false, but its not
			 * documented)
			 */
			return (persist == true);
		}
	}
}
