// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAny.
	/// </summary>
	public class XmlSchemaAny : XmlSchemaParticle
	{
		private string nameSpace;
		private XmlSchemaContentProcessing processing;
		private static string xmlname = "any";

		public XmlSchemaAny()
		{
		}

		[System.Xml.Serialization.XmlAttribute("namespace")]
		public string Namespace 
		{
			get{ return  nameSpace; } 
			set{ nameSpace = value; }
		}
		
		[DefaultValue(XmlSchemaContentProcessing.None)]
		[System.Xml.Serialization.XmlAttribute("processContents")]
		public XmlSchemaContentProcessing ProcessContents
		{ 
			get{ return processing; } 
			set{ processing = value; }
		}

		/// <remarks>
		/// 1. id must be of type ID
		/// 2. namespace can have one of the following values:
		///		a) ##any or ##other
		///		b) list of anyURI and ##targetNamespace and ##local
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			errorCount = 0;

			XmlSchemaUtil.CompileID(Id,this, schema.IDCollection,h);

			//define ##any=1,##other=2,##targetNamespace=4,##local=8,anyURI=16
			int nscount = 0;
			string[] nslist = XmlSchemaUtil.SplitList(Namespace);
			foreach(string ns in nslist)
			{
				switch(ns)
				{
					case "##any": 
						nscount |= 1;
						break;
					case "##other":
						nscount |= 2;
						break;
					case "##targetNamespace":
						nscount |= 4;
						break;
					case "##local":
						nscount |= 8;
						break;
					default:
						if(!XmlSchemaUtil.CheckAnyUri(ns))
							error(h,"the namespace is not a valid anyURI");
						else
							nscount |= 16;
						break;
				}
			}
			if((nscount&1) == 1 && nscount != 1)
				error(h,"##any if present must be the only namespace attribute");
			if((nscount&2) == 2 && nscount != 2)
				error(h,"##other if present must be the only namespace attribute");
			
			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}
		//<any
		//  id = ID
		//  maxOccurs =  (nonNegativeInteger | unbounded)  : 1
		//  minOccurs = nonNegativeInteger : 1
		//  namespace = ((##any | ##other) | List of (anyURI | (##targetNamespace | ##local)) )  : ##any
		//  processContents = (lax | skip | strict) : strict
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</any>
		internal static XmlSchemaAny Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaAny any = new XmlSchemaAny();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaAny.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			any.LineNumber = reader.LineNumber;
			any.LinePosition = reader.LinePosition;
			any.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					any.Id = reader.Value;
				}
				else if(reader.Name == "maxOccurs")
				{
					try
					{
						any.MaxOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for maxOccurs",e);
					}
				}
				else if(reader.Name == "minOccurs")
				{
					try
					{
						any.MinOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for minOccurs", e);
					}
				}
				else if(reader.Name == "namespace")
				{
					any.nameSpace = reader.Value;
				}
				else if(reader.Name == "processContents")
				{
					Exception innerex;
					any.processing = XmlSchemaUtil.ReadProcessingAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for processContents",innerex);
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for any",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,any);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return any;
			
			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaAny.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						any.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return any;
		}
	}
}
