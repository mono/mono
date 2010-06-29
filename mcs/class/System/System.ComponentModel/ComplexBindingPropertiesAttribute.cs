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

//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

namespace System.ComponentModel 
{
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class ComplexBindingPropertiesAttribute : Attribute
	{
		string data_source;
		string data_member;

		public static readonly ComplexBindingPropertiesAttribute Default = new ComplexBindingPropertiesAttribute ();

		public ComplexBindingPropertiesAttribute (string dataSource, string dataMember)
		{
			data_source = dataSource;
			data_member = dataMember;
		}

		public ComplexBindingPropertiesAttribute (string dataSource)
		{
			data_source = dataSource;
		}

		public ComplexBindingPropertiesAttribute ()
		{
		}

		public string DataMember {
			get { return data_member; }
		}

		public string DataSource {
			get { return data_source; }
		}

		public override bool Equals (object obj)
		{
			ComplexBindingPropertiesAttribute a = obj as ComplexBindingPropertiesAttribute;
			if (a == null)
				return false;

			return a.DataMember == data_member && a.DataSource == data_source;
		}

		public override int GetHashCode ()
		{
			int hc = data_source == null ? 0 : data_source.GetHashCode ();
			hc ^= data_member == null ? 0 : data_member.GetHashCode ();
			return hc;
		}
	}
}

#endif
