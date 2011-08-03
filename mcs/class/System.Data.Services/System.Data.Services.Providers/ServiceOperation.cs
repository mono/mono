// 
// ServiceOperation.cs
//  
// Author:
//       Marek Habersack <grendel@twistedcode.net>
// 
// Copyright (c) 2011 Novell, Inc
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Providers;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace System.Data.Services.Providers
{
	[DebuggerVisualizer ("ServiceOperation={Name}")]
	public class ServiceOperation
	{
		public string Method {
			get { throw new NotImplementedException (); }
		}

		public string MimeType {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string Name {
			get { throw new NotImplementedException (); }
		}

		public ReadOnlyCollection <ServiceOperationParameter> Parameters {
			get { throw new NotImplementedException (); }
		}

		public ServiceOperationResultKind ResultKind {
			get { throw new NotImplementedException (); }
		}

		public ResourceType ResultType {
			get { throw new NotImplementedException (); }
		}

		public object CustomState {
			get; set;
		}

		public bool IsReadOnly {
			get { throw new NotImplementedException (); }
		}

		public ResourceSet ResourceSet {
			get { throw new NotImplementedException (); }
		}

		public ServiceOperation (string name, ServiceOperationResultKind resultKind, ResourceType resultType, ResourceSet resultSet, string method, IEnumerable <ServiceOperationParameter> parameters)
		{
			throw new NotImplementedException ();
		}

		public void SetReadOnly ()
		{
			throw new NotImplementedException ();
		}
	}
}
