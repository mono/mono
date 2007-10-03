//
// System.Management.WqlEventQuery
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
	public class WqlEventQuery : EventQuery
	{
		[MonoTODO]
		public WqlEventQuery ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public WqlEventQuery (string queryOrEventClassName)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public WqlEventQuery (string eventClassName, TimeSpan withinInterval)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public WqlEventQuery (string eventClassName, string condition)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public WqlEventQuery (string eventClassName, TimeSpan withinInterval, string condition)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public WqlEventQuery (string eventClassName, string condition, TimeSpan groupWithinInterval)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public WqlEventQuery (string eventClassName, string condition, TimeSpan groupWithinInterval, string [] groupByPropertyList)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public WqlEventQuery (string eventClassName, TimeSpan withinInterval, string condition, TimeSpan groupWithinInterval, string [] groupByPropertyList, string havingCondition)
		{
			throw new NotImplementedException ();
		}
		
		// Properties
		
		[MonoTODO]
		public string Condition {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string EventClassName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public StringCollection GroupByPropertyList {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public TimeSpan GroupWithinInterval {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string HavingCondition {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override string QueryLanguage {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override string QueryString {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public TimeSpan WithinInterval {
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

