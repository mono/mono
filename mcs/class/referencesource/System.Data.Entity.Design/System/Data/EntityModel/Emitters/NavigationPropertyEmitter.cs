//---------------------------------------------------------------------
// <copyright file="NavigationPropertyEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Data;
using System.Collections.Generic;
using System.Data.Entity.Design;
using Som=System.Data.EntityModel.SchemaObjectModel;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Data.Entity.Design.SsdlGenerator;
using System.Data.Entity.Design.Common;


namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// Summary description for NavigationPropertyEmitter.
    /// </summary>
    internal sealed class NavigationPropertyEmitter : PropertyEmitterBase
    {
        private const string ValuePropertyName = "Value";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="navigationProperty"></param>
        public NavigationPropertyEmitter(ClientApiGenerator generator, NavigationProperty navigationProperty, bool declaringTypeUsesStandardBaseType)
            : base(generator, navigationProperty, declaringTypeUsesStandardBaseType)
        {
        }

        /// <summary>
        /// Generate the navigation property
        /// </summary>
        /// <param name="typeDecl">The type to add the property to.</param>
        protected override void EmitProperty(CodeTypeDeclaration typeDecl)
        {
            EmitNavigationProperty(typeDecl);
        }

        /// <summary>
        /// Generate the navigation property specified 
        /// </summary>
        /// <param name="typeDecl">The type to add the property to.</param>
        private void EmitNavigationProperty( CodeTypeDeclaration typeDecl )
        {
            // create a regular property
            CodeMemberProperty property = EmitNavigationProperty(Item.ToEndMember, false);
            typeDecl.Members.Add(property);

            if (Item.ToEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many)
            {
                // create a ref property
                property = EmitNavigationProperty(Item.ToEndMember, true);
                typeDecl.Members.Add(property);

            }
        }

        /// <summary>
        /// Generate a navigation property
        /// </summary>
        /// <param name="target">the other end</param>
        /// <param name="referenceProperty">True to emit Reference navigation property</param>
        /// <returns>the generated property</returns>
        private CodeMemberProperty EmitNavigationProperty(RelationshipEndMember target, bool referenceProperty)
        {
            CodeTypeReference typeRef = GetReturnType(target, referenceProperty);

            // raise the PropertyGenerated event
            PropertyGeneratedEventArgs eventArgs = new PropertyGeneratedEventArgs(Item, 
                                                                                  null, // no backing field
                                                                                  typeRef);
            this.Generator.RaisePropertyGeneratedEvent(eventArgs);

            // [System.ComponentModel.Browsable(false)]
            // public TargetType TargetName
            // public EntityReference<TargetType> TargetName
            // or
            // public EntityCollection<targetType> TargetNames
            CodeMemberProperty property = new CodeMemberProperty();
            if (referenceProperty)
            {
                AttributeEmitter.AddBrowsableAttribute(property);
                Generator.AttributeEmitter.EmitGeneratedCodeAttribute(property);
            }
            else
            {
                Generator.AttributeEmitter.EmitNavigationPropertyAttributes(Generator, target, property, eventArgs.AdditionalAttributes);

                // Only reference navigation properties are currently currently supported with XML serialization
                // and thus we should use the XmlIgnore and SoapIgnore attributes on other property types.
                AttributeEmitter.AddIgnoreAttributes(property);                
            }
            
            AttributeEmitter.AddDataMemberAttribute(property);

            CommentEmitter.EmitSummaryComments(Item, property.Comments);

            property.Name = Item.Name;
            if (referenceProperty)
            {
                property.Name += "Reference";
                if (IsNameAlreadyAMemberName(Item.DeclaringType, property.Name, Generator.LanguageAppropriateStringComparer))
                {
                    Generator.AddError(Strings.GeneratedNavigationPropertyNameConflict(Item.Name, Item.DeclaringType.Name, property.Name),
                        ModelBuilderErrorCode.GeneratedNavigationPropertyNameConflict,
                        EdmSchemaErrorSeverity.Error, Item.DeclaringType.FullName, property.Name);
                }
            }

            if (eventArgs.ReturnType != null && !eventArgs.ReturnType.Equals(typeRef))
            {
                property.Type = eventArgs.ReturnType;
            }
            else
            {
                property.Type = typeRef;
            }

            property.Attributes = MemberAttributes.Final;

            CodeMethodInvokeExpression getMethod = EmitGetMethod(target);
            CodeExpression getReturnExpression;

            property.Attributes |= AccessibilityFromGettersAndSetters(Item);
            // setup the accessibility of the navigation property setter and getter
            MemberAttributes propertyAccessibility = property.Attributes & MemberAttributes.AccessMask;
            PropertyEmitter.AddGetterSetterFixUp(Generator.FixUps, GetFullyQualifiedPropertyName(property.Name),
                PropertyEmitter.GetGetterAccessibility(Item), propertyAccessibility, true);
            PropertyEmitter.AddGetterSetterFixUp(Generator.FixUps, GetFullyQualifiedPropertyName(property.Name),
                PropertyEmitter.GetSetterAccessibility(Item), propertyAccessibility, false);

            if (target.RelationshipMultiplicity != RelationshipMultiplicity.Many)
            {
                
                // insert user-supplied Set code here, before the assignment
                //
                List<CodeStatement> additionalSetStatements = eventArgs.AdditionalSetStatements;
                if (additionalSetStatements != null && additionalSetStatements.Count > 0)
                {
                    try
                    {
                        property.SetStatements.AddRange(additionalSetStatements.ToArray());
                    }
                    catch (ArgumentNullException ex)
                    {
                        Generator.AddError(Strings.InvalidSetStatementSuppliedForProperty(Item.Name),
                                           ModelBuilderErrorCode.InvalidSetStatementSuppliedForProperty,
                                           EdmSchemaErrorSeverity.Error,
                                           ex);
                    }
                }

                CodeExpression valueRef =  new CodePropertySetValueReferenceExpression();
                if(typeRef != eventArgs.ReturnType)
                {
                    // we need to cast to the actual type
                    valueRef = new CodeCastExpression(typeRef, valueRef);
                }

                if (referenceProperty)
                {
                    // get
                    //     return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<TTargetEntity>("CSpaceQualifiedRelationshipName", "TargetRoleName");
                    getReturnExpression = getMethod;

                    // set
                    // if (value != null)
                    // {
                    //    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<TTargetEntity>"CSpaceQualifiedRelationshipName", "TargetRoleName", value);
                    // }
                    
                    CodeMethodReferenceExpression initReferenceMethod = new CodeMethodReferenceExpression();
                    initReferenceMethod.MethodName = "InitializeRelatedReference";

                    initReferenceMethod.TypeArguments.Add(Generator.GetLeastPossibleQualifiedTypeReference(GetEntityType(target)));
                    initReferenceMethod.TargetObject = new CodePropertyReferenceExpression(
                        new CodeCastExpression(TypeReference.IEntityWithRelationshipsTypeBaseClass, ThisRef),
                        "RelationshipManager");

                    // relationships aren't backed by types so we won't map the namespace
                    // or we can't find the relationship again later
                    string cspaceNamespaceNameQualifiedRelationshipName = target.DeclaringType.FullName;
                                         
                    property.SetStatements.Add(
                        new CodeConditionStatement(
                            EmitExpressionDoesNotEqualNull(valueRef),                           
                            new CodeExpressionStatement(
                                new CodeMethodInvokeExpression(
                                initReferenceMethod, new CodeExpression[] {
                                    new CodePrimitiveExpression(cspaceNamespaceNameQualifiedRelationshipName), new CodePrimitiveExpression(target.Name), valueRef}))));
                }
                else
                {
                    CodePropertyReferenceExpression valueProperty = new CodePropertyReferenceExpression(getMethod, ValuePropertyName);

                    // get                
                    //     return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<TTargetEntity>("CSpaceQualifiedRelationshipName", "TargetRoleName").Value;
                    getReturnExpression = valueProperty;

                    // set
                    //     ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<TTargetEntity>("CSpaceQualifiedRelationshipName", "TargetRoleName").Value = value;
                    property.SetStatements.Add(
                        new CodeAssignStatement(valueProperty, valueRef));
                }
            }
            else
            {
                // get
                //     return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<TTargetEntity>("CSpaceQualifiedRelationshipName", "TargetRoleName");
                getReturnExpression = getMethod;

                // set
                // if (value != null)
                // {
                //    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<TTargetEntity>"CSpaceQualifiedRelationshipName", "TargetRoleName", value);
                // }
                CodeExpression valueRef = new CodePropertySetValueReferenceExpression();
                
                CodeMethodReferenceExpression initCollectionMethod = new CodeMethodReferenceExpression();
                initCollectionMethod.MethodName = "InitializeRelatedCollection";

                initCollectionMethod.TypeArguments.Add(Generator.GetLeastPossibleQualifiedTypeReference(GetEntityType(target)));
                initCollectionMethod.TargetObject = new CodePropertyReferenceExpression(
                    new CodeCastExpression(TypeReference.IEntityWithRelationshipsTypeBaseClass, ThisRef),
                    "RelationshipManager");

                // relationships aren't backed by types so we won't map the namespace
                // or we can't find the relationship again later
                string cspaceNamespaceNameQualifiedRelationshipName = target.DeclaringType.FullName;

                property.SetStatements.Add(
                    new CodeConditionStatement(
                        EmitExpressionDoesNotEqualNull(valueRef),
                        new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(
                            initCollectionMethod, new CodeExpression[] {
                                    new CodePrimitiveExpression(cspaceNamespaceNameQualifiedRelationshipName), new CodePrimitiveExpression(target.Name), valueRef}))));

            }

            // if additional Get statements were specified by the event subscriber, insert them now
            //
            List<CodeStatement> additionalGetStatements = eventArgs.AdditionalGetStatements;
            if (additionalGetStatements != null && additionalGetStatements.Count > 0)
            {
                try
                {
                    property.GetStatements.AddRange(additionalGetStatements.ToArray());
                }
                catch (ArgumentNullException ex)
                {
                    Generator.AddError(Strings.InvalidGetStatementSuppliedForProperty(Item.Name),
                                       ModelBuilderErrorCode.InvalidGetStatementSuppliedForProperty,
                                       EdmSchemaErrorSeverity.Error,
                                       ex);
                }
            }

            property.GetStatements.Add(new CodeMethodReturnStatement(getReturnExpression));

            return property;
        }

        internal static bool IsNameAlreadyAMemberName(StructuralType type, string generatedPropertyName, StringComparison comparison)
        {
            foreach (EdmMember member in type.Members)
            {
                if (member.DeclaringType == type &&
                    member.Name.Equals(generatedPropertyName, comparison))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetFullyQualifiedPropertyName(string propertyName)
        {
            return Item.DeclaringType.FullName + "." + propertyName;
        }

        /// <summary>
        /// Gives the SchemaElement back cast to the most
        /// appropriate type
        /// </summary>
        private new NavigationProperty Item
        {
            get
            {
                return base.Item as NavigationProperty;
            }
        }

        /// <summary>
        /// Get the return type for the get method, given the target end
        /// </summary>
        /// <param name="target"></param>
        /// <param name="referenceMethod">true if the is the return type for a reference property</param>
        /// <returns>the return type for a target</returns>
        private CodeTypeReference GetReturnType(RelationshipEndMember target, bool referenceMethod)
        {
            CodeTypeReference returnType = Generator.GetLeastPossibleQualifiedTypeReference(GetEntityType(target));
            if (referenceMethod)
            {
                returnType = TypeReference.AdoFrameworkGenericDataClass("EntityReference", returnType);
            }
            else if (target.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                returnType = TypeReference.AdoFrameworkGenericDataClass("EntityCollection", returnType);
            }

            return returnType;
        }


        private static EntityTypeBase GetEntityType(RelationshipEndMember endMember)
        {
            Debug.Assert(endMember.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.RefType, "not a reference type");
            EntityTypeBase type = ((RefType)endMember.TypeUsage.EdmType).ElementType;
            return type;
        }

        /// <summary>
        /// Emit the GetRelatedCollection or GetRelatedReference methods
        /// </summary>
        /// <param name="target">Target end of the relationship</param>        
        /// <returns>Expression to invoke the appropriate method</returns>
        private CodeMethodInvokeExpression EmitGetMethod(RelationshipEndMember target)
        {
            // ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<TargetType>("CSpaceQualifiedRelationshipName", "TargetRoleName");
            // or
            // ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<TargetType>("CSpaceQualifiedRelationshipName", "TargetRoleName");

            CodeMethodReferenceExpression getMethod = new CodeMethodReferenceExpression();
            if (target.RelationshipMultiplicity != RelationshipMultiplicity.Many)
                getMethod.MethodName = "GetRelatedReference";
            else
                getMethod.MethodName = "GetRelatedCollection";

            getMethod.TypeArguments.Add(Generator.GetLeastPossibleQualifiedTypeReference(GetEntityType(target)));
            getMethod.TargetObject = new CodePropertyReferenceExpression(
                new CodeCastExpression(TypeReference.IEntityWithRelationshipsTypeBaseClass, ThisRef),
                "RelationshipManager");

            // relationships aren't backed by types so we won't map the namespace
            // or we can't find the relationship again later
            string cspaceNamespaceNameQualifiedRelationshipName = target.DeclaringType.FullName;
            return new CodeMethodInvokeExpression(
                getMethod, new CodeExpression[] { new CodePrimitiveExpression(cspaceNamespaceNameQualifiedRelationshipName), new CodePrimitiveExpression(target.Name)});
        }
    }
}
