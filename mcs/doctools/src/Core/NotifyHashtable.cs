// NotifyHashtable.cs
// John Sohn (jsohn@columbus.rr.com)
// 
// Copyright (c) 2002 John Sohn
//
// This file is part of Monodoc, a multilingual API documentation tool.
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Mono.Doc.Core
{
	/// <summary>
	/// An implementation of the Hashtable class that provides 
	/// a mechanism to receive a notification event
	/// when the elements of the Hashtable are changed
	// </summary>
	public class NotifyHashtable : Hashtable, INotifyCollection
	{
		#region Protected Instance Fields
		
		protected NotifyCollectionHandler handler = null;

		#endregion

		#region Constructors and Destructor
		
		public NotifyHashtable() : base()
		{
			handler = new NotifyCollectionHandler(this);	
		}
		
		public NotifyHashtable(IDictionary d) : base(d) 
		{
			handler = new NotifyCollectionHandler(this);	
		}
		
		public NotifyHashtable(int i) : base(i) 
		{
			handler = new NotifyCollectionHandler(this);	
		}
		
		public NotifyHashtable(IDictionary d, float f) : base(d, f)
		{
			handler = new NotifyCollectionHandler(this);	
		}
		
		public NotifyHashtable(IHashCodeProvider hp, IComparer c)
			: base(hp, c)
		{
			handler = new NotifyCollectionHandler(this);	
		}
		
		public NotifyHashtable(int i, float f) : base(i, f)
		{
			handler = new NotifyCollectionHandler(this);	
		}
		
		protected NotifyHashtable(SerializationInfo si, 
			StreamingContext sc) : base(si, sc)
		{
			handler = new NotifyCollectionHandler(this);	
		}
		
		public NotifyHashtable(IDictionary d, IHashCodeProvider hcp, 
			IComparer c) : base(d, hcp, c)
		{
			handler = new NotifyCollectionHandler(this);	
		}
		
		public NotifyHashtable(int i, IHashCodeProvider hcp, 
			IComparer c) : base(i, hcp, c)
		{
			handler = new NotifyCollectionHandler(this);
		}
		
		public NotifyHashtable(IDictionary d, float f, 
			IHashCodeProvider hcp, IComparer c) 
			: base(d, f, hcp, c)
		{
			handler = new NotifyCollectionHandler(this);
		}
		
		public NotifyHashtable(int i, float f, IHashCodeProvider hcp,
			IComparer c) : base(i, f, hcp, c)
 		{
			handler = new NotifyCollectionHandler(this);
		}

		#endregion

		#region Public Instance Fields

		/// <summary>
		/// The event handler that will be called whenever
		/// a change is made to the Hashtable.
		/// </summary>	
		public event CollectionModifiedEventHandler Modified;

		#endregion // Public Instance Fields
		
		#region Public Instance Methods

		/// <summary>
		/// Turns off notification event calling. Notification 
		/// events will not be sent until EndUpdate() is called.
		/// </summary>
		public virtual void BeginUpdate()
		{
			handler.BeginUpdate();
		}

		/// <summary>
		/// Resumes notification event calling previously
		/// turned off by calling BeginUpdate().
		/// </summary>
		public virtual void EndUpdate()
		{
			handler.EndUpdate();
		}

		public virtual void SetModifiedEvent()
		{
			if (Modified != null)
			{
				Modified(this);
			}			
		}

		public override object this[object key]
		{
			set
			{
				base[key] = value;
				handler.OnModified();
			}
		}

		public override void Add(object key,object value)
		{
			base.Add(key, value);
			handler.OnModified();
		}
		
		public override void Clear()
		{
			base.Clear();
			handler.OnModified();
		}

		public override void Remove(object key)
		{
			base.Remove(key);
			handler.OnModified();
		}
		#endregion // Public Instance Methods
	}
}

