//
// StreamContentTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Linq;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class StreamContentTest
	{
		class StreamContentMock : StreamContent
		{
			public Func<long> OnTryComputeLength;
			public Action OnSerializeToStreamAsync;

			public StreamContentMock (Stream stream)
				: base (stream)
			{
			}

			protected override bool TryComputeLength (out long length)
			{
				if (OnTryComputeLength != null) {
					length = OnTryComputeLength ();
					return true;
				}

				return base.TryComputeLength (out length);
			}

			protected override Task SerializeToStreamAsync (Stream stream, TransportContext context)
			{
				if (OnSerializeToStreamAsync != null)
					OnSerializeToStreamAsync ();

				return base.SerializeToStreamAsync (stream, context);
			}
		}

		class ExceptionStream : MemoryStream
		{
			public ExceptionStream ()
			{
				base.WriteByte (10);
				base.Seek (0, SeekOrigin.Begin);
			}

			public override int Read (byte[] buffer, int offset, int count)
			{
				throw new ApplicationException ("Read");
			}

			public override byte[] GetBuffer ()
			{
				throw new ApplicationException ("GetBuffer");
			}
		}

		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new StreamContent (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				new StreamContent (new MemoryStream (), 0);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Ctor ()
		{
			var ms = new MemoryStream ();
			ms.WriteByte (44);

			using (var m = new StreamContent (ms)) {
			}
		}

		[Test]
		public void CopyToAsync_Invalid ()
		{
			var m = new MemoryStream ();

			var sc = new StreamContent (new MemoryStream ());
			try {
				sc.CopyToAsync (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			//
			// For some reason does not work on .net
			//
			/*
			sc = new StreamContent (new ExceptionStream ());
			try {
			    sc.CopyToAsync (m).Wait ();
			    Assert.Fail ("#2");
			} catch (AggregateException) {
			}
			*/ 
		}

		[Test]
		/*
		 * The .NET runtime hits the "#9" assertion.
		 * The test succeeds with Mono.
		 */
		[Category ("NotWorking")]
		public void CopyToAsync ()
		{
			var ms = new MemoryStream ();
			ms.WriteByte (4);
			ms.WriteByte (2);
			ms.Seek (0, SeekOrigin.Begin);

			var sc = new StreamContent (ms);

			var dest = new MemoryStream ();
			var task = sc.CopyToAsync (dest);
			task.Wait ();
			Assert.AreEqual (2, dest.Length, "#1");

			bool hit = false;
			dest = new MemoryStream ();
			var scm = new StreamContentMock (new ExceptionStream ());
			scm.OnSerializeToStreamAsync = () => { hit = true; };
			task = scm.CopyToAsync (dest);
			try {
				task.Wait ();
				Assert.Fail ("#9");
			} catch (AggregateException) {
			}

			Assert.IsTrue (hit, "#10");
		}

		[Test]
		public void CopyToAsync_ClosedInput ()
		{
			var stream = new MemoryStream (new byte[] { 1 });
			var content = new StreamContent (stream);
			Assert.IsTrue (content.LoadIntoBufferAsync ().Wait (3000), "#1");
			stream.Close ();

			var stream_out = new MemoryStream (10);
			Assert.IsTrue (content.CopyToAsync (stream_out).Wait (3000), "#2");
		}

		[Test]
		public void Headers ()
		{
			var ms = new MemoryStream ();
			ms.WriteByte (4);
			ms.WriteByte (2);
			ms.Seek (0, SeekOrigin.Begin);

			var sc = new StreamContent (ms);
			var headers = sc.Headers;
			Assert.AreEqual (2, headers.ContentLength, "#1");

			headers.ContentLength = 400;
			Assert.AreEqual (400, headers.ContentLength, "#1a");
			headers.ContentLength = null;

			var scm = new StreamContentMock (MemoryStream.Null);
			scm.OnTryComputeLength = () => 330;
			Assert.AreEqual (330, scm.Headers.ContentLength, "#2");

			headers.Allow.Add ("a1");
			headers.ContentEncoding.Add ("ce1");
			headers.ContentLanguage.Add ("cl1");
			headers.ContentLength = 23;
			headers.ContentLocation = new Uri ("http://xamarin.com");
			headers.ContentMD5 = new byte[] { 3, 5 };
			headers.ContentRange = new ContentRangeHeaderValue (88, 444);
			headers.ContentType = new MediaTypeHeaderValue ("multipart/*");
			headers.Expires = new DateTimeOffset (DateTime.Today);
			headers.LastModified = new DateTimeOffset (DateTime.Today);


			headers.Add ("allow", "a2");
			headers.Add ("content-encoding", "ce3");
			headers.Add ("content-language", "cl2");

			try {
				headers.Add ("content-length", "444");
				Assert.Fail ("content-length");
			} catch (FormatException) {
			}

			try {
				headers.Add ("content-location", "cl2");
				Assert.Fail ("content-location");
			} catch (FormatException) {
			}

			try {
				headers.Add ("content-MD5", "cmd5");
				Assert.Fail ("content-MD5");
			} catch (FormatException) {
			}

			try {
				headers.Add ("content-range", "133");
				Assert.Fail ("content-range");
			} catch (FormatException) {
			}

			try {
				headers.Add ("content-type", "ctype");
				Assert.Fail ("content-type");
			} catch (FormatException) {
			}

			try {
				headers.Add ("expires", "ctype");
				Assert.Fail ("expires");
			} catch (FormatException) {
			}

			try {
				headers.Add ("last-modified", "lmo");
				Assert.Fail ("last-modified");
			} catch (FormatException) {
			}

			Assert.IsTrue (headers.Allow.SequenceEqual (
				new[] {
					"a1",
					"a2"
				}
			));

			Assert.IsTrue (headers.ContentEncoding.SequenceEqual (
				new[] {
					"ce1",
					"ce3"
				}
			));

			Assert.IsTrue (headers.ContentLanguage.SequenceEqual (
				new[] {
					"cl1",
					"cl2"
				}
			));

			Assert.AreEqual (23, headers.ContentLength);
			Assert.AreEqual (new Uri ("http://xamarin.com"), headers.ContentLocation);
			Assert.AreEqual (new byte[] { 3, 5 }, headers.ContentMD5);
			Assert.AreEqual (new ContentRangeHeaderValue (88, 444), headers.ContentRange);
			Assert.AreEqual (new MediaTypeHeaderValue ("multipart/*"), headers.ContentType);
			Assert.AreEqual (new DateTimeOffset (DateTime.Today), headers.Expires);
			Assert.AreEqual (new DateTimeOffset (DateTime.Today), headers.LastModified);
		}

		[Test]
		public void Headers_Invalid ()
		{
			var sc = new StreamContent (MemoryStream.Null);
			var h = sc.Headers;

			try {
				h.Add ("Age", "");
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void LoadIntoBuffer ()
		{
			var ms = new MemoryStream ();
			ms.WriteByte (4);
			ms.Seek (0, SeekOrigin.Begin);

			var sc = new StreamContent (ms);
			Assert.IsTrue (sc.LoadIntoBufferAsync (400).Wait (200));
		}

		[Test]
		public void LoadIntoBuffer_BufferOverflow ()
		{
			var ms = new MemoryStream ();
			ms.Write (new byte[10000], 0, 10000);
			ms.Seek (0, SeekOrigin.Begin);

			var sc = new StreamContent (ms);
			try {
				Assert.IsTrue (sc.LoadIntoBufferAsync (50).Wait (200));
				Assert.Fail ("#1");
			} catch (AggregateException e) {
				Assert.IsTrue (e.InnerException is HttpRequestException, "#2");
			}
		}

		[Test]
		public void ReadAsByteArrayAsync ()
		{
			var ms = new MemoryStream ();
			ms.WriteByte (4);
			ms.WriteByte (55);

			var sc = new StreamContent (ms);
			var res = sc.ReadAsByteArrayAsync ().Result;
			Assert.AreEqual (0, res.Length, "#1");

			ms.Seek (0, SeekOrigin.Begin);
			sc = new StreamContent (ms);
			res = sc.ReadAsByteArrayAsync ().Result;
			Assert.AreEqual (2, res.Length, "#10");
			Assert.AreEqual (55, res[1], "#11");
		}

		[Test]
		public void ReadAsString ()
		{
			var ms = new MemoryStream ();
			ms.WriteByte (77);
			ms.WriteByte (55);
			ms.Seek (0, SeekOrigin.Begin);

			var sc = new StreamContent (ms);
			var res = sc.ReadAsStringAsync ().Result;
			Assert.AreEqual ("M7", res, "#1");
		}

		[Test]
		public void ReadAsStream ()
		{
			var ms = new MemoryStream ();
			ms.WriteByte (77);
			ms.WriteByte (55);
			ms.Seek (0, SeekOrigin.Begin);

			var sc = new StreamContent (ms);
			var res = sc.ReadAsStreamAsync ().Result;
			Assert.AreEqual (77, res.ReadByte (), "#1");
		}

		[Test]
		public void ReadAsStreamAsync_ClosedInput ()
		{
			var stream = new MemoryStream (new byte[] { 1 });
			var content = new StreamContent (stream);
			Assert.IsTrue (content.LoadIntoBufferAsync ().Wait (3000), "#1");
			stream.Close ();

			var stream_read = content.ReadAsStreamAsync ().Result;
			Assert.IsTrue (stream_read.CanSeek, "#2");
			Assert.AreEqual (0, stream_read.Position, "#3");	
			Assert.AreEqual (1, stream_read.Length, "#4");
		}
	}
}
