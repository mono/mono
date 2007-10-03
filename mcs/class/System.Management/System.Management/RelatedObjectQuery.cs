//
// System.Management.RelatedObjectQuery
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

namespace System.Management
{
	public class RelatedObjectQuery : WqlObjectQuery
	{
		[MonoTODO]
		public RelatedObjectQuery ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public RelatedObjectQuery (string queryOrSourceObject)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public RelatedObjectQuery (string sourceObject, string relatedClass)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public RelatedObjectQuery (bool isSchemaQuery, string sourceObject, string relatedClass, string relationshipClass, string relatedQualifier, string relationshipQualifier, string relatedRole, string thisRole)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public RelatedObjectQuery (string sourceObject, string relatedClass, string relationshipClass, string relatedQualifier, string relationshipQualifier, string relatedRole, string thisRole, bool classDefinitionsOnly)
		{
			throw new NotImplementedException ();
		}
		
		// Properties
		
		[MonoTODO]
		public bool ClassDefinitionsOnly {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public bool IsSchemaQuery {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string RelatedClass {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string RelatedQualifier {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string RelatedRole {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string RelationshipClass {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string RelationshipQualifier {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string SourceObject {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string ThisRole {
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

