//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
//	(C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.ComponentModel;
using System.Messaging.Design;

namespace System.Messaging 
{
	[TypeConverter (typeof(MessageFormatterConverter))]
	public interface IMessageFormatter: ICloneable 
	{
		bool CanRead(Message message);
		
		object Read(Message message);
		
		void Write(Message message, object obj);
	}
}
