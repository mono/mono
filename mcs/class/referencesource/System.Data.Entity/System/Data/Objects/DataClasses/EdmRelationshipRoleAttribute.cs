//---------------------------------------------------------------------
// <copyright file="EdmRelationshipRoleAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Data.Metadata.Edm; //for RelationshipMultiplicity

namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class EdmRelationshipAttribute : System.Attribute
    {
        private readonly string _relationshipNamespaceName;
        private readonly string _relationshipName;
        private readonly string _role1Name;
        private readonly string _role2Name;
        private readonly RelationshipMultiplicity _role1Multiplicity;
        private readonly RelationshipMultiplicity _role2Multiplicity;
        private readonly Type _role1Type;
        private readonly Type _role2Type;
        private readonly bool _isForeignKey;

        /// <summary>
        /// Attribute containing the details for a relationship
        /// This should match the C-Space relationship information, but having it available in this
        /// attribute allows us access to this information even in O-Space when there is no context.
        /// There can be multiple attributes of this type in an assembly.
        /// </summary>
        public EdmRelationshipAttribute(string relationshipNamespaceName,
            string relationshipName,
            string role1Name,
            RelationshipMultiplicity role1Multiplicity,
            Type role1Type,
            string role2Name,
            RelationshipMultiplicity role2Multiplicity,
            Type role2Type)
        {
            _relationshipNamespaceName = relationshipNamespaceName;
            _relationshipName = relationshipName;

            _role1Name = role1Name;
            _role1Multiplicity = role1Multiplicity;
            _role1Type = role1Type;

            _role2Name = role2Name;
            _role2Multiplicity = role2Multiplicity;
            _role2Type = role2Type;
        }

        /// <summary>
        /// Attribute containing the details for a relationship
        /// This should match the C-Space relationship information, but having it available in this
        /// attribute allows us access to this information even in O-Space when there is no context.
        /// There can be multiple attributes of this type in an assembly.
        /// </summary>
        public EdmRelationshipAttribute(string relationshipNamespaceName,
            string relationshipName,
            string role1Name,
            RelationshipMultiplicity role1Multiplicity,
            Type role1Type,
            string role2Name,
            RelationshipMultiplicity role2Multiplicity,
            Type role2Type,
            bool isForeignKey)
        {
            _relationshipNamespaceName = relationshipNamespaceName;
            _relationshipName = relationshipName;

            _role1Name = role1Name;
            _role1Multiplicity = role1Multiplicity;
            _role1Type = role1Type;

            _role2Name = role2Name;
            _role2Multiplicity = role2Multiplicity;
            _role2Type = role2Type;

            _isForeignKey = isForeignKey;
        }

        /// <summary>
        /// The name of the namespace that the relationship is in
        /// </summary>
        public string RelationshipNamespaceName
        {
            get { return _relationshipNamespaceName; }
        }

        /// <summary>
        /// The name of a relationship
        /// </summary>
        public string RelationshipName
        {
            get { return _relationshipName; }
        }

        /// <summary>
        /// The name of the role
        /// </summary>
        public string Role1Name
        {
            get { return _role1Name; }
        }

        /// <summary>
        /// The multiplicity of the the RoleName in RelationshipName 
        /// </summary>
        public RelationshipMultiplicity Role1Multiplicity
        {
            get { return _role1Multiplicity; }
        }

        /// <summary>
        /// The CLR type for the role associated with this relationship
        /// </summary>
        public Type Role1Type
        {
            get { return _role1Type; }
        }
        /// <summary>
        /// The name of the role
        /// </summary>
        public string Role2Name
        {
            get { return _role2Name; }
        }

        /// <summary>
        /// The multiplicity of the the RoleName in RelationshipName 
        /// </summary>
        public RelationshipMultiplicity Role2Multiplicity
        {
            get { return _role2Multiplicity; }
        }

        /// <summary>
        /// The CLR type for the role associated with this relationship
        /// </summary>
        public Type Role2Type
        {
            get { return _role2Type; }
        }

        /// <summary>
        /// Indicates whether this is a common-value (or FK-based) relationship.
        /// </summary>
        public bool IsForeignKey
        {
            get { return _isForeignKey; }
        }
    }
}
