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
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  US

using System;
using System.Collections;

namespace Mono.Doc.Core
{
	/// <summary>
	/// An implementation of the ArrayList class that provides 
	/// a mechanism to receive a notification event
	/// when the elements of the array are changed
	/// </summary>
	public class NotifyArrayList : ArrayList
	{
		#region Private Instance Fields

		private bool callModifiedEvent = true;
		private bool changedDuringUpdate ;

		#endregion // Private Instance Fields

		#region Protected Instance Methods

		protected void OnModified()
		{
			// if in the middle of begin/end update
			// save the event call until EndUpdate()
			if (!callModifiedEvent)
			{
				changedDuringUpdate = true;
			}
			else if (Modified != null)
			{
				Modified(this);
			}
		}

		#endregion // Protected Instance Methods

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
			callModifiedEvent = false;
			changedDuringUpdate = false;
		}

		/// <summary>
		/// Resumes notification event calling previously
		/// turned off by calling BeginUpdate().
		/// </summary>
		public virtual void EndUpdate()
		{
			// call the event if changes occured between the
			// begin/end update calls
			if (changedDuringUpdate && Modified != null)
			{
				Modified(this);
			}
			changedDuringUpdate = false;
			callModifiedEvent = true;
		}

		public override object this[int index] 
		{
			set
			{
				base[index] = value;
				OnModified();
			}
		}
	
		public override int Add(object value) 
		{
			int status = base.Add(value);
			OnModified();
			return status;
		}

		public override void AddRange(ICollection c) 
		{
			base.AddRange(c);
			OnModified();
		}

		public override void Clear() 
		{
			base.Clear();
			OnModified();
		}

		public override void Insert(int index, object value) 
		{
			base.Insert(index, value);
			OnModified();
		}

		public override void InsertRange(int index, ICollection c) {
			base.InsertRange(index, c);
			OnModified();
		}

		public override void Remove(object obj) {
			base.Remove(obj);
			OnModified();
		}

		public override void RemoveAt(int index) {
			base.RemoveAt(index);
			OnModified();
		}

		public override void RemoveRange(int index,int count) {
			base.RemoveRange(index, count);
			OnModified();
		}

		public override void Reverse() {
			base.Reverse();
			OnModified();
		}

		public override void SetRange(int index,ICollection c) {
			base.SetRange(index, c);
			OnModified();
		}

		public override void Sort() {
			base.Sort();
			OnModified();
		}
		
		#endregion // Public Instance Methods
	}
}

