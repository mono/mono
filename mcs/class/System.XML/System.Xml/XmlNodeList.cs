//
// System.Xml.XmlNodeList
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.Collections;

namespace System.Xml
{
	public abstract class XmlNodeList : IEnumerable
	{
		#region Constructors
		///////////////////////////////////////////////////////////////////////
		//
		//	Constructors
		//
		///////////////////////////////////////////////////////////////////////

		protected internal XmlNodeList() { }

		#endregion

		#region Properties
		///////////////////////////////////////////////////////////////////////
		//
		//	Properties
		//
		///////////////////////////////////////////////////////////////////////

		public abstract int Count {	get; }

		[System.Runtime.CompilerServices.IndexerName("ItemOf")]
		public virtual XmlNode this [int i]	{
			get { return Item(i); }
		}

		#endregion

		#region Methods
		///////////////////////////////////////////////////////////////////////
		//
		//	Methods
		//
		///////////////////////////////////////////////////////////////////////

		public abstract IEnumerator GetEnumerator ();

		public abstract XmlNode Item (int index);

		#endregion
	}
}
