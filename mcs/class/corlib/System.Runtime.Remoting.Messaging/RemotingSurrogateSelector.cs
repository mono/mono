//
// System.Runtime.Remoting.Messaging.RemotingSurrogateSelector.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	public class RemotingSurrogateSelector : ISurrogateSelector
	{
		ISurrogateSelector _next;
		static ObjRefSurrogate _objRefSurrogate = new ObjRefSurrogate();

		public RemotingSurrogateSelector ()
		{
		}
		
		[MonoTODO]
		public MessageSurrogateFilter Filter {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
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

		[MonoTODO]
		public object GetRootObject ()
		{
			throw new NotImplementedException ();
		}

		public virtual ISerializationSurrogate GetSurrogate (
			Type type, StreamingContext context, out ISurrogateSelector ssout)
		{
			if (type.IsSubclassOf (typeof(MarshalByRefObject)))
			{
				ssout = this;
				return _objRefSurrogate;
			}
			if (_next != null) return _next.GetSurrogate (type, context, out ssout);

			ssout = null;
			return null;
		}

		[MonoTODO]
		public void SetRootObject (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ();
			
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void UseSoapFormat ()
		{
			throw new NotImplementedException ();
		}
	}

	public class ObjRefSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			RemotingServices.GetObjectData (obj, info, context);
			info.AddValue ("fIsMarshalled", 0);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			// ObjRef is deserialized using the IObjectReference interface
			throw new NotSupportedException ("Do not use RemotingSurrogateSelector when deserializating");
		}
	}
}
