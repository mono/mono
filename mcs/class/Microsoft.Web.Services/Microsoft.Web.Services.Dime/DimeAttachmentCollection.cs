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
				return IndexOf (key);
			}
		}

		public DimeAttachment this [string key] {
			get {
				return IndexOf (key);
			}
		}

		public void Add (DimeAttachment attachment)
		{
			Add (attachment);
		}

		public void AddRange (ICollection collection)
		{
			foreach (object o in collection)
				Add (o);
		}
		
		public bool Contains (string id)
		{
			return Contains (id);
		}
		
		public void CopyTo (DimeAttachment[] attachements, int index)
		{
			CopyTo (attachments, index);
		}

		public int IndexOf (DimeAttachment attachment)
		{
			return IndexOf(attachment);
		}

		public int IndexOf (string id)
		{
			return IndexOf(id);
		}

		public void Remove (DimeAttachment attachment)
		{
			Remove (attachement);
		}
	}
}

 
