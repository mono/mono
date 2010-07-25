//
// System.Runtime.Remoting.Messaging.RemotingSurrogateSelector.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//         Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[System.Runtime.InteropServices.ComVisible (true)]
	public class RemotingSurrogateSelector : ISurrogateSelector
	{
		static Type s_cachedTypeObjRef = typeof(ObjRef);
		static ObjRefSurrogate _objRefSurrogate = new ObjRefSurrogate();
		static RemotingSurrogate _objRemotingSurrogate = new RemotingSurrogate();

		Object _rootObj = null;    
		MessageSurrogateFilter _filter = null;
		ISurrogateSelector _next;

		public RemotingSurrogateSelector ()
		{
		}
		
		public MessageSurrogateFilter Filter {
			get { return _filter; }
			set { _filter = value; }
		}

		public virtual void ChainSelector (ISurrogateSelector selector)
		{
			if (_next != null) selector.ChainSelector (_next);
			_next = selector;
		}

		public virtual ISurrogateSelector GetNextSelector()
		{
			return _next;
		}

		public object GetRootObject ()
		{
			return _rootObj;
		}

		public virtual ISerializationSurrogate GetSurrogate (
			Type type, StreamingContext context, out ISurrogateSelector ssout)
		{
			if (type.IsMarshalByRef)
			{
				ssout = this;
				return _objRemotingSurrogate;
			}

			if (s_cachedTypeObjRef.IsAssignableFrom (type))
			{
				ssout = this;
				return _objRefSurrogate;
			}

			if (_next != null) return _next.GetSurrogate (type, context, out ssout);

			ssout = null;
			return null;
		}

		public void SetRootObject (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ();
			
			_rootObj = obj;
		}
		
		[MonoTODO]
		public virtual void UseSoapFormat ()
		{
			throw new NotImplementedException ();
		}
	}
}
