
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
