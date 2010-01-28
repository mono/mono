//
// DefaultOperationInvoker.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Reflection;
using System.Threading;

namespace System.ServiceModel.Dispatcher
{
	class DefaultOperationInvoker : IOperationInvoker
	{
		readonly OperationDescription od;
		readonly ParameterInfo [] in_params, out_params;
		readonly bool is_synchronous;

		public DefaultOperationInvoker (OperationDescription od)
		{
			this.od = od;
			is_synchronous = od.SyncMethod != null;
			var mi = od.SyncMethod ?? od.BeginMethod;
			var il = new List<ParameterInfo> ();
			var ol = new List<ParameterInfo> ();
			var pl = mi.GetParameters ();
			int count = mi == od.BeginMethod ? pl.Length - 2 : pl.Length;
			for (int i = 0; i < count; i++) {
				var pi = pl [i];
				if (!pi.IsOut)
					il.Add (pi);
				if (pi.IsOut || pi.ParameterType.IsByRef)
					ol.Add (pi);
			}
			in_params = il.ToArray ();
			out_params = ol.ToArray ();
		}

		public bool IsSynchronous {
			get { return is_synchronous; }
		}

		public object [] AllocateInputs ()
		{
			return new object [in_params.Length];
		}

		public object Invoke (object instance, object [] inputs, out object [] outputs)
		{
			var arr = new object [od.SyncMethod.GetParameters ().Length];
			for (int i = 0; i < in_params.Length; i++)
				arr [in_params [i].Position] = inputs [i];

			var ret = od.SyncMethod.Invoke (instance, arr);

			outputs = new object [out_params.Length];
			for (int i = 0; i < out_params.Length; i++)
				outputs [i] = arr [out_params [i].Position];

			return ret;
		}

		public IAsyncResult InvokeBegin (object instance, object [] inputs, AsyncCallback callback, object state)
		{
			var arr = new object [in_params.Length + out_params.Length + 2];
			for (int i = 0; i < in_params.Length; i++)
				arr [in_params [i].Position] = inputs [i];
			arr [arr.Length - 2] = callback;
			arr [arr.Length - 1] = state;
			return new InvokeAsyncResult (arr, (IAsyncResult) od.BeginMethod.Invoke (instance, arr));
		}

		public object InvokeEnd (object instance, out object [] outputs, IAsyncResult result)
		{
			var r = (InvokeAsyncResult) result;
			var ret = od.EndMethod.Invoke (instance, new object [] {r.Source});
			var arr = r.Parameters;
			outputs = new object [out_params.Length];
			for (int i = 0; i < out_params.Length; i++)
				outputs [i] = arr [out_params [i].Position];
			return ret;
		}

		class InvokeAsyncResult : IAsyncResult
		{
			public InvokeAsyncResult (object [] parameters, IAsyncResult source)
			{
				Source = source;
				Parameters = parameters;
			}

			public IAsyncResult Source;
			public object [] Parameters;

			public WaitHandle AsyncWaitHandle {
				get { return Source.AsyncWaitHandle; }
			}
			public bool CompletedSynchronously {
				get { return Source.CompletedSynchronously; }
			}
			public bool IsCompleted {
				get { return Source.IsCompleted; }
			}
			public object AsyncState {
				get { return Source.AsyncState; }
			}
		}
	}
}
