//
// System.Runtime.Remoting.Channels.BaseChannelWithProperties.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
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
		
		protected BaseChannelWithProperties ()
		{
		}
		
		public override IDictionary Properties
		{
			get
			{
				if (SinksWithProperties == null || SinksWithProperties.Properties == null)
					return base.Properties;
				else {
					IDictionary[] dics = new IDictionary [] { base.Properties, SinksWithProperties.Properties };
					return new AggregateDictionary (dics);
				}
			}
		}
	}
}
