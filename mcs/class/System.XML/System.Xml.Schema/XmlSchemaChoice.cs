//
// System.Xml.Schema.XmlSchemaChoice.cs
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
	public class XmlSchemaChoice : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		const string xmlname = "choice";
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

		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			XmlSchemaUtil.CompileID(Id, this, schema.IDCollection, h);
			CompileOccurence (h, schema);

			if (Items.Count == 0)
				this.warn (h, "Empty choice is unsatisfiable if minOccurs not equals to 0");

			foreach(XmlSchemaObject obj in Items)
			{
#if NET_2_0
				obj.Parent = this;
#endif

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

		internal override XmlSchemaParticle GetOptimizedParticle (bool isTop)
		{
			if (OptimizedParticle != null)
				return OptimizedParticle;

			if (Items.Count == 0 || ValidatedMaxOccurs == 0)
				OptimizedParticle = XmlSchemaParticle.Empty;
			// LAMESPEC: Regardless of isTop, it should remove pointless particle. It seems ContentTypeParticle design bug.
			else if (!isTop && Items.Count == 1 && ValidatedMinOccurs == 1 && ValidatedMaxOccurs == 1)
				OptimizedParticle = ((XmlSchemaParticle) Items [0]).GetOptimizedParticle (false);
			else {
				XmlSchemaChoice c = new XmlSchemaChoice ();
				CopyInfo (c);
				for (int i = 0; i < Items.Count; i++) {
					XmlSchemaParticle p = Items [i] as XmlSchemaParticle;
					p = p.GetOptimizedParticle (false);
					if (p == XmlSchemaParticle.Empty)
						continue;
					else if (p is XmlSchemaChoice && p.ValidatedMinOccurs == 1 && p.ValidatedMaxOccurs == 1) {
						XmlSchemaChoice pc = p as XmlSchemaChoice;
						for (int ci = 0; ci < pc.Items.Count; ci++) {
							c.Items.Add (pc.Items [ci]);
							c.CompiledItems.Add (pc.Items [ci]);
						}
					}
					else {
						c.Items.Add (p);
						c.CompiledItems.Add (p);
					}
				}
				if (c.Items.Count == 0)
					OptimizedParticle = XmlSchemaParticle.Empty;
				else
					OptimizedParticle = c;
			}
			return OptimizedParticle;
		}

		internal override int Validate (ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.CompilationId))
				return errorCount;

			CompiledItems.Clear ();
			foreach (XmlSchemaParticle p in Items) {
				errorCount += p.Validate (h, schema); // This is basically extraneous for pointless item, but needed to check validation error.
				CompiledItems.Add (p);
			}

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		internal override bool ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			XmlSchemaAny any = baseParticle as XmlSchemaAny;
			if (any != null) {
				// NSRecurseCheckCardinality
				return ValidateNSRecurseCheckCardinality (any, h, schema, raiseError);
			}

			XmlSchemaChoice choice = baseParticle as XmlSchemaChoice;
			if (choice != null) {
				// RecurseLax
				if (!ValidateOccurenceRangeOK (choice, h, schema, raiseError))
					return false;

				// If it is totally optional, then ignore their contents.
				if (choice.ValidatedMinOccurs == 0 && choice.ValidatedMaxOccurs == 0 &&
					this.ValidatedMinOccurs == 0 && this.ValidatedMaxOccurs == 0)
					return true;
//				return ValidateRecurseLax (choice, h, schema, raiseError);
				return this.ValidateSeqRecurseMapSumCommon (choice, h, schema, true, false, raiseError);
			}

			if (raiseError)
				error (h, "Invalid choice derivation by restriction was found.");
			return false;
		}

		private bool ValidateRecurseLax (XmlSchemaGroupBase baseGroup,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			int index = 0;
			for (int i = 0; i < baseGroup.CompiledItems.Count; i++) {
				XmlSchemaParticle pb = (XmlSchemaParticle) baseGroup.CompiledItems [i];
				pb = pb.GetOptimizedParticle (false);
				if (pb == XmlSchemaParticle.Empty)
					continue;
				XmlSchemaParticle pd = null;
				while (this.CompiledItems.Count > index) {
					pd = (XmlSchemaParticle) this.CompiledItems [index];
					pd = pd.GetOptimizedParticle (false);
					index++;
					if (pd != XmlSchemaParticle.Empty)
						break;
				}
				if (!ValidateParticleSection (ref index, pd, pb, h, schema, raiseError))
					continue;
			}
			if (this.CompiledItems.Count > 0 && index != this.CompiledItems.Count) {
				if (raiseError)
					error (h, "Invalid particle derivation by restriction was found. Extraneous derived particle was found.");
				return false;
			}
			return true;
		}

		private bool ValidateParticleSection (ref int index, XmlSchemaParticle pd, XmlSchemaParticle pb, ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			if (pd == pb) // they are same particle
				return true;

			if (pd != null) {
//				XmlSchemaElement el = pd as XmlSchemaElement;
				XmlSchemaParticle pdx = pd;
//				if (el != null && el.SubstitutingElements.Count > 0)
//					pdx = el.SubstitutingChoice;

				if (!pdx.ValidateDerivationByRestriction (pb, h, schema, false)) {
					if (!pb.ValidateIsEmptiable ()) {
						if (raiseError)
							error (h, "Invalid particle derivation by restriction was found. Invalid sub-particle derivation was found.");
						return false;
					}
					else {
						index--; // try the same derived particle and next base particle.
						return false;
					}
				}
			} else if (!pb.ValidateIsEmptiable ()) {
				if (raiseError)
					error (h, "Invalid particle derivation by restriction was found. Base schema particle has non-emptiable sub particle that is not mapped to the derived particle.");
				return false;
			}

			return true;
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
