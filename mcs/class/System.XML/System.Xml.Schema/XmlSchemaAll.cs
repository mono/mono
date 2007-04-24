//
// System.Xml.Schema.XmlSchemaAll.cs
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
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAll.
	/// </summary>
	public class XmlSchemaAll : XmlSchemaGroupBase
	{
		private XmlSchema schema;
		private XmlSchemaObjectCollection items;
		const string xmlname = "all";
		private bool emptiable;

		public XmlSchemaAll()
		{
			items = new XmlSchemaObjectCollection();
		}

		[XmlElement("element",typeof(XmlSchemaElement))]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}

		internal bool Emptiable
		{
			get { return emptiable; }
		}

		internal override void SetParent (XmlSchemaObject parent)
		{
			base.SetParent (parent);

			foreach (XmlSchemaObject obj in Items)
				obj.SetParent (this);
		}

		/// <remarks>
		/// 1. MaxOccurs must be one. (default is also one)
		/// 2. MinOccurs must be zero or one.
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (CompilationId == schema.CompilationId)
				return 0;

			this.schema = schema;

			if(MaxOccurs != Decimal.One)
				error(h,"maxOccurs must be 1");
			if(MinOccurs != Decimal.One && MinOccurs != Decimal.Zero)
				error(h,"minOccurs must be 0 or 1");

			XmlSchemaUtil.CompileID(Id, this, schema.IDCollection, h);
			CompileOccurence (h, schema);

			foreach(XmlSchemaObject obj in Items)
			{

				XmlSchemaElement elem = obj as XmlSchemaElement;
				if(elem != null)
				{
					if(elem.ValidatedMaxOccurs != Decimal.One && elem.ValidatedMaxOccurs != Decimal.Zero)
					{
						elem.error (h,"The {max occurs} of all the elements of 'all' must be 0 or 1. ");
					}
					errorCount += elem.Compile(h, schema);
				}
				else
				{
					error(h,"XmlSchemaAll can only contain Items of type Element");
				}
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
			else if (Items.Count == 1) {
				if (ValidatedMinOccurs == 1 && ValidatedMaxOccurs == 1) {
					XmlSchemaSequence seq = new XmlSchemaSequence ();
					this.CopyInfo (seq);
					XmlSchemaParticle p = (XmlSchemaParticle) Items [0];
					p = p.GetOptimizedParticle (false);
					if (p == XmlSchemaParticle.Empty)
						OptimizedParticle = p;
					else {
						seq.Items.Add (p);
						seq.CompiledItems.Add (p);
						seq.Compile (null, schema);
						OptimizedParticle = seq;
					}
					return OptimizedParticle;
				}
			}

			XmlSchemaAll all = new XmlSchemaAll ();
			CopyInfo (all);
			CopyOptimizedItems (all);
			OptimizedParticle = all;
			all.ComputeEmptiable ();

			return OptimizedParticle;
		}

		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.CompilationId))
				return errorCount;

			// 3.8.6 All Group Limited :: 1.
			// Beware that this section was corrected: E1-26 of http://www.w3.org/2001/05/xmlschema-errata#Errata1
			if (!this.parentIsGroupDefinition && ValidatedMaxOccurs != 1)
				error (h, "-all- group is limited to be content of a model group, or that of a complex type with maxOccurs to be 1.");

			CompiledItems.Clear ();
			foreach (XmlSchemaParticle obj in Items) {
				errorCount += obj.Validate (h, schema);
				if (obj.ValidatedMaxOccurs != 0 &&
					obj.ValidatedMaxOccurs != 1)
					error (h, "MaxOccurs of a particle inside -all- compositor must be either 0 or 1.");
				CompiledItems.Add (obj);
			}
			ComputeEmptiable ();

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		private void ComputeEmptiable ()
		{
			emptiable = true;
			for (int i = 0; i < Items.Count; i++) {
				if (((XmlSchemaParticle) Items [i]).ValidatedMinOccurs > 0) {
					emptiable = false;
					break;
				}
			}
		}

		internal override bool ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			XmlSchemaAny any = baseParticle as XmlSchemaAny;
			XmlSchemaAll derivedAll = baseParticle as XmlSchemaAll;
			if (any != null) {
				// NSRecurseCheckCardinality
				return ValidateNSRecurseCheckCardinality (any, h, schema, raiseError);
			} else if (derivedAll != null) {
				// Recurse
				if (!ValidateOccurenceRangeOK (derivedAll, h, schema, raiseError))
					return false;
				return ValidateRecurse (derivedAll, h, schema, raiseError);
			}
			else {
				if (raiseError)
					error (h, "Invalid -all- particle derivation was found.");
				return false;
			}
		}

		internal override decimal GetMinEffectiveTotalRange ()
		{
			return GetMinEffectiveTotalRangeAllAndSequence ();
		}

		internal override void ValidateUniqueParticleAttribution (XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema)
		{
			foreach (XmlSchemaElement el in this.Items)
				el.ValidateUniqueParticleAttribution (qnames, nsNames, h, schema);
		}

		internal override void ValidateUniqueTypeAttribution (XmlSchemaObjectTable labels,
			ValidationEventHandler h, XmlSchema schema)
		{
			foreach (XmlSchemaElement el in this.Items)
				el.ValidateUniqueTypeAttribution (labels, h, schema);
		}


		//<all
		//  id = ID
		//  maxOccurs = 1 : 1
		//  minOccurs = (0 | 1) : 1
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, element*)
		//</all>
		internal static XmlSchemaAll Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaAll all = new XmlSchemaAll();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaAll.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}
			
			all.LineNumber = reader.LineNumber;
			all.LinePosition = reader.LinePosition;
			all.SourceUri = reader.BaseURI;

			//Read Attributes
			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					all.Id = reader.Value;
				}
				else if(reader.Name == "maxOccurs")
				{
					try
					{
						all.MaxOccursString = reader.Value;
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
						all.MinOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for minOccurs",e);
					}
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for all",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,all);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return all;

			//Content: (annotation?, element*)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaAll.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						all.Annotation = annotation;
					continue;
				}
				if(level <=2 && reader.LocalName == "element")
				{
					level = 2;
					XmlSchemaElement element = XmlSchemaElement.Read(reader,h);
					if(element != null)
						all.items.Add(element);
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return all;
		}
	}
}
