// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com

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
using System.Xml;
using System.Xml.Serialization;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeList.
	/// </summary>
	public class XmlSchemaSimpleTypeList : XmlSchemaSimpleTypeContent
	{
		private XmlSchemaSimpleType itemType;
		private XmlQualifiedName itemTypeName;
		const string xmlname = "list";
		private object validatedListItemType;
#if NET_2_0
		private XmlSchemaSimpleType validatedListItemSchemaType;
#endif

		public XmlSchemaSimpleTypeList()
		{
			this.ItemTypeName = XmlQualifiedName.Empty;
		}

		[System.Xml.Serialization.XmlAttribute("itemType")]
		public XmlQualifiedName ItemTypeName
		{
			get{ return itemTypeName; } 
			set
			{
				itemTypeName = value;
			}
		}

		[XmlElement("simpleType",Namespace=XmlSchema.Namespace)]
		public XmlSchemaSimpleType ItemType 
		{
			get{ return itemType; } 
			set
			{
				itemType = value;
			}
		}

#if NET_2_0
		// LAMESPEC: this name is really ambiguous. Actually it just
		// holds compiled itemType, not baseType of the itemType.
		[XmlIgnore]
		public XmlSchemaSimpleType BaseItemType {
			get { return validatedListItemSchemaType; }
		}
#endif

		internal object ValidatedListItemType
		{
			get { return validatedListItemType; }
		}

		/// <remarks>
		/// 1. One of itemType or a <simpleType> must be present, but not both.
		/// 2. id must be of type ID
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

#if NET_2_0
			if (ItemType != null)
				ItemType.Parent = this;
#endif

			errorCount = 0;

			if(ItemType != null && !ItemTypeName.IsEmpty)
				error(h, "both itemType and simpletype can't be present");
			if(ItemType == null && ItemTypeName.IsEmpty)
				error(h, "one of itemType or simpletype must be present");
			if(ItemType != null)
			{
				errorCount += ItemType.Compile(h,schema);
			}
			if(!XmlSchemaUtil.CheckQName(ItemTypeName))
				error(h,"BaseTypeName must be a XmlQualifiedName");
			
			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.ValidationId))
				return errorCount;

			// ListItemType
			XmlSchemaSimpleType type = itemType;
			if (type == null)
				type = schema.SchemaTypes [itemTypeName] as XmlSchemaSimpleType;
			if (type != null) {
				errorCount += type.Validate (h, schema);
				validatedListItemType = type;
			} else if (itemTypeName == XmlSchemaComplexType.AnyTypeName) {
				validatedListItemType = XmlSchemaSimpleType.AnySimpleType;
			} else if (XmlSchemaUtil.IsBuiltInDatatypeName (itemTypeName)) {
				validatedListItemType = XmlSchemaDatatype.FromName (itemTypeName);
				if (validatedListItemType == null)
					error (h, "Invalid schema type name was specified: " + itemTypeName);
			}
			// otherwise, it might be missing sub components.
			else if (!schema.IsNamespaceAbsent (itemTypeName.Namespace))
				error (h, "Referenced base list item schema type " + itemTypeName + " was not found.");

#if NET_2_0
			XmlSchemaSimpleType st = validatedListItemType as XmlSchemaSimpleType;
			if (st == null && validatedListItemType != null)
				st = XmlSchemaType.GetBuiltInSimpleType (((XmlSchemaDatatype) validatedListItemType).TypeCode);
			validatedListItemSchemaType = st;
#endif

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		//<list 
		//  id = ID 
		//  itemType = QName 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (simpleType?))
		//</list>
		internal static XmlSchemaSimpleTypeList Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaSimpleTypeList list = new XmlSchemaSimpleTypeList();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaSimpleTypeList.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			list.LineNumber = reader.LineNumber;
			list.LinePosition = reader.LinePosition;
			list.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					list.Id = reader.Value;
				}
				else if(reader.Name == "itemType")
				{
					Exception innerex;
					list.ItemTypeName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for itemType attribute",innerex);
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for list",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,list);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return list;
			//  Content: annotation?, simpleType?
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaSimpleTypeList.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						list.Annotation = annotation;
					continue;
				}
				if(level <= 2 && reader.LocalName == "simpleType")
				{
					level = 3;
					XmlSchemaSimpleType stype = XmlSchemaSimpleType.Read(reader,h);
					if(stype != null)
						list.itemType = stype;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return list;
		}
	}
}
