//
// System.Xml.XPath.XPathNodeIterator
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;

namespace System.Xml.XPath
{
	public abstract class XPathNodeIterator : ICloneable
	{
		private int _count;

		#region Constructor

		protected XPathNodeIterator ()
		{
		}

		#endregion

		#region Properties

		public virtual int Count
		{
			get
			{
				if (_count == 0)
				{
					// compute and cache the count
					XPathNodeIterator tmp = Clone ();
					while (tmp.MoveNext ())
						;
					_count = tmp.CurrentPosition;
				}
				return _count;
			}
		}

		public abstract XPathNavigator Current { get; }

		public abstract int CurrentPosition { get; }

		#endregion

		#region Methods

		public abstract XPathNodeIterator Clone ();

		object ICloneable.Clone ()
		{
			return Clone ();
		}

		public abstract bool MoveNext ();
		
		#endregion
	}
}
