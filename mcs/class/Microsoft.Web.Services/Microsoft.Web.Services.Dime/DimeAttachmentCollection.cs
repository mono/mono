//
// Microsoft.Web.Services.Dime.DimeAttachmentCollection.cs
//
// Name: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) Ximian, Inc. 2003
//

using Microsoft.Web.Services;
using System;
using System.Collections;
using System.Globalization;

namespace Microsoft.Web.Services.Dime { 


	public class DimeAttachmentCollection : CollectionBase
	{		
		
		DimeReader reader;		

		public DimeAttachmentCollection ()
		{
		}

		public DimeAttachmentCollection (DimeReader reader)
		{
			if (reader == null) 
				throw new ArgumentNullException (
					Locale.GetText ("Argument is null."));


			if (reader.CanRead == false) 
				throw new ArgumentException (
					Locale.GetText ("The reader is not readable"));

			this.reader = reader;
		}

		public DimeAttachment this [int key] {
			get {
				return (DimeAttachment) InnerList [key];
			}
		}

		public DimeAttachment this [string key] {
			get {
				// FIXME: must iterate in collection
				return null;
			}
		}

		public void Add (DimeAttachment attachment)
		{
			InnerList.Add (attachment);
		}

		public void AddRange (ICollection collection)
		{
			foreach (object o in collection)
				InnerList.Add (o);
		}
		
		public bool Contains (string id)
		{
			return InnerList.Contains (id);
		}
		
		public void CopyTo (DimeAttachment[] attachments, int index)
		{
			InnerList.CopyTo (attachments, index);
		}

		public int IndexOf (DimeAttachment attachment)
		{
			return InnerList.IndexOf(attachment);
		}

		public int IndexOf (string id)
		{
			return InnerList.IndexOf(id);
		}

		public void Remove (DimeAttachment attachment)
		{
			InnerList.Remove (attachment);
		}
	}
}

 
