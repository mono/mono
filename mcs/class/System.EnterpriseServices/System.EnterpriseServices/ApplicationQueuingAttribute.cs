// 
// System.EnterpriseServices.ApplicationQueuingAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Assembly)]
	[ComVisible(false)]
	public sealed class ApplicationQueuingAttribute : Attribute {

		#region Fields

		bool enabled;
		int maxListenerThreads;
		bool queueListenerEnabled;

		#endregion // Fields

		#region Constructors

		public ApplicationQueuingAttribute ()
		{
			enabled = true;
			queueListenerEnabled = false;
			maxListenerThreads = 0;
		}

		#endregion // Constructors

		#region Properties

		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}

		public int MaxListenerThreads {
			get { return maxListenerThreads; }
			set { maxListenerThreads = value; }
		}

		public bool QueueListenerEnabled {
			get { return queueListenerEnabled; }
			set { queueListenerEnabled = value; }
		}

		#endregion // Properties
	}
}
