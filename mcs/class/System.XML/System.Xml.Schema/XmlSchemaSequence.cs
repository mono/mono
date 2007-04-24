//
// System.Xml.Schema.XmlSchemaSequence.cs
//
// Author:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto  ginga@kit.hi-ho.ne.jp
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
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSequence.
	/// </summary>
	public class XmlSchemaSequence : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		const string xmlname = "sequence";

		public XmlSchemaSequence()
		{
			items = new XmlSchemaObjectCollection();
		}

		[XmlElement("element",typeof(XmlSchemaElement))]
		[XmlElement("group",typeof(XmlSchemaGroupRef))]
		[XmlElement("choice",typeof(XmlSchemaChoice))]
		[XmlElement("sequence",typeof(XmlSchemaSequence))]
		[XmlElement("any",typeof(XmlSchemaAny))]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}

		internal override void SetParent (XmlSchemaObject parent)
		{
			base.SetParent (parent);
			foreach (XmlSchemaObject obj in Items)
				obj.SetParent (this);
		}

		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (CompilationId == schema.CompilationId)
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


		internal override XmlSchemaParticle GetOptimizedParticle (bool isTop)
		{
			if (OptimizedParticle != null)
				return OptimizedParticle;
			if (Items.Count == 0 || ValidatedMaxOccurs == 0) {
				OptimizedParticle = XmlSchemaParticle.Empty;
				return OptimizedParticle;
			}
			if (!isTop && ValidatedMinOccurs == 1 && ValidatedMaxOccurs == 1) {
				if (Items.Count == 1)
					return ((XmlSchemaParticle) Items [0]).GetOptimizedParticle (false);
			}

			XmlSchemaSequence seq = new XmlSchemaSequence ();
			CopyInfo (seq);
			for (int i = 0; i < Items.Count; i++) {
				XmlSchemaParticle p = Items [i] as XmlSchemaParticle;
				p = p.GetOptimizedParticle (false);
				if (p == XmlSchemaParticle.Empty)
					continue;

				else if (p is XmlSchemaSequence && p.ValidatedMinOccurs == 1 && p.ValidatedMaxOccurs == 1) {
					XmlSchemaSequence ps = p as XmlSchemaSequence;
					for (int pi = 0; pi < ps.Items.Count; pi++) {
						seq.Items.Add (ps.Items [pi]);
						seq.CompiledItems.Add (ps.Items [pi]);
					}
				}
				else {
					seq.Items.Add (p);
					seq.CompiledItems.Add (p);
				}
			}
			if (seq.Items.Count == 0)
				OptimizedParticle = XmlSchemaParticle.Empty;
			else
				OptimizedParticle = seq;
			return OptimizedParticle;
		}

		internal override int Validate (ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.CompilationId))
				return errorCount;

			CompiledItems.Clear ();
			foreach (XmlSchemaParticle p in Items) {
				errorCount += p.Validate (h, schema); // This is basically extraneous for pointless item, but needed to check validation error.
//				XmlSchemaParticle particleInPoint = p.GetParticleWithoutPointless ();
//				if (particleInPoint != XmlSchemaParticle.Empty)
//					CompiledItems.Add (particleInPoint);
				CompiledItems.Add (p);
			}

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		internal override bool ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			if (this == baseParticle) // quick check
				return true;

			XmlSchemaElement el = baseParticle as XmlSchemaElement;
			if (el != null) {
				// Forbidden
				if (raiseError)
					error (h, "Invalid sequence paricle derivation.");
				return false;
			}

			XmlSchemaSequence seq = baseParticle as XmlSchemaSequence;
			if (seq != null) {
				// Recurse
				if (!ValidateOccurenceRangeOK (seq, h, schema, raiseError))
					return false;

				// If it is totally optional, then ignore their contents.
				if (seq.ValidatedMinOccurs == 0 && seq.ValidatedMaxOccurs == 0 &&
					this.ValidatedMinOccurs == 0 && this.ValidatedMaxOccurs == 0)
					return true;
				return ValidateRecurse (seq, h, schema, raiseError);
			} 

			XmlSchemaAll all = baseParticle as XmlSchemaAll;
			if (all != null) {
				// RecurseUnordered
				XmlSchemaObjectCollection already = new XmlSchemaObjectCollection ();
				for (int i = 0; i < this.Items.Count; i++) {
					XmlSchemaElement de = this.Items [i] as XmlSchemaElement;
					if (de == null) {
						if (raiseError)
							error (h, "Invalid sequence particle derivation by restriction from all.");
						return false;
					}
					foreach (XmlSchemaElement e in all.Items) {
						if (e.QualifiedName == de.QualifiedName) {
							if (already.Contains (e)) {
								if (raiseError)
									error (h, "Base element particle is mapped to the derived element particle in a sequence two or more times.");
								return false;
							} else {
								already.Add (e);
								if (!de.ValidateDerivationByRestriction (e, h, schema, raiseError))
									return false;
							}
						}
					}
				}
				foreach (XmlSchemaElement e in all.Items)
					if (!already.Contains (e))
						if (!e.ValidateIsEmptiable ()) {
							if (raiseError)
								error (h, "In base -all- particle, mapping-skipped base element which is not emptiable was found.");
							return false;
						}
				return true;
			}
			XmlSchemaAny any = baseParticle as XmlSchemaAny;
			if (any != null) {
				// NSRecurseCheckCardinality
				return ValidateNSRecurseCheckCardinality (any, h, schema, raiseError);
			}
			XmlSchemaChoice choice = baseParticle as XmlSchemaChoice;
			if (choice != null) {
				// MapAndSum
				// In fact it is not Recurse, but it looks almost common.
				return ValidateSeqRecurseMapSumCommon (choice, h, schema, false, true, raiseError);
			}
			return true;
		}

		internal override decimal GetMinEffectiveTotalRange ()
		{
			return GetMinEffectiveTotalRangeAllAndSequence ();
		}

		internal override void ValidateUniqueParticleAttribution (XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema)
		{
			ValidateUPAOnHeadingOptionalComponents (qnames, nsNames, h, schema);
			ValidateUPAOnItems (qnames, nsNames, h, schema);
		}

		void ValidateUPAOnHeadingOptionalComponents (XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema)
		{
			// heading optional components
			foreach (XmlSchemaParticle p in this.Items) {
				p.ValidateUniqueParticleAttribution (qnames, nsNames, h, schema);
				if (p.ValidatedMinOccurs != 0)
					break;
			}
		}

		void ValidateUPAOnItems (XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema)
		{
			// non-optional components
			XmlSchemaObjectTable elems = new XmlSchemaObjectTable ();
			ArrayList wildcards = new ArrayList ();
			XmlSchemaObjectTable tmpElems = new XmlSchemaObjectTable ();
			ArrayList tmpWildcards = new ArrayList ();
			for (int i=0; i<Items.Count; i++) {
				XmlSchemaParticle p1 = Items [i] as XmlSchemaParticle;
				p1.ValidateUniqueParticleAttribution (elems, wildcards, h, schema);
				if (p1.ValidatedMinOccurs == p1.ValidatedMaxOccurs) {
					elems.Clear ();
					wildcards.Clear ();
				}
				else {
					if (p1.ValidatedMinOccurs != 0) {
						foreach (XmlQualifiedName n in tmpElems.Names)
							elems.Set (n, null); // remove
						foreach (object o in tmpWildcards)
							wildcards.Remove (o);
					}
					foreach (XmlQualifiedName n in elems.Names)
						tmpElems.Set (n, elems [n]);
					tmpWildcards.Clear ();
					tmpWildcards.AddRange (wildcards);
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
