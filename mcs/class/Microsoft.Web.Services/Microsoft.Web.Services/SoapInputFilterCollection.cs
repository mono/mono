//
// SoapInputFilterCollection.cs: Soap Input Filter Collection
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;

namespace Microsoft.Web.Services {

	public class SoapInputFilterCollection : CollectionBase, ICloneable {

		public SoapInputFilterCollection () {}

		internal SoapInputFilterCollection (ArrayList list) 
		{
			InnerList.AddRange (list);
		}

		public SoapInputFilter this [int index] {
			get { return (SoapInputFilter) InnerList [index]; }
		}

		public int Add (SoapInputFilter filter) 
		{
			if (filter == null)
				throw new ArgumentNullException ("filter");
			return InnerList.Add (filter);
		}

		public void AddRange (ICollection filters) 
		{
			// can't use list.AddRange because we must check every items
			// in the collection
			foreach (object o in filters) {
				if (! (o is SoapInputFilter))
					throw new ArgumentException ("not SoapInputFilter");
				// we'll get the ArgumentNullException in Add
				InnerList.Add (o as SoapInputFilter);
			}
		}

		// LAMESPEC: Shallow (implemented) or deep clone (todo)
		public object Clone () 
		{
			return new SoapInputFilterCollection ((ArrayList) InnerList.Clone ());
		}

		public bool Contains (SoapInputFilter filter) 
		{
			if (filter == null)
				throw new ArgumentNullException ("filter");
			return InnerList.Contains (filter);
		}

		public bool Contains (Type filterType) 
		{
			foreach (object o in InnerList) {
				if (o.GetType () == filterType)
					return true;
			}
			return false;
		}

		public int IndexOf (SoapInputFilter filter) 
		{
			if (filter == null)
				throw new ArgumentNullException ("filter");
			return InnerList.IndexOf (filter);
		}

		public int IndexOf (Type filterType) 
		{
			if (filterType == null)
				throw new ArgumentNullException ("filterType");
			int i = 0;
			foreach (object o in InnerList) {
				if (o.GetType () == filterType)
					return i;
				i++;
			}
			return -1;
		}

		public void Insert (int index, SoapInputFilter filter) 
		{
			if (filter == null)
				throw new ArgumentNullException ("filter");
			InnerList.Insert (index, filter);
		}

		public void Remove (SoapInputFilter filter) 
		{
			if (filter == null)
				throw new ArgumentNullException ("filter");
			InnerList.Remove (filter);
		}

		public void Remove (Type filterType) 
		{
			if (filterType == null)
				throw new ArgumentNullException ("filterType");
			int i = 0;
			foreach (object o in InnerList) {
				if (o.GetType () == filterType)
					InnerList.RemoveAt (i);
				i++;
			}
		}
	}
}
