//---------------------------------------------------------------------
// <copyright file="StructuredType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;

    /// <summary>
    /// Summary description for StructuredType.
    /// </summary>
    internal abstract class StructuredType : SchemaType
    {
        #region Instance Fields
        private bool? _baseTypeResolveResult;
        private string _unresolvedBaseType = null;
        private StructuredType _baseType = null;
        private bool _isAbstract = false;
        private SchemaElementLookUpTable<SchemaElement> _namedMembers = null;
        private ISchemaElementLookUpTable<StructuredProperty> _properties = null;
        #endregion

        #region Static Fields
        private static readonly char[] NameSeparators = new char[] { '.' };
        #endregion

        #region Public Properties
        /// <summary>
        /// 
        /// </summary>
        public StructuredType BaseType
        {
            get
            {
                return _baseType;
            }
            private set
            {
                _baseType = value;
            }
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        public ISchemaElementLookUpTable<StructuredProperty> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new FilteredSchemaElementLookUpTable<StructuredProperty, SchemaElement>(NamedMembers);
                }
                return _properties;
            }
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        protected SchemaElementLookUpTable<SchemaElement> NamedMembers
        {
            get
            {
                if (_namedMembers == null)
                {
                    _namedMembers = new SchemaElementLookUpTable<SchemaElement>();
                }
                return _namedMembers;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsTypeHierarchyRoot
        {
            get
            {
                Debug.Assert((BaseType == null && _unresolvedBaseType == null) ||
                             (BaseType != null && _unresolvedBaseType != null), "you are checking for the hierarchy root before the basetype has been set");

                // any type without a base is a base type
                return BaseType == null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool IsAbstract
        {
            get
            {
                return _isAbstract;
            }
        }


        #endregion

        #region More Public Methods
        /// <summary>
        /// Find a property by name in the type hierarchy
        /// </summary>
        /// <param name="name">simple property name</param>
        /// <returns>the StructuredProperty object if name exists, null otherwise</returns>
        public StructuredProperty FindProperty(string name)
        {
            StructuredProperty property = Properties.LookUpEquivalentKey(name);
            if (property != null)
                return property;

            if (IsTypeHierarchyRoot)
                return null;

            return BaseType.FindProperty(name);
        }


        /// <summary>
        /// Determines whether this type is of the same type as baseType, 
        /// or is derived from baseType.
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns>true if this type is of the baseType, false otherwise</returns>
        public bool IsOfType(StructuredType baseType)
        {
            StructuredType type = this;

            while (type != null && type != baseType)
            {
                type = type.BaseType;
            }

            return (type == baseType);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            TryResolveBaseType();

            foreach (SchemaElement member in NamedMembers)
                member.ResolveTopLevelNames();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override void Validate()
        {
            base.Validate();

            foreach (SchemaElement member in NamedMembers)
            {
                if (BaseType != null)
                {
                    StructuredType definingType;
                    SchemaElement definingMember;
                    string errorMessage = null;
                    if(HowDefined.AsMember == BaseType.DefinesMemberName(member.Name, out definingType, out definingMember))
                    {
                        errorMessage = System.Data.Entity.Strings.DuplicateMemberName(member.Name, FQName, definingType.FQName);
                    }
                    if (errorMessage != null)
                        member.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, errorMessage);
                }

                member.Validate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        protected StructuredType(Schema parentElement)
            : base(parentElement)
        {
        }

        /// <summary>
        /// Add a member to the type
        /// </summary>
        /// <param name="newMember">the member being added</param>
        protected void AddMember(SchemaElement newMember)
        {
            Debug.Assert(newMember != null, "newMember parameter is null");

            if (string.IsNullOrEmpty(newMember.Name))
            {
                // this is an error condition that has already been reported.
                return;
            }

            if (this.Schema.DataModel != SchemaDataModelOption.ProviderDataModel &&
                 Utils.CompareNames(newMember.Name, Name) == 0)
            {
                newMember.AddError(ErrorCode.BadProperty, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.InvalidMemberNameMatchesTypeName(newMember.Name, FQName));
            }

            NamedMembers.Add(newMember, true, Strings.PropertyNameAlreadyDefinedDuplicate);
        }


        /// <summary>
        /// See if a name is a member in a type or any of its base types
        /// </summary>
        /// <param name="name">name to look for</param>
        /// <param name="definingType">if defined, the type that defines it</param>
        /// <param name="definingMember">if defined, the member that defines it</param>
        /// <returns>how name was defined</returns>
        private HowDefined DefinesMemberName(string name, out StructuredType definingType, out SchemaElement definingMember)
        {
            if (NamedMembers.ContainsKey(name))
            {
                definingType = this;
                definingMember = NamedMembers[name];
                return HowDefined.AsMember;
            }

            definingMember = NamedMembers.LookUpEquivalentKey(name);
            Debug.Assert(definingMember == null, "we allow the scenario that members can have same name but different cases");

            if (IsTypeHierarchyRoot)
            {
                definingType = null;
                definingMember = null;
                return HowDefined.NotDefined;
            }

            return BaseType.DefinesMemberName(name, out definingType, out definingMember);
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// 
        /// </summary>
        protected string UnresolvedBaseType
        {
            get
            {
                return _unresolvedBaseType;
            }
            set
            {
                _unresolvedBaseType = value;
            }
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.Property))
            {
                HandlePropertyElement(reader);
                return true;
            }
            return false;
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.BaseType))
            {
                HandleBaseTypeAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Abstract))
            {
                HandleAbstractAttribute(reader);
                return true;
            }

            return false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        private bool TryResolveBaseType()
        {
            if (_baseTypeResolveResult.HasValue)
            {
                return _baseTypeResolveResult.Value;
            }

            if (BaseType != null)
            {
                _baseTypeResolveResult = true;
                return _baseTypeResolveResult.Value;
            }

            if (UnresolvedBaseType == null)
            {
                _baseTypeResolveResult = true;
                return _baseTypeResolveResult.Value;
            }

            SchemaType element;
            if (!Schema.ResolveTypeName(this, UnresolvedBaseType, out element))
            {
                _baseTypeResolveResult = false;
                return _baseTypeResolveResult.Value;
            }

            BaseType = element as StructuredType;
            if (BaseType == null)
            {
                AddError(ErrorCode.InvalidBaseType, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.InvalidBaseTypeForStructuredType(UnresolvedBaseType, FQName));
                _baseTypeResolveResult = false;
                return _baseTypeResolveResult.Value;
            }

            // verify that creating this link to the base type will not introduce a cycle;
            // if so, break the link and add an error
            if (CheckForInheritanceCycle())
            {
                BaseType = null;

                AddError(ErrorCode.CycleInTypeHierarchy, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.CycleInTypeHierarchy(FQName));
                _baseTypeResolveResult = false;
                return _baseTypeResolveResult.Value;
            }

            _baseTypeResolveResult = true;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleBaseTypeAttribute(XmlReader reader)
        {
            Debug.Assert(UnresolvedBaseType == null, string.Format(CultureInfo.CurrentCulture, "{0} is already defined", reader.Name));

            string baseType;
            if (!Utils.GetDottedName(this.Schema, reader, out baseType))
                return;

            UnresolvedBaseType = baseType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleAbstractAttribute(XmlReader reader)
        {
            HandleBoolAttribute(reader, ref _isAbstract);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandlePropertyElement(XmlReader reader)
        {
            StructuredProperty property = new StructuredProperty(this);

            property.Parse(reader);

            AddMember(property);
        }

        /// <summary>
        /// Determine if a cycle exists in the type hierarchy: use two pointers to
        /// walk the chain, if one catches up with the other, we have a cycle.
        /// </summary>
        /// <returns>true if a cycle exists in the type hierarchy, false otherwise</returns>
        private bool CheckForInheritanceCycle()
        {
            StructuredType baseType = BaseType;
            Debug.Assert(baseType != null);

            StructuredType ref1 = baseType;
            StructuredType ref2 = baseType;

            do
            {
                ref2 = ref2.BaseType;

                if (Object.ReferenceEquals(ref1, ref2))
                    return true;

                if (ref1 == null)
                    return false;

                ref1 = ref1.BaseType;

                if (ref2 != null)
                    ref2 = ref2.BaseType;
            }
            while (ref2 != null);

            return false;
        }

        #endregion

        #region Private Properties
        #endregion

        private enum HowDefined
        {
            NotDefined,
            AsMember,
        }
    }
}
