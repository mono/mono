//------------------------------------------------------------------------------
// <copyright file="XsdBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                 
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.IO;
    using System.Collections;
    using System.Diagnostics;
    using System.Xml.Serialization;


    internal sealed class XsdBuilder : SchemaBuilder {
        private enum State {
            Root,
            Schema,
            Annotation,
            Include,
            Import,
            Element,
            Attribute,
            AttributeGroup,
            AttributeGroupRef,
            AnyAttribute,
            Group,
            GroupRef,
            All,
            Choice,
            Sequence,
            Any,
            Notation,
            SimpleType,
            ComplexType,
            ComplexContent,
            ComplexContentRestriction,
            ComplexContentExtension,
            SimpleContent,
            SimpleContentExtension,
            SimpleContentRestriction,
            SimpleTypeUnion,
            SimpleTypeList,
            SimpleTypeRestriction,
            Unique,
            Key,
            KeyRef,
            Selector,
            Field,
            MinExclusive,
            MinInclusive,
            MaxExclusive,
            MaxInclusive,
            TotalDigits,
            FractionDigits,
            Length,
            MinLength,
            MaxLength,
            Enumeration,
            Pattern,
            WhiteSpace,
            AppInfo,
            Documentation,
            Redefine,
        }
        private const int STACK_INCREMENT         = 10;

        private delegate void XsdBuildFunction(XsdBuilder builder, string value);
        private delegate void XsdInitFunction(XsdBuilder builder, string value);
        private delegate void XsdEndChildFunction(XsdBuilder builder);

        private sealed class XsdAttributeEntry {
            public SchemaNames.Token Attribute;               // possible attribute names
            public XsdBuildFunction BuildFunc;  // Corresponding build functions for attribute value

            public XsdAttributeEntry(SchemaNames.Token a, XsdBuildFunction build) {
                Attribute = a;
                BuildFunc = build;
            }
        };

        //
        // XsdEntry controls the states of parsing a schema document
        // and calls the corresponding "init", "end" and "build" functions when necessary
        //
        private sealed class XsdEntry {
            public SchemaNames.Token Name;                  // the name of the object it is comparing to
            public State CurrentState;                     
            public State[] NextStates;                   // possible next states
            public XsdAttributeEntry[] Attributes;       // allowed attributes
            public XsdInitFunction InitFunc;             // "init" functions in XsdBuilder
            public XsdEndChildFunction EndChildFunc;     // "end" functions in XsdBuilder for EndChildren
            public bool ParseContent;                       // whether text content is allowed  

            public XsdEntry(SchemaNames.Token n, 
                            State   state, 
                            State[] nextStates, 
                            XsdAttributeEntry[] attributes, 
                            XsdInitFunction init, 
                            XsdEndChildFunction end, 
                            bool parseContent) {
                Name = n;
                CurrentState = state;
                NextStates = nextStates;
                Attributes = attributes;
                InitFunc = init;
                EndChildFunc = end;
                ParseContent = parseContent; 
            }
        };

        
        //required for Parsing QName
        class BuilderNamespaceManager : XmlNamespaceManager {
            XmlNamespaceManager nsMgr;
            XmlReader reader;

            public BuilderNamespaceManager(XmlNamespaceManager nsMgr, XmlReader reader) {
                this.nsMgr = nsMgr;
                this.reader = reader;
            }

            public override string LookupNamespace(string prefix) { 
                string ns = nsMgr.LookupNamespace(prefix);
                if (ns == null) {
                    ns = reader.LookupNamespace(prefix);
                }
                return ns;
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Data structures for XSD Schema, Sept 2000 version
        //

        //
        //Elements
        //
        private static readonly State[] SchemaElement = {   
            State.Schema};
        private static readonly State[] SchemaSubelements = {   
            State.Annotation, State.Include, State.Import, State.Redefine,
            State.ComplexType, State.SimpleType, State.Element, State.Attribute,
            State.AttributeGroup, State.Group, State.Notation}; 
        private static readonly State[] AttributeSubelements = { 
            State.Annotation, State.SimpleType};
        private static readonly State[] ElementSubelements   = { 
            State.Annotation, State.SimpleType, State.ComplexType,
            State.Unique, State.Key, State.KeyRef};
        private static readonly State[] ComplexTypeSubelements = {
            State.Annotation, State.SimpleContent, State.ComplexContent,
            State.GroupRef, State.All, State.Choice, State.Sequence, 
            State.Attribute, State.AttributeGroupRef, State.AnyAttribute};
        private static readonly State[] SimpleContentSubelements = { 
            State.Annotation, State.SimpleContentRestriction, State.SimpleContentExtension };
        private static readonly State[] SimpleContentExtensionSubelements = { 
            State.Annotation, State.Attribute, State.AttributeGroupRef, State.AnyAttribute};
        private static readonly State[] SimpleContentRestrictionSubelements = { 
            State.Annotation, State.SimpleType,  
            State.Enumeration, State.Length, State.MaxExclusive, State.MaxInclusive, State.MaxLength, State.MinExclusive, 
            State.MinInclusive, State.MinLength, State.Pattern, State.TotalDigits, State.FractionDigits, State.WhiteSpace, 
            State.Attribute, State.AttributeGroupRef, State.AnyAttribute};
        private static readonly State[] ComplexContentSubelements = { 
            State.Annotation, State.ComplexContentRestriction, State.ComplexContentExtension };
        private static readonly State[] ComplexContentExtensionSubelements = { 
            State.Annotation, State.GroupRef, State.All, State.Choice, State.Sequence, 
            State.Attribute, State.AttributeGroupRef, State.AnyAttribute};
        private static readonly State[] ComplexContentRestrictionSubelements = { 
            State.Annotation, State.GroupRef, State.All, State.Choice, State.Sequence, 
            State.Attribute, State.AttributeGroupRef, State.AnyAttribute};
        private static readonly State[] SimpleTypeSubelements   = { 
            State.Annotation, State.SimpleTypeList, State.SimpleTypeRestriction, State.SimpleTypeUnion};
        private static readonly State[] SimpleTypeRestrictionSubelements   = { 
            State.Annotation, State.SimpleType, 
            State.Enumeration, State.Length, State.MaxExclusive, State.MaxInclusive, State.MaxLength, State.MinExclusive, 
            State.MinInclusive, State.MinLength, State.Pattern, State.TotalDigits, State.FractionDigits, State.WhiteSpace};
        private static readonly State[] SimpleTypeListSubelements   = { 
            State.Annotation, State.SimpleType};
        private static readonly State[] SimpleTypeUnionSubelements   = { 
            State.Annotation, State.SimpleType};
        private static readonly State[] RedefineSubelements   = { 
            State.Annotation, State.AttributeGroup, State.ComplexType, State.Group, State.SimpleType };
        private static readonly State[] AttributeGroupSubelements = { 
            State.Annotation, State.Attribute, State.AttributeGroupRef, State.AnyAttribute};
        private static readonly State[] GroupSubelements = { 
            State.Annotation, State.All, State.Choice, State.Sequence};
        private static readonly State[] AllSubelements = { 
            State.Annotation, State.Element};
        private static readonly State[] ChoiceSequenceSubelements = { 
            State.Annotation, State.Element, State.GroupRef, State.Choice, State.Sequence, State.Any};
        private static readonly State[] IdentityConstraintSubelements = { 
            State.Annotation, State.Selector, State.Field};
        private static readonly State[] AnnotationSubelements = { 
            State.AppInfo, State.Documentation};
        private static readonly State[] AnnotatedSubelements = {
            State.Annotation}; 


        //
        //Attributes
        //
        private static readonly XsdAttributeEntry[] SchemaAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaAttributeFormDefault,    new XsdBuildFunction(BuildSchema_AttributeFormDefault) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaElementFormDefault,      new XsdBuildFunction(BuildSchema_ElementFormDefault) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaTargetNamespace,         new XsdBuildFunction(BuildSchema_TargetNamespace) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaVersion,                 new XsdBuildFunction(BuildSchema_Version) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaFinalDefault,            new XsdBuildFunction(BuildSchema_FinalDefault) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaBlockDefault,            new XsdBuildFunction(BuildSchema_BlockDefault) )
        };

        private static readonly XsdAttributeEntry[] AttributeAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaDefault,                 new XsdBuildFunction(BuildAttribute_Default) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaFixed,                   new XsdBuildFunction(BuildAttribute_Fixed) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaForm,                    new XsdBuildFunction(BuildAttribute_Form) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaName,                    new XsdBuildFunction(BuildAttribute_Name) ),       
            new XsdAttributeEntry(SchemaNames.Token.SchemaRef,                     new XsdBuildFunction(BuildAttribute_Ref) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaType,                    new XsdBuildFunction(BuildAttribute_Type) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaUse,                     new XsdBuildFunction(BuildAttribute_Use) )
        };

        private static readonly XsdAttributeEntry[] ElementAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaAbstract,                new XsdBuildFunction(BuildElement_Abstract) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaBlock,                   new XsdBuildFunction(BuildElement_Block) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaDefault,                 new XsdBuildFunction(BuildElement_Default) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaFinal,                   new XsdBuildFunction(BuildElement_Final) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaFixed,                   new XsdBuildFunction(BuildElement_Fixed) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaForm,                    new XsdBuildFunction(BuildElement_Form) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMaxOccurs,               new XsdBuildFunction(BuildElement_MaxOccurs) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMinOccurs,               new XsdBuildFunction(BuildElement_MinOccurs) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaName,                    new XsdBuildFunction(BuildElement_Name) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaNillable,                new XsdBuildFunction(BuildElement_Nillable) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaRef,                     new XsdBuildFunction(BuildElement_Ref) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaSubstitutionGroup,       new XsdBuildFunction(BuildElement_SubstitutionGroup) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaType,                    new XsdBuildFunction(BuildElement_Type) )
        };

        private static readonly XsdAttributeEntry[] ComplexTypeAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaAbstract,                new XsdBuildFunction(BuildComplexType_Abstract) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaBlock,                   new XsdBuildFunction(BuildComplexType_Block) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaFinal,                   new XsdBuildFunction(BuildComplexType_Final) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMixed,                   new XsdBuildFunction(BuildComplexType_Mixed) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaName,                    new XsdBuildFunction(BuildComplexType_Name) )
        };

        private static readonly XsdAttributeEntry[] SimpleContentAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
        };

        private static readonly XsdAttributeEntry[] SimpleContentExtensionAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaBase,                    new XsdBuildFunction(BuildSimpleContentExtension_Base) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) )
        };

        private static readonly XsdAttributeEntry[] SimpleContentRestrictionAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaBase,                    new XsdBuildFunction(BuildSimpleContentRestriction_Base) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
        };

        private static readonly XsdAttributeEntry[] ComplexContentAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMixed,                   new XsdBuildFunction(BuildComplexContent_Mixed) ),
        };

        private static readonly XsdAttributeEntry[] ComplexContentExtensionAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaBase,                    new XsdBuildFunction(BuildComplexContentExtension_Base) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
        };

        private static readonly XsdAttributeEntry[] ComplexContentRestrictionAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaBase,                    new XsdBuildFunction(BuildComplexContentRestriction_Base) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
        };

        private static readonly XsdAttributeEntry[] SimpleTypeAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaFinal,                   new XsdBuildFunction(BuildSimpleType_Final) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaName,                    new XsdBuildFunction(BuildSimpleType_Name) )
        };

        private static readonly XsdAttributeEntry[] SimpleTypeRestrictionAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaBase,                    new XsdBuildFunction(BuildSimpleTypeRestriction_Base) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
        };

        private static readonly XsdAttributeEntry[] SimpleTypeUnionAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMemberTypes,             new XsdBuildFunction(BuildSimpleTypeUnion_MemberTypes) ),
        };

        private static readonly XsdAttributeEntry[] SimpleTypeListAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaItemType,                new XsdBuildFunction(BuildSimpleTypeList_ItemType) ),
        };

        private static readonly XsdAttributeEntry[] AttributeGroupAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaName,                    new XsdBuildFunction(BuildAttributeGroup_Name) ),
        };

        private static readonly XsdAttributeEntry[] AttributeGroupRefAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaRef,                     new XsdBuildFunction(BuildAttributeGroupRef_Ref) )
        };

        private static readonly XsdAttributeEntry[] GroupAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaName,                    new XsdBuildFunction(BuildGroup_Name) ),
        };

        private static readonly XsdAttributeEntry[] GroupRefAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMaxOccurs,               new XsdBuildFunction(BuildParticle_MaxOccurs) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMinOccurs,               new XsdBuildFunction(BuildParticle_MinOccurs) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaRef,                     new XsdBuildFunction(BuildGroupRef_Ref) )
        }; 

        private static readonly XsdAttributeEntry[] ParticleAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMaxOccurs,               new XsdBuildFunction(BuildParticle_MaxOccurs) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMinOccurs,               new XsdBuildFunction(BuildParticle_MinOccurs) ),
        };


        private static readonly XsdAttributeEntry[] AnyAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMaxOccurs,               new XsdBuildFunction(BuildParticle_MaxOccurs) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaMinOccurs,               new XsdBuildFunction(BuildParticle_MinOccurs) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaNamespace,               new XsdBuildFunction(BuildAny_Namespace) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaProcessContents,         new XsdBuildFunction(BuildAny_ProcessContents) )
        };

        private static readonly XsdAttributeEntry[] IdentityConstraintAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaName,                    new XsdBuildFunction(BuildIdentityConstraint_Name) ), 
            new XsdAttributeEntry(SchemaNames.Token.SchemaRefer,                   new XsdBuildFunction(BuildIdentityConstraint_Refer) ) 
        };

        private static readonly XsdAttributeEntry[] SelectorAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaXPath,                   new XsdBuildFunction(BuildSelector_XPath) ) 
        };

        private static readonly XsdAttributeEntry[] FieldAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaXPath,                   new XsdBuildFunction(BuildField_XPath) ) 
        };

        private static readonly XsdAttributeEntry[] NotationAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaName,                    new XsdBuildFunction(BuildNotation_Name) ), 
            new XsdAttributeEntry(SchemaNames.Token.SchemaPublic,                  new XsdBuildFunction(BuildNotation_Public) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaSystem,                  new XsdBuildFunction(BuildNotation_System) ) 
        };

        private static readonly XsdAttributeEntry[] IncludeAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaSchemaLocation,          new XsdBuildFunction(BuildInclude_SchemaLocation) )
        };

        private static readonly XsdAttributeEntry[] ImportAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaNamespace,               new XsdBuildFunction(BuildImport_Namespace) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaSchemaLocation,          new XsdBuildFunction(BuildImport_SchemaLocation) )
        };

        private static readonly XsdAttributeEntry[] FacetAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaFixed,                   new XsdBuildFunction(BuildFacet_Fixed) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaValue,                   new XsdBuildFunction(BuildFacet_Value) )
        };

        private static readonly XsdAttributeEntry[] AnyAttributeAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaNamespace,               new XsdBuildFunction(BuildAnyAttribute_Namespace) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaProcessContents,         new XsdBuildFunction(BuildAnyAttribute_ProcessContents) )
        };

        private static readonly XsdAttributeEntry[] DocumentationAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaSource,                  new XsdBuildFunction(BuildDocumentation_Source) ),
            new XsdAttributeEntry(SchemaNames.Token.XmlLang,                       new XsdBuildFunction(BuildDocumentation_XmlLang) )
        };

        private static readonly XsdAttributeEntry[] AppinfoAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaSource,                  new XsdBuildFunction(BuildAppinfo_Source) )
        };

        private static readonly XsdAttributeEntry[] RedefineAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
            new XsdAttributeEntry(SchemaNames.Token.SchemaSchemaLocation,          new XsdBuildFunction(BuildRedefine_SchemaLocation) )
        };

        private static readonly XsdAttributeEntry[] AnnotationAttributes = {
            new XsdAttributeEntry(SchemaNames.Token.SchemaId,                      new XsdBuildFunction(BuildAnnotated_Id) ),
        };
        //
        // XSD Schema entries
        //                        

       private static readonly XsdEntry[] SchemaEntries = {
       /* Root */                       new XsdEntry( SchemaNames.Token.Empty, State.Root, SchemaElement, null, 
                                                      null, 
                                                      null, 
                                                      true),
       /* Schema */                     new XsdEntry( SchemaNames.Token.XsdSchema, State.Schema,     SchemaSubelements, SchemaAttributes, 
                                                      new XsdInitFunction(InitSchema), 
                                                      null,
                                                      true),
       /* Annotation */                 new XsdEntry( SchemaNames.Token.XsdAnnotation, State.Annotation,     AnnotationSubelements, AnnotationAttributes,
                                                      new XsdInitFunction(InitAnnotation), 
                                                      null,
                                                      true),
       /* Include */                    new XsdEntry( SchemaNames.Token.XsdInclude, State.Include,    AnnotatedSubelements, IncludeAttributes,
                                                      new XsdInitFunction(InitInclude),       
                                                      null,
                                                      true),
       /* Import */                     new XsdEntry( SchemaNames.Token.XsdImport, State.Import,     AnnotatedSubelements, ImportAttributes,
                                                      new XsdInitFunction(InitImport),   
                                                      null,
                                                      true),
       /* Element */                    new XsdEntry( SchemaNames.Token.XsdElement, State.Element,     ElementSubelements, ElementAttributes,
                                                      new XsdInitFunction(InitElement),    
                                                      null,
                                                      true),
       /* Attribute */                  new XsdEntry( SchemaNames.Token.XsdAttribute, State.Attribute,     AttributeSubelements, AttributeAttributes,
                                                      new XsdInitFunction(InitAttribute),    
                                                      null,
                                                      true),
       /* AttributeGroup */             new XsdEntry( SchemaNames.Token.xsdAttributeGroup, State.AttributeGroup,     AttributeGroupSubelements, AttributeGroupAttributes,
                                                      new XsdInitFunction(InitAttributeGroup),    
                                                      null,
                                                      true),
       /* AttributeGroupRef */          new XsdEntry( SchemaNames.Token.xsdAttributeGroup, State.AttributeGroupRef,  AnnotatedSubelements, AttributeGroupRefAttributes,
                                                      new XsdInitFunction(InitAttributeGroupRef),    
                                                      null,
                                                      true),
       /* AnyAttribute */               new XsdEntry( SchemaNames.Token.XsdAnyAttribute, State.AnyAttribute,     AnnotatedSubelements, AnyAttributeAttributes,
                                                      new XsdInitFunction(InitAnyAttribute),    
                                                      null,
                                                      true),
       /* Group */                      new XsdEntry( SchemaNames.Token.XsdGroup, State.Group,     GroupSubelements, GroupAttributes,
                                                      new XsdInitFunction(InitGroup),    
                                                      null,
                                                      true),
       /* GroupRef */                   new XsdEntry( SchemaNames.Token.XsdGroup, State.GroupRef,     AnnotatedSubelements, GroupRefAttributes,
                                                      new XsdInitFunction(InitGroupRef),    
                                                      null,
                                                      true),
       /* All */                        new XsdEntry( SchemaNames.Token.XsdAll, State.All,     AllSubelements, ParticleAttributes,
                                                      new XsdInitFunction(InitAll),    
                                                      null,
                                                      true),
       /* Choice */                     new XsdEntry( SchemaNames.Token.XsdChoice, State.Choice,     ChoiceSequenceSubelements, ParticleAttributes,
                                                      new XsdInitFunction(InitChoice),    
                                                      null,
                                                      true),
       /* Sequence */                   new XsdEntry( SchemaNames.Token.XsdSequence, State.Sequence,     ChoiceSequenceSubelements, ParticleAttributes,
                                                      new XsdInitFunction(InitSequence),    
                                                      null,
                                                      true),
       /* Any */                        new XsdEntry( SchemaNames.Token.XsdAny, State.Any,     AnnotatedSubelements, AnyAttributes,
                                                      new XsdInitFunction(InitAny),    
                                                      null,
                                                      true),
       /* Notation */                   new XsdEntry( SchemaNames.Token.XsdNotation, State.Notation,     AnnotatedSubelements, NotationAttributes,
                                                      new XsdInitFunction(InitNotation),    
                                                      null,
                                                      true),
       /* SimpleType */                 new XsdEntry( SchemaNames.Token.XsdSimpleType, State.SimpleType,     SimpleTypeSubelements, SimpleTypeAttributes,
                                                      new XsdInitFunction(InitSimpleType),    
                                                      null,
                                                      true),
       /* ComplexType */                new XsdEntry( SchemaNames.Token.XsdComplexType, State.ComplexType,     ComplexTypeSubelements, ComplexTypeAttributes,
                                                      new XsdInitFunction(InitComplexType),    
                                                      null,
                                                      true),
       /* ComplexContent */             new XsdEntry( SchemaNames.Token.XsdComplexContent, State.ComplexContent,  ComplexContentSubelements, ComplexContentAttributes,
                                                      new XsdInitFunction(InitComplexContent),    
                                                      null,
                                                      true),
       /* ComplexContentRestriction */    new XsdEntry( SchemaNames.Token.XsdComplexContentRestriction, State.ComplexContentRestriction,  ComplexContentRestrictionSubelements, ComplexContentRestrictionAttributes,
                                                      new XsdInitFunction(InitComplexContentRestriction),    
                                                      null,
                                                      true),
       /* ComplexContentExtension */  new XsdEntry( SchemaNames.Token.XsdComplexContentExtension, State.ComplexContentExtension,  ComplexContentExtensionSubelements, ComplexContentExtensionAttributes,
                                                      new XsdInitFunction(InitComplexContentExtension),    
                                                      null,
                                                      true),
       /* SimpleContent */              new XsdEntry( SchemaNames.Token.XsdSimpleContent, State.SimpleContent,  SimpleContentSubelements, SimpleContentAttributes,
                                                      new XsdInitFunction(InitSimpleContent),    
                                                      null,
                                                      true),
       /* SimpleContentExtension */     new XsdEntry( SchemaNames.Token.XsdSimpleContentExtension, State.SimpleContentExtension,  SimpleContentExtensionSubelements, SimpleContentExtensionAttributes,
                                                      new XsdInitFunction(InitSimpleContentExtension),    
                                                      null,
                                                      true),
       /* SimpleContentRestriction */   new XsdEntry( SchemaNames.Token.XsdSimpleContentRestriction, State.SimpleContentRestriction,  SimpleContentRestrictionSubelements, SimpleContentRestrictionAttributes,
                                                      new XsdInitFunction(InitSimpleContentRestriction),    
                                                      null,
                                                      true),
       /* SimpleTypeUnion */            new XsdEntry( SchemaNames.Token.XsdSimpleTypeUnion, State.SimpleTypeUnion,    SimpleTypeUnionSubelements, SimpleTypeUnionAttributes,
                                                      new XsdInitFunction(InitSimpleTypeUnion),    
                                                      null,
                                                      true),
       /* SimpleTypeList */             new XsdEntry( SchemaNames.Token.XsdSimpleTypeList, State.SimpleTypeList,     SimpleTypeListSubelements, SimpleTypeListAttributes,
                                                      new XsdInitFunction(InitSimpleTypeList),    
                                                      null,
                                                      true),
       /* SimpleTypeRestriction */      new XsdEntry( SchemaNames.Token.XsdSimpleTypeRestriction, State.SimpleTypeRestriction,  SimpleTypeRestrictionSubelements, SimpleTypeRestrictionAttributes,
                                                      new XsdInitFunction(InitSimpleTypeRestriction),    
                                                      null,
                                                      true),
       /* Unique */                     new XsdEntry( SchemaNames.Token.XsdUnique,  State.Unique,    IdentityConstraintSubelements, IdentityConstraintAttributes,
                                                      new XsdInitFunction(InitIdentityConstraint),    
                                                      null,
                                                      true),
       /* Key */                        new XsdEntry( SchemaNames.Token.XsdKey, State.Key,        IdentityConstraintSubelements, IdentityConstraintAttributes,
                                                      new XsdInitFunction(InitIdentityConstraint),    
                                                      null,
                                                      true),
       /* KeyRef */                     new XsdEntry( SchemaNames.Token.XsdKeyref, State.KeyRef,     IdentityConstraintSubelements, IdentityConstraintAttributes,
                                                      new XsdInitFunction(InitIdentityConstraint),    
                                                      null,
                                                      true),
       /* Selector */                   new XsdEntry( SchemaNames.Token.XsdSelector, State.Selector,     AnnotatedSubelements, SelectorAttributes,
                                                      new XsdInitFunction(InitSelector),
                                                      null,
                                                      true),
       /* Field */                      new XsdEntry( SchemaNames.Token.XsdField, State.Field,     AnnotatedSubelements, FieldAttributes,
                                                      new XsdInitFunction(InitField),    
                                                      null,
                                                      true),
       /* MinExclusive */               new XsdEntry( SchemaNames.Token.XsdMinExclusive, State.MinExclusive,     AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* MinInclusive */               new XsdEntry( SchemaNames.Token.XsdMinInclusive, State.MinInclusive,     AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* MaxExclusive */               new XsdEntry( SchemaNames.Token.XsdMaxExclusive, State.MaxExclusive,     AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* MaxInclusive */               new XsdEntry( SchemaNames.Token.XsdMaxInclusive, State.MaxInclusive,     AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* TotalDigits */                new XsdEntry( SchemaNames.Token.XsdTotalDigits, State.TotalDigits,     AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* FractionDigits */             new XsdEntry( SchemaNames.Token.XsdFractionDigits, State.FractionDigits,     AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* Length */                     new XsdEntry( SchemaNames.Token.XsdLength, State.Length,     AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* MinLength */                  new XsdEntry( SchemaNames.Token.XsdMinLength, State.MinLength,     AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* MaxLength */                  new XsdEntry( SchemaNames.Token.XsdMaxLength, State.MaxLength,     AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* Enumeration */                new XsdEntry( SchemaNames.Token.XsdEnumeration, State.Enumeration,    AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* Pattern */                    new XsdEntry( SchemaNames.Token.XsdPattern, State.Pattern,    AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* WhiteSpace */                 new XsdEntry( SchemaNames.Token.XsdWhitespace, State.WhiteSpace, AnnotatedSubelements, FacetAttributes,
                                                      new XsdInitFunction(InitFacet),    
                                                      null,
                                                      true),
       /* AppInfo */                    new XsdEntry( SchemaNames.Token.XsdAppInfo, State.AppInfo,    null, AppinfoAttributes,
                                                      new XsdInitFunction(InitAppinfo), 
                                                      new XsdEndChildFunction(EndAppinfo),
                                                      false),
       /* Documentation */              new XsdEntry( SchemaNames.Token.XsdDocumentation, State.Documentation,    null, DocumentationAttributes,
                                                      new XsdInitFunction(InitDocumentation),    
                                                      new XsdEndChildFunction(EndDocumentation),
                                                      false),
       /* Redefine */                   new XsdEntry( SchemaNames.Token.XsdRedefine, State.Redefine,    RedefineSubelements, RedefineAttributes,
                                                      new XsdInitFunction(InitRedefine),    
                                                      new XsdEndChildFunction(EndRedefine),
                                                      true)
        };

        //
        // for 'block' and 'final' attribute values
        //
        private static readonly int[]    DerivationMethodValues = { 
            (int)XmlSchemaDerivationMethod.Substitution,
            (int)XmlSchemaDerivationMethod.Extension, 
            (int)XmlSchemaDerivationMethod.Restriction, 
            (int)XmlSchemaDerivationMethod.List, 
            (int)XmlSchemaDerivationMethod.Union, 
            (int)XmlSchemaDerivationMethod.All, 
        };        
        private static readonly string[] DerivationMethodStrings = { 
            "substitution",
            "extension", 
            "restriction", 
            "list", 
            "union", 
            "#all", 
        };
                 
        private static readonly string[] FormStringValues = { "qualified", "unqualified"};
        private static readonly string[] UseStringValues = { "optional", "prohibited", "required" };
        private static readonly string[] ProcessContentsStringValues = {"skip", "lax", "strict"};

        private XmlReader reader;
        private PositionInfo positionInfo;
        private XsdEntry currentEntry;
        private XsdEntry nextEntry;
        private bool hasChild;
        private HWStack stateHistory = new HWStack(STACK_INCREMENT);
        private Stack containerStack = new Stack();
        private XmlNameTable nameTable;
        private SchemaNames schemaNames;
        private XmlNamespaceManager namespaceManager;
        private bool canIncludeImport;

        private XmlSchema schema;
        private XmlSchemaObject xso;
        private XmlSchemaElement element;
        private XmlSchemaAny anyElement;
        private XmlSchemaAttribute attribute;
        private XmlSchemaAnyAttribute anyAttribute;
        private XmlSchemaComplexType complexType;
        private XmlSchemaSimpleType simpleType;
        private XmlSchemaComplexContent complexContent;
        private XmlSchemaComplexContentExtension complexContentExtension;
        private XmlSchemaComplexContentRestriction complexContentRestriction;
        private XmlSchemaSimpleContent simpleContent;
        private XmlSchemaSimpleContentExtension simpleContentExtension;
        private XmlSchemaSimpleContentRestriction simpleContentRestriction;
        private XmlSchemaSimpleTypeUnion simpleTypeUnion;
        private XmlSchemaSimpleTypeList simpleTypeList;
        private XmlSchemaSimpleTypeRestriction simpleTypeRestriction;
        private XmlSchemaGroup group;  
        private XmlSchemaGroupRef groupRef; 
        private XmlSchemaAll all;  
        private XmlSchemaChoice choice;  
        private XmlSchemaSequence sequence;
        private XmlSchemaParticle particle;
        private XmlSchemaAttributeGroup attributeGroup;
        private XmlSchemaAttributeGroupRef attributeGroupRef;
        private XmlSchemaNotation notation; 
        private XmlSchemaIdentityConstraint identityConstraint; 
        private XmlSchemaXPath xpath;
        private XmlSchemaInclude include;
        private XmlSchemaImport import;
        private XmlSchemaAnnotation annotation;
        private XmlSchemaAppInfo appInfo;  
        private XmlSchemaDocumentation documentation;
        private XmlSchemaFacet facet;
        private XmlNode[] markup;
        private XmlSchemaRedefine redefine;

        private ValidationEventHandler validationEventHandler;
        private ArrayList unhandledAttributes = new ArrayList();
        private Hashtable namespaces;

        internal XsdBuilder( 
                           XmlReader reader,
                           XmlNamespaceManager curmgr, 
                           XmlSchema schema, 
                           XmlNameTable nameTable,
                           SchemaNames schemaNames,
                           ValidationEventHandler eventhandler
                           ) {
            this.reader = reader;
            this.xso = this.schema = schema;
            this.namespaceManager = new BuilderNamespaceManager(curmgr, reader);
            this.validationEventHandler = eventhandler;
            this.nameTable = nameTable;
            this.schemaNames = schemaNames;
            this.stateHistory = new HWStack(STACK_INCREMENT);
            this.currentEntry = SchemaEntries[0];
            positionInfo = PositionInfo.GetPositionInfo(reader);
        }

        internal override bool ProcessElement(string prefix, string name, string ns) {
            XmlQualifiedName qname = new XmlQualifiedName(name, ns);
            if (GetNextState(qname)) {
                Push();
                Debug.Assert(this.currentEntry.InitFunc != null);
                xso = null;
                this.currentEntry.InitFunc(this, null);
                Debug.Assert(xso != null);
                RecordPosition();
            }
            else {
                if (!IsSkipableElement(qname)) {
                    SendValidationEvent(Res.Sch_UnsupportedElement, qname.ToString());
                }
                return false;
            }
            return true;
        }

        internal override void ProcessAttribute(string prefix, string name, string ns, string value) {
            XmlQualifiedName qname = new XmlQualifiedName(name, ns);
            if (this.currentEntry.Attributes != null) {
                for (int i = 0; i < this.currentEntry.Attributes.Length; i++) {
                    XsdAttributeEntry a = this.currentEntry.Attributes[i];
                    if (this.schemaNames.TokenToQName[(int)a.Attribute].Equals(qname)) {
                        try {
                            a.BuildFunc(this, value);
                        } 
                        catch (XmlSchemaException e) {
                            e.SetSource(this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition);
                            SendValidationEvent(Res.Sch_InvalidXsdAttributeDatatypeValue, new string[] {name, e.Message},XmlSeverityType.Error);
                        }
                        return;
                    }
                }
            }

            // Check non-supported attribute
            if ((ns != this.schemaNames.NsXs) && (ns.Length != 0)) {
                if (ns == this.schemaNames.NsXmlNs) {
                    if (this.namespaces == null) {
                        this.namespaces = new Hashtable();
                    }
                    this.namespaces.Add((name == this.schemaNames.QnXmlNs.Name) ? string.Empty : name, value);
                }
                else {
                    XmlAttribute attribute = new XmlAttribute(prefix, name, ns, this.schema.Document);
                    attribute.Value = value;
                    this.unhandledAttributes.Add(attribute);
                }
            } 
            else {
                SendValidationEvent(Res.Sch_UnsupportedAttribute, qname.ToString());
            }
        }

        internal override bool IsContentParsed() {
            return this.currentEntry.ParseContent;
        }

        internal override void ProcessMarkup(XmlNode[] markup) {
            this.markup = markup;
        }

        internal override void ProcessCData(string value) {
            SendValidationEvent(Res.Sch_TextNotAllowed, value);
        }

        internal override void StartChildren() {
            if (this.xso != null ) {
                if (this.namespaces != null && this.namespaces.Count > 0) {
                    this.xso.Namespaces.Namespaces = this.namespaces;
                    this.namespaces = null;
                }
                if (this.unhandledAttributes.Count != 0) {
                    this.xso.SetUnhandledAttributes((XmlAttribute[])this.unhandledAttributes.ToArray(typeof(System.Xml.XmlAttribute)));
                    this.unhandledAttributes.Clear();
                }
            }
        }

        internal override void EndChildren() {
            if (this.currentEntry.EndChildFunc != null) {
                (this.currentEntry.EndChildFunc)(this);
            }
            Pop();
        }


        // State stack push & pop
        private void Push() {
            this.stateHistory.Push();
            this.stateHistory[this.stateHistory.Length - 1] = this.currentEntry;
            containerStack.Push(GetContainer(this.currentEntry.CurrentState));
            this.currentEntry = this.nextEntry;
            if (this.currentEntry.Name != SchemaNames.Token.XsdAnnotation) {
                this.hasChild = false;
            }
        }

        private void Pop() {
            this.currentEntry = (XsdEntry)this.stateHistory.Pop();
            SetContainer(this.currentEntry.CurrentState, containerStack.Pop());
            this.hasChild = true;
        }

        private SchemaNames.Token CurrentElement {
            get { return this.currentEntry.Name;}
        }

        private SchemaNames.Token ParentElement {
            get { return((XsdEntry)this.stateHistory[this.stateHistory.Length - 1]).Name;}
        }

        private XmlSchemaObject ParentContainer {
            get { return (XmlSchemaObject)containerStack.Peek(); }
        }

        private XmlSchemaObject GetContainer(State state) {
            XmlSchemaObject container = null;
            switch (state) {
                case State.Root:
                    break;
                case State.Schema:
                    container = this.schema;
                    break;
                case State.Annotation:
                    container = this.annotation;
                    break;
                case State.Include:
                    container = this.include;
                    break;
                case State.Import:
                    container = this.import;
                    break;
                case State.Element:
                    container = this.element;
                    break;
                case State.Attribute:
                    container = this.attribute;
                    break;
                case State.AttributeGroup:
                    container = this.attributeGroup;
                    break;
                case State.AttributeGroupRef:
                    container = this.attributeGroupRef;
                    break;
                case State.AnyAttribute:
                    container = this.anyAttribute;
                    break;
                case State.Group:
                    container = this.group;
                    break;
                case State.GroupRef:
                    container = this.groupRef;
                    break;
                case State.All:
                    container = this.all;
                    break;
                case State.Choice:
                    container = this.choice;
                    break;
                case State.Sequence:
                    container = this.sequence;
                    break;
                case State.Any:
                    container = this.anyElement;
                    break;
                case State.Notation:
                    container = this.notation;
                    break;
                case State.SimpleType:
                    container = this.simpleType;
                    break;
                case State.ComplexType:
                    container = this.complexType;
                    break;
                case State.ComplexContent:
                    container = this.complexContent;
                    break;
                case State.ComplexContentExtension:
                    container = this.complexContentExtension;
                    break;
                case State.ComplexContentRestriction:
                    container = this.complexContentRestriction;
                    break;
                case State.SimpleContent:
                    container = this.simpleContent;
                    break;
                case State.SimpleContentExtension:
                    container = this.simpleContentExtension;
                    break;
                case State.SimpleContentRestriction:
                    container = this.simpleContentRestriction;
                    break;
                case State.SimpleTypeUnion:
                    container = this.simpleTypeUnion;
                    break;
                case State.SimpleTypeList:
                    container = this.simpleTypeList;
                    break;
                case State.SimpleTypeRestriction:
                    container = this.simpleTypeRestriction;
                    break;
                case State.Unique:
                case State.Key:
                case State.KeyRef:
                    container = this.identityConstraint;
                    break;
                case State.Selector:
                case State.Field:
                    container = this.xpath;
                    break;
                case State.MinExclusive:
                case State.MinInclusive:
                case State.MaxExclusive:
                case State.MaxInclusive:
                case State.TotalDigits:
                case State.FractionDigits:
                case State.Length:
                case State.MinLength:
                case State.MaxLength:
                case State.Enumeration:
                case State.Pattern:
                case State.WhiteSpace:
                    container = this.facet;
                    break;
                case State.AppInfo:
                    container = this.appInfo;
                    break;
                case State.Documentation:
                    container = this.documentation;
                    break;
                case State.Redefine:
                    container = this.redefine;
                    break;
                default:
                    Debug.Assert(false, "State is " + state);
                    break;
            }
            return container;
        }

        private void SetContainer(State state, object container) {
            switch (state) {
                case State.Root:
                    break;
                case State.Schema:
                    break;
                case State.Annotation:
                    this.annotation = (XmlSchemaAnnotation)container;
                    break;
                case State.Include:
                    this.include = (XmlSchemaInclude)container;
                    break;
                case State.Import:
                    this.import = (XmlSchemaImport)container;
                    break;
                case State.Element:
                    this.element = (XmlSchemaElement)container;
                    break;
                case State.Attribute:
                    this.attribute = (XmlSchemaAttribute)container;
                    break;
                case State.AttributeGroup:
                    this.attributeGroup = (XmlSchemaAttributeGroup)container;
                    break;
                case State.AttributeGroupRef:
                    this.attributeGroupRef = (XmlSchemaAttributeGroupRef)container;
                    break;
                case State.AnyAttribute:
                    this.anyAttribute = (XmlSchemaAnyAttribute)container;
                    break;
                case State.Group:
                    this.group = (XmlSchemaGroup)container;
                    break;
                case State.GroupRef:
                    this.groupRef = (XmlSchemaGroupRef)container;
                    break;
                case State.All:
                    this.all = (XmlSchemaAll)container;
                    break;
                case State.Choice:
                    this.choice = (XmlSchemaChoice)container;
                    break;
                case State.Sequence:
                    this.sequence = (XmlSchemaSequence)container;
                    break;
                case State.Any:
                    this.anyElement = (XmlSchemaAny)container;
                    break;
                case State.Notation:
                    this.notation = (XmlSchemaNotation)container;
                    break;
                case State.SimpleType:
                    this.simpleType = (XmlSchemaSimpleType)container;
                    break;
                case State.ComplexType:
                    this.complexType = (XmlSchemaComplexType)container;
                    break;
                case State.ComplexContent:
                    this.complexContent = (XmlSchemaComplexContent)container;
                    break;
                case State.ComplexContentExtension:
                    this.complexContentExtension = (XmlSchemaComplexContentExtension)container;
                    break;
                case State.ComplexContentRestriction:
                    this.complexContentRestriction = (XmlSchemaComplexContentRestriction)container;
                    break;
                case State.SimpleContent:
                    this.simpleContent = (XmlSchemaSimpleContent)container;
                    break;
                case State.SimpleContentExtension:
                    this.simpleContentExtension = (XmlSchemaSimpleContentExtension)container;
                    break;
                case State.SimpleContentRestriction:
                    this.simpleContentRestriction = (XmlSchemaSimpleContentRestriction)container;
                    break;
                case State.SimpleTypeUnion:
                    this.simpleTypeUnion = (XmlSchemaSimpleTypeUnion)container;
                    break;
                case State.SimpleTypeList:
                    this.simpleTypeList = (XmlSchemaSimpleTypeList)container;
                    break;
                case State.SimpleTypeRestriction:
                    this.simpleTypeRestriction = (XmlSchemaSimpleTypeRestriction)container;
                    break;
                case State.Unique:
                case State.Key:
                case State.KeyRef:
                    this.identityConstraint = (XmlSchemaIdentityConstraint)container;
                    break;
                case State.Selector:
                case State.Field:
                    this.xpath = (XmlSchemaXPath)container;
                    break;
                case State.MinExclusive:
                case State.MinInclusive:
                case State.MaxExclusive:
                case State.MaxInclusive:
                case State.TotalDigits:
                case State.FractionDigits:
                case State.Length:
                case State.MinLength:
                case State.MaxLength:
                case State.Enumeration:
                case State.Pattern:
                case State.WhiteSpace:
                    this.facet = (XmlSchemaFacet)container;
                    break;
                case State.AppInfo:
                    this.appInfo = (XmlSchemaAppInfo)container;
                    break;
                case State.Documentation:
                    this.documentation = (XmlSchemaDocumentation)container;
                    break;
                case State.Redefine:
                    this.redefine = (XmlSchemaRedefine)container;
                    break;
                default:
                    Debug.Assert(false, "State is " + state);
                    break;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // XSD Schema
        //

        private static void BuildAnnotated_Id(XsdBuilder builder, string value) {
            builder.xso.IdAttribute = value;
        }

        /*
            <schema 
              attributeFormDefault = qualified | unqualified : unqualified
              blockDefault = #all or (possibly empty) subset of {substitution, extension, restriction} 
              elementFormDefault = qualified | unqualified : unqualified
              finalDefault = #all or (possibly empty) subset of {extension, restriction} 
              id = ID 
              targetNamespace = uriReference 
              version = string 
              {any attributes with non-schema namespace . . .}>
              Content: ((include | import | redefine | annotation)* , ((attribute | attributeGroup | complexType | element | group | notation | simpleType) , annotation*)*)
            </schema>
        */
        
        private static void BuildSchema_AttributeFormDefault(XsdBuilder builder, string value) {
            builder.schema.AttributeFormDefault = (XmlSchemaForm)builder.ParseEnum(value, "attributeFormDefault", FormStringValues);
        }

        private static void BuildSchema_ElementFormDefault(XsdBuilder builder, string value) {
            builder.schema.ElementFormDefault = (XmlSchemaForm)builder.ParseEnum(value, "elementFormDefault", FormStringValues);
        }
 
        private static void BuildSchema_TargetNamespace(XsdBuilder builder, string value) {
            builder.schema.TargetNamespace = value;    
        }

        private static void BuildSchema_Version(XsdBuilder builder, string value) {
            builder.schema.Version = value;
        }

        private static void BuildSchema_FinalDefault(XsdBuilder builder, string value) {
            builder.schema.FinalDefault = (XmlSchemaDerivationMethod)builder.ParseBlockFinalEnum(value, "finalDefault");
        }

        private static void BuildSchema_BlockDefault(XsdBuilder builder, string value) {
            builder.schema.BlockDefault = (XmlSchemaDerivationMethod)builder.ParseBlockFinalEnum(value, "blockDefault");
        }

        private static void InitSchema(XsdBuilder builder, string value) {
            builder.canIncludeImport = true;
            builder.xso = builder.schema;
        }

        /*
            <include 
              id = ID 
              schemaLocation = uriReference 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation?)
            </include>
        */
        private static void InitInclude(XsdBuilder builder, string value) {
            if (!builder.canIncludeImport) {
                builder.SendValidationEvent(Res.Sch_IncludeLocation, null);        
            }
            builder.xso = builder.include = new XmlSchemaInclude();
            builder.schema.Includes.Add(builder.include);
        }

        private static void BuildInclude_SchemaLocation(XsdBuilder builder, string value) {
            builder.include.SchemaLocation = value;
        }

        /*
            <import 
              id = ID 
              namespace = uriReference 
              schemaLocation = uriReference 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation?)
            </import>
        */
        private static void InitImport(XsdBuilder builder, string value) {
            if (!builder.canIncludeImport) {
                builder.SendValidationEvent(Res.Sch_ImportLocation, null);            
            }
            builder.xso = builder.import = new XmlSchemaImport();
            builder.schema.Includes.Add(builder.import);
        }

        private static void BuildImport_Namespace(XsdBuilder builder, string value) {
            builder.import.Namespace = value;
        }

        private static void BuildImport_SchemaLocation(XsdBuilder builder, string value) {
            builder.import.SchemaLocation = value;
        }

        /*
            <redefine 
              schemaLocation = uriReference 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation | (attributeGroup | complexType | group | simpleType))*
            </redefine>
        */
        private static void InitRedefine(XsdBuilder builder, string value) {
            if (!builder.canIncludeImport) {
                builder.SendValidationEvent(Res.Sch_RedefineLocation, null);            
            }
            builder.xso = builder.redefine = new XmlSchemaRedefine();
            builder.schema.Includes.Add(builder.redefine);
        }

        private static void BuildRedefine_SchemaLocation(XsdBuilder builder, string value) {
            builder.redefine.SchemaLocation = value;
        }

        private static void EndRedefine(XsdBuilder builder) {    
            builder.canIncludeImport = true;
        }

        /*
            <attribute 
              form = qualified | unqualified 
              id = ID 
              name = NCName 
              ref = QName 
              type = QName 
              use = prohibited | optional | required | default | fixed : optional
              value = string 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (simpleType?))
            </attribute>
        */
        private static void InitAttribute(XsdBuilder builder, string value) {
            builder.xso = builder.attribute = new XmlSchemaAttribute();
            if (builder.ParentElement == SchemaNames.Token.XsdSchema)
                builder.schema.Items.Add(builder.attribute);
            else 
                builder.AddAttribute(builder.attribute);
            builder.canIncludeImport = false;  // disable import and include elements in schema
        }

        private static void BuildAttribute_Default(XsdBuilder builder, string value) {
            builder.attribute.DefaultValue = value;
        }

        private static void BuildAttribute_Fixed(XsdBuilder builder, string value) {
            builder.attribute.FixedValue = value;
        }

        private static void BuildAttribute_Form(XsdBuilder builder, string value) {
            builder.attribute.Form = (XmlSchemaForm)builder.ParseEnum(value, "form", FormStringValues);
        }

        private static void BuildAttribute_Use(XsdBuilder builder, string value) {
            builder.attribute.Use = (XmlSchemaUse)builder.ParseEnum(value, "use", UseStringValues);
        }

        private static void BuildAttribute_Ref(XsdBuilder builder, string value) {
            builder.attribute.RefName = builder.ParseQName(value, "ref");
        }

        private static void BuildAttribute_Name(XsdBuilder builder, string value) {
            builder.attribute.Name = value;
        }

        private static void BuildAttribute_Type(XsdBuilder builder, string value) {
            builder.attribute.SchemaTypeName = builder.ParseQName(value, "type");
        }

        /*
            <element 
              abstract = boolean : false
              block = #all or (possibly empty) subset of {substitution, extension, restriction} 
              default = string 
              final = #all or (possibly empty) subset of {extension, restriction} 
              fixed = string 
              form = qualified | unqualified 
              id = ID 
              maxOccurs = for maxOccurs : 1
              minOccurs = nonNegativeInteger : 1
              name = NCName 
              nillable = boolean : false
              ref = QName 
              substitutionGroup = QName 
              type = QName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , ((simpleType | complexType)? , (key | keyref | unique)*))
            </element>
        */    
        private static void InitElement(XsdBuilder builder, string value) {
            builder.xso = builder.element = new XmlSchemaElement();
            builder.canIncludeImport = false;
            switch (builder.ParentElement) {
                case SchemaNames.Token.XsdSchema:
                    builder.schema.Items.Add(builder.element);
                    break;
                case SchemaNames.Token.XsdAll:
                    builder.all.Items.Add(builder.element);
                    break;
                case SchemaNames.Token.XsdChoice:
                    builder.choice.Items.Add(builder.element);
                    break;
                case SchemaNames.Token.XsdSequence:
                    builder.sequence.Items.Add(builder.element);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private static void BuildElement_Abstract(XsdBuilder builder, string value) {
            builder.element.IsAbstract = builder.ParseBoolean(value, "abstract");
        }

        private static void BuildElement_Block(XsdBuilder builder, string value) {
            builder.element.Block = (XmlSchemaDerivationMethod)builder.ParseBlockFinalEnum(value, "block");
        }

        private static void BuildElement_Default(XsdBuilder builder, string value) {
            builder.element.DefaultValue = value;
        }

        private static void BuildElement_Form(XsdBuilder builder, string value) {
            builder.element.Form = (XmlSchemaForm)builder.ParseEnum(value, "form", FormStringValues);
        }

        private static void BuildElement_SubstitutionGroup(XsdBuilder builder, string value) {
            builder.element.SubstitutionGroup = builder.ParseQName(value, "substitutionGroup");
        }

        private static void BuildElement_Final(XsdBuilder builder, string value) {
            builder.element.Final = (XmlSchemaDerivationMethod)builder.ParseBlockFinalEnum(value, "final");
        }

        private static void BuildElement_Fixed(XsdBuilder builder, string value) {
            builder.element.FixedValue = value;
        }

        private static void BuildElement_MaxOccurs(XsdBuilder builder, string value) {
            builder.SetMaxOccurs(builder.element, value);
        }

        private static void BuildElement_MinOccurs(XsdBuilder builder, string value) {
            builder.SetMinOccurs(builder.element, value);
        }

        private static void BuildElement_Name(XsdBuilder builder, string value) {
            builder.element.Name = value;
        }

        private static void BuildElement_Nillable(XsdBuilder builder, string value) {
            builder.element.IsNillable = builder.ParseBoolean(value, "nillable");
        }

        private static void BuildElement_Ref(XsdBuilder builder, string value) {
            builder.element.RefName = builder.ParseQName(value, "ref");
        }

        private static void BuildElement_Type(XsdBuilder builder, string value) {
            builder.element.SchemaTypeName = builder.ParseQName(value, "type");
        }

        /*
            <simpleType 
              id = ID 
              name = NCName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , ((list | restriction | union)))
            </simpleType>
        */
        private static void InitSimpleType(XsdBuilder builder, string value) {
            builder.xso = builder.simpleType = new XmlSchemaSimpleType();
            switch (builder.ParentElement) {
                case SchemaNames.Token.XsdSchema:
                    builder.canIncludeImport = false;  // disable import and include elements in schema
                    builder.schema.Items.Add(builder.simpleType);                    
                    break;
                case SchemaNames.Token.XsdRedefine:
                    builder.redefine.Items.Add(builder.simpleType);                    
                    break;
                case SchemaNames.Token.XsdAttribute:
                    if (builder.attribute.SchemaType != null) {
                        builder.SendValidationEvent(Res.Sch_DupXsdElement, "simpleType");
                    }
                    builder.attribute.SchemaType = builder.simpleType;
                    break;
                case SchemaNames.Token.XsdElement:
                    if (builder.element.SchemaType != null) {
                        builder.SendValidationEvent(Res.Sch_DupXsdElement, "simpleType");
                    }
                    if (builder.element.Constraints.Count != 0) {
                        builder.SendValidationEvent(Res.Sch_TypeAfterConstraints, null);
                    }
                    builder.element.SchemaType = builder.simpleType;
                    break;
                case SchemaNames.Token.XsdSimpleTypeList:
                    if (builder.simpleTypeList.ItemType != null) {
                        builder.SendValidationEvent(Res.Sch_DupXsdElement, "simpleType");
                    }
                    builder.simpleTypeList.ItemType = builder.simpleType;
                    break;
                case SchemaNames.Token.XsdSimpleTypeRestriction:
                    if (builder.simpleTypeRestriction.BaseType != null) {
                        builder.SendValidationEvent(Res.Sch_DupXsdElement, "simpleType");
                    }
                    builder.simpleTypeRestriction.BaseType = builder.simpleType;
                    break;
                case SchemaNames.Token.XsdSimpleContentRestriction:
                    if (builder.simpleContentRestriction.BaseType != null) {
                        builder.SendValidationEvent(Res.Sch_DupXsdElement, "simpleType");
                    }
                    if (
                        builder.simpleContentRestriction.Attributes.Count != 0 || 
                        builder.simpleContentRestriction.AnyAttribute != null || 
                        builder.simpleContentRestriction.Facets.Count != 0
                    ) {
                        builder.SendValidationEvent(Res.Sch_SimpleTypeRestriction, null);
                    }
                    builder.simpleContentRestriction.BaseType = builder.simpleType;
                    break;

                case SchemaNames.Token.XsdSimpleTypeUnion:
                    builder.simpleTypeUnion.BaseTypes.Add(builder.simpleType);  
                    break;
            }
        }

        private static void BuildSimpleType_Name(XsdBuilder builder, string value) {
            builder.simpleType.Name =value;
        }

        private static void BuildSimpleType_Final(XsdBuilder builder, string value) {
            builder.simpleType.Final = (XmlSchemaDerivationMethod)builder.ParseBlockFinalEnum(value, "final");
        }


        /*
            <union 
              id = ID 
              memberTypes = List of [anon]
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (simpleType*))
            </union>
        */
        private static void InitSimpleTypeUnion(XsdBuilder builder, string value) {
            if (builder.simpleType.Content != null) {
                builder.SendValidationEvent(Res.Sch_DupSimpleTypeChild, null);
            }    
            builder.xso = builder.simpleTypeUnion = new XmlSchemaSimpleTypeUnion();
            builder.simpleType.Content = builder.simpleTypeUnion;
        }

        private static void BuildSimpleTypeUnion_MemberTypes(XsdBuilder builder, string value) {
            XmlSchemaDatatype dt = XmlSchemaDatatype.FromXmlTokenizedTypeXsd(XmlTokenizedType.QName).DeriveByList(null);
            try {
                builder.simpleTypeUnion.MemberTypes = (XmlQualifiedName[])dt.ParseValue(value, builder.nameTable, builder.namespaceManager);
            } 
            catch (XmlSchemaException e) {
                e.SetSource(builder.reader.BaseURI, builder.positionInfo.LineNumber, builder.positionInfo.LinePosition);
                builder.SendValidationEvent(e);
            }
        }


        /*
            <list 
              id = ID 
              itemType = QName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (simpleType?))
            </list>
        */
        private static void InitSimpleTypeList(XsdBuilder builder, string value) {   
            if (builder.simpleType.Content != null) {
                builder.SendValidationEvent(Res.Sch_DupSimpleTypeChild, null);
            }
            builder.xso = builder.simpleTypeList = new XmlSchemaSimpleTypeList();
            builder.simpleType.Content = builder.simpleTypeList;
        }

        private static void BuildSimpleTypeList_ItemType(XsdBuilder builder, string value) {
            builder.simpleTypeList.ItemTypeName = builder.ParseQName(value, "itemType");
        }

        /*
            <restriction 
              base = QName 
              id = ID 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (simpleType? , ((duration | encoding | enumeration | length | maxExclusive | maxInclusive | maxLength | minExclusive | minInclusive | minLength | pattern | period | TotalDigits | FractionDigits)*)))
            </restriction>
        */
        private static void InitSimpleTypeRestriction(XsdBuilder builder, string value) {
            if (builder.simpleType.Content != null) {
                builder.SendValidationEvent(Res.Sch_DupSimpleTypeChild, null);
            }
            builder.xso = builder.simpleTypeRestriction = new XmlSchemaSimpleTypeRestriction();
            builder.simpleType.Content = builder.simpleTypeRestriction;
        }

        private static void BuildSimpleTypeRestriction_Base(XsdBuilder builder, string value) {
            builder.simpleTypeRestriction.BaseTypeName = builder.ParseQName(value, "base");
        }

        /*
            <complexType 
              abstract = boolean : false
              block = #all or (possibly empty) subset of {extension, restriction} 
              final = #all or (possibly empty) subset of {extension, restriction} 
              id = ID 
              mixed = boolean : false
              name = NCName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (simpleContent | complexContent | ((group | all | choice | sequence)? , ((attribute | attributeGroup)* , anyAttribute?))))
            </complexType>
        */
        private static void InitComplexType(XsdBuilder builder, string value) {
            builder.xso = builder.complexType = new XmlSchemaComplexType();
            switch (builder.ParentElement) {
                case SchemaNames.Token.XsdSchema:
                    builder.canIncludeImport = false;  // disable import and include elements in schema
                    builder.schema.Items.Add(builder.complexType);                    
                    break;
                case SchemaNames.Token.XsdRedefine:
                    builder.redefine.Items.Add(builder.complexType);                    
                    break;
                case SchemaNames.Token.XsdElement:
                    if (builder.element.SchemaType != null) {
                        builder.SendValidationEvent(Res.Sch_DupElement, "complexType");
                    }
                    if (builder.element.Constraints.Count != 0) {
                        builder.SendValidationEvent(Res.Sch_TypeAfterConstraints, null);
                    }
                    builder.element.SchemaType = builder.complexType;
                    break;
            }
        }

        private static void BuildComplexType_Abstract(XsdBuilder builder, string value) {
            builder.complexType.IsAbstract = builder.ParseBoolean(value, "abstract");
        }

        private static void BuildComplexType_Block(XsdBuilder builder, string value) {
            builder.complexType.Block = (XmlSchemaDerivationMethod)builder.ParseBlockFinalEnum(value, "block");
        }

        private static void BuildComplexType_Final(XsdBuilder builder, string value) {
            builder.complexType.Final = (XmlSchemaDerivationMethod)builder.ParseBlockFinalEnum(value, "final");
        }

        private static void BuildComplexType_Mixed(XsdBuilder builder, string value) {
            builder.complexType.IsMixed = builder.ParseBoolean(value, "mixed");
        }

        private static void BuildComplexType_Name(XsdBuilder builder, string value) {
            builder.complexType.Name = value;
        }

        /*
            <complexContent 
              id = ID 
              mixed = boolean 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (restriction | extension))
            </complexContent>
        */
        private static void InitComplexContent(XsdBuilder builder, string value) {
            if ( (builder.complexType.ContentModel != null) ||
                 (builder.complexType.Particle != null || builder.complexType.Attributes.Count != 0 || builder.complexType.AnyAttribute != null)
               ) {
                 builder.SendValidationEvent(Res.Sch_ComplexTypeContentModel, "complexContent");
            }
            builder.xso = builder.complexContent = new XmlSchemaComplexContent();
            builder.complexType.ContentModel = builder.complexContent;
        }

        private static void BuildComplexContent_Mixed(XsdBuilder builder, string value) {
            builder.complexContent.IsMixed = builder.ParseBoolean(value, "mixed");
        }

        /*
            <extension 
              base = QName 
              id = ID 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , ((group | all | choice | sequence)? , ((attribute | attributeGroup)* , anyAttribute?)))
            </extension>
        */
        private static void InitComplexContentExtension(XsdBuilder builder, string value) {
            if (builder.complexContent.Content != null) {
                builder.SendValidationEvent(Res.Sch_ComplexContentContentModel, "extension");
            }
            builder.xso = builder.complexContentExtension = new XmlSchemaComplexContentExtension();
            builder.complexContent.Content = builder.complexContentExtension;
        }

        private static void BuildComplexContentExtension_Base(XsdBuilder builder, string value) {
            builder.complexContentExtension.BaseTypeName = builder.ParseQName(value, "base");
        }

        /*
            <restriction 
              base = QName 
              id = ID 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (group | all | choice | sequence)? , ((attribute | attributeGroup)* , anyAttribute?))
            </restriction>
        */
        private static void InitComplexContentRestriction(XsdBuilder builder, string value) {
            builder.xso = builder.complexContentRestriction = new XmlSchemaComplexContentRestriction();
            builder.complexContent.Content = builder.complexContentRestriction;
        }

        private static void BuildComplexContentRestriction_Base(XsdBuilder builder, string value) {
            builder.complexContentRestriction.BaseTypeName = builder.ParseQName(value, "base");
        }

        /*
            <simpleContent 
              id = ID 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (restriction | extension))
            </simpleContent>
        */
        private static void InitSimpleContent(XsdBuilder builder, string value) {
            if ( (builder.complexType.ContentModel != null) || 
                 (builder.complexType.Particle != null || builder.complexType.Attributes.Count != 0 || builder.complexType.AnyAttribute != null)
                 ) {
                   builder.SendValidationEvent(Res.Sch_ComplexTypeContentModel, "simpleContent");
            }
            builder.xso = builder.simpleContent = new XmlSchemaSimpleContent();
            builder.complexType.ContentModel = builder.simpleContent;
        }

        /*
            <extension 
              base = QName 
              id = ID 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , ((attribute | attributeGroup)* , anyAttribute?))
            </extension>
        */

        private static void InitSimpleContentExtension(XsdBuilder builder, string value) {
            if (builder.simpleContent.Content != null) {
                builder.SendValidationEvent(Res.Sch_DupElement, "extension");
            }
            builder.xso = builder.simpleContentExtension = new XmlSchemaSimpleContentExtension();
            builder.simpleContent.Content = builder.simpleContentExtension;
        }

        private static void BuildSimpleContentExtension_Base(XsdBuilder builder, string value) {
            builder.simpleContentExtension.BaseTypeName = builder.ParseQName(value, "base");
        }


        /*
            <restriction 
              base = QName 
              id = ID 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , ((duration | encoding | enumeration | length | maxExclusive | maxInclusive | maxLength | minExclusive | minInclusive | minLength | pattern | period | totalDigits | fractionDigits)*)? , ((attribute | attributeGroup)* , anyAttribute?))
            </restriction>
        */
        private static void InitSimpleContentRestriction(XsdBuilder builder, string value) {
            if (builder.simpleContent.Content != null) {
                builder.SendValidationEvent(Res.Sch_DupElement, "restriction");
            }
            builder.xso = builder.simpleContentRestriction = new XmlSchemaSimpleContentRestriction();
            builder.simpleContent.Content = builder.simpleContentRestriction;
        }

        private static void BuildSimpleContentRestriction_Base(XsdBuilder builder, string value) {
            builder.simpleContentRestriction.BaseTypeName = builder.ParseQName(value, "base");
        }

        /*
            <attributeGroup 
              id = ID 
              name = NCName 
              ref = QName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , ((attribute | attributeGroup)* , anyAttribute?))
            </attributeGroup>
        */
        private static void InitAttributeGroup(XsdBuilder builder, string value) {
            builder.canIncludeImport = false;
            builder.xso = builder.attributeGroup = new XmlSchemaAttributeGroup();
            switch (builder.ParentElement) {
                case SchemaNames.Token.XsdSchema:
                    builder.schema.Items.Add(builder.attributeGroup);
                    break;
                case SchemaNames.Token.XsdRedefine:
                    builder.redefine.Items.Add(builder.attributeGroup);                    
                    break;
            }
        }

        private static void BuildAttributeGroup_Name(XsdBuilder builder, string value) {
            builder.attributeGroup.Name = value;
        }

        /*
            <attributeGroup 
              id = ID 
              ref = QName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation?)
            </attributeGroup>
        */
        private static void InitAttributeGroupRef(XsdBuilder builder, string value) {
            builder.xso = builder.attributeGroupRef = new XmlSchemaAttributeGroupRef();
            builder.AddAttribute(builder.attributeGroupRef);
        }

        private static void BuildAttributeGroupRef_Ref(XsdBuilder builder, string value) {
            builder.attributeGroupRef.RefName = builder.ParseQName(value, "ref");
        }

        /*
            <anyAttribute 
              id = ID 
              namespace = ##any | ##other | list of {uri, ##targetNamespace, ##local} : ##any
              processContents = skip | lax | strict : strict
              {any attributes with non-schema namespace . . .}>
              Content: (annotation?)
            </anyAttribute>
        */
        private static void InitAnyAttribute(XsdBuilder builder, string value) {
            builder.xso = builder.anyAttribute = new XmlSchemaAnyAttribute();
            switch (builder.ParentElement) {
                case SchemaNames.Token.XsdComplexType:
                    if (builder.complexType.ContentModel != null) {
                        builder.SendValidationEvent(Res.Sch_AttributeMutuallyExclusive, "anyAttribute");
                    }
                    if (builder.complexType.AnyAttribute != null) {
                        builder.SendValidationEvent(Res.Sch_DupElement, "anyAttribute");
                    }
                    builder.complexType.AnyAttribute = builder.anyAttribute;
                    break;
                case SchemaNames.Token.XsdSimpleContentRestriction:
                    if (builder.simpleContentRestriction.AnyAttribute != null) {
                        builder.SendValidationEvent(Res.Sch_DupElement, "anyAttribute");
                    }
                    builder.simpleContentRestriction.AnyAttribute = builder.anyAttribute;
                    break;
                case SchemaNames.Token.XsdSimpleContentExtension:
                    if (builder.simpleContentExtension.AnyAttribute != null) {
                        builder.SendValidationEvent(Res.Sch_DupElement, "anyAttribute");
                    }
                    builder.simpleContentExtension.AnyAttribute = builder.anyAttribute;
                    break;
                case SchemaNames.Token.XsdComplexContentExtension:
                    if (builder.complexContentExtension.AnyAttribute != null) {
                        builder.SendValidationEvent(Res.Sch_DupElement, "anyAttribute");
                    }
                    builder.complexContentExtension.AnyAttribute = builder.anyAttribute;
                    break;
                case SchemaNames.Token.XsdComplexContentRestriction:
                    if (builder.complexContentRestriction.AnyAttribute != null) {
                        builder.SendValidationEvent(Res.Sch_DupElement, "anyAttribute");
                    }
                    builder.complexContentRestriction.AnyAttribute = builder.anyAttribute;
                    break;
                case SchemaNames.Token.xsdAttributeGroup:
                    if (builder.attributeGroup.AnyAttribute != null) {
                        builder.SendValidationEvent(Res.Sch_DupElement, "anyAttribute");
                    }
                    builder.attributeGroup.AnyAttribute = builder.anyAttribute;
                    break;
            }
        }

        private static void BuildAnyAttribute_Namespace(XsdBuilder builder, string value) {
            builder.anyAttribute.Namespace = value;
        }

        private static void BuildAnyAttribute_ProcessContents(XsdBuilder builder, string value) {
            builder.anyAttribute.ProcessContents = (XmlSchemaContentProcessing)builder.ParseEnum(value, "processContents", ProcessContentsStringValues);
        }

        /*
            <group 
              id = ID 
              name = NCName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (all | choice | sequence)?)
            </group>
        */
        private static void InitGroup(XsdBuilder builder, string value) {
            builder.xso = builder.group = new XmlSchemaGroup();
            builder.canIncludeImport = false;  // disable import and include elements in schema
            switch (builder.ParentElement) {
                case SchemaNames.Token.XsdSchema:
                    builder.schema.Items.Add(builder.group);
                    break;
                case SchemaNames.Token.XsdRedefine:
                    builder.redefine.Items.Add(builder.group);                    
                    break;
            }
        }

        private static void BuildGroup_Name(XsdBuilder builder, string value) {
            builder.group.Name = value;
        }

        /*
            <group 
              id = ID 
              maxOccurs = for maxOccurs : 1
              minOccurs = nonNegativeInteger : 1
              ref = QName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation?)
            </group>
        */
        private static void InitGroupRef(XsdBuilder builder, string value) {
            builder.xso = builder.particle = builder.groupRef = new XmlSchemaGroupRef();
            builder.AddParticle(builder.groupRef);
        }

        private static void BuildParticle_MaxOccurs(XsdBuilder builder, string value) {
            builder.SetMaxOccurs(builder.particle, value);
        }

        private static void BuildParticle_MinOccurs(XsdBuilder builder, string value) {
            builder.SetMinOccurs(builder.particle, value);
        }

        private static void BuildGroupRef_Ref(XsdBuilder builder, string value) {
            builder.groupRef.RefName = builder.ParseQName(value, "ref");
        }

        /*
            <all 
              id = ID 
              maxOccurs = for maxOccurs : 1
              minOccurs = nonNegativeInteger : 1
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , element*)
            </all>
        */
        private static void InitAll(XsdBuilder builder, string value) {
            builder.xso = builder.particle = builder.all = new XmlSchemaAll();
            builder.AddParticle(builder.all);
        }

        /*
            <choice 
              id = ID 
              maxOccurs = for maxOccurs : 1
              minOccurs = nonNegativeInteger : 1
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (element | group | choice | sequence | any)*)
            </choice>
        */
        private static void InitChoice(XsdBuilder builder, string value) {
            builder.xso = builder.particle = builder.choice = new XmlSchemaChoice();
            builder.AddParticle(builder.choice);
        }

        /*
             <sequence 
              id = ID 
              maxOccurs = for maxOccurs : 1
              minOccurs = nonNegativeInteger : 1
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (element | group | choice | sequence | any)*)
            </sequence>
        */
        private static void InitSequence(XsdBuilder builder, string value) {
            builder.xso = builder.particle = builder.sequence = new XmlSchemaSequence();
            builder.AddParticle(builder.sequence);
        }

        /*
            <any 
              id = ID 
              maxOccurs = for maxOccurs : 1
              minOccurs = nonNegativeInteger : 1
              namespace = ##any | ##other | list of {uri, ##targetNamespace, ##local} : ##any
              processContents = skip | lax | strict : strict
              {any attributes with non-schema namespace . . .}>
              Content: (annotation?)
            </any>
        */
        private static void InitAny(XsdBuilder builder, string value) {
            builder.xso = builder.particle = builder.anyElement = new XmlSchemaAny();
            builder.AddParticle(builder.anyElement);
        } 

        private static void BuildAny_Namespace(XsdBuilder builder, string value) {
            builder.anyElement.Namespace = value;
        } 

        private static void BuildAny_ProcessContents(XsdBuilder builder, string value) {
            builder.anyElement.ProcessContents = (XmlSchemaContentProcessing)builder.ParseEnum(value, "processContents", ProcessContentsStringValues);
        }

        /*
            <notation 
              id = ID 
              name = NCName 
              public = A public identifier, per ISO 8879 
              system = uriReference 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation?)
            </notation>
        */
        private static void InitNotation(XsdBuilder builder, string value) {
            builder.xso = builder.notation = new XmlSchemaNotation();
            builder.canIncludeImport = false;
            builder.schema.Items.Add(builder.notation);
        }

        private static void BuildNotation_Name(XsdBuilder builder, string value) {
            builder.notation.Name = value;
        }

        private static void BuildNotation_Public(XsdBuilder builder, string value) {
            builder.notation.Public = value;
        }

        private static void BuildNotation_System(XsdBuilder builder, string value) {
            builder.notation.System = value;
        }

        //
        // Facets
        //
        /*
            <duration 
              id = ID 
              value = timeDuration 
              fixed = boolean : false>
              Content: (annotation?)
            </duration>
        */
        private static void InitFacet(XsdBuilder builder, string value) {
            switch (builder.CurrentElement) {
                case SchemaNames.Token.XsdEnumeration:
                    builder.facet = new XmlSchemaEnumerationFacet();
                    break;
                case SchemaNames.Token.XsdLength:
                    builder.facet = new XmlSchemaLengthFacet();
                    break;
                case SchemaNames.Token.XsdMaxExclusive:
                    builder.facet = new XmlSchemaMaxExclusiveFacet();
                    break;
                case SchemaNames.Token.XsdMaxInclusive:
                    builder.facet = new XmlSchemaMaxInclusiveFacet();
                    break;
                case SchemaNames.Token.XsdMaxLength:
                    builder.facet = new XmlSchemaMaxLengthFacet();
                    break;
                case SchemaNames.Token.XsdMinExclusive:
                    builder.facet = new XmlSchemaMinExclusiveFacet();
                    break;
                case SchemaNames.Token.XsdMinInclusive:
                    builder.facet = new XmlSchemaMinInclusiveFacet();
                    break;
                case SchemaNames.Token.XsdMinLength:
                    builder.facet = new XmlSchemaMinLengthFacet();
                    break;
                case SchemaNames.Token.XsdPattern:
                    builder.facet = new XmlSchemaPatternFacet();
                    break;
                case SchemaNames.Token.XsdTotalDigits:
                    builder.facet = new XmlSchemaTotalDigitsFacet();
                    break;
                case SchemaNames.Token.XsdFractionDigits:
                    builder.facet = new XmlSchemaFractionDigitsFacet();
                    break;
                case SchemaNames.Token.XsdWhitespace:
                    builder.facet = new XmlSchemaWhiteSpaceFacet();
                    break;
            }
            builder.xso = builder.facet;
            if (SchemaNames.Token.XsdSimpleTypeRestriction == builder.ParentElement) {
                builder.simpleTypeRestriction.Facets.Add(builder.facet);
            }
            else {
                if (builder.simpleContentRestriction.Attributes.Count != 0 || (builder.simpleContentRestriction.AnyAttribute != null)) {
                    builder.SendValidationEvent(Res.Sch_InvalidFacetPosition, null);
                }
                builder.simpleContentRestriction.Facets.Add(builder.facet);
            }
        }

        private static void BuildFacet_Fixed(XsdBuilder builder, string value) {
            builder.facet.IsFixed = builder.ParseBoolean(value, "fixed");
        }

        private static void BuildFacet_Value(XsdBuilder builder, string value) {
            builder.facet.Value = value;
        }

        /*
            <unique 
              id = ID 
              name = NCName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (selector , field+))
            </unique>
 
            <key 
              id = ID 
              name = NCName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (selector , field+))
            </key>
 
            <keyref 
              id = ID 
              name = NCName 
              refer = QName 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation? , (selector , field+))
            </keyref>
        */
        private static void InitIdentityConstraint(XsdBuilder builder, string value) {
            if (!builder.element.RefName.IsEmpty) {
                builder.SendValidationEvent(Res.Sch_ElementRef, null);
            }

            switch (builder.CurrentElement) {
                case SchemaNames.Token.XsdUnique:
                    builder.xso = builder.identityConstraint = new XmlSchemaUnique();
                    break;
                case SchemaNames.Token.XsdKey:
                    builder.xso = builder.identityConstraint = new XmlSchemaKey();
                    break;
                case SchemaNames.Token.XsdKeyref:
                    builder.xso = builder.identityConstraint = new XmlSchemaKeyref();
                    break;
            }
            builder.element.Constraints.Add(builder.identityConstraint);
        }

        private static void BuildIdentityConstraint_Name(XsdBuilder builder, string value) {
            builder.identityConstraint.Name = value;
        } 

        private static void BuildIdentityConstraint_Refer(XsdBuilder builder, string value) {
            if (builder.identityConstraint is XmlSchemaKeyref) {
                ((XmlSchemaKeyref)builder.identityConstraint).Refer = builder.ParseQName(value, "refer");
            }
            else {
                builder.SendValidationEvent(Res.Sch_UnsupportedAttribute, "refer");
            }
        } 

        /*
            <selector 
              id = ID 
              xpath = An XPath expression 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation?)
            </selector>
        */
        private static void InitSelector(XsdBuilder builder, string value) {
            builder.xso = builder.xpath = new XmlSchemaXPath();
            if ( builder.identityConstraint.Selector == null ) {
                builder.identityConstraint.Selector = builder.xpath;
            }
            else {
                builder.SendValidationEvent(Res.Sch_DupSelector, builder.identityConstraint.Name);
            }
        }

        private static void BuildSelector_XPath(XsdBuilder builder, string value) {
            builder.xpath.XPath = value;
        } 

        /*
            <field 
              id = ID 
              xpath = An XPath expression 
              {any attributes with non-schema namespace . . .}>
              Content: (annotation?)
            </field>
        */
        private static void InitField(XsdBuilder builder, string value) {
            builder.xso = builder.xpath = new XmlSchemaXPath();
            // no selector before fields?
            if ( builder.identityConstraint.Selector == null ) {
                builder.SendValidationEvent(Res.Sch_SelectorBeforeFields, builder.identityConstraint.Name);
            }
            builder.identityConstraint.Fields.Add(builder.xpath);
        }

        private static void BuildField_XPath(XsdBuilder builder, string value) {
            builder.xpath.XPath = value;
        } 

        /*
            <annotation>
              Content: (appinfo | documentation)*
            </annotation>
        */
        private static void InitAnnotation(XsdBuilder builder, string value) {
            // On most elements annotations are only allowed to be the first child 
            //   (so the element must not have any children by now), and only one annotation is allowed.
            // Exceptions are xs:schema and xs:redefine, these can have any number of annotations
            //   in any place.
            if (builder.hasChild && 
                builder.ParentElement != SchemaNames.Token.XsdSchema &&
                builder.ParentElement != SchemaNames.Token.XsdRedefine) {
                builder.SendValidationEvent(Res.Sch_AnnotationLocation, null);
            }
            builder.xso = builder.annotation = new XmlSchemaAnnotation();
            builder.ParentContainer.AddAnnotation(builder.annotation);
        }

        /*
            <appinfo 
              source = uriReference>
              Content: ({any})*
            </appinfo>
        */
        private static void InitAppinfo(XsdBuilder builder, string value) {
            builder.xso = builder.appInfo = new XmlSchemaAppInfo();
            builder.annotation.Items.Add(builder.appInfo);
            builder.markup = new XmlNode[] {};
        }

        private static void BuildAppinfo_Source(XsdBuilder builder, string value) {
            builder.appInfo.Source = ParseUriReference(value);
        }

        private static void EndAppinfo(XsdBuilder builder) {
            builder.appInfo.Markup = builder.markup;
        }


        /*
            <documentation 
              source = uriReference>
              Content: ({any})*
            </documentation>
        */
        private static void InitDocumentation(XsdBuilder builder, string value) {
            builder.xso = builder.documentation = new XmlSchemaDocumentation();
            builder.annotation.Items.Add(builder.documentation);
            builder.markup = new XmlNode[] {};
        }

        private static void BuildDocumentation_Source(XsdBuilder builder, string value) {
            builder.documentation.Source = ParseUriReference(value);
        }

        private static void BuildDocumentation_XmlLang(XsdBuilder builder, string value) {
            try {
                builder.documentation.Language = value;
            } 
            catch (XmlSchemaException e) {
                e.SetSource(builder.reader.BaseURI, builder.positionInfo.LineNumber, builder.positionInfo.LinePosition);
                builder.SendValidationEvent(e);
            }
        }

        private static void EndDocumentation(XsdBuilder builder) {
            builder.documentation.Markup = builder.markup;

        }


        ///////////////////////////////////////////////////////////////////////////////////////////////
        //
        // helper functions

        private void AddAttribute(XmlSchemaObject value) {
            switch (this.ParentElement) {
                case SchemaNames.Token.XsdComplexType:
                    if (complexType.ContentModel != null) {
                        SendValidationEvent(Res.Sch_AttributeMutuallyExclusive, "attribute");
                    }
                    if (complexType.AnyAttribute != null) {
                        SendValidationEvent(Res.Sch_AnyAttributeLastChild, null);
                    }
                    this.complexType.Attributes.Add(value);
                    break;
                case SchemaNames.Token.XsdSimpleContentRestriction:
                    if (simpleContentRestriction.AnyAttribute != null) {
                        SendValidationEvent(Res.Sch_AnyAttributeLastChild, null);
                    }
                    this.simpleContentRestriction.Attributes.Add(value);
                    break;
                case SchemaNames.Token.XsdSimpleContentExtension:
                    if (simpleContentExtension.AnyAttribute != null) {
                        SendValidationEvent(Res.Sch_AnyAttributeLastChild, null);
                    }
                    this.simpleContentExtension.Attributes.Add(value);
                    break;
                case SchemaNames.Token.XsdComplexContentExtension:
                    if (complexContentExtension.AnyAttribute != null) {
                        SendValidationEvent(Res.Sch_AnyAttributeLastChild, null);
                    }
                    this.complexContentExtension.Attributes.Add(value);
                    break;
                case SchemaNames.Token.XsdComplexContentRestriction:
                    if (complexContentRestriction.AnyAttribute != null) {
                        SendValidationEvent(Res.Sch_AnyAttributeLastChild, null);
                    }
                    this.complexContentRestriction.Attributes.Add(value);
                    break;
                case SchemaNames.Token.xsdAttributeGroup:
                    if (attributeGroup.AnyAttribute != null) {
                        SendValidationEvent(Res.Sch_AnyAttributeLastChild, null);
                    }
                    this.attributeGroup.Attributes.Add(value);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void AddParticle(XmlSchemaParticle particle) {
            switch (this.ParentElement) {
                case SchemaNames.Token.XsdComplexType:
                    if ( (complexType.ContentModel != null) || 
                         (complexType.Attributes.Count != 0 || complexType.AnyAttribute != null) ||
                         (complexType.Particle != null)
                         ) {
                        SendValidationEvent(Res.Sch_ComplexTypeContentModel, "complexType");
                    }
                    complexType.Particle = particle;
                    break;
                case SchemaNames.Token.XsdComplexContentExtension:
                    if ( (complexContentExtension.Particle != null) ||
                         (complexContentExtension.Attributes.Count != 0 || complexContentExtension.AnyAttribute != null)
                       ) {
                         SendValidationEvent(Res.Sch_ComplexContentContentModel, "ComplexContentExtension");
                    }
                    complexContentExtension.Particle = particle;
                    break;
                case SchemaNames.Token.XsdComplexContentRestriction:
                    if ( (complexContentRestriction.Particle != null) ||
                         (complexContentRestriction.Attributes.Count != 0 || complexContentRestriction.AnyAttribute != null)
                       ) {
                         SendValidationEvent(Res.Sch_ComplexContentContentModel, "ComplexContentExtension");
                    }
                    complexContentRestriction.Particle = particle;
                    break;
                case SchemaNames.Token.XsdGroup:
                    if (group.Particle != null) {
                        SendValidationEvent(Res.Sch_DupGroupParticle, "particle");
                    }
                    group.Particle = (XmlSchemaGroupBase)particle;
                    break;
                case SchemaNames.Token.XsdChoice:
                case SchemaNames.Token.XsdSequence:
                    ((XmlSchemaGroupBase)this.ParentContainer).Items.Add(particle);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private bool GetNextState(XmlQualifiedName qname) {
            if (this.currentEntry.NextStates != null) {
                for (int i = 0; i < this.currentEntry.NextStates.Length; ++i) {
                    int state = (int)this.currentEntry.NextStates[i];
                    if (this.schemaNames.TokenToQName[(int)SchemaEntries[state].Name].Equals(qname)) {
                        this.nextEntry = SchemaEntries[state];
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsSkipableElement(XmlQualifiedName qname) {
            return ((CurrentElement == SchemaNames.Token.XsdDocumentation) ||
                    (CurrentElement == SchemaNames.Token.XsdAppInfo));
        }

        private void SetMinOccurs(XmlSchemaParticle particle, string value) {
            try {
                particle.MinOccursString = value;
            }
            catch(Exception) {
                SendValidationEvent(Res.Sch_MinOccursInvalidXsd, null);
            }
        }

        private void SetMaxOccurs(XmlSchemaParticle particle, string value) {
            try {
                particle.MaxOccursString = value;
            }
            catch(Exception) {
                SendValidationEvent(Res.Sch_MaxOccursInvalidXsd, null);
            }
        }

        private bool ParseBoolean(string value, string attributeName) {
            try {
                return XmlConvert.ToBoolean(value);
            }
            catch(Exception) {
                SendValidationEvent(Res.Sch_InvalidXsdAttributeValue, attributeName, value, null);
                return false;
            }
        }

        private int ParseEnum(string value, string attributeName, string[] values) {
            string s = value.Trim();
            for (int i = 0; i < values.Length; i++) {
                if (values[i] == s)
                    return i + 1;
            }
            SendValidationEvent(Res.Sch_InvalidXsdAttributeValue, attributeName, s, null);
            return 0;
        }

        private XmlQualifiedName ParseQName(string value, string attributeName) {
            try {
                string prefix;
				value = XmlComplianceUtil.NonCDataNormalize(value); //Normalize QName
                return XmlQualifiedName.Parse(value, this.namespaceManager, out prefix);
            } 
            catch(Exception) {
                SendValidationEvent(Res.Sch_InvalidXsdAttributeValue, attributeName, value, null);
                return XmlQualifiedName.Empty;
            }
        }

        private int ParseBlockFinalEnum(string value, string attributeName) {
            const int HashAllLength = 4; // Length of "#all"
            int r = 0;
            string[] stringValues = XmlConvert.SplitString(value);
            for (int i = 0; i < stringValues.Length; i++) {
                bool matched = false;
                for (int j = 0; j < DerivationMethodStrings.Length; j++) {
                    if (stringValues[i] == DerivationMethodStrings[j]) {
                        if ((r & DerivationMethodValues[j]) != 0 && (r & DerivationMethodValues[j]) != DerivationMethodValues[j]) {
                            SendValidationEvent(Res.Sch_InvalidXsdAttributeValue, attributeName, value, null);
                            return 0;
                        }
                        r |= DerivationMethodValues[j];
                        matched = true;
                        break;
                    }
                }
                if (!matched) {
                    SendValidationEvent(Res.Sch_InvalidXsdAttributeValue, attributeName, value, null);
                    return 0;
                }
                if (r == (int)XmlSchemaDerivationMethod.All && value.Length > HashAllLength) { //#all is not allowed with other values
                    SendValidationEvent(Res.Sch_InvalidXsdAttributeValue, attributeName, value, null);
                    return 0;
                }   
            }
            return r;
            
        }

        private static string ParseUriReference(string s) {
            return s;
        }

        private void SendValidationEvent(string code, string arg0, string arg1, string arg2) {
            SendValidationEvent(new XmlSchemaException(code, new string[] { arg0, arg1, arg2 }, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition));
        }

        private void SendValidationEvent(string code, string msg) {
            SendValidationEvent(new XmlSchemaException(code, msg, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition));
        }

        private void SendValidationEvent(string code, string[] args, XmlSeverityType severity) {
            SendValidationEvent(new XmlSchemaException(code, args, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition), severity);
        }

        private void SendValidationEvent(XmlSchemaException e, XmlSeverityType severity) {
            this.schema.ErrorCount++;
            e.SetSchemaObject(this.schema);
            if (validationEventHandler != null) {
                validationEventHandler(null, new ValidationEventArgs(e, severity));
            }
            else if (severity == XmlSeverityType.Error) {
                throw e;
            }
        }

        private void SendValidationEvent(XmlSchemaException e) {
            SendValidationEvent(e, XmlSeverityType.Error);
        }

        private void RecordPosition() {
            this.xso.SourceUri = this.reader.BaseURI;
            this.xso.LineNumber = this.positionInfo.LineNumber;
            this.xso.LinePosition = this.positionInfo.LinePosition;
            if (this.xso != this.schema) {
                this.xso.Parent = this.ParentContainer;
            }

        }

    }; // class XsdBuilder

} // namespace System.Xml.Schema
