//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
//	(C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Messaging 
{
	public interface IMessageFormatter: ICloneable 
	{
		bool CanRead(Message message);
		
		object Read(Message message);
		
		void Write(Message message, object obj);
	}
}
