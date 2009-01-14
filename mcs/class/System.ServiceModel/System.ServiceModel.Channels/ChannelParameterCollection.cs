//
// ChannelParameterCollection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;

namespace System.ServiceModel.Channels
{
	public class ChannelParameterCollection : Collection<object>
	{
		IChannel channel;

		public ChannelParameterCollection ()
		{
		}

		public ChannelParameterCollection (IChannel channel)
		{
			this.channel = channel;
		}

		protected virtual IChannel Channel {
			get { return channel; }
		}

		public void PropagateChannelParameters (IChannel innerChannel)
		{
			if (innerChannel == null)
				throw new ArgumentNullException ("innerChannel");
			var pc = innerChannel.GetProperty<ChannelParameterCollection> ();
			if (pc == null)
				throw new ArgumentException (String.Format ("The argument channel (of type '{0}') does not have ChannelParameterCollection property.", innerChannel.GetType ()));
			foreach (var p in this)
				pc.Add (p);
		}

		protected override void ClearItems ()
		{
			base.ClearItems ();
		}

		protected override void InsertItem (int index, object item)
		{
			base.InsertItem (index, item);
		}

		protected override void RemoveItem (int index)
		{
			base.RemoveItem (index);
		}

		protected override void SetItem (int index, object item)
		{
			base.SetItem (index, item);
		}
	}
}
