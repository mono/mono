//
// System.Xml.Schema.XmlSchemaChoice.cs
//
// Author:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto  ginga@kit.hi-ho.ne.jp
//
using System;
using System.Collections;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAll.
	/// </summary>
	public class XmlSchemaChoice : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		private static string xmlname = "choice";
		private decimal minEffectiveTotalRange = -1;

		public XmlSchemaChoice ()
		{
			items = new XmlSchemaObjectCollection();
		}

		[XmlElement("element",typeof(XmlSchemaElement),Namespace=XmlSchema.Namespace)]
		[XmlElement("group",typeof(XmlSchemaGroupRef),Namespace=XmlSchema.Namespace)]
		[XmlElement("choice",typeof(XmlSchemaChoice),Namespace=XmlSchema.Namespace)]
		[XmlElement("sequence",typeof(XmlSchemaSequence),Namespace=XmlSchema.Namespace)]
		[XmlElement("any",typeof(XmlSchemaAny),Namespace=XmlSchema.Namespace)]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}

		internal override XmlSchemaParticle ActualParticle {
			get {
				if (this.ValidatedMinOccurs == 1 &&
					this.ValidatedMaxOccurs == 1 &&
					CompiledItems.Count == 1)
					return ((XmlSchemaParticle) CompiledItems [0]).ActualParticle;
				else
					return this;
			}
		}

		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			XmlSchemaUtil.CompileID(Id, this, schema.IDCollection, h);
			CompileOccurence (h, schema);

			foreach(XmlSchemaObject obj in Items)
			{
				if(obj is XmlSchemaElement ||
					obj is XmlSchemaGroupRef ||
					obj is XmlSchemaChoice ||
					obj is XmlSchemaSequence ||
					obj is XmlSchemaAny)
				{
					errorCount += obj.Compile(h,schema);
				}
				else
					error(h, "Invalid schema object was specified in the particles of the choice model group.");
			}
			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.CompilationId))
				return errorCount;

			CompiledItems.Clear ();
			foreach (XmlSchemaParticle p in Items) {
				errorCount += p.Validate (h, schema);
				CompiledItems.Add (p);
			}

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		internal override void ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
			ValidationEventHandler h, XmlSchema schema)
		{
			XmlSchemaAny any = baseParticle as XmlSchemaAny;
			if (any != null) {
				// NSRecurseCheckCardinality
				this.ValidateNSRecurseCheckCardinality (any, h, schema);
				return;
			}

			XmlSchemaChoice choice = baseParticle as XmlSchemaChoice;
			if (choice != null) {
				// RecurseLax
				this.ValidateOccurenceRangeOK (choice, h, schema);

				// If it is totally optional, then ignore their contents.
				if (choice.ValidatedMinOccurs == 0 && choice.ValidatedMaxOccurs == 0 &&
					this.ValidatedMinOccurs == 0 && this.ValidatedMaxOccurs == 0)
					return;
				this.ValidateRecurse (choice, h, schema);
				return;
			}

			error (h, "Invalid choice derivation by restriction was found.");
		}

		internal override decimal GetMinEffectiveTotalRange ()
		{
			if (minEffectiveTotalRange >= 0)
				return minEffectiveTotalRange;

			decimal product = 0; //this.ValidatedMinOccurs;
			if (Items.Count == 0)
				product = 0;
			else {
				foreach (XmlSchemaParticle p in this.Items) {
					decimal got = p.GetMinEffectiveTotalRange ();
					if (product > got)
						product= got;
				}
			}
			minEffectiveTotalRange = product;
			return product;
		}

		internal override void ValidateUniqueParticleAttribution (XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema)
		{
			foreach (XmlSchemaParticle p in this.Items)
				p.ValidateUniqueParticleAttribution (qnames, nsNames, h, schema);
		}

		internal override void ValidateUniqueTypeAttribution (XmlSchemaObjectTable labels,
			ValidationEventHandler h, XmlSchema schema)
		{
			foreach (XmlSchemaParticle p in this.Items)
				p.ValidateUniqueTypeAttribution (labels, h, schema);
		}

		//<choice
		//  id = ID
		//  maxOccurs =  (nonNegativeInteger | unbounded)  : 1
		//  minOccurs = nonNegativeInteger : 1
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (element | group | choice | sequence | any)*)
		//</choice>
		internal static XmlSchemaChoice Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaChoice choice = new XmlSchemaChoice();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaChoice.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			choice.LineNumber = reader.LineNumber;
			choice.LinePosition = reader.LinePosition;
			choice.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					choice.Id = reader.Value;
				}
				else if(reader.Name == "maxOccurs")
				{
					try
					{
						choice.MaxOccursString = reader.Value;
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
						choice.MinOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for minOccurs",e);
					}
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for choice",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,choice);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return choice;

			//  Content: (annotation?, (element | group | choice | sequence | any)*)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaChoice.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						choice.Annotation = annotation;
					continue;
				}
				if(level <=2)
				{
					if(reader.LocalName == "element")
					{
						level = 2;
						XmlSchemaElement element = XmlSchemaElement.Read(reader,h);
						if(element != null)
							choice.items.Add(element);
						continue;
					}
					if(reader.LocalName == "group")
					{
						level = 2;
						XmlSchemaGroupRef group = XmlSchemaGroupRef.Read(reader,h);
						if(group != null)
							choice.items.Add(group);
						continue;
					}
					if(reader.LocalName == "choice")
					{
						level = 2;
						XmlSchemaChoice ch = XmlSchemaChoice.Read(reader,h);
						if(ch != null)
							choice.items.Add(ch);
						continue;
					}
					if(reader.LocalName == "sequence")
					{
						level = 2;
						XmlSchemaSequence sequence = XmlSchemaSequence.Read(reader,h);
						if(sequence != null)
							choice.items.Add(sequence);
						continue;
					}
					if(reader.LocalName == "any")
					{
						level = 2;
						XmlSchemaAny any = XmlSchemaAny.Read(reader,h);
						if(any != null)
							choice.items.Add(any);
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
			return choice;
		}
	}
}
