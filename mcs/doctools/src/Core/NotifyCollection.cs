// NotifyCollections.cs
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
	public delegate void CollectionModifiedEventHandler(object sender);

	public interface INotifyCollection
	{
		event CollectionModifiedEventHandler Modified;
		void BeginUpdate();
		void EndUpdate();
		void SetModifiedEvent();
	}

	public class NotifyCollectionHandler
	{
		private bool callModifiedEvent = true;
		private bool changedDuringUpdate = false;
		INotifyCollection collection;
		
		public NotifyCollectionHandler(INotifyCollection collection)
		{
			this.collection = collection;
		}
		
		public void BeginUpdate()
		{
			callModifiedEvent = false;
			changedDuringUpdate = false;
		}
		
		public void EndUpdate()
		{
			// call the event if changes occured between the
			// begin/end update calls
			if (changedDuringUpdate)
			{
				collection.SetModifiedEvent();
			}

			changedDuringUpdate = false;
			callModifiedEvent = true;
		}
		
		public void OnModified()
		{
			// if in the middle of begin/end update
			// save the event call until EndUpdate()
			if (!callModifiedEvent)
			{
				changedDuringUpdate = true;
			}
			else
			{
				collection.SetModifiedEvent();
			}
		}
	}
}

