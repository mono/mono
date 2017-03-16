//
// System.Web.UI.WebControls.HotSpotCollection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	[EditorAttribute ("System.Web.UI.Design.WebControls.HotSpotCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HotSpotCollection: StateManagedCollection
	{
		static Type[] _knownTypes = new Type[] { 
			typeof (CircleHotSpot),
			typeof (PolygonHotSpot),
			typeof (RectangleHotSpot)
		};
						    
		public HotSpot this [int index] {
			get { return (HotSpot) ((IList)this)[index]; }
		}

		public int Add (HotSpot spot)
		{
			return ((IList)this).Add (spot);
		}
		
		protected override object CreateKnownType (int index)
		{
			switch (index) {
				case 0:
					return new CircleHotSpot ();
				case 1:
					return new PolygonHotSpot ();
				case 2:
					return new RectangleHotSpot ();
			}

			throw new ArgumentOutOfRangeException ("index");
		}
		
		protected override Type[] GetKnownTypes ()
		{
			return _knownTypes;
		}
		
		public void Insert (int index, HotSpot spot)
		{
			((IList)this).Insert (index, spot);
		}
		
		protected override void OnValidate (object o)
		{
			base.OnValidate (o);
			
			if ((o is HotSpot) == false)
				throw new ArgumentException ("o is not a HotSpot");
		}

		public void Remove (HotSpot spot)
		{
			((IList)this).Remove (spot);
		}

		public void RemoveAt (int index)
		{
			((IList)this).RemoveAt (index);
		}

		protected override void SetDirtyObject (object o)
		{
			HotSpot spot = (HotSpot)o;
			spot.SetDirty ();
		}
	}
}

