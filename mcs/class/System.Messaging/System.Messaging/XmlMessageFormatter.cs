//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
//	(C) 2003 Peter Van Isacker
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
using System.Collections;
using System.ComponentModel;

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

		[MessagingDescription ("XmlMsgTargetTypeNames")]
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

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("XmlMsgTargetTypes")]
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
