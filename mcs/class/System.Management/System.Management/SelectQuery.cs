//
// System.Management.SelectQuery
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Collections.Specialized;

namespace System.Management
{
	public class SelectQuery : WqlObjectQuery
	{
		[MonoTODO]
		public SelectQuery ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public SelectQuery (string queryOrClassName)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public SelectQuery (bool isSchemaQuery, string condition)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public SelectQuery (string className, string condition)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public SelectQuery (string className, string condition, string [] selectedProperties)
		{
			throw new NotImplementedException ();
		}
		
		// Properties
		
		[MonoTODO]
		public string ClassName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string Condition {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public bool IsSchemaQuery {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override string QueryString {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public StringCollection SelectedProperties {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		// Methods
		
		[MonoTODO]
		protected internal void BuildQuery ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override object Clone ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override void ParseQuery (string query)
		{
			throw new NotImplementedException ();
		}
		
	}
}

