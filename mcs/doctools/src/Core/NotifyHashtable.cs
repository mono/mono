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

namespace Mono.Doc.Core
{
	/// <summary>
	/// An implementation of the Hashtable class that provides 
	/// a mechanism to receive a notification event
	/// when the elements of the Hashtable are changed
	/// </summary>
	public class NotifyHashtable : Hashtable
	{
		#region Private Instance Fields
		
		private bool callModifiedEvent = true;

		#endregion // Private Instance Fields

		#region Public Instance Properties

		/// <summary>
		/// The event handler that will be called whenever
		/// a change is made to the Hashtable.
		/// </summary>	
		public event CollectionModifiedEventHandler Modified;

		/// <summary>
		/// Turns off notification event calling. Notification 
		/// events will not be sent until EndUpdate() is called.
		/// </summary>
		public virtual void BeginUpdate()
		{
			callModifiedEvent = false;
		}

		/// <summary>
		/// Resumes notification event calling previously
		/// turned off by calling BeginUpdate().
		/// </summary>
		public virtual void EndUpdate()
		{
			callModifiedEvent = true;
		}

		public override object this[object key]
		{
			set
			{
				base[key] = value;
				if (Modified != null && callModifiedEvent)
				{
					Modified(this);
				}

			}
		}

		public override void Add(object key,object value)
		{
			base.Add(key, value);
			if (Modified != null && callModifiedEvent) 
			{
				Modified(this);
			}
		}
		
		public override void Clear()
		{
			base.Clear();
			if (Modified != null && callModifiedEvent) 
			{
				Modified(this);
			}
		}

		public override void Remove(object key)
		{
			base.Remove(key);
			if (Modified != null && callModifiedEvent)
			{
				Modified(this);
			}
		}
		#endregion // Public Instance Properties
	}
}

