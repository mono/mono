//
// System.ComponentModel.EventHandlerList.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
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
	public class EventHandlerList : IDisposable {
		Hashtable table;
		
		public EventHandlerList ()
		{
		}

		public Delegate this [object key] {
			get {
				if (table == null)
					return null;

				return (Delegate) table [key];
			}

			set {
				if (table == null)
					table = new Hashtable ();

				table.Add (key, value);
			}
		}

		public void AddHandler (object key, Delegate value)
		{
			if (table == null)
				table = new Hashtable ();

			table.Add (key, value);
		}

		public void RemoveHandler (object key, Delegate value)
		{
			table.Remove (key);
		}

		public void Dispose ()
		{
			table = null;
		}
	}
	
}
