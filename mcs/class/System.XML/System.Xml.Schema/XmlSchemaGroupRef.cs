//
// System.Xml.Schema.XmlSchemaGroupBase.cs
//
// Author:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto  ginga@kit.hi-ho.ne.jp
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
		private XmlSchemaGroupBase particle;
		private XmlQualifiedName refName;
		private XmlQualifiedName resolvedRefName;
		private static string xmlname = "group";
		private XmlSchemaGroup referencedGroup;

		public XmlSchemaGroupRef()
		{
			refName = XmlQualifiedName.Empty;
		}
		[System.Xml.Serialization.XmlAttribute("ref")]
		public XmlQualifiedName RefName 
		{
			get{ return  refName; } 
			set{ refName = value; }
		}
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
		internal override XmlSchemaParticle ActualParticle
		{
			get {
				if (TargetGroup != null)
					return TargetGroup.Particle.ActualParticle;
				else
					// For ValidationEventHandler and missing sub components.
					return XmlSchemaParticle.Empty;
			}
		}

		/// <remarks>
		/// 1. RefName must be present
		/// </remarks>
		[MonoTODO]
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

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
		
		[MonoTODO]
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.ValidationId))
				return errorCount;

			referencedGroup = schema.Groups [RefName] as XmlSchemaGroup;
			// it might be missing sub components.
			if (referencedGroup == null && !schema.IsNamespaceAbsent (RefName.Namespace))
				error (h, "Referenced group " + RefName + " was not found in the corresponding schema.");
			else if (TargetGroup != null)
				TargetGroup.Validate (h, schema);

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		internal override bool ParticleEquals (XmlSchemaParticle other)
		{
			return ActualParticle.ParticleEquals (other.ActualParticle);
		}

		internal override void ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
			ValidationEventHandler h, XmlSchema schema)
		{
			if (TargetGroup != null)
				TargetGroup.Particle.ValidateDerivationByRestriction (baseParticle, h, schema);
		}


		internal override void CheckRecursion (int depth, ValidationEventHandler h, XmlSchema schema)
		{
			if (TargetGroup == null)
				return;

			if (this.recursionDepth == -1) {
				recursionDepth = depth;
				TargetGroup.Particle.CheckRecursion (depth, h, schema);
				recursionDepth = -2;
			} else if (depth == recursionDepth)
				throw new XmlSchemaException ("Circular group reference was found.", this, null);
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
	}
}
