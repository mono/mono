//
// System.Xml.Serialization.SerializationCodeGeneratorConfiguration.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
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
		
		public ArrayList GetHooks (HookType hookType, HookDir dir, HookAction action, Type type, string member)
		{
			if (dir == HookDir.Read)
				return FindHook (ReaderHooks, hookType, action, type, member);
			else
				return FindHook (WriterHooks, hookType, action, type, member);
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
	
	internal enum HookDir
	{
		Read,
		Write
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
