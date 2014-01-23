//
// System.Xml.Schema.XmlSchemaGroupBase.cs
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
	/// Summary description for XmlSchemaGroupRef.
	/// </summary>
	public class XmlSchemaGroupRef : XmlSchemaParticle
	{
		private XmlSchema schema;
		private XmlQualifiedName refName;
		const string xmlname = "group";
		private XmlSchemaGroup referencedGroup;

		public XmlSchemaGroupRef()
		{
			refName = XmlQualifiedName.Empty;
		}

		// Attribute
		[System.Xml.Serialization.XmlAttribute("ref")]
		public XmlQualifiedName RefName 
		{
			get{ return  refName; } 
			set{ refName = value; }
		}

		// Post Compilation Schema Information
		[XmlIgnore]
		public XmlSchemaGroupBase Particle 
		{
			get{
				if (TargetGroup != null)
					return TargetGroup.Particle;
				else
					return null;
			}
		}

		internal XmlSchemaGroup TargetGroup
		{
			get {
				if (referencedGroup != null && referencedGroup.IsCircularDefinition)
					return null;
				else
					return referencedGroup;
			}
		}

		/// <remarks>
		/// 1. RefName must be present
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (CompilationId == schema.CompilationId)
				return 0;
			this.schema = schema;

			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);
			CompileOccurence (h, schema);

			if(refName == null || refName.IsEmpty)
			{
				error(h,"ref must be present");
			}
			else if(!XmlSchemaUtil.CheckQName(RefName))
				error(h, "RefName must be a valid XmlQualifiedName");

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.ValidationId))
				return errorCount;

			referencedGroup = schema.Groups [RefName] as XmlSchemaGroup;
			// it might be missing sub components.
			if (referencedGroup == null) {
				if (!schema.IsNamespaceAbsent (RefName.Namespace))
					error (h, "Referenced group " + RefName + " was not found in the corresponding schema.");
			}
			// See Errata E1-26: minOccurs=0 is now allowed.
			else if (referencedGroup.Particle is XmlSchemaAll && ValidatedMaxOccurs != 1)
				error (h, "Group reference to -all- particle must have schema component {maxOccurs}=1.");
			if (TargetGroup != null)
				TargetGroup.Validate (h, schema);

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		bool busy; // only for avoiding infinite loop on illegal recursion cases.
		internal override XmlSchemaParticle GetOptimizedParticle (bool isTop)
		{
			if (busy)
				return XmlSchemaParticle.Empty;
			if (OptimizedParticle != null)
				return OptimizedParticle;
			busy = true;
			XmlSchemaGroup g = referencedGroup != null ? referencedGroup : schema.Groups [RefName] as XmlSchemaGroup;
			if (g != null && g.Particle != null) {
				OptimizedParticle = g.Particle;
				OptimizedParticle = OptimizedParticle.GetOptimizedParticle (isTop);
				if (OptimizedParticle != XmlSchemaParticle.Empty && (ValidatedMinOccurs != 1 || ValidatedMaxOccurs != 1)) {
					OptimizedParticle = OptimizedParticle.GetShallowClone ();
					OptimizedParticle.OptimizedParticle = null;
					OptimizedParticle.MinOccurs = this.MinOccurs;
					OptimizedParticle.MaxOccurs = this.MaxOccurs;
					OptimizedParticle.CompileOccurence (null, null);
				}
			}
			else
				OptimizedParticle = XmlSchemaParticle.Empty;
			busy = false;
			return OptimizedParticle;
		}


		internal override bool ParticleEquals (XmlSchemaParticle other)
		{
			return this.GetOptimizedParticle (true).ParticleEquals (other);
		}

		internal override bool ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			if (TargetGroup != null)
				return TargetGroup.Particle.ValidateDerivationByRestriction (baseParticle, h, schema, raiseError);
			else
				return false; // should not occur
		}


		internal override void CheckRecursion (Stack stack, ValidationEventHandler h, XmlSchema schema)
		{
			if (TargetGroup == null)
				return;

			if (stack.Contains (this))
				throw new XmlSchemaException ("Circular group reference was found.", this, null);
			stack.Push (this);
			TargetGroup.Particle.CheckRecursion (stack, h, schema);
			stack.Pop ();
		}

		internal override void ValidateUniqueParticleAttribution (XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema)
		{
			if (TargetGroup != null)
				TargetGroup.Particle.ValidateUniqueParticleAttribution (qnames, nsNames, h, schema);
		}

		internal override void ValidateUniqueTypeAttribution (XmlSchemaObjectTable labels,
			ValidationEventHandler h, XmlSchema schema)
		{
			if (TargetGroup != null)
				TargetGroup.Particle.ValidateUniqueTypeAttribution (labels, h, schema);
		}


		#region Read
		//	<group 
		//		 id = ID 
		//		 ref = QName
		//		 minOccurs = ? : 1
		//		 maxOccurs = ? : 1>
		//		 Content: (annotation?)
		//	</group>
		internal static XmlSchemaGroupRef Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaGroupRef groupref = new XmlSchemaGroupRef();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaGroup.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			groupref.LineNumber = reader.LineNumber;
			groupref.LinePosition = reader.LinePosition;
			groupref.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					groupref.Id = reader.Value;
				}
				else if(reader.Name == "ref")
				{
					Exception innerex;
					groupref.refName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for ref attribute",innerex);
				}
				else if(reader.Name == "maxOccurs")
				{
					try
					{
						groupref.MaxOccursString = reader.Value;
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
						groupref.MinOccursString = reader.Value;
					}
					catch(Exception e)
					{
						error(h,reader.Value + " is an invalid value for minOccurs", e);
					}
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for group",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,groupref);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return groupref;

			//  Content: (annotation?)
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaGroupRef.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2;	//Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						groupref.Annotation = annotation;
					continue;
				}
				reader.RaiseInvalidElementError();
			}			
			return groupref;
		}
		#endregion
	}
}
