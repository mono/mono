//
// Mono.Data.SybaseClient.SybaseCommandBuilder.cs
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
using System.Data;
using System.ComponentModel;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseCommandBuilder : Component 
	{
		#region Constructors

		[MonoTODO]
		public SybaseCommandBuilder() 
		{
		}

		[MonoTODO]
		public SybaseCommandBuilder(SybaseDataAdapter adapter) 
		{
		}
		
		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public SybaseDataAdapter DataAdapter {
			get { throw new NotImplementedException (); }
			set{ throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string QuotePrefix {
			get { throw new NotImplementedException (); } 
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string QuoteSuffix {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static void DeriveParameters (SybaseCommand command) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose(bool disposing) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SybaseCommand GetDeleteCommand() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SybaseCommand GetInsertCommand() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SybaseCommand GetUpdateCommand() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RefreshSchema() 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

