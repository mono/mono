//
// System.Xml.Schema.XmlSchemaSequence.cs
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
	/// Summary description for XmlSchemaSequence.
	/// </summary>
	public class XmlSchemaSequence : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		private static string xmlname = "sequence";

		public XmlSchemaSequence()
		{
			items = new XmlSchemaObjectCollection();
		}

		[XmlElement("element",typeof(XmlSchemaElement),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("group",typeof(XmlSchemaGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("choice",typeof(XmlSchemaChoice),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("sequence",typeof(XmlSchemaSequence),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("any",typeof(XmlSchemaAny),Namespace="http://www.w3.org/2001/XMLSchema")]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}

		internal override XmlSchemaParticle ActualParticle {
			get {
				if (CompiledItems.Count == 0)
					return XmlSchemaParticle.Empty;
				if (ValidatedMaxOccurs == 1 &&
					ValidatedMinOccurs == 1 &&
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
					error(h, "Invalid schema object was specified in the particles of the sequence model group.");
			}
			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.CompilationId))
				return errorCount;

			CompiledItems.Clear ();
			foreach (XmlSchemaObject obj in Items) {
				errorCount += obj.Validate (h, schema);
				CompiledItems.Add (obj);
			}

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		internal override void ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
			ValidationEventHandler h, XmlSchema schema)
		{
			if (this == baseParticle) // quick check
				return;

			XmlSchemaElement el = baseParticle as XmlSchemaElement;
			if (el != null) {
				// Forbidden
				error (h, "Invalid sequence paricle derivation.");
				return;
			}

			XmlSchemaSequence seq = baseParticle as XmlSchemaSequence;
			if (seq != null) {
				// Recurse
				ValidateOccurenceRangeOK (seq, h, schema);

				// If it is totally optional, then ignore their contents.
				if (seq.ValidatedMinOccurs == 0 && seq.ValidatedMaxOccurs == 0 &&
					this.ValidatedMinOccurs == 0 && this.ValidatedMaxOccurs == 0)
					return;
				this.ValidateRecurse (seq, h, schema);
				return;
			} 

			XmlSchemaAll all = baseParticle as XmlSchemaAll;
			XmlSchemaAny any = baseParticle as XmlSchemaAny;
			XmlSchemaChoice choice = baseParticle as XmlSchemaChoice;
			if (all != null) {
				// RecurseUnordered
				XmlSchemaObjectCollection already = new XmlSchemaObjectCollection ();
				for (int i = 0; i < this.Items.Count; i++) {
					XmlSchemaElement de = this.Items [i] as XmlSchemaElement;
					if (de == null) {
						error (h, "Invalid sequence particle derivation by restriction from all.");
						continue;
					}
					foreach (XmlSchemaElement e in all.Items) {
						if (e.QualifiedName == de.QualifiedName) {
							if (already.Contains (e))
								error (h, "Base element particle is mapped to the derived element particle in a sequence two or more times.");
							else {
								already.Add (e);
								de.ValidateDerivationByRestriction (e, h, schema);
							}
						}
					}
				}
				foreach (XmlSchemaElement e in all.Items)
					if (!already.Contains (e))
						if (!e.ValidateIsEmptiable ())
							error (h, "In base -all- particle, mapping-skipped base element which is not emptiable was found.");
			} else if (any != null) {
				// NSRecurseCheckCardinality
				ValidateNSRecurseCheckCardinality (any, h, schema);
				return;
			} else if (choice != null) {
				// MapAndSum
				// In fact it is not Recurse, but it looks common.
				ValidateRecurse (choice, h, schema);
			}
		}

		internal override decimal GetMinEffectiveTotalRange ()
		{
			return GetMinEffectiveTotalRangeAllAndSequence ();
		}

		internal override void ValidateUniqueParticleAttribution (XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema)
		{
			foreach (XmlSchemaParticle p in this.Items) {
				p.ValidateUniqueParticleAttribution (qnames, nsNames, h, schema);
				if (p.ValidatedMinOccurs == p.ValidatedMaxOccurs)
					break;
			}
			XmlSchemaObjectTable tmpTable = new XmlSchemaObjectTable ();
			ArrayList al = new ArrayList ();
			for (int i=0; i<Items.Count; i++) {
				XmlSchemaParticle p1 = Items [i] as XmlSchemaParticle;
				p1.ValidateUniqueParticleAttribution (tmpTable, al, h, schema);
				if (p1.ValidatedMinOccurs == p1.ValidatedMaxOccurs) {
					tmpTable.Clear ();
					al.Clear ();
				}
			}
		}

		internal override void ValidateUniqueTypeAttribution (XmlSchemaObjectTable labels,
			ValidationEventHandler h, XmlSchema schema)
		{
			foreach (XmlSchemaParticle p in this.Items)
				p.ValidateUniqueTypeAttribution (labels, h, schema);
		}

		//<sequence
		//  id = ID
		//  maxOccurs =  (nonNegativeInteger | unbounded)  : 1
		//  minOccurs = nonNegativeInteger : 1
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (element | group | choice | sequence | any)*)
		//</sequence>
		internal static XmlSchemaSequence Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaSequence sequence = new XmlSchemaSequence();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaSequence.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			sequence.LineNumber = reader.LineNumber;
			sequence.LinePosition = reader.LinePosition;
			sequence.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					sequence.Id = reader.Value;
				}
				else if(reader.Name == "maxOccurs")
				{
					try
					{
						sequence.MaxOccursString = reader.Value;
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
						sequence.MinOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for minOccurs",e);
					}
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for sequence",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,sequence);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return sequence;

			//  Content: (annotation?, (element | group | choice | sequence | any)*)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaSequence.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						sequence.Annotation = annotation;
					continue;
				}
				if(level <=2)
				{
					if(reader.LocalName == "element")
					{
						level = 2;
						XmlSchemaElement element = XmlSchemaElement.Read(reader,h);
						if(element != null)
							sequence.items.Add(element);
						continue;
					}
					if(reader.LocalName == "group")
					{
						level = 2;
						XmlSchemaGroupRef group = XmlSchemaGroupRef.Read(reader,h);
						if(group != null)
							sequence.items.Add(group);
						continue;
					}
					if(reader.LocalName == "choice")
					{
						level = 2;
						XmlSchemaChoice choice = XmlSchemaChoice.Read(reader,h);
						if(choice != null)
							sequence.items.Add(choice);
						continue;
					}
					if(reader.LocalName == "sequence")
					{
						level = 2;
						XmlSchemaSequence seq = XmlSchemaSequence.Read(reader,h);
						if(seq != null)
							sequence.items.Add(seq);
						continue;
					}
					if(reader.LocalName == "any")
					{
						level = 2;
						XmlSchemaAny any = XmlSchemaAny.Read(reader,h);
						if(any != null)
							sequence.items.Add(any);
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
			return sequence;
		}
	}
}
