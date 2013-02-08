//
// System.Xml.Schema.XmlSchemaAny.cs
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
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Mono.Xml.Schema;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAny.
	/// </summary>
	public class XmlSchemaAny : XmlSchemaParticle
	{
		static XmlSchemaAny anyTypeContent;
		internal static XmlSchemaAny AnyTypeContent {
			get {
				if (anyTypeContent == null) {
					anyTypeContent = new XmlSchemaAny ();
					anyTypeContent.MaxOccursString = "unbounded";
					anyTypeContent.MinOccurs = 0;
					anyTypeContent.CompileOccurence (null, null);
					anyTypeContent.Namespace = "##any";
					anyTypeContent.wildcard.HasValueAny = true;
					anyTypeContent.wildcard.ResolvedNamespaces = new StringCollection ();
					// It is not documented by W3C but it should be.
					anyTypeContent.wildcard.ResolvedProcessing =
						anyTypeContent.ProcessContents = XmlSchemaContentProcessing.Lax;
					anyTypeContent.wildcard.SkipCompile = true;
				}
				return anyTypeContent;
			}
		}

		private string nameSpace;
		private XmlSchemaContentProcessing processing;
		const string xmlname = "any";

		private XsdWildcard wildcard;

		public XmlSchemaAny()
		{
			wildcard = new XsdWildcard (this);
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

		// Post Compilation Schema Infoset
		internal bool HasValueAny {
			get { return wildcard.HasValueAny; }
		}

		internal bool HasValueLocal {
			get { return wildcard.HasValueLocal; }
		}

		internal bool HasValueOther {
			get { return wildcard.HasValueOther; }
		}

		internal bool HasValueTargetNamespace {
			get { return wildcard.HasValueTargetNamespace; }
		}

		internal StringCollection ResolvedNamespaces {
			get { return wildcard.ResolvedNamespaces; }
		}

		internal XmlSchemaContentProcessing ResolvedProcessContents 
		{ 
			get{ return wildcard.ResolvedProcessing; } 
		}

		internal string TargetNamespace
		{
			get { return wildcard.TargetNamespace; }
		}

		/// <remarks>
		/// 1. id must be of type ID
		/// 2. namespace can have one of the following values:
		///		a) ##any or ##other
		///		b) list of anyURI and ##targetNamespace and ##local
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (CompilationId == schema.CompilationId)
				return 0;

			errorCount = 0;

			XmlSchemaUtil.CompileID(Id,this, schema.IDCollection,h);
			wildcard.TargetNamespace = AncestorSchema.TargetNamespace;
			if (wildcard.TargetNamespace == null)
				wildcard.TargetNamespace = "";
			CompileOccurence (h, schema);

			wildcard.Compile (Namespace, h, schema);

			if (processing == XmlSchemaContentProcessing.None)
				wildcard.ResolvedProcessing = XmlSchemaContentProcessing.Strict;
			else
				wildcard.ResolvedProcessing = processing;

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}

		internal override XmlSchemaParticle GetOptimizedParticle (bool isTop)
		{
			if (OptimizedParticle != null)
				return OptimizedParticle;
			// Uncommenting this causes incorrect validation. 
			// It will prevent UPA e.g. msxsdtest/Particles/particlesJf006.xsd
//			if (ValidatedMaxOccurs == 0) {
//				OptimizedParticle = XmlSchemaParticle.Empty;
//				return OptimizedParticle;
//			}

			XmlSchemaAny any = new XmlSchemaAny ();
			CopyInfo (any);
			any.CompileOccurence (null, null);
			any.wildcard = this.wildcard;
			OptimizedParticle = any;

			// properties which never contribute to validation
			any.Namespace = Namespace;
			any.ProcessContents = ProcessContents;
			any.Annotation = Annotation;
			any.UnhandledAttributes = UnhandledAttributes;

			return OptimizedParticle;
		}
		
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			return errorCount;
		}

		internal override bool ParticleEquals (XmlSchemaParticle other)
		{
			XmlSchemaAny any = other as XmlSchemaAny;
			if (any == null)
				return false;
			if (this.HasValueAny != any.HasValueAny ||
				this.HasValueLocal != any.HasValueLocal ||
				this.HasValueOther != any.HasValueOther ||
				this.HasValueTargetNamespace != any.HasValueTargetNamespace ||
				this.ResolvedProcessContents != any.ResolvedProcessContents ||
				this.ValidatedMaxOccurs != any.ValidatedMaxOccurs ||
				this.ValidatedMinOccurs != any.ValidatedMinOccurs ||
				this.ResolvedNamespaces.Count != any.ResolvedNamespaces.Count)
				return false;
			for (int i = 0; i < ResolvedNamespaces.Count; i++)
				if (ResolvedNamespaces [i] != any.ResolvedNamespaces [i])
					return false;
			return true;
		}


		// 3.8.6. Attribute Wildcard Intersection
		// Only try to examine if their intersection is expressible, and
		// returns if the result is empty.
		internal bool ExamineAttributeWildcardIntersection (XmlSchemaAny other,
			ValidationEventHandler h, XmlSchema schema)
		{
			return wildcard.ExamineAttributeWildcardIntersection (other, h, schema);
		}

		internal override bool ValidateDerivationByRestriction (XmlSchemaParticle baseParticle, 
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			XmlSchemaAny baseAny = baseParticle as XmlSchemaAny;
			if (baseAny == null) {
				if (raiseError)
					error (h, "Invalid particle derivation by restriction was found.");
				return false;
			}
			// 3.9.6 Particle Derivation OK (Any:Any - NSSubset)
			if (!ValidateOccurenceRangeOK (baseParticle, h, schema, raiseError))
				return false;
			return wildcard.ValidateWildcardSubset (baseAny.wildcard, h, schema, raiseError);
		}


		internal override void CheckRecursion (Stack depth, ValidationEventHandler h, XmlSchema schema)
		{
			// do nothing
		}

		internal override void ValidateUniqueParticleAttribution (
			XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema)
		{
			// Wildcard Intersection check.
			foreach (XmlSchemaAny other in nsNames)
				if (!ExamineAttributeWildcardIntersection (other, h, schema))
					error (h, "Ambiguous -any- particle was found.");
			nsNames.Add (this);
		}

		internal override void ValidateUniqueTypeAttribution (XmlSchemaObjectTable labels,
			ValidationEventHandler h, XmlSchema schema)
		{
			// do nothing
		}

		// 3.10.4 Wildcard Allows Namespace Name. (In fact it is almost copy...)
		internal bool ValidateWildcardAllowsNamespaceName (string ns,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			return wildcard.ValidateWildcardAllowsNamespaceName (ns, h, schema, raiseError);
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
