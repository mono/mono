//
// System.Runtime.Remoting.Channels.ChannelCore.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
//

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
			if (asm == null)
				return null;
			return asm.GetType (typeName, true);
		}
	}
}

