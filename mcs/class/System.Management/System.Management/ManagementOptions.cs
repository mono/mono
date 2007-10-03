//
// System.Management.ManagementOptions
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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
using System.ComponentModel;

namespace System.Management
{
	[TypeConverter (typeof (ExpandableObjectConverter))]
	public abstract class ManagementOptions : ICloneable
	{
		public static readonly TimeSpan InfiniteTimeout = TimeSpan.MaxValue;
		ManagementNamedValueCollection context;
		TimeSpan timeout;

		internal ManagementOptions ()
			: this (null, InfiniteTimeout)
		{
		}

		internal ManagementOptions (ManagementNamedValueCollection context, TimeSpan timeout)
		{
			this.context = context;
			this.timeout = timeout;
		}

		[MonoTODO]
		public abstract object Clone ();

		public ManagementNamedValueCollection Context {
			get { return context; }
			set { context = value; }
		}

		public TimeSpan Timeout {
			get { return timeout; }
			set { timeout = value; }
		}
	}
}

