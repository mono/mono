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
		#region Constructor

		protected XPathNodeIterator ()
		{
		}

		#endregion

		#region Properties

		[MonoTODO]
		public virtual int Count { 
			get {
				throw new NotImplementedException ();
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
