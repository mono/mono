//
// DynamicRecord.cs
//
// Copyright (c) 2011 Novell
//
// Authors:
//     Jérémie "garuma" Laval
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
//

#if NET_4_0

using System;
using System.Dynamic;
using System.Data.Common;
using System.Collections.Generic;
using System.ComponentModel;

namespace WebMatrix.Data
{
	public sealed class DynamicRecord : DynamicObject, ICustomTypeDescriptor
	{
		readonly Dictionary<string, object> fields;

		internal DynamicRecord (Dictionary<string, object> fields)
		{
			this.fields = fields;
			Columns = fields.Keys;
		}

		public IList<string> Columns {
			get;
			private set;
		}

		public object this[string name] {
			get {
				return fields[name];
			}
		}

		public object this[int index] {
			get {				
				return fields.Keys[index];
			}
		}

		public override IEnumerable<string> GetDynamicMemberNames ()
		{
			return Columns;
		}

		public override bool TryGetMember (GetMemberBinder binder, out object result)
		{
			return fields.TryGetValue (binder.Name, out result);
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			return null;
		}

		string ICustomTypeDescriptor.GetClassName ()
		{
			return null;
		}

		string ICustomTypeDescriptor.GetComponentName ()
		{
			return null;
		}

		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			return null;
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			return null;
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			return null;
		}

		Object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			return null;
		}

		Object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			return null;
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			return null;
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute[] attributes)
		{
			return null;
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			return null;
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute[] attributes)
		{
			return null;
		}
	}
}

#endif