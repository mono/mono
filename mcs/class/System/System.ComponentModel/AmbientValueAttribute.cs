//
// System.ComponentModel.AmbientValueAttribute
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
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

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class AmbientValueAttribute : Attribute
	{

		private object AmbientValue;

		public AmbientValueAttribute (bool value)
		{
			AmbientValue = value;
		}

		public AmbientValueAttribute (byte value)
		{
			AmbientValue = value;
		}

		public AmbientValueAttribute (char value)
		{
			AmbientValue = value;
		}

		public AmbientValueAttribute (double value)
		{
			AmbientValue = value;
		}

		public AmbientValueAttribute (short value)
		{
			AmbientValue = value;
		}

		public AmbientValueAttribute (int value)
		{
			AmbientValue = value;
		}

		public AmbientValueAttribute (long value)
		{
			AmbientValue = value;
		}

		public AmbientValueAttribute (object value)
		{
			AmbientValue = value;
		}

		public AmbientValueAttribute (float value)
		{
			AmbientValue = value;
		}

		public AmbientValueAttribute (string value)
		{
			AmbientValue = value;
		}

		public AmbientValueAttribute (Type type, string value)
		{
			try {
				AmbientValue = Convert.ChangeType (value, type);
			} catch {
				AmbientValue = null;
			}
		}

		public object Value {
			get { return AmbientValue; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is AmbientValueAttribute))
				return false;

			if (obj == this)
				return true;

			return ((AmbientValueAttribute) obj).Value == AmbientValue;
		}

		public override int GetHashCode()
		{
			return AmbientValue.GetHashCode ();
		}
	}
}
