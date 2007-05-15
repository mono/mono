//
// System.ComponentModel.DefaultValueAttribute.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
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

using System.Globalization;
namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public
#if !NET_2_0
	sealed
#endif
	class DefaultValueAttribute : Attribute
	{

		private object DefaultValue;

		public DefaultValueAttribute (bool value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (byte value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (char value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (double value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (short value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (int value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (long value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (object value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (float value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (string value)
		{
			DefaultValue = value;
		}

		public DefaultValueAttribute (Type type, string value)
		{
			try {
				TypeConverter converter = TypeDescriptor.GetConverter (type);
				DefaultValue = converter.ConvertFromString (null, CultureInfo.InvariantCulture, value);
			} catch { }
		}

		public object Value {
			get { return DefaultValue; }
		}

#if NET_2_0
		protected void SetValue (object value)
		{
			DefaultValue = value;
		}
#endif

		public override bool Equals (object obj)
		{
			if (!(obj is DefaultValueAttribute))
				return false;
			if (obj == this)
				return true;
			return ((DefaultValueAttribute) obj).Value == DefaultValue;
		}

		public override int GetHashCode()
		{
			return DefaultValue.GetHashCode();
		}
	}
}
