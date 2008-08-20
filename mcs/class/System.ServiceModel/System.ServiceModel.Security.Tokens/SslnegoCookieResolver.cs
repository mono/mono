//
// SslnegoCookieResolver.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

/*

LAMESPEC: The cookie value is encoded by
ServiceCredential.SecureConversationAuthentication.SecurityStateEncoder.

When a custom SecurityStateEncoder is used,
- at service side it is actually invoked,
- at client side it is impossible to specify such custom state decoder, so
  it is treated as if the key is passed as is, and thus if it could raise
  verification error (it is LAMESPEC, since if the custom state encoder is
  pass-through it just works fine).

Raw Cookie data format (via pass-through SecurityStateEncoder)

<42 00 42 02 83 42 06 99> L[uuid-_________] bbbb-bb 
<42 04 AD> (16bytes)  <42 08 9E 1E> (43 bytes)
<C9 08 42 10 8F> (6 bytes)
<C9 08 42 14 8F> (6 bytes)
<C9 08 42 16 8F> (6 bytes)
<C9 08 01>

The uuid seems kept identical while one service is running (i.e. unique per ServiceHost).

Actually the raw octets corresponds to 
XmlBinaryWriter output format, so it is likely.
So, it will be parsed as below:
42 00 
42 02 
83 
42 06 99 2B 75 75 69 64 2D 31 65 38 33 62 63 37 39 2D 35 30 33 37 2D 34 61 32 30 2D 38 32 66 37 2D 64 32 39 37 31 34 61 30 32 62 37 66 2D 31 // UniqueId wsu:Id
42 04 AD 45 34 07 4E 38 D2 18 4D 8B 22 FD 6C E6 CE B2 17 // UniqueIdFromGuid ContextId
42 08 9E 1E CA AC F2 71 6E 61 99 DA FB 71 B2 A8 DC 51 36 5B CD F3 F9 60 D2 B6 67 BF 5D B0 CE ED 37 35 9F 02 DC 7D // Base64 Key
42 0E 8F F4 4C 9C 48 61 33 C9 08 // Int64 KeyCreation
42 10 8F F4 5C 48 1A B5 33 C9 08 // Int64 KeyExpiration
42 14 8F F4 4C 9C 48 61 33 C9 08 // Int64 CookieCreatation
42 16 8F F4 5C 48 1A B5 33 C9 08 // Int64 CookieExpiration
01


The actual XML looks like:
<n1><n2>1</n2><n4>uuid-950f764e-f6dc-4f5d-8df36699e28618cf-1</n4><n3>urn:uuid:a13aa8b0-f0b5-4a78-967e-fbd05459d882</n3><n5>W0I2qFT/H5ElE14l3wy8rqZHVvjbesvtshaLOdQdXyk=</n5><n8>633092852947500000</n8><n9>633093212947500000</n9><n11>633092852947500000</n11><n12>633093212947500000</n12></n1>

where n[x] are presumed names. They would be meaningful in MS implementation,
but as a binary XML array with preconfigured IXmlDictionary (sigh), it doesn't
matter.

n2 matches the context Identifier for SecurityContextToken.
n3 matches the u:Id for SecurityContextToken.

*/


