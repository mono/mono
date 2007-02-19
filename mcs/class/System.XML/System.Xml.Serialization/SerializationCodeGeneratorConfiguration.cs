//
// System.Xml.Serialization.SerializationCodeGeneratorConfiguration.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
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
using System.Xml.Serialization;

namespace System.Xml.Serialization
{
	[XmlType ("configuration")]
	internal class SerializationCodeGeneratorConfiguration
	{
		[XmlElement ("serializer")]
		public SerializerInfo[] Serializers;
	}
	
	[XmlType ("serializer")]
	internal class SerializerInfo
	{
		[XmlAttribute ("class")]
		public string ClassName;
	
		[XmlAttribute ("assembly")]
		public string Assembly;
	
		[XmlElement ("reader")]
		public string ReaderClassName;
		
		[XmlElement ("writer")]
		public string WriterClassName;
	
		[XmlElement ("baseSerializer")]
		public string BaseSerializerClassName;
	
		[XmlElement ("implementation")]
		public string ImplementationClassName;
	
		[XmlElement ("noreader")]
		public bool NoReader;
		
		[XmlElement ("nowriter")]
		public bool NoWriter;
		
		[XmlElement ("generateAsInternal")]
		public bool GenerateAsInternal;
		
		[XmlElement ("namespace")]
		public string Namespace;
		
		[XmlArray ("namespaceImports")]
		[XmlArrayItem ("namespaceImport")]
		public string [] NamespaceImports;
		
		[System.ComponentModel.DefaultValue (SerializationFormat.Literal)]
		public SerializationFormat SerializationFormat = SerializationFormat.Literal;
		
		[XmlElement ("outFileName")]
		public string OutFileName;
		
		[XmlArray ("readerHooks")]
		public Hook[] ReaderHooks;
	
		[XmlArray ("writerHooks")]
		public Hook[] WriterHooks;
		
		public ArrayList GetHooks (HookType hookType, XmlMappingAccess dir, HookAction action, Type type, string member)
		{
			if ((dir & XmlMappingAccess.Read) != 0)
				return FindHook (ReaderHooks, hookType, action, type, member);
			if ((dir & XmlMappingAccess.Write) != 0)
				return FindHook (WriterHooks, hookType, action, type, member);
			else
				throw new Exception ("INTERNAL ERROR");
		}
		
		ArrayList FindHook (Hook[] hooks, HookType hookType, HookAction action, Type type, string member)
		{
			ArrayList foundHooks = new ArrayList ();
			if (hooks == null) return foundHooks;
	
			foreach (Hook hook in hooks)
			{
				if (action == HookAction.InsertBefore && (hook.InsertBefore == null || hook.InsertBefore == ""))
					continue;
				else if (action == HookAction.InsertAfter && (hook.InsertAfter == null || hook.InsertAfter == ""))
					continue;
				else if (action == HookAction.Replace && (hook.Replace == null || hook.Replace == ""))
					continue;
					
				if (hook.HookType != hookType)
					continue;
					
				if (hook.Select != null)
				{
					if (hook.Select.TypeName != null && hook.Select.TypeName != "")
						if (hook.Select.TypeName != type.FullName) continue;
		
					if (hook.Select.TypeMember != null && hook.Select.TypeMember != "")
						if (hook.Select.TypeMember != member) continue;
						
					if (hook.Select.TypeAttributes != null && hook.Select.TypeAttributes.Length > 0)
					{
						object[] ats = type.GetCustomAttributes (true);
						bool found = false;
						foreach (object at in ats)
							if (Array.IndexOf (hook.Select.TypeAttributes, at.GetType().FullName) != -1) { found = true; break; }
						if (!found) continue;
					}
				}
				foundHooks.Add (hook);
			}
			return foundHooks;
		}
	}
	
	[XmlType ("hook")]
	internal class Hook
	{
		[XmlAttribute ("type")]
		public HookType HookType;
	
		[XmlElement ("select")]
		public Select Select;
		
		[XmlElement ("insertBefore")]
		public string InsertBefore;

		[XmlElement ("insertAfter")]
		public string InsertAfter;

		[XmlElement ("replace")]
		public string Replace;
		
		public string GetCode (HookAction action)
		{
			if (action == HookAction.InsertBefore)
				return InsertBefore;
			else if (action == HookAction.InsertAfter)
				return InsertAfter;
			else
				return Replace;
		}
	}
	
	[XmlType ("select")]
	internal class Select
	{
		[XmlElement ("typeName")]
		public string TypeName;
		
		[XmlElement("typeAttribute")]
		public string[] TypeAttributes;
		
		[XmlElement ("typeMember")]
		public string TypeMember;
	}
	
	internal enum HookAction
	{
		InsertBefore,
		InsertAfter,
		Replace
	}
	
	[XmlType ("hookType")]
	internal enum HookType
	{
		attributes,
		elements,
		unknownAttribute,
		unknownElement,
		member,
		type
	}
}
