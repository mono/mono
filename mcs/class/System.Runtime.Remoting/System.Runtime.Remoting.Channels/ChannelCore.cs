//
// System.Runtime.Remoting.Channels.ChannelCore.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
//

using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Channels
{
	internal class ChannelCore
	{
		public static SerializationBinder SimpleBinder = new SimpleBinder();
	}
	
	internal class SimpleBinder: SerializationBinder
	{
		public override Type BindToType (String assemblyName, string typeName)
		{
			Assembly asm;
			
			if (assemblyName.IndexOf (',') != -1)
			{
				// Try using the full name
				try
				{
					asm = Assembly.Load (assemblyName);
					Type t = asm.GetType (typeName);
					if (t != null) return t;
				}
				catch {}
			}
			
			// Try using the simple name
			asm = Assembly.LoadWithPartialName (assemblyName);
			return asm.GetType (typeName, true);
		}
	}
}

