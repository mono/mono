//---------------------------------------------------------------------
// <copyright file="AssociationTypeEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.EntityModel.SchemaObjectModel;
using System.Diagnostics;


namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// Summary description for NestedTypeEmitter.
    /// </summary>
    internal sealed class AssociationTypeEmitter : SchemaTypeEmitter
    {
        public AssociationTypeEmitter(ClientApiGenerator generator, AssociationType associationType)
            : base(generator, associationType)
        {
        }


        public override CodeTypeDeclarationCollection EmitApiClass()
        {
            Debug.Assert(Item.AssociationEndMembers.Count == 2, "must have exactly two ends");

            AssociationEndMember end1 = Item.AssociationEndMembers[0];
            AssociationEndMember end2 = Item.AssociationEndMembers[1];

            Generator.CompileUnit.AssemblyCustomAttributes.Add(
                AttributeEmitter.EmitSimpleAttribute(
                Utils.FQAdoFrameworkDataClassesName("EdmRelationshipAttribute"),
                Item.NamespaceName,  //it is ok to use the c namespace because relationships aren't backed by clr objects
                Item.Name,
                end1.Name,
                GetMultiplicityCodeExpression(end1.RelationshipMultiplicity),
                GetEndTypeCodeExpression(end1),
                end2.Name,
                GetMultiplicityCodeExpression(end2.RelationshipMultiplicity),
                GetEndTypeCodeExpression(end2)
                ));

            // this method doesn't actually create a new type, just a new assembly level attribute for each end
            return new CodeTypeDeclarationCollection();
        }

        private CodeTypeOfExpression GetEndTypeCodeExpression(AssociationEndMember end)
        {
            return new CodeTypeOfExpression(Generator.GetFullyQualifiedTypeReference(((RefType)end.TypeUsage.EdmType).ElementType));
        }

        private CodeExpression GetMultiplicityCodeExpression(RelationshipMultiplicity multiplicity)
        {
            // example:
            // [System.Data.Objects.DataClasses.EdmRelationshipRoleAttribute("CustomerOrder", "Customer", global::System.Data.Metadata.Edm.RelationshipMultiplicity.One, typeof(Customer))]
            string roleMultiplicity = multiplicity.ToString();
            CodeExpression roleMultiplicityExpression = Emitter.EmitEnumMemberExpression(
                TypeReference.AdoFrameworkMetadataEdmType("RelationshipMultiplicity"), roleMultiplicity);
            return roleMultiplicityExpression;
        }

        internal new AssociationType Item
        {
            get
            {
                return base.Item as AssociationType;
            }
        }
    }
}
