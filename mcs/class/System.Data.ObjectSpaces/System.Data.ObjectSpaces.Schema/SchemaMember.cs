//
// System.Data.ObjectSpaces.Schema.SchemaMember.cs
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

using System.Data.Mapping;
using System.Reflection;

namespace System.Data.ObjectSpaces.Schema {
	public class SchemaMember : IDomainField
	{
		#region Fields

		string alias;
		bool isHidden;
		bool isKey;
		bool isLazyLoad;
		KeyGenerator keyGenerator;
		string name;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SchemaMember ()
		{
		}

		[MonoTODO]
		public SchemaMember (string name)
		{
			Name = name;
		}

		[MonoTODO]
		public SchemaMember (string name, bool key)
			: this (name)
		{
			IsKey = key;
		}

		#endregion // Constructors

		#region Properties

		public string Alias {
			get { return alias; }
			set { alias = value; }
		}

		[MonoTODO]
		public SchemaClass DeclaringSchemaClass {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ExtendedPropertyCollection ExtendedProperties {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		IDomainStructure IDomainField.DomainStructure {
			get { throw new NotImplementedException (); }
		}

		public bool IsHidden {
			get { return isHidden; }
			set { isHidden = value; }
		}

		public bool IsKey {
			get { return isKey; }
			set { isKey = value; }
		}

		public bool IsLazyLoad {
			get { return isLazyLoad; }
			set { isLazyLoad = value; }
		}

		public KeyGenerator KeyGenerator {
			get { return keyGenerator; }
			set { keyGenerator = value; }
		}

		[MonoTODO]
		public MemberInfo MemberInfo {
			get { throw new NotImplementedException (); }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		[MonoTODO]
		public ObjectRelationship ObjectRelationship {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}

#endif // NET_2_0
