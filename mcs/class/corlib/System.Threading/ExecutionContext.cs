// 
// System.Threading.ExecutionContext.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//
// Copyright (C) Novell, Inc., 2004
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

#if NET_2_0

using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Threading 
{
	[ComVisible (false)]
	[Serializable]
	public sealed class ExecutionContext : ISerializable
	{
		internal ExecutionContext ()
		{
		}
		
		[MonoTODO]
		internal ExecutionContext (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static ExecutionContext Capture()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public ExecutionContext CreateCopy()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static bool IsFlowSuppressed()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void RestoreFlow()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void Run (ExecutionContext executionContext, ContextCallback callBack, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static ExecutionContextSwitcher SetExecutionContext (ExecutionContext executionContext)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static AsyncFlowControl SuppressFlow()
		{
			throw new NotImplementedException ();
		}

	}
}

#endif
