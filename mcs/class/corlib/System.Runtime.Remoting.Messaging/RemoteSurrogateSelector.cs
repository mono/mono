//
// System.Runtime.Remoting.Messaging.RemoteSurrogateSelector.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	public class RemoteSurrogateSelector : ISurrogateSelector
	{
		public RemoteSurrogateSelector ()
		{
		}
		
		[MonoTODO]
		public MessageSurrogateFilter Filter {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual void ChainSelector (ISurrogateSelector selector)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual ISurrogateSelector GetNextSelector()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetRootObject ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual ISerializationSurrogate GetSurrogate (
			Type type, StreamingContext context, out ISurrogateSelector ssout)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object SetRootObject ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void UseSoapFormat ()
		{
			throw new NotImplementedException ();
		}
	}
}
