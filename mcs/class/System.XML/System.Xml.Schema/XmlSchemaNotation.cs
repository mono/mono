// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaNotation.
	/// </summary>
	public class XmlSchemaNotation : XmlSchemaAnnotated
	{
		private string name;
		private string pub;
		private string system;
		private XmlQualifiedName qualifiedName;
		private static string xmlname = "notation";

		public XmlSchemaNotation()
		{
		}
		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return  name; } 
			set{ name = value; }
		}
		[System.Xml.Serialization.XmlAttribute("public")]
		public string Public 
		{
			get{ return  pub; } 
			set{ pub = value; }
		}
		[System.Xml.Serialization.XmlAttribute("system")]
		public string System 
		{
			get{ return  system; } 
			set{ system = value; }
		}

		[XmlIgnore]
		internal XmlQualifiedName QualifiedName 
		{
			get{ return qualifiedName;}
		}

		// 1. name and public must be present
		// public and system must be anyURI
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			if(Name == null)
				error(h,"Required attribute name must be present");
			else if(!XmlSchemaUtil.CheckNCName(this.name)) 
				error(h,"attribute name must be NCName");
			else
				qualifiedName = new XmlQualifiedName(Name,schema.TargetNamespace);

			if(Public==null)
				error(h,"public must be present");
			else if(!XmlSchemaUtil.CheckAnyUri(Public))
				error(h,"public must be anyURI");

			if(system != null && !XmlSchemaUtil.CheckAnyUri(system))
				error(h,"system must be present and of Type anyURI");
			
			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		//<notation 
		//  id = ID 
		//  name = NCName 
		//  public = anyURI 
		//  system = anyURI 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</notation>
		internal static XmlSchemaNotation Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaNotation notation = new XmlSchemaNotation();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaInclude.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			notation.LineNumber = reader.LineNumber;
			notation.LinePosition = reader.LinePosition;
			notation.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					notation.Id = reader.Value;
				}
				else if(reader.Name == "name")
				{
					notation.name = reader.Value;
				}
				else if(reader.Name == "public")
				{
					notation.pub = reader.Value;
				}
				else if(reader.Name == "system")
				{
					notation.system = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for notation",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,notation);
				}
			}

			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return notation;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaNotation.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						notation.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return notation;
		}
	}
}
