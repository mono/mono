
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
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Messaging 
{
	public class BinaryMessageFormatter: IMessageFormatter, ICloneable 
	{
		public BinaryMessageFormatter()
		{
			TopObjectFormat = FormatterAssemblyStyle.Full;
			TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
		}
		
		public BinaryMessageFormatter(FormatterAssemblyStyle topObjectFormat, FormatterTypeStyle typeFormat)
		{
			TopObjectFormat = topObjectFormat;
			TypeFormat = typeFormat;
		}
		
		public FormatterAssemblyStyle TopObjectFormat;
		
		public FormatterTypeStyle TypeFormat;
		
		public bool CanRead(Message message) 
		{
			if (message == null)
				throw new ArgumentNullException();
			return message.BodyStream.CanRead;
		}
		
		public object Read(Message message)
		{
			if (message == null)
				throw new ArgumentNullException();
				
			BinaryFormatter bf = new BinaryFormatter();
			bf.AssemblyFormat = TopObjectFormat;
			bf.TypeFormat = TypeFormat;
			return bf.Deserialize(message.BodyStream);
		}
		
		public void Write(Message message, object obj)
		{
			if (message == null)
				throw new ArgumentNullException();
				
			BinaryFormatter bf = new BinaryFormatter();
			bf.AssemblyFormat = TopObjectFormat;
			bf.TypeFormat = TypeFormat;		
			bf.Serialize(message.BodyStream, obj);
		}	
		
		public object Clone()
		{
			return new BinaryMessageFormatter(TopObjectFormat, TypeFormat);
		}
		
		[MonoTODO]
		~BinaryMessageFormatter()
		{
		}
	}
}
