// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeUnion.
	/// </summary>
	public class XmlSchemaSimpleTypeUnion : XmlSchemaSimpleTypeContent
	{
		private XmlSchemaObjectCollection baseTypes;
		private XmlQualifiedName[] memberTypes;
		private static string xmlname = "union";

		public XmlSchemaSimpleTypeUnion()
		{
			baseTypes = new XmlSchemaObjectCollection();
		}

		[XmlElement("simpleType",typeof(XmlSchemaSimpleType),Namespace=XmlSchema.Namespace)]
		public XmlSchemaObjectCollection BaseTypes 
		{
			get{ return baseTypes; }
		}

		[System.Xml.Serialization.XmlAttribute("memberTypes")]
		public XmlQualifiedName[] MemberTypes
		{
			get{ return  memberTypes; } 
			set{ memberTypes = value; }
		}
		/// <remarks>
		/// 1. Circular union type definition is disallowed. (WTH is this?)
		/// 2. id must be a valid ID
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			errorCount = 0;

			int count = BaseTypes.Count;

			foreach(XmlSchemaObject obj in baseTypes)
			{
				if(obj != null && obj is XmlSchemaSimpleType)
				{
					XmlSchemaSimpleType stype = (XmlSchemaSimpleType) obj;
					errorCount += stype.Compile(h, schema);
				}
				else
				{
					error(h, "baseTypes can't have objects other than a simpletype");
				}
			}
			
			if(memberTypes!=null)
			{
				for(int i=0; i< memberTypes.Length; i++)
				{
					if(memberTypes[i] == null || !XmlSchemaUtil.CheckQName(MemberTypes[i]))
					{
						warn(h,"Invalid membertype");
						memberTypes[i] = XmlQualifiedName.Empty;
					}
					else
					{
						count += MemberTypes.Length;
					}
				}
			}

			if(count == 0)
				error(h, "Atleast one simpletype or membertype must be present");

			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}
		//<union 
		//  id = ID 
		//  memberTypes = List of QName 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (simpleType*))
		//</union>
		internal static XmlSchemaSimpleTypeUnion Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaSimpleTypeUnion union = new XmlSchemaSimpleTypeUnion();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaSimpleTypeUnion.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			union.LineNumber = reader.LineNumber;
			union.LinePosition = reader.LinePosition;
			union.SourceUri = reader.BaseURI;

			//Read Attributes
			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					union.Id = reader.Value;
				}
				else if(reader.Name == "memberTypes")
				{
					Exception innerEx;
					string[] names = XmlSchemaUtil.SplitList(reader.Value);
					union.memberTypes = new XmlQualifiedName[names.Length];
					for(int i=0;i<names.Length;i++)
					{
						union.memberTypes[i] = XmlSchemaUtil.ToQName(reader,names[i],out innerEx);
						if(innerEx != null)
							error(h,"'"+names[i] + "' is not a valid memberType",innerEx);
					}
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for union",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,union);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return union;

			//  Content: annotation?, simpleType*
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaSimpleTypeUnion.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						union.Annotation = annotation;
					continue;
				}
				if(level <=2 && reader.LocalName == "simpleType")
				{
					level = 2;
					XmlSchemaSimpleType stype = XmlSchemaSimpleType.Read(reader,h);
					if(stype != null)
						union.baseTypes.Add(stype);
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return union;
		}

	}
}
