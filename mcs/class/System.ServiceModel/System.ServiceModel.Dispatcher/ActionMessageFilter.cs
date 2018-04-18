//
// System.ServiceModel.ActionMessageFilter.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Dispatcher {
	[DataContract]
	public class ActionMessageFilter : MessageFilter
	{
		ReadOnlyCollection<string> actions;

		public ActionMessageFilter (params string [] actions)
		{
			if (actions == null)
				throw new ArgumentNullException ("actions");

			// remove duplicates
			List<string> l = new List<string> ();

			foreach (string action in actions) {
				if (action == null)
					throw new ArgumentNullException ("actions");
				if (l.Contains (action) == false)
					l.Add (action);
			}

			this.actions = new ReadOnlyCollection<string> (l);
		}

		protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData> ()
		{
			return new ActionMessageFilterTable<FilterData> ();
		}

		public override bool Match (Message message)
		{
 			foreach (string action in actions)
 				if (message.Headers.Action == action || action == "*")
 					return true;

			return false;
		}

		public override bool Match (MessageBuffer messageBuffer)
		{
			bool retval;
			Message m = messageBuffer.CreateMessage ();
			retval = Match (m);
			m.Close ();
			
			return retval;
		}

		public ReadOnlyCollection<string> Actions {
			get { return actions; }
		}
	}
}