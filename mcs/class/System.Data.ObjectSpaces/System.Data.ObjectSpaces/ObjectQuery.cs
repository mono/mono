//
// System.Data.ObjectSpaces.ObjectQuery.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

namespace System.Data.ObjectSpaces
{
        public class ObjectQuery
        {
		bool baseTypeOnly;
		Type objectType;
		string query;
		string sort;
		string span;

		[MonoTODO()]
		public ObjectQuery (Type type, string query)
			: this (type, query, null)
		{
		}

		[MonoTODO()]
		public ObjectQuery (Type type, string query, string span)
		{
			SetObjectType (type);
			SetQuery (query);

			this.baseTypeOnly = false;
			this.sort = null;
			this.span = span;
		}

		[MonoTODO("Error handling")]
		public bool BaseTypeOnly {
			get { return baseTypeOnly; }
			set { baseTypeOnly = value; }
		}

		public Type ObjectType {
			get { return objectType; }
		}

		public string Query {
			get { return query; }
		}

		public string Sort {
			get { return sort; }
			set { sort = value; }
		}

		public string Span {
			get { return span; }
		}

		[MonoTODO()]
		private void SetObjectType (Type type)
		{
			objectType = type;
		}

		[MonoTODO()]
		private void SetQuery (string query)
		{
			this.query = query;
		}
        }
}

#endif
