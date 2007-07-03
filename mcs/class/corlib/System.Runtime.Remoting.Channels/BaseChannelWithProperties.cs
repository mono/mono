//
// System.Runtime.Remoting.Channels.BaseChannelWithProperties.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
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

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
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
