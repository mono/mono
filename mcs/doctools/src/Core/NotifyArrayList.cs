// NotifyArrayList.cs
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
// but WITHOUT ANY WARRANTY; without even the implied 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  

using System;
using System.Collections;

namespace Mono.Doc.Core
{
	/// <summary>
	/// An implementation of the ArrayList class that provides 
	/// a mechanism to receive a notification event
	/// when the elements of the array are changed
	/// </summary>
	public class NotifyArrayList : ArrayList, INotifyCollection
	{
		#region Protected Instance Fields
		
		protected NotifyCollectionHandler handler = null;

		#endregion

		#region Constructors and Destructors
		
		public NotifyArrayList() : base()
		{
			handler = new NotifyCollectionHandler(this);	
		}
		
		public NotifyArrayList(ICollection collection) 
			: base(collection)
		{
			handler = new NotifyCollectionHandler(this);
		}
		
		public NotifyArrayList(int i) : base(i)
		{
			handler = new NotifyCollectionHandler(this);
		}
		
		#endregion // Constructors and Destructors

		#region Public Instance Fields
		
		/// <summary>
		/// The event handler that will be called whenever
		/// a change is made to the ArrayList.
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

		/// <summary>
		/// Invokes the Modified event.
		/// </summary>
		public void SetModifiedEvent()
		{
			if (Modified != null)
			{
				Modified(this);
			}
		}
		
		// the following overridden functions modify the contents
		// of the collection
		public override object this[int index] 
		{
			set
			{
				base[index] = value;
				handler.OnModified();
			}
		}
	
		public override int Add(object value) 
		{
			int status = base.Add(value);
			handler.OnModified();
			return status;
		}

		public override void AddRange(ICollection c) 
		{
			base.AddRange(c);
			handler.OnModified();
		}

		public override void Clear() 
		{
			base.Clear();
			handler.OnModified();
		}

		public override void Insert(int index, object value) 
		{
			base.Insert(index, value);
			handler.OnModified();
		}

		public override void InsertRange(int index, ICollection c) {
			base.InsertRange(index, c);
			handler.OnModified();
		}

		public override void Remove(object obj) {
			base.Remove(obj);
			handler.OnModified();
		}

		public override void RemoveAt(int index) {
			base.RemoveAt(index);
			handler.OnModified();
		}

		public override void RemoveRange(int index,int count) {
			base.RemoveRange(index, count);
			handler.OnModified();
		}

		public override void Reverse() {
			base.Reverse();
			handler.OnModified();
		}

		public override void SetRange(int index,ICollection c) {
			base.SetRange(index, c);
			handler.OnModified();
		}

		public override void Sort() {
			base.Sort();
			handler.OnModified();
		}
		
		#endregion // Public Instance Methods
	}
}

