//
// System.Runtime.Remoting.Channels.BaseChannelWithProperties.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
	public abstract class BaseChannelWithProperties :
		BaseChannelObjectWithProperties
	{
		protected IChannelSinkBase SinksWithProperties;
		
		[MonoTODO]
		protected BaseChannelWithProperties ()
		{
			throw new NotImplementedException ();
		}
		
		public override IDictionary Properties
		{
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
	}
}
