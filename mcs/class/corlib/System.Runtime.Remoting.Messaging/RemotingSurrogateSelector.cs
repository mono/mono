//
// System.Runtime.Remoting.Messaging.RemotingSurrogateSelector.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	public class RemotingSurrogateSelector : ISurrogateSelector
	{
		public RemotingSurrogateSelector ()
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
}
