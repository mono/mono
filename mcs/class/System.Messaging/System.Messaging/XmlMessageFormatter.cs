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
using System.IO;
using System.Xml.Serialization;


namespace System.Messaging 
{
	public class XmlMessageFormatter: IMessageFormatter, ICloneable 
	{
		private ArrayList targetTypes = new ArrayList ();		
	
		public XmlMessageFormatter()
		{
		}
		
		public XmlMessageFormatter(string[] targetTypeNames) : this (GetTypesFromNames (targetTypeNames))
		{
		}
		
		public XmlMessageFormatter(Type[] targetTypes)
		{
			this.targetTypes = new ArrayList (targetTypes);
		}

		private static Type[] GetTypesFromNames(string[] targetTypeNames)
		{
			Type[] ts = new Type[targetTypeNames.Length];
			for (int i = 0; i < targetTypeNames.Length; i++)
				ts[i] = Type.GetType (targetTypeNames[i]);
			
			return ts;
		}

		[MessagingDescription ("XmlMsgTargetTypeNames")]
		public string[] TargetTypeNames 
		{
			get 
			{
				if (targetTypes == null)
					return null;
					
				ArrayList listOfNames = new ArrayList (targetTypes.Count);
				foreach (Type type in targetTypes)
					listOfNames.Add (type.FullName);
				return (string[]) listOfNames.ToArray (typeof (string));
			}
			set { TargetTypes = GetTypesFromNames (value); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("XmlMsgTargetTypes")]
		public Type[] TargetTypes
		{
			get { return (Type[]) targetTypes.ToArray (typeof (Type)); }
			set { targetTypes = new ArrayList (value); }
		}
		
		[MonoTODO]
		public bool CanRead(Message message)
		{
			// Need an implementation of XmlNodeReader.
			throw new NotImplementedException();
		}
		
		public object Clone()
		{
			return new XmlMessageFormatter((Type[])TargetTypes.Clone());
		}
		
		private void AddType (Type t)
		{
			targetTypes.Add (t);
		}
				
		public object Read(Message message)
		{
			message.BodyStream.Seek (0, SeekOrigin.Begin);
			foreach (Type t in targetTypes) {
				XmlSerializer serializer = new XmlSerializer (t);
				try {
					return serializer.Deserialize (message.BodyStream);
				} catch (InvalidOperationException e) {
					Console.WriteLine (e);
				}
			}
			string error = "Unable to deserialize message body.  Type is not one of: " 
				+ String.Join (",", TargetTypeNames);
			throw new InvalidOperationException (error);
		}
		
		public void Write(Message message, object obj)
		{
			if (message == null)
				throw new ArgumentNullException ();
				
			Stream stream = message.BodyStream;
			if (stream == null) {
				stream = new MemoryStream ();
				message.BodyStream = stream;
			}

			XmlSerializer serializer = new XmlSerializer (obj.GetType ());
			serializer.Serialize (stream, obj);

			message.BodyType = (int) FormatterTypes.Xml;
			AddType (obj.GetType ());
		}
	}
}
