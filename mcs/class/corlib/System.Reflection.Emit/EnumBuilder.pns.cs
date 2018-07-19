//
// EnumBuilder.pns.cs
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

#if FULL_AOT_RUNTIME

namespace System.Reflection.Emit
{
	public abstract class EnumBuilder : TypeInfo
	{
		public FieldBuilder UnderlyingField {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Assembly Assembly {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string AssemblyQualifiedName {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Type BaseType {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string FullName {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Guid GUID {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Module Module {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string Name {
			get {
				throw new PlatformNotSupportedException ();
			}
		}
		
		public override string Namespace {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public TypeInfo CreateTypeInfo ()
		{
			throw new PlatformNotSupportedException ();
		}

		public FieldBuilder DefineLiteral (string literalName, object literalValue)
		{
			throw new PlatformNotSupportedException ();
		}

		public override System.Type GetElementType ()
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