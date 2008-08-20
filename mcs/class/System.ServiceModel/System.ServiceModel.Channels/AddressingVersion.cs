//
// System.ServiceModel.AddressingVersion.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;

namespace System.ServiceModel.Channels
{
	public sealed class AddressingVersion
	{
		string name;
		string address;

		AddressingVersion (string name, string address)
		{
			this.name = name;
			this.address = address;
		}

		static AddressingVersion addressing200408 = new AddressingVersion (
			"Addressing200408",
			"http://schemas.xmlsoap.org/ws/2004/08/addressing");

		static AddressingVersion addressing1_0 = new AddressingVersion (
			"Addressing10",
			"http://www.w3.org/2005/08/addressing");

		static AddressingVersion none = new AddressingVersion (
			"AddressingNone",
			"http://schemas.microsoft.com/ws/2005/05/addressing/none");

		public static AddressingVersion WSAddressing10 {
			get { return addressing1_0; }
		}

		public static AddressingVersion WSAddressingAugust2004 {
			get { return addressing200408; }
		}

		public static AddressingVersion None {
			get { return none; }
		}

		internal string Namespace {
			get { return address; }
		}

		internal string ActionNotSupported {
			get { return "ActionNotSupported"; }
		}

		public override string ToString ()
		{
			return name + " (" + address + ")";
		}
	}
}