//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
//	(C) 2003 Peter Van Isacker
//
using System;
using System.Collections;

namespace System.Messaging 
{
	public class XmlMessageFormatter: IMessageFormatter, ICloneable 
	{
		[MonoTODO]
		public XmlMessageFormatter()
		{
		}
		
		[MonoTODO]
		public XmlMessageFormatter(string[] targetTypeNames)
		{
			initializeFromNames(targetTypeNames);
		}
		
		[MonoTODO]
		public XmlMessageFormatter(Type[] targetTypes)
		{
			this.targetTypes = targetTypes;
		}

		private Type[] targetTypes = null;
		
		[MonoTODO]
		private void initializeFromNames(string[] targetTypeNames)
		{
		}
		
		public string[] TargetTypeNames 
		{
			get 
			{
				if (targetTypes == null)
					return null;
					
				ArrayList listOfNames = new ArrayList();
				foreach(Type type in targetTypes)
					listOfNames.Add(type.FullName);
				return (string[])listOfNames.ToArray(typeof(string));
			}
			set { initializeFromNames(value); }
		}
		
		public Type[] TargetTypes 
		{
			get {return this.targetTypes;}
			set {targetTypes = value;}
		}
		
		[MonoTODO]
		public bool CanRead(Message message)
		{
			throw new NotImplementedException();
		}
		
		public object Clone()
		{
			return new XmlMessageFormatter((Type[])targetTypes.Clone());
		}
		
		[MonoTODO]
		public object Read(Message message)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void Write(Message message, object obj)
		{
			throw new NotImplementedException();
		}
	}
}
