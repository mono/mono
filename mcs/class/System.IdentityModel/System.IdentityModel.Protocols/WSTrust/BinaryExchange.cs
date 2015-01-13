//
// BinaryExchange.cs
//
// Author:
//   Noesis Labs (Ryan.Melena@noesislabs.com)
//
// Copyright (C) 2014 Noesis Labs, LLC  https://noesislabs.com
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

namespace System.IdentityModel.Protocols.WSTrust
{
	public class BinaryExchange
	{
		private const string defaultEncodingTypeUrl = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";

		public byte[] BinaryData { get; private set; }
		public Uri EncodingType { get; private set; }
		public Uri ValueType { get; private set; }

		public BinaryExchange (byte[] binaryData, Uri valueType)
			: this (binaryData, valueType, new Uri (defaultEncodingTypeUrl))
		{ }

		public BinaryExchange (byte[] binaryData, Uri valueType, Uri encodingType) {
			BinaryData = binaryData;
			ValueType = valueType;
			EncodingType = encodingType;
		}
	}
}
