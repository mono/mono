//------------------------------------------------------------------------------
// <copyright file="XmlSchemaComplexType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>  
// <owner current="true" primary="true">Microsoft</owner>                                                              
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Collections;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaComplexType : XmlSchemaType {
        XmlSchemaDerivationMethod block = XmlSchemaDerivationMethod.None;

        XmlSchemaContentModel contentModel;
        XmlSchemaParticle particle;
        XmlSchemaObjectCollection attributes;
        XmlSchemaAnyAttribute anyAttribute;

        XmlSchemaParticle contentTypeParticle = XmlSchemaParticle.Empty;
        XmlSchemaDerivationMethod blockResolved;
        XmlSchemaObjectTable localElements;
        XmlSchemaObjectTable attributeUses;
        XmlSchemaAnyAttribute attributeWildcard;

        static XmlSchemaComplexType anyTypeLax;
        static XmlSchemaComplexType anyTypeSkip;
        static XmlSchemaComplexType untypedAnyType;

        //additional info for Partial validation
        byte pvFlags;
        const byte wildCardMask = 0x01;
        const byte isMixedMask = 0x02;
        const byte isAbstractMask = 0x04;
        //const byte dupDeclMask = 0x08;

        static XmlSchemaComplexType() {
            anyTypeLax = CreateAnyType(XmlSchemaContentProcessing.Lax);
            anyTypeSkip = CreateAnyType(XmlSchemaContentProcessing.Skip);

            // Create xdt:untypedAny
            untypedAnyType = new XmlSchemaComplexType();
            untypedAnyType.SetQualifiedName(new XmlQualifiedName("untypedAny", XmlReservedNs.NsXQueryDataType));
            untypedAnyType.IsMixed = true;
            untypedAnyType.SetContentTypeParticle(anyTypeLax.ContentTypeParticle);
            untypedAnyType.SetContentType(XmlSchemaContentType.Mixed);

            untypedAnyType.ElementDecl = SchemaElementDecl.CreateAnyTypeElementDecl();
            untypedAnyType.ElementDecl.SchemaType = untypedAnyType;
            untypedAnyType.ElementDecl.ContentValidator = AnyTypeContentValidator;

        }

        static XmlSchemaComplexType CreateAnyType(XmlSchemaContentProcessing processContents) {
            XmlSchemaComplexType localAnyType = new XmlSchemaComplexType();
            localAnyType.SetQualifiedName(DatatypeImplementation.QnAnyType);

            XmlSchemaAny anyElement = new XmlSchemaAny();
            anyElement.MinOccurs = decimal.Zero;
            anyElement.MaxOccurs = decimal.MaxValue;

            anyElement.ProcessContents = processContents;
            anyElement.BuildNamespaceList(null);
            XmlSchemaSequence seq = new XmlSchemaSequence();
            seq.Items.Add(anyElement);

            localAnyType.SetContentTypeParticle(seq);
            localAnyType.SetContentType(XmlSchemaContentType.Mixed);

            localAnyType.ElementDecl = SchemaElementDecl.CreateAnyTypeElementDecl();
            localAnyType.ElementDecl.SchemaType = localAnyType;

            //Create contentValidator for Any
            ParticleContentValidator contentValidator = new ParticleContentValidator(XmlSchemaContentType.Mixed);
            contentValidator.Start();
            contentValidator.OpenGroup();
            contentValidator.AddNamespaceList(anyElement.NamespaceList, anyElement);
            contentValidator.AddStar();
            contentValidator.CloseGroup();
            ContentValidator anyContentValidator = contentValidator.Finish(true);
            localAnyType.ElementDecl.ContentValidator = anyContentValidator;

            XmlSchemaAnyAttribute anyAttribute = new XmlSchemaAnyAttribute();
            anyAttribute.ProcessContents = processContents;
            anyAttribute.BuildNamespaceList(null);
            localAnyType.SetAttributeWildcard(anyAttribute);
            localAnyType.ElementDecl.AnyAttribute = anyAttribute;
            return localAnyType;
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.XmlSchemaComplexType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchemaComplexType() {
        }


        [XmlIgnore]
        internal static XmlSchemaComplexType AnyType {
             get { return anyTypeLax; }
        }

        [XmlIgnore]
        internal static XmlSchemaComplexType UntypedAnyType {
             get { return untypedAnyType; }
        }
        
        [XmlIgnore]
        internal static XmlSchemaComplexType AnyTypeSkip {
             get { return anyTypeSkip; }
        }

        internal static ContentValidator AnyTypeContentValidator {
            get {
                return anyTypeLax.ElementDecl.ContentValidator;
            }
        }
        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.IsAbstract"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("abstract"), DefaultValue(false)]
        public bool IsAbstract {
            get {
                return (pvFlags & isAbstractMask) != 0;
            }
            set {
                if (value) {
                    pvFlags = (byte)(pvFlags | isAbstractMask);
                }
                else {
                    pvFlags = (byte)(pvFlags & ~isAbstractMask);
                }
            }
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.Block"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("block"), DefaultValue(XmlSchemaDerivationMethod.None)]
        public XmlSchemaDerivationMethod Block {
            get { return block; }
            set { block = value; }
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.IsMixed"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("mixed"), DefaultValue(false)]
        public override bool IsMixed {
            get {
                return (pvFlags & isMixedMask) != 0;
            }
            set {
                if (value) {
                    pvFlags = (byte)(pvFlags | isMixedMask);
                }
                else {
                    pvFlags = (byte)(pvFlags & ~isMixedMask);
                }
            }
        }


        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.ContentModel"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("simpleContent", typeof(XmlSchemaSimpleContent)),
         XmlElement("complexContent", typeof(XmlSchemaComplexContent))]
        public XmlSchemaContentModel ContentModel {
            get { return contentModel; }
            set { contentModel = value; }
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.Particle"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("group", typeof(XmlSchemaGroupRef)),
         XmlElement("choice", typeof(XmlSchemaChoice)),
         XmlElement("all", typeof(XmlSchemaAll)),
         XmlElement("sequence", typeof(XmlSchemaSequence))]
        public XmlSchemaParticle Particle {
            get { return particle; }
            set { particle = value; }
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.Attributes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("attribute", typeof(XmlSchemaAttribute)),
         XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef))]
        public XmlSchemaObjectCollection Attributes {
            get {
                if (attributes == null) {
                    attributes = new XmlSchemaObjectCollection();
                }
                return attributes;
            }
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.AnyAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("anyAttribute")]
        public XmlSchemaAnyAttribute AnyAttribute {
            get { return anyAttribute; }
            set { anyAttribute = value; }
        }


        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.ContentType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaContentType ContentType {
            get { return SchemaContentType; }
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.ContentTypeParticle"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaParticle ContentTypeParticle {
            get { return contentTypeParticle; }
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.BlockResolved"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaDerivationMethod BlockResolved {
             get { return blockResolved; }
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.AttributeUses"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaObjectTable AttributeUses {
            get {
                if (attributeUses == null) {
                    attributeUses = new XmlSchemaObjectTable();
                }
                return attributeUses;
            }
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.AttributeWildcard"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaAnyAttribute AttributeWildcard {
            get { return attributeWildcard; }
        }

        /// <include file='doc\XmlSchemaComplexType.uex' path='docs/doc[@for="XmlSchemaComplexType.LocalElements"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        internal XmlSchemaObjectTable LocalElements {
            get {
                if (localElements == null) {
                    localElements = new XmlSchemaObjectTable();
                }
                return localElements;
            }
        }

        internal void SetContentTypeParticle(XmlSchemaParticle value) { 
            contentTypeParticle = value; 
        }

        internal void SetBlockResolved(XmlSchemaDerivationMethod value) {
             blockResolved = value; 
        }

        internal void SetAttributeWildcard(XmlSchemaAnyAttribute value) {
             attributeWildcard = value; 
        }

        internal bool HasWildCard {
            get {
                return (pvFlags & wildCardMask) != 0;
            }
            set {
                if (value) {
                    pvFlags = (byte)(pvFlags | wildCardMask);
                }
                else {
                    pvFlags = (byte)(pvFlags & ~wildCardMask);
                }
            }
        }

        internal override XmlQualifiedName DerivedFrom {
            get {
                if (contentModel == null) {
                    // type derived from anyType
                    return XmlQualifiedName.Empty;
                }
                if (contentModel.Content is XmlSchemaComplexContentRestriction)
                    return ((XmlSchemaComplexContentRestriction)contentModel.Content).BaseTypeName;
                else if (contentModel.Content is XmlSchemaComplexContentExtension)
                    return ((XmlSchemaComplexContentExtension)contentModel.Content).BaseTypeName;
                else if (contentModel.Content is XmlSchemaSimpleContentRestriction)
                    return ((XmlSchemaSimpleContentRestriction)contentModel.Content).BaseTypeName;
                else if (contentModel.Content is XmlSchemaSimpleContentExtension)
                    return ((XmlSchemaSimpleContentExtension)contentModel.Content).BaseTypeName;
                else
                    return XmlQualifiedName.Empty;
            }
        }
        
        internal void SetAttributes(XmlSchemaObjectCollection newAttributes) {
            attributes = newAttributes;
        }

        internal bool ContainsIdAttribute(bool findAll) {
            int idCount = 0;
            foreach(XmlSchemaAttribute attribute in this.AttributeUses.Values) {
                if (attribute.Use != XmlSchemaUse.Prohibited) {
                    XmlSchemaDatatype datatype = attribute.Datatype;
                    if (datatype != null && datatype.TypeCode == XmlTypeCode.Id) {
                        idCount++;
                        if (idCount > 1) { //two or more attributes is error
                            break;
                        }
                    }
                }
            }
            return findAll ? (idCount > 1) : (idCount > 0);
        }

        internal override XmlSchemaObject Clone() {
            System.Diagnostics.Debug.Assert(false, "Should never call Clone() on XmlSchemaComplexType. Call Clone(XmlSchema) instead.");
            return Clone(null);
        }

        internal XmlSchemaObject Clone(XmlSchema parentSchema) {
            XmlSchemaComplexType complexType = (XmlSchemaComplexType)MemberwiseClone();
            
            //Deep clone the QNames as these will be updated on chameleon includes
            if (complexType.ContentModel != null) { //simpleContent or complexContent

                XmlSchemaSimpleContent simpleContent = complexType.ContentModel as XmlSchemaSimpleContent; 
                if (simpleContent != null) {
                    XmlSchemaSimpleContent newSimpleContent = (XmlSchemaSimpleContent)simpleContent.Clone();
                    
                    XmlSchemaSimpleContentExtension simpleExt =  simpleContent.Content as XmlSchemaSimpleContentExtension;   
                    if (simpleExt != null) {
                        XmlSchemaSimpleContentExtension newSimpleExt = (XmlSchemaSimpleContentExtension)simpleExt.Clone();
                        newSimpleExt.BaseTypeName = simpleExt.BaseTypeName.Clone();
                        newSimpleExt.SetAttributes(CloneAttributes(simpleExt.Attributes));
                        newSimpleContent.Content = newSimpleExt;
                    }
                    else { //simpleContent.Content is XmlSchemaSimpleContentRestriction
                        XmlSchemaSimpleContentRestriction simpleRest = (XmlSchemaSimpleContentRestriction)simpleContent.Content;
                        XmlSchemaSimpleContentRestriction newSimpleRest = (XmlSchemaSimpleContentRestriction)simpleRest.Clone();
                        newSimpleRest.BaseTypeName = simpleRest.BaseTypeName.Clone();
                        newSimpleRest.SetAttributes(CloneAttributes(simpleRest.Attributes));
                        newSimpleContent.Content = newSimpleRest;
                    }
                    
                    complexType.ContentModel = newSimpleContent;
                }
                else { // complexType.ContentModel is XmlSchemaComplexContent
                    XmlSchemaComplexContent complexContent = (XmlSchemaComplexContent)complexType.ContentModel;
                    XmlSchemaComplexContent newComplexContent = (XmlSchemaComplexContent)complexContent.Clone();

                    XmlSchemaComplexContentExtension complexExt = complexContent.Content as XmlSchemaComplexContentExtension;
                    if (complexExt != null) {
                        XmlSchemaComplexContentExtension newComplexExt = (XmlSchemaComplexContentExtension)complexExt.Clone();
                        newComplexExt.BaseTypeName = complexExt.BaseTypeName.Clone();
                        newComplexExt.SetAttributes(CloneAttributes(complexExt.Attributes));
                        if (HasParticleRef(complexExt.Particle, parentSchema)) {
                            newComplexExt.Particle = CloneParticle(complexExt.Particle, parentSchema);
                        }
                        newComplexContent.Content = newComplexExt;
                    }
                    else { // complexContent.Content is XmlSchemaComplexContentRestriction
                        XmlSchemaComplexContentRestriction complexRest = complexContent.Content as XmlSchemaComplexContentRestriction;
                        XmlSchemaComplexContentRestriction newComplexRest = (XmlSchemaComplexContentRestriction)complexRest.Clone();
                        newComplexRest.BaseTypeName = complexRest.BaseTypeName.Clone();
                        newComplexRest.SetAttributes(CloneAttributes(complexRest.Attributes));
                        if (HasParticleRef(newComplexRest.Particle, parentSchema)) {
                            newComplexRest.Particle = CloneParticle(newComplexRest.Particle, parentSchema);
                        }
                        newComplexContent.Content = newComplexRest;
                    }
                    complexType.ContentModel = newComplexContent;
                }
            }
            else { //equals XmlSchemaComplexContent with baseType is anyType
                if (HasParticleRef(complexType.Particle, parentSchema)) {
                    complexType.Particle = CloneParticle(complexType.Particle, parentSchema);
                }
                complexType.SetAttributes(CloneAttributes(complexType.Attributes));
            }
            complexType.ClearCompiledState();
            return complexType;
        }

        private void ClearCompiledState() {
            //Re-set post-compiled state for cloned object
            this.attributeUses = null;
            this.localElements = null;
            this.attributeWildcard = null;
            this.contentTypeParticle = XmlSchemaParticle.Empty;
            this.blockResolved = XmlSchemaDerivationMethod.None;
        }

        internal static XmlSchemaObjectCollection CloneAttributes(XmlSchemaObjectCollection attributes) {
            if (HasAttributeQNameRef(attributes)) {
                XmlSchemaObjectCollection newAttributes = attributes.Clone();
                XmlSchemaAttributeGroupRef attributeGroupRef;
                XmlSchemaAttributeGroupRef newAttGroupRef;
                XmlSchemaObject xso;
                XmlSchemaAttribute att;

                for (int i = 0; i < attributes.Count; i++) {
                    xso = attributes[i];
                    attributeGroupRef = xso as XmlSchemaAttributeGroupRef;
                    if (attributeGroupRef != null) {
                        newAttGroupRef = (XmlSchemaAttributeGroupRef)attributeGroupRef.Clone();
                        newAttGroupRef.RefName = attributeGroupRef.RefName.Clone();
                        newAttributes[i] = newAttGroupRef;
                    }
                    else { //Its XmlSchemaAttribute
                        att = xso as XmlSchemaAttribute;
                        if (!att.RefName.IsEmpty || !att.SchemaTypeName.IsEmpty) {
                            newAttributes[i] = att.Clone();     
                        }
                    }
                }
                return newAttributes;
            }
            return attributes;
        }

        private static XmlSchemaObjectCollection CloneGroupBaseParticles(XmlSchemaObjectCollection groupBaseParticles, XmlSchema parentSchema) {
            XmlSchemaObjectCollection newParticles = groupBaseParticles.Clone();

            for (int i = 0; i < groupBaseParticles.Count; i++) {
                XmlSchemaParticle p = (XmlSchemaParticle)groupBaseParticles[i];
                newParticles[i] = CloneParticle(p, parentSchema);
            }
            return newParticles; 
        }

        internal static XmlSchemaParticle CloneParticle(XmlSchemaParticle particle, XmlSchema parentSchema) {
            XmlSchemaGroupBase groupBase = particle as XmlSchemaGroupBase;
            if (groupBase != null) { //Choice or sequence
                XmlSchemaGroupBase newGroupBase = groupBase;

                XmlSchemaObjectCollection newGroupbaseParticles = CloneGroupBaseParticles(groupBase.Items, parentSchema);
                newGroupBase = (XmlSchemaGroupBase)groupBase.Clone();
                newGroupBase.SetItems(newGroupbaseParticles);
                return newGroupBase;
            }
            else if (particle is XmlSchemaGroupRef) { // group ref
                XmlSchemaGroupRef newGroupRef = (XmlSchemaGroupRef)particle.Clone();
                newGroupRef.RefName = newGroupRef.RefName.Clone();
                return newGroupRef;
            }
            else {
                XmlSchemaElement oldElem = particle as XmlSchemaElement;
                // If the particle is an element and one of the following is true:
                //   - it references another element by name
                //   - it references its type by name
                //   - it's form (effective) is qualified (meaning it will inherint namespace from chameleon includes if that happens)
                // then the element itself needs to be cloned.
                if (oldElem != null && (!oldElem.RefName.IsEmpty || !oldElem.SchemaTypeName.IsEmpty ||
                    GetResolvedElementForm(parentSchema, oldElem) == XmlSchemaForm.Qualified)) {
                    XmlSchemaElement newElem = (XmlSchemaElement)oldElem.Clone(parentSchema);
                    return newElem;
                }
            }
            return particle;
        }

        // This method returns the effective value of the "element form" for the specified element in the specified
        //   parentSchema. Element form is either qualified, unqualified or none. If it's qualified it means that
        //   if the element doesn't declare its own namespace the targetNamespace of the schema is used instead.
        // The element form can be either specified on the element itself via the "form" attribute or 
        //   if that one is not present its inheritted from the value of the elementFormDefault attribute on the owning 
        //   schema.
        private static XmlSchemaForm GetResolvedElementForm(XmlSchema parentSchema, XmlSchemaElement element) {
            if (element.Form == XmlSchemaForm.None && parentSchema != null) {
                return parentSchema.ElementFormDefault;
            }
            else {
                return element.Form;
            }
        }

        internal static bool HasParticleRef(XmlSchemaParticle particle, XmlSchema parentSchema) {
            XmlSchemaGroupBase groupBase = particle as XmlSchemaGroupBase;
            if (groupBase != null) {
                bool foundRef = false;
                int i = 0;
                while (i < groupBase.Items.Count && !foundRef) {
                    XmlSchemaParticle p = (XmlSchemaParticle)groupBase.Items[i++];
                    if (p is XmlSchemaGroupRef) {
                        foundRef = true;
                    }
                    else {
                        XmlSchemaElement elem = p as XmlSchemaElement;
                        // This is the same condition as in the CloneParticle method
                        //   that's on purpose. This method is used to determine if we need to clone the whole particle.
                        //   If we do, then the CloneParticle is called and it will try to clone only
                        //   those elements which need cloning - and those are the ones matching this condition.
                        if (elem != null && (!elem.RefName.IsEmpty || !elem.SchemaTypeName.IsEmpty ||
                            GetResolvedElementForm(parentSchema, elem) == XmlSchemaForm.Qualified)) {
                            foundRef = true;
                        }
                        else {
                            foundRef = HasParticleRef(p, parentSchema);
                        }
                    }
                }
                return foundRef;
            }
            else if (particle is XmlSchemaGroupRef) {
                return true;
            }
            return false;
        }

        internal static bool HasAttributeQNameRef(XmlSchemaObjectCollection attributes) {
            for (int i = 0; i < attributes.Count; ++i) {
                if (attributes[i] is XmlSchemaAttributeGroupRef) {
                    return true;
                }
                else {
                    XmlSchemaAttribute attribute = attributes[i] as XmlSchemaAttribute;
                    if (!attribute.RefName.IsEmpty || !attribute.SchemaTypeName.IsEmpty) {
                        return true;
                    }
                }
            }
            return false;
        }
    }

}
