//
// SoapInputFilterCollection.cs: Soap Input Filter Collection
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using Microsoft.Web.Services;
using System;
using System.Collections;

namespace Microsoft.Web.Services {

	public class SoapOutputFilterCollection : CollectionBase, ICloneable {
		
		public SoapOutputFilterCollection () {}

		public SoapOutputFilter this [int index] {
			get { return (SoapOutputFilter) InnerList [index]; }
		}

		public int Add (SoapOutputFilter filter) 
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
				if (! (o is SoapOutputFilter))
					throw new ArgumentException ("not SoapOutputFilter");
				// we'll get the ArgumentNullException in Add
				InnerList.Add (o);
			}
		}

		// LAMESPEC: Shallow (implemented) or deep clone (todo)
		public object Clone () 
		{
			return InnerList.Clone ();
		}

		public bool Contains (SoapOutputFilter filter) 
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

		public int IndexOf (SoapOutputFilter filter) 
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

		public void Insert (int index, SoapOutputFilter filter)
		{
			if (filter == null)
				throw new ArgumentNullException ("filter");
			InnerList.Insert (index, filter);
		}

		public void Remove (SoapOutputFilter filter) 
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
