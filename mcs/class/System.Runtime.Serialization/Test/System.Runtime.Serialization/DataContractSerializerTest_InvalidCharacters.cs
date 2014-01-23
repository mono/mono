//
// DataContractSerializerTest_InvalidCharacters.cs
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
using System.IO;
using System.Text;
using System.Runtime.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class DataContractSerializerTest_InvalidCharacters
	{
		[Serializable]
		public class MyData
		{
			public string Text;
		}

		[Test]
		public void Test ()
		{
			var data = new MyData
			{
				Text = "Test " + ASCIIEncoding.ASCII.GetString (new byte[] { 0x06 })
			};

			var serializer = new DataContractSerializer (typeof(MyData), "MyData", string.Empty);

			string serialized;
			using (var ms = new MemoryStream ()) {
				serializer.WriteObject (ms, data);
				serialized = new string (Encoding.UTF8.GetChars (ms.GetBuffer ()));

				Assert.IsTrue (serialized.Contains ("Test &#x6;"), "#1");

				ms.Seek (0, SeekOrigin.Begin);

				var data2 = (MyData)serializer.ReadObject (ms);
				Assert.AreEqual (data2.Text.Length, 6, "#2");
				Assert.AreEqual (data2.Text [5], (char)0x06, "#3");
			}
		}

		
	}
}
