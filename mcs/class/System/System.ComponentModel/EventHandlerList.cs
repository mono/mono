//
// System.ComponentModel.EventHandlerList.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;

namespace System.ComponentModel {

	// <summary>
	//   List of Event delegates.
	// </summary>
	//
	// <remarks>
	//   Longer description
	// </remarks>
	public sealed class EventHandlerList : IDisposable {
		Hashtable table;
		
		public EventHandlerList ()
		{
		}

		public Delegate this [object key] {
			get {
				if (table == null)
					return null;

				return table [key] as Delegate;
			}

			set {
				AddHandler (key, value);
			}
		}

		public void AddHandler (object key, Delegate value)
		{
			if (table == null)
				table = new Hashtable ();

			Delegate prev = table [key] as Delegate;
			prev = Delegate.Combine (prev, value);
			table [key] = prev;
		}

		public void RemoveHandler (object key, Delegate value)
		{
			if (table == null)
				return;

			Delegate prev = table [key] as Delegate;
			prev = Delegate.Remove (prev, value);
			table [key] = prev;
		}

		public void Dispose ()
		{
			table = null;
		}
	}
	
}
