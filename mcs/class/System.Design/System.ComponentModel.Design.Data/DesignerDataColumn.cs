//
// System.ComponentModel.Design.Data.DesignerDataColumn
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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


using System.Data;

namespace System.ComponentModel.Design.Data
{
	public sealed class DesignerDataColumn
	{
		[MonoTODO]
		public DesignerDataColumn (string name, DbType dataType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DesignerDataColumn (string name, DbType dataType, object defaultValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DesignerDataColumn (string name, DbType dataType, object defaultValue, bool identity, bool nullable, bool primaryKey, int precision, int scale, int length)
		{
			this.name = name;
			this.data_type =dataType;
			this.default_value = defaultValue;
			this.identity = identity;
			this.nullable = nullable;
			this.primary_key = primaryKey;
			this.precision = precision;
			this.scale = scale;
			this.length = length;
		}

		string name;
		DbType data_type;
		object default_value;
		bool identity;
		bool nullable;
		bool primary_key;
		int precision;
		int scale;
		int length;

		public string Name {
			get { return name; }
		}
		public DbType DataType {
			get { return data_type; }
		}

		public object DefaultValue {
			get { return default_value; }
		}
		public bool Identity {
			get { return identity; }
		}
		public bool Nullable {
			get { return nullable; }
		}
		public bool PrimaryKey {
			get { return primary_key; }
		}
		public int Precision {
			get { return precision; }
		}
		public int Scale {
			get { return scale; }
		}
		public int Length {
			get { return length; }
		}
	}
}

