//
// Exceptions
//
// Authors:
//      Andi McClure (andi.mcclure@xamarin.com)
//
// Copyright 2016 Xamarin Inc. (http://www.xamarin.com)
//
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class Exceptions
	{
		[Serializable]
		public class SerializableException : Exception
		{
			public string Data;

			public SerializableException (string data) {
				Data = data;

				SerializeObjectState += HandleSerialization;
			}

			private static void HandleSerialization (object exception, SafeSerializationEventArgs eventArgs) {
				eventArgs.AddSerializedState (new SerializableExceptionState (exception));
			}

			[Serializable]
			private class SerializableExceptionState : ISafeSerializationData {
				private string Data;

				public SerializableExceptionState (object _exception) {
					SerializableException exception = (SerializableException)_exception;

					Data = exception.Data;
				}

				public void CompleteDeserialization (object _exception) {
					SerializableException exception = (SerializableException)_exception;
					exception.SerializeObjectState += HandleSerialization;

					exception.Data = Data;
				}
			}
		}

		// Effectively tests SerializeObjectState handler support on System.Exception
		[Test]
		public void Exception_SerializeObjectState () {
			SerializableException exception = new SerializableException ("success");
			SerializableException deserializedException;
			BinaryFormatter binaryFormatter = new BinaryFormatter ();

			using (MemoryStream memoryStream = new MemoryStream ())
			{
				binaryFormatter.Serialize (memoryStream, exception);
				memoryStream.Flush ();

				memoryStream.Seek (0, SeekOrigin.Begin);

				deserializedException = (SerializableException)binaryFormatter.Deserialize (memoryStream);
			}

			Assert.AreEqual ("success", deserializedException.Data);
		}
	}
}