namespace System.ServiceModel.Security.Tokens
{
	internal class SslnegoCookieResolver
	{
		public static SecurityContextSecurityToken ResolveCookie (byte [] bytes, byte [] cookie)
		{
			string id = null;
			UniqueId context = null;
			DateTime validFrom = DateTime.MinValue,
				 validTo = DateTime.MaxValue,
				 keyEffective = DateTime.MinValue,
				 keyExpired = DateTime.MaxValue;
			byte [] key = null;
			X509Certificate2 cert = null;
			X500DistinguishedName issuer = null;

			XmlDictionary dic = new XmlDictionary ();
			for (int i = 0; i < 30; i++)
				dic.Add ("n" + i);
			// FIXME: create proper quotas
			XmlDictionaryReaderQuotas quotas =
				new XmlDictionaryReaderQuotas ();
			XmlDictionaryReader cr = XmlDictionaryReader.CreateBinaryReader (bytes, 0, bytes.Length, dic, quotas);

			cr.MoveToContent (); // -> n1
			cr.ReadStartElement ("n0", String.Empty);
			do {
				cr.MoveToContent ();
				if (cr.NodeType == XmlNodeType.EndElement)
					break;
				if (cr.NodeType != XmlNodeType.Element)
					throw new Exception ("Unxpected non-element content:" + cr.NodeType);

				switch (cr.Name) {
				case "n1":
					// FIXME: some integer here
					int n1 = cr.ReadElementContentAsInt ();
					if (n1 != 1)
						throw new Exception ("INTERNAL ERROR: there was unexpected n2 content: " + n1);
					break;
				case "n2":
					context = cr.ReadElementContentAsUniqueId ();
					break;
				case "n3":
					id = cr.ReadElementContentAsString ();
					break;
				case "n4":
					key = cr.ReadElementContentAsBase64 ();
					break;
				case "n7":
					validFrom = new DateTime (cr.ReadElementContentAsLong ());
					break;
				case "n8":
					validTo = new DateTime (cr.ReadElementContentAsLong ());
					break;
				case "n10":
					keyEffective = new DateTime (cr.ReadElementContentAsLong ());
					break;
				case "n11":
					keyExpired = new DateTime (cr.ReadElementContentAsLong ());
					break;
				case "n13":
					// <n18>X509Certificate</n18>
					cr.Read ();
					cr.MoveToContent ();
					cert = new X509Certificate2 (cr.ReadElementContentAsBase64 ());
					cr.ReadEndElement ();
					break;
				case "n15":
					// <n16><n24 n25="IssuerName" /></n16>
					cr.Read ();
					cr.ReadStartElement ("n16", String.Empty);
					issuer = new X500DistinguishedName (cr.GetAttribute ("n25"));
					bool empty = cr.IsEmptyElement;
					cr.ReadStartElement ("n24", String.Empty);
					if (!empty)
						cr.ReadEndElement (); // n24
					cr.ReadEndElement (); // n16
					cr.ReadEndElement (); // n15
					break;
				default:
					throw new Exception ("INTERNAL ERROR: there was an unhandled element: " + cr.Name);
				}
			} while (true);

			SecurityContextSecurityToken sct = new SecurityContextSecurityToken (
				context, id, key, validFrom, validTo,
				null, keyEffective, keyExpired, null);
			sct.Cookie = cookie;
			return sct;
		}

		public static byte [] CreateData (UniqueId contextId, UniqueId session, byte [] key, DateTime tokenSince, DateTime tokenUntil, DateTime keySince, DateTime keyUntil)
		{
			XmlDictionary dic = new XmlDictionary ();
			for (int i = 0; i < 12; i++)
				dic.Add ("n" + i);
			MemoryStream ms = new MemoryStream ();
			XmlDictionaryWriter w = XmlDictionaryWriter.CreateBinaryWriter (ms, dic);
			XmlDictionaryString e = XmlDictionaryString.Empty;
			w.WriteStartElement (dic.Add ("n0"), e);
			w.WriteStartElement (dic.Add ("n1"), e);
			w.WriteValue (1);
			w.WriteEndElement ();
			w.WriteStartElement (dic.Add ("n3"), e);
			w.WriteValue (contextId);
			w.WriteEndElement ();
			w.WriteStartElement (dic.Add ("n2"), e);
			w.WriteValue (contextId);
			w.WriteEndElement ();
			w.WriteStartElement (dic.Add ("n4"), e);
			w.WriteBase64 (key, 0, key.Length);
			w.WriteEndElement ();
			w.WriteStartElement (dic.Add ("n7"), e);
			w.WriteValue (tokenSince.Ticks);
			w.WriteEndElement ();
			w.WriteStartElement (dic.Add ("n8"), e);
			w.WriteValue (tokenUntil.Ticks);
			w.WriteEndElement ();
			w.WriteStartElement (dic.Add ("n10"), e);
			w.WriteValue (keySince.Ticks);
			w.WriteEndElement ();
			w.WriteStartElement (dic.Add ("n11"), e);
			w.WriteValue (keyUntil.Ticks);
			w.WriteEndElement ();
			w.Close ();
			return ms.ToArray ();
		}
	}
}
