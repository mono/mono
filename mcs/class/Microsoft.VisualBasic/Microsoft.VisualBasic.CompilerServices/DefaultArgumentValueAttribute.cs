//
// Microsoft.VisualBasic.CompilerServices.__DefaultArgumentValueAttribute.cs
//
// Authors:
//   Jambunathan K (kjambunathan@novell.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;

namespace Microsoft.VisualBasic.CompilerServices
{
	// Many of the methods of Microsoft.VisualBasic.dll take
	// default argument values. The whole of Mono's
	// Microsoft.VisualBasic.dll is implemented in C# and C#
	// provides no standard constructs that lets a method specify
	// default argument values and set appropriate metadata on a
	// method parameter.

	// This internal attribute:
	// Microsoft.VisualBasic.CompilerServices.__DefaultArgumentValueAttribute
	// is used to associate a default argument value with a method
	// parameter. 

	// The compiled Microsoft.VisualBasic.dll assembly is then
	// post-processed through a "disassemble-fixup-assemble cycle"
	// which strips off the "__DefaultArgumentValue" custom
	// attribute from the method parameter and replaces it with
	// the suitable CIL parameter flags and constants.

	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class __DefaultArgumentValueAttribute : Attribute
	{
		private object DefaultArgumentValue;

		public __DefaultArgumentValueAttribute (bool value)
		{
			DefaultArgumentValue = value;
		}

		public __DefaultArgumentValueAttribute (char value)
		{
			DefaultArgumentValue = value;
		}

		public __DefaultArgumentValueAttribute (byte value)
		{
			DefaultArgumentValue = value;
		}

		public __DefaultArgumentValueAttribute (short value)
		{
			DefaultArgumentValue = value;
		}

		public __DefaultArgumentValueAttribute (int value)
		{
			DefaultArgumentValue = value;
		}

		public __DefaultArgumentValueAttribute (long value)
		{
			DefaultArgumentValue = value;
		}

		public __DefaultArgumentValueAttribute (float value)
		{
			DefaultArgumentValue = value;
		}

		public __DefaultArgumentValueAttribute (double value)
		{
			DefaultArgumentValue = value;
		}

		public __DefaultArgumentValueAttribute (string value)
		{
			DefaultArgumentValue = value;
		}

		public __DefaultArgumentValueAttribute (object value)
		{
			DefaultArgumentValue = value;
		}

		public object Value {
			get { return DefaultArgumentValue; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is __DefaultArgumentValueAttribute))
				return false;
			if (obj == this)
				return true;
			return ((__DefaultArgumentValueAttribute) obj).Value == DefaultArgumentValue;
		}

		public override int GetHashCode()
		{
			return DefaultArgumentValue.GetHashCode();
		}
	}
}
