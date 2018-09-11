//
// ParameterBuilder.cs
//
// Authors:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

#if !FEATURE_SRE

namespace System.Reflection.Emit
{
	public class ParameterBuilder
	{
		ParameterBuilder ()
		{
		}

		public int Attributes {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool IsIn {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool IsOptional {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool IsOut {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public string Name {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public int Position {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public virtual void SetConstant (object defaultValue)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetCustomAttribute (CustomAttributeBuilder customBuilder)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetCustomAttribute (ConstructorInfo con, byte[] binaryAttribute)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
