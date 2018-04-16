//
// CalcSampleProxy.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
#if !MOBILE
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
#endif
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Threading;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[ServiceContract]
	public interface ICalc
	{
		[OperationContract]
		int Sum (int a, int b);

		[OperationContract (AsyncPattern = true)]
		IAsyncResult BeginSum (int a, int b, AsyncCallback cb, object state);

		int EndSum (IAsyncResult result);
	}

	public class CalcProxy : ClientBase<ICalc>, ICalc
	{
		public CalcProxy (Binding binding, EndpointAddress address)
			: base (binding, address)
		{
		}

		public int Sum (int a, int b)
		{
			return Channel.Sum (a, b);
		}

		public IAsyncResult BeginSum (int a, int b, AsyncCallback cb, object state)
		{
			return Channel.BeginSum (a, b, cb, state);
		}

		public int EndSum (IAsyncResult result)
		{
			return Channel.EndSum (result);
		}
	}

	public class CalcService : ICalc
	{
		public int Sum (int a, int b)
		{
			return a + b;
		}

		public IAsyncResult BeginSum (int a, int b, AsyncCallback cb, object state)
		{
			return new CalcAsyncResult (a, b, cb, state);
		}

		public int EndSum (IAsyncResult result)
		{
			CalcAsyncResult c = (CalcAsyncResult) result;
			return c.A + c.B;
		}
	}

	class CalcAsyncResult : IAsyncResult
	{
		public int A, B;
		AsyncCallback callback;
		object state;

		public CalcAsyncResult (int a, int b, AsyncCallback cb, object state)
		{
			A = a;
			B = b;
			callback = cb;
			this.state = state;
		}

		public object AsyncState {
			get { return state; }
		}

		public WaitHandle AsyncWaitHandle {
			get { return null; }
		}

		public bool CompletedSynchronously {
			get { return true; }
		}

		public bool IsCompleted {
			get { return true; }
		}
	}
}
