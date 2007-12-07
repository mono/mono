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
// Copyright © 2006, 2007 Nauck IT KG		http://www.nauck-it.de
//
// Author:
//	Daniel Nauck		<d.nauck(at)nauck-it.de>

#if NET_2_0
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace System.Web.Util
{
	internal class SerializationHelper
	{
		internal string SerializeToBase64(object value)
		{
			return Convert.ToBase64String(SerializeToBinary(value));
		}

		internal object DeserializeFromBase64(string value)
		{
			return DeserializeFromBinary(Convert.FromBase64String(value));
		}

		internal string SerializeToXml(object value)
		{
			using (MemoryStream mStream = new MemoryStream())
			{
				XmlSerializer xmlFormatter = new XmlSerializer(typeof(object), "http://www.nauck-it.de/PostgreSQLProvider");
				xmlFormatter.Serialize(mStream, value);
				return Convert.ToBase64String(mStream.ToArray());
			}
		}

		internal object DeserializeFromXml(string value)
		{
			using (MemoryStream mStream = new MemoryStream(Convert.FromBase64String(value)))
			{
				XmlSerializer xmlFormatter = new XmlSerializer(typeof(object), "http://www.nauck-it.de/PostgreSQLProvider");
				return xmlFormatter.Deserialize(mStream);
			}
		}

		internal byte[] SerializeToBinary(object value)
		{
			using (MemoryStream mStream = new MemoryStream())
			{
				BinaryFormatter binFormatter = new BinaryFormatter();
				binFormatter.Serialize(mStream, value);

				return mStream.ToArray();
			}
		}

		internal object DeserializeFromBinary(byte[] value)
		{
			using (MemoryStream mStream = new MemoryStream(value))
			{
				BinaryFormatter binFormatter = new BinaryFormatter();
				return binFormatter.Deserialize(mStream);
			}
		}
	}
}
#endif
