//
// System.ServiceModel.EnvelopeVersion.cs
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

namespace System.ServiceModel
{

	public sealed class EnvelopeVersion
	{
		const string Soap11NextReceiver = "http://schemas.xmlsoap.org/soap/actor/next";
		const string Soap12NextReceiver = "http://www.w3.org/2003/05/soap-envelope/role/next";
		internal const string Soap12UltimateReceiver = "http://www.w3.org/2003/05/soap-envelope/role/ultimateReceiver";

		string name, uri, next_destination;
		string [] ultimate_destination;

		static EnvelopeVersion soap11 = new EnvelopeVersion ("Soap11",
								     "http://schemas.xmlsoap.org/soap/envelope/",
								     Soap11NextReceiver,
								     String.Empty,
								     Soap11NextReceiver);

		static EnvelopeVersion soap12 = new EnvelopeVersion ("Soap12",
								     "http://www.w3.org/2003/05/soap-envelope",
								     Soap12NextReceiver,
								     String.Empty,
								     Soap12UltimateReceiver,
								     Soap12NextReceiver);

		static EnvelopeVersion none = new EnvelopeVersion ("EnvelopeNone",
								     "http://schemas.microsoft.com/ws/2005/05/envelope/none",
								     String.Empty,
								     null);

		EnvelopeVersion (string name, string uri, string next_destination, params string [] ultimate_destination)
		{
			this.name = name;
			this.uri = uri;
			this.next_destination = next_destination;
			this.ultimate_destination = ultimate_destination;
		}


		internal string Namespace { get { return uri; }}

		public static EnvelopeVersion Soap11 {
			get {  return soap11; }
		}

		public static EnvelopeVersion Soap12 {
			get { return soap12; }
		}

		public static EnvelopeVersion None {
			get { return none; }
		}

		public string NextDestinationActorValue {
			get { return next_destination; }
		}

		public string [] GetUltimateDestinationActorValues ()
		{
			return ultimate_destination;
		}

		public override string ToString ()
		{
			return name + "(" + uri + ")";
		}
	}
}