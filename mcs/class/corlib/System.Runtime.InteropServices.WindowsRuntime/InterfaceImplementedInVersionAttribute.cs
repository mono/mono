#if NET_4_5
//
// InterfaceImplementedInVersionAttribute.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System;
using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices.WindowsRuntime
{
	[AttributeUsageAttribute(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
	public sealed class InterfaceImplementedInVersionAttribute : Attribute
	{
		public InterfaceImplementedInVersionAttribute (Type interfaceType, byte majorVersion, byte minorVersion,
			byte buildVersion, byte revisionVersion)
		{
			InterfaceType = interfaceType;
			MajorVersion = majorVersion;
			MinorVersion = minorVersion;
			BuildVersion = buildVersion;
			RevisionVersion = revisionVersion;
		}

		public byte BuildVersion {
			get;
			private set;
		}

		public Type InterfaceType {
			get;
			private set;
		}

		public byte MajorVersion {
			get;
			private set;
		}

		public byte MinorVersion {
			get;
			private set;
		}
	
		public byte RevisionVersion {
			get;
			private set;
		}
	}
}
#endif
