
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

//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker
//

using System;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Messaging
{
	public class BinaryMessageFormatter : IMessageFormatter, ICloneable
	{
		private BinaryFormatter _formatter;

		public BinaryMessageFormatter ()
		{
			_formatter = new BinaryFormatter ();
		}

		public BinaryMessageFormatter (FormatterAssemblyStyle topObjectFormat, FormatterTypeStyle typeFormat)
		{
			_formatter = new BinaryFormatter ();
			_formatter.AssemblyFormat = topObjectFormat;
			_formatter.TypeFormat = typeFormat;
		}

		[DefaultValue (0)]
		[MessagingDescription ("MsgTopObjectFormat")]
		public FormatterAssemblyStyle TopObjectFormat {
			get {
				return _formatter.AssemblyFormat;
			}
			set {
				_formatter.AssemblyFormat = value;
			}
		}

		[DefaultValue (0)]
		[MessagingDescription ("MsgTypeFormat")]
		public FormatterTypeStyle TypeFormat {
			get {
				return _formatter.TypeFormat;
			}
			set {
				_formatter.TypeFormat = value;
			}
		}

		[MonoTODO ("only return true if body type is binary")]
		public bool CanRead (Message message)
		{
			if (message == null)
				throw new ArgumentNullException ();
			return message.BodyStream.CanRead;
		}

		[MonoTODO ("throw InvalidOperationException if message body is not binary")]
		public object Read (Message message)
		{
			if (message == null)
				throw new ArgumentNullException ();

			return _formatter.Deserialize (message.BodyStream);
		}

		[MonoTODO ("throw InvalidOperationException if message body is not binary")]
		public void Write (Message message, object obj)
		{
			if (message == null)
				throw new ArgumentNullException ();

			_formatter.Serialize (message.BodyStream, obj);
		}

		public object Clone ()
		{
			return new BinaryMessageFormatter (TopObjectFormat, TypeFormat);
		}
	}
}
