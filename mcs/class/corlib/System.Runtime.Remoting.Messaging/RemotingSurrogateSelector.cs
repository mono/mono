//
// System.Runtime.Remoting.Messaging.RemotingSurrogateSelector.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//         Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

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
