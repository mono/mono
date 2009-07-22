//
// TableAdapterSchemaInfo.cs
//
// Author:
//	Veerapuram Varadhan (vvaradhan@novell.com)
//
// Copyright (C) 2009 Novell, Inc.
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
using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace System.Data
{
	internal enum GenerateMethodsType {
		None,
		Get,
		Fill,
		Both
	}
	
	internal enum QueryType {
		NoData,
		Rowset,
		Scalar
	}
	
	internal class DbSourceMethodInfo
	{
		public GenerateMethodsType MethodType;
		public string Name;
		public string Modifier;
		public string QueryType;
		public string ScalarCallRetval;
	}
	
	internal class DbCommandInfo
	{
		public DbCommand Command;
		public DbSourceMethodInfo[] Methods;
	}
	
	internal class TableAdapterSchemaInfo
	{
		public TableAdapterSchemaInfo (DbProviderFactory provider) {
			this.Provider = provider;
			this.Adapter = provider.CreateDataAdapter ();
			this.Connection = provider.CreateConnection ();
			this.Commands = new ArrayList ();
			this.ShortCommands = false;
		}
		
		public TableAdapterSchemaInfo ()
		{
			this.Commands = new ArrayList ();
			this.ShortCommands = false;
		}

		public DbProviderFactory Provider;
		public DbDataAdapter Adapter;
		public DbConnection Connection;
		public string ConnectionString;
		public string BaseClass;
		public string Name;
		public bool ShortCommands;
		// List of DbCommandInfo objects
		public ArrayList Commands;
	}
}
#endif