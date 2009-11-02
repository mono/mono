//
// System.UriFormatException.cs
//
// Author:
//   Scott Sanders (scott@stonecobra.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Scott Sanders
// (C) 2002 Ximian, Inc.
// Copyright (C) 2005, 2008 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;

namespace System {

	[Serializable]
	public class UriFormatException : FormatException, ISerializable
	{

		// Constructors
		public UriFormatException ()
			: base (Locale.GetText ("Invalid URI format"))
		{
		}

		public UriFormatException (string message)
			: base (message)
		{
		}

#if NET_2_1 || NET_4_0
		public UriFormatException (string message, Exception exception)
			: base (message, exception)
		{
		}
#endif
		protected UriFormatException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{			
		}

		// Methods

		// This effectively kills the LinkDemand from Exception.GetObjectData (if someone
		// use the ISerializable interface to serialize the object). See unit tests.
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}
	}
}
