//
// System.Data.Common.TdsDataColumn.cs
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

using System.Collections;
using System.Text;

namespace Mono.Data.Tds.Protocol {
	public class TdsDataColumn
	{
		Hashtable properties;

		public TdsDataColumn ()
		{
#if NET_2_0
			IsAutoIncrement = false;
			IsIdentity = false;
			IsRowVersion = false;
			IsUnique = false;
			IsHidden = false;
#else
			object bool_false = false;
			
			this ["IsAutoIncrement"] = bool_false;
			this ["IsIdentity"] = bool_false;
			this ["IsRowVersion"] = bool_false;
			this ["IsUnique"] = bool_false;
			this ["IsHidden"] = bool_false;
#endif
		}


#if NET_2_0
		public TdsColumnType? ColumnType {
			get;
			set;
		}
		
		public string ColumnName {
			get;
			set;
		}

		public int? ColumnSize {
			get;
			set;
		}
		
		public int? ColumnOrdinal {
			get;
			set;
		}

		public bool? IsAutoIncrement {
			get;
			set;
		}

		public bool? IsIdentity {
			get;
			set;
		}

		public bool? IsRowVersion {
			get;
			set;
		}

		public bool? IsUnique {
			get;
			set;
		}

		public bool? IsHidden {
			get;
			set;
		}

		public bool? IsKey {
			get;
			set;
		}

		public bool? IsAliased {
			get;
			set;
		}

		public bool? IsExpression {
			get;
			set;
		}

		public bool? IsReadOnly {
			get;
			set;
		}
		
		public short? NumericPrecision {
			get;
			set;
		}

		public short? NumericScale {
			get;
			set;
		}

		public string BaseServerName {
			get;
			set;
		}

		public string BaseCatalogName {
			get;
			set;
		}

		public string BaseColumnName {
			get;
			set;
		}

		public string BaseSchemaName {
			get;
			set;
		}

		public string BaseTableName {
			get;
			set;
		}

		public bool? AllowDBNull {
			get;
			set;
		}
		
		public int? LCID {
			get;
			set;
		}
		
		public int? SortOrder {
			get;
			set;
		}
		
		public string DataTypeName {
			get;
			set;
		}
#endif
		
		// This allows the storage of arbitrary properties in addition to the predefined ones
		public object this [string key] {
			get {
				if (properties == null)
					return null;
				return properties [key];
			}
			set {
				if (properties == null)
					properties = new Hashtable ();
				properties [key] = value;
			}
		}
	}
}
