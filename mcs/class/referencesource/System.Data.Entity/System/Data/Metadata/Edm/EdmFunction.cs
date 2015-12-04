//---------------------------------------------------------------------
// <copyright file="EdmFunction.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class for representing a function
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    public sealed class EdmFunction : EdmType
    {
        #region Constructors
        internal EdmFunction(string name, string namespaceName, DataSpace dataSpace, EdmFunctionPayload payload)
            : base(name, namespaceName, dataSpace)
        {
            //---- name of the 'schema'
            //---- this is used by the SQL Gen utility and update pipeline to support generation of the correct function name in the store
            _schemaName = payload.Schema;
            _fullName = this.NamespaceName + "." + this.Name;

            FunctionParameter[] returnParameters = payload.ReturnParameters;

           Debug.Assert(returnParameters.All((returnParameter) => returnParameter != null), "All return parameters must be non-null");
           Debug.Assert(returnParameters.All((returnParameter) => returnParameter.Mode == ParameterMode.ReturnValue), "Return parameter in a function must have the ParameterMode equal to ReturnValue.");
            
            _returnParameters = new ReadOnlyMetadataCollection<FunctionParameter>(
                returnParameters
                    .Select((returnParameter) => SafeLink<EdmFunction>.BindChild<FunctionParameter>(this, FunctionParameter.DeclaringFunctionLinker, returnParameter))
                    .ToList());
            
            if (payload.IsAggregate.HasValue) SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.Aggregate, payload.IsAggregate.Value);
            if (payload.IsBuiltIn.HasValue) SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.BuiltIn, payload.IsBuiltIn.Value);
            if (payload.IsNiladic.HasValue) SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.NiladicFunction, payload.IsNiladic.Value);
            if (payload.IsComposable.HasValue) SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.IsComposable, payload.IsComposable.Value);
            if (payload.IsFromProviderManifest.HasValue) SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.IsFromProviderManifest, payload.IsFromProviderManifest.Value);
            if (payload.IsCachedStoreFunction.HasValue) SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.IsCachedStoreFunction, payload.IsCachedStoreFunction.Value);
            if (payload.IsFunctionImport.HasValue) SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.IsFunctionImport, payload.IsFunctionImport.Value);

            if (payload.ParameterTypeSemantics.HasValue)
            {
                _parameterTypeSemantics = payload.ParameterTypeSemantics.Value;
            }

            if (payload.StoreFunctionName != null)
            {
                _storeFunctionNameAttribute = payload.StoreFunctionName;
            }

            if (payload.EntitySets != null)
            {
                Debug.Assert(_returnParameters.Count == payload.EntitySets.Length, "The number of entity sets should match the number of return parameters");
                _entitySets = new ReadOnlyMetadataCollection<EntitySet>(payload.EntitySets);
            }
            else
            {
                var list = new List<EntitySet>();
                if (_returnParameters.Count != 0)
                {
                    Debug.Assert(_returnParameters.Count == 1, "If there was more than one result set payload.EntitySets should not have been null");
                    list.Add(null);
                }
                _entitySets = new ReadOnlyMetadataCollection<EntitySet>(list);
            }

            if (payload.CommandText != null)
            {
                _commandTextAttribute = payload.CommandText;
            }

            if (payload.Parameters != null)
            {
                // validate the parameters
                foreach (FunctionParameter parameter in payload.Parameters)
                {
                    if (parameter == null)
                    {
                        throw EntityUtil.CollectionParameterElementIsNull("parameters");
                    }
                    Debug.Assert(parameter.Mode != ParameterMode.ReturnValue, "No function parameter can have ParameterMode equal to ReturnValue.");
                }

                // Populate the parameters
                _parameters = new SafeLinkCollection<EdmFunction, FunctionParameter>(this, FunctionParameter.DeclaringFunctionLinker, new MetadataCollection<FunctionParameter>(payload.Parameters));
            }
            else
            {
                _parameters = new ReadOnlyMetadataCollection<FunctionParameter>(new MetadataCollection<FunctionParameter>());
            }
        }

        #endregion

        #region Fields
        private readonly ReadOnlyMetadataCollection<FunctionParameter> _returnParameters;
        private readonly ReadOnlyMetadataCollection<FunctionParameter> _parameters;
        private readonly FunctionAttributes _functionAttributes = FunctionAttributes.Default;
        private readonly string _storeFunctionNameAttribute;
        private readonly ParameterTypeSemantics _parameterTypeSemantics;
        private readonly string _commandTextAttribute;
        private readonly string _schemaName;
        private readonly ReadOnlyMetadataCollection<EntitySet> _entitySets;
        private readonly string _fullName;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.EdmFunction; } }

        /// <summary>
        /// Returns the full name of this type, which is namespace + "." + name. 
        /// </summary>
        public override string FullName
        {
            get
            {
                return _fullName;
            }
        }

        /// <summary>
        /// Gets the collection of parameters
        /// </summary>
        public ReadOnlyMetadataCollection<FunctionParameter> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        /// <summary>
        /// Returns true if this is a C-space function and it has an eSQL body defined as DefiningExpression.
        /// </summary>
        internal bool HasUserDefinedBody
        {
            get
            {
                return this.IsModelDefinedFunction && !String.IsNullOrEmpty(this.CommandTextAttribute);
            }
        }

        /// <summary>
        /// For function imports, optionally indicates the entity set to which the result is bound.
        /// If the function import has multiple result sets, returns the entity set to which the first result is bound
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EntitySet, false)]
        internal EntitySet EntitySet
        {
            get
            {
                return _entitySets.Count != 0 ? _entitySets[0] : null;
            }
        }

        /// <summary>
        /// For function imports, indicates the entity sets to which the return parameters are bound.
        /// The number of elements in the collection matches the number of return parameters. 
        /// A null element in the collection indicates that the corresponding are not bound to an entity set.
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EntitySet, true)]
        internal ReadOnlyMetadataCollection<EntitySet> EntitySets
        {
            get
            {
                return _entitySets;
            }
        }

        /// <summary>
        /// Gets the return parameter of this function
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.FunctionParameter, false)]
        public FunctionParameter ReturnParameter
        {
            get
            {
                return _returnParameters.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the return parameters of this function
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.FunctionParameter, true)]
        public ReadOnlyMetadataCollection<FunctionParameter> ReturnParameters
        {
            get
            {
                return _returnParameters;
            }
        }
        
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        internal string StoreFunctionNameAttribute
        {
            get { return _storeFunctionNameAttribute; }
        }

        [MetadataProperty(typeof(ParameterTypeSemantics), false)]
        internal ParameterTypeSemantics ParameterTypeSemanticsAttribute
        {
            get { return _parameterTypeSemantics; }
        }

        // Function attribute parameters
        [MetadataProperty(PrimitiveTypeKind.Boolean, false)]
        internal bool AggregateAttribute
        {
            get
            {
                return GetFunctionAttribute(FunctionAttributes.Aggregate);
            }
        }

        [MetadataProperty(PrimitiveTypeKind.Boolean, false)]
        internal bool BuiltInAttribute
        {
            get
            {
                return GetFunctionAttribute(FunctionAttributes.BuiltIn);
            }
        }

        [MetadataProperty(PrimitiveTypeKind.Boolean, false)]
        internal bool IsFromProviderManifest
        {
            get
            {
                return GetFunctionAttribute(FunctionAttributes.IsFromProviderManifest);
            }
        }

        [MetadataProperty(PrimitiveTypeKind.Boolean, false)]
        internal bool NiladicFunctionAttribute
        {
            get
            {
                return GetFunctionAttribute(FunctionAttributes.NiladicFunction);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Composable")]
        [MetadataProperty(PrimitiveTypeKind.Boolean, false)]
        public bool IsComposableAttribute
        {
            get
            {
                return GetFunctionAttribute(FunctionAttributes.IsComposable);
            }
        }

        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public string CommandTextAttribute
        {
            get
            {
                return _commandTextAttribute;
            }
        }

        internal bool IsCachedStoreFunction
        {
            get
            {
                return GetFunctionAttribute(FunctionAttributes.IsCachedStoreFunction);
            }
        }

        internal bool IsModelDefinedFunction
        {
            get
            {
                return this.DataSpace == DataSpace.CSpace && !IsCachedStoreFunction && !IsFromProviderManifest && !IsFunctionImport;
            }
        }

        internal bool IsFunctionImport
        {
            get
            {
                return GetFunctionAttribute(FunctionAttributes.IsFunctionImport);
            }
        }

        [MetadataProperty(PrimitiveTypeKind.String, false)]
        internal string Schema
        {
            get
            {
                return _schemaName;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets this item to be readonly, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();
                this.Parameters.Source.SetReadOnly();
                foreach (FunctionParameter returnParameter in ReturnParameters)
                {
                    returnParameter.SetReadOnly();
                }
            }
        }

        /// <summary>
        /// Builds function identity string in the form of "functionName (param1, param2, ... paramN)".
        /// </summary>
        internal override void BuildIdentity(StringBuilder builder)
        {
            // If we've already cached the identity, simply append it
            if (null != CacheIdentity)
            {
                builder.Append(CacheIdentity);
                return;
            }

            EdmFunction.BuildIdentity(
                builder, 
                FullName, 
                Parameters,
                (param) => param.TypeUsage,
                (param) => param.Mode);
        }

        /// <summary>
        /// Builds identity based on the functionName and parameter types. All parameters are assumed to be <see cref="ParameterMode.In"/>.
        /// Returns string in the form of "functionName (param1, param2, ... paramN)".
        /// </summary>
        internal static string BuildIdentity(string functionName, IEnumerable<TypeUsage> functionParameters)
        {
            StringBuilder identity = new StringBuilder();
            
            BuildIdentity(
                identity, 
                functionName, 
                functionParameters,
                (param) => param,
                (param) => ParameterMode.In);
            
            return identity.ToString();
        }

        /// <summary>
        /// Builds identity based on the functionName and parameters metadata.
        /// Returns string in the form of "functionName (param1, param2, ... paramN)".
        /// </summary>
        internal static void BuildIdentity<TParameterMetadata>(StringBuilder builder, 
                                                               string functionName, 
                                                               IEnumerable<TParameterMetadata> functionParameters,
                                                               Func<TParameterMetadata, TypeUsage> getParameterTypeUsage,
                                                               Func<TParameterMetadata, ParameterMode> getParameterMode)
        {
            //
            // Note: some callers depend on the format of the returned identity string.
            //

            // Start with the function name
            builder.Append(functionName);
            
            // Then add the string representing the list of parameters
            builder.Append('(');
            bool first = true;
            foreach (TParameterMetadata parameter in functionParameters)
            {
                if (first) { first = false; }
                else { builder.Append(","); }
                builder.Append(Helper.ToString(getParameterMode(parameter)));
                builder.Append(' ');
                getParameterTypeUsage(parameter).BuildIdentity(builder);
            }
            builder.Append(')');
        }

        private bool GetFunctionAttribute(FunctionAttributes attribute)
        {
            return attribute == (attribute & _functionAttributes);
        }

        private static void SetFunctionAttribute(ref FunctionAttributes field, FunctionAttributes attribute, bool isSet)
        {
            if (isSet)
            {
                // make sure that attribute bits are set to 1
                field |= attribute;
            }
            else
            {
                // make sure that attribute bits are set to 0
                field ^= field & attribute;
            }
        }

        #endregion

        #region Nested types
        [Flags]
        private enum FunctionAttributes : byte
        {
            None = 0,
            Aggregate = 1,
            BuiltIn = 2,
            NiladicFunction = 4,
            IsComposable = 8,
            IsFromProviderManifest = 16,
            IsCachedStoreFunction = 32,
            IsFunctionImport = 64,
            Default = IsComposable,
        }
        #endregion
    }

    internal struct EdmFunctionPayload
    {
        public string Name;
        public string NamespaceName;
        public string Schema;
        public string StoreFunctionName;
        public string CommandText;
        public EntitySet[] EntitySets;
        public bool? IsAggregate;
        public bool? IsBuiltIn;
        public bool? IsNiladic;
        public bool? IsComposable;
        public bool? IsFromProviderManifest;
        public bool? IsCachedStoreFunction;
        public bool? IsFunctionImport;
        public FunctionParameter[] ReturnParameters;
        public ParameterTypeSemantics? ParameterTypeSemantics;
        public FunctionParameter[] Parameters;
        public DataSpace DataSpace;
    }
}
