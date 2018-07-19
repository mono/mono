//
// PeerResolversSerializationTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.PeerResolvers;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.PeerResolvers
{

[TestFixture]
public class PeerResolverSerializationTest
{
	[Test]
	public void HasBody ()
	{
		Assert.IsTrue (new RegisterInfo ().HasBody (), "#1");
		Assert.IsTrue (new RegisterResponseInfo ().HasBody (), "#2");
		Assert.IsTrue (new ResolveInfo ().HasBody (), "#3");
		Assert.IsTrue (new ResolveResponseInfo ().HasBody (), "#4");
		Assert.IsTrue (new RefreshInfo ().HasBody (), "#5");
		Assert.IsTrue (new RefreshResponseInfo ().HasBody (), "#6");
	}

	[Test]
	public void ResolveResponseInfo ()
	{
		var ser = new DataContractSerializer (typeof (ResolveResponseInfo));
		var rri = new ResolveResponseInfo ();
		var pna = new PeerNodeAddress (
			new EndpointAddress ("http://localhost:8080"),
			new ReadOnlyCollection<IPAddress> (new IPAddress [0]));
		rri.Addresses = new List<PeerNodeAddress> ();
		rri.Addresses.Add (pna);
		var sw = new StringWriter ();
		using (var xw = XmlWriter.Create (sw))
			ser.WriteObject (xw, rri);
		rri = (ResolveResponseInfo) ser.ReadObject (XmlReader.Create (new StringReader (sw.ToString ())));
		Assert.AreEqual (1, rri.Addresses.Count, "#1");
	}
}

/*
[DataContract]
public class ResolveResponseInfo
{
	public ResolveResponseInfo ()
	{
		Addresses = new List<PeerNodeAddress> ();
	}

	[DataMember]
	public IList<PeerNodeAddress> Addresses { get; set; }
}

public class PeerNodeAddress
{
	public PeerNodeAddress (EndpointAddress ea, IList<IPAddress> al)
	{
		Endpoint = ea;
		Addresses = al;
	}

	public EndpointAddress Endpoint { get; set; }
	public IList<IPAddress> Addresses { get; set; }
}
*/

}
#endif

