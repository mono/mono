// 
// DataServiceConfiguration.cs
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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Text;

namespace System.Data.Services
{
	public sealed class DataServiceConfiguration : IDataServiceConfiguration
	{
		public bool EnableTypeConversion {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int MaxBatchCount {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int MaxChangesetCount {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int MaxExpandCount {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int MaxExpandDepth {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int MaxResultsPerCollection {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int MaxObjectCountOnInsert {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public bool UseVerboseErrors {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public DataServiceBehavior DataServiceBehavior {
			get { throw new NotImplementedException (); }
		}

		public void SetEntitySetAccessRule (string name, EntitySetRights rights)
		{
			throw new NotImplementedException ();
		}

		public void SetServiceOperationAccessRule (string name, ServiceOperationRights rights)
		{
			throw new NotImplementedException ();
		}

		public void RegisterKnownType (Type type)
		{
			throw new NotImplementedException ();
		}

		public void SetEntitySetPageSize (string name, int size)
		{
			throw new NotImplementedException ();
		}

		public void EnableTypeAccess (string typeName)
		{
			throw new NotImplementedException ();
		}
	}
}
