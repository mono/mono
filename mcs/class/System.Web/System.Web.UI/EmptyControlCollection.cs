//
// System.Web.UI.EmptyControlCollection.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	public class EmptyControlCollection : ControlCollection
	{
		public EmptyControlCollection (Control owner)
			: base (owner)
		{
		}
		
		public override void Add (Control child)
		{
			throw new HttpException ();
		}

		public override void AddAt (int index, Control child)
		{
			throw new HttpException ();
		}
	}
}
