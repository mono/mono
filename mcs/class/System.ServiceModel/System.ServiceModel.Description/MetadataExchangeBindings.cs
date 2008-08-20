//
// MetadataExchangeBindings.cs
//
// Author:
//	Ankit Jain <jankit@novell.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel.Channels;
using System.Text;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public static class MetadataExchangeBindings
	{
		public static Binding CreateMexHttpBinding ()
		{
			//FIXME: .net uses WSHttpBinding.. 
			return new CustomBinding (
				"MetadataExchangeHttpBinding",
				"http://schemas.microsoft.com/ws/2005/02/mex/bindings",
				new TextMessageEncodingBindingElement (
					MessageVersion.Soap12WSAddressing10, 
					Encoding.UTF8),
				new HttpTransportBindingElement ());
		}

		public static Binding CreateMexHttpsBinding ()
		{
			throw new NotImplementedException ();
		}

		public static Binding CreateMexNamedPipeBinding ()
		{
			throw new NotImplementedException ();
		}

		public static Binding CreateMexTcpBinding ()
		{
			throw new NotImplementedException ();
		}
	}
}
