namespace System.Activities.Presentation.PropertyEditing
{
    using System;
    using System.Collections.Generic;
    using System.Activities.Presentation.Internal;
    using System.Activities.Presentation.Internal.Properties;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This attributes is used to specify which object Types can be assigned as the value of a given
    /// property or as the value of a given property type.  If the property represents a collection,
    /// this attribute specifies the object Types of which instances can be created as the items
    /// of that collection.
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    sealed class NewItemTypesAttribute : Attribute
    {

        private Type _factoryType;
        private Type[] _types;

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        /// <param name="type">Type that this attribute declares as being valid new item Type.</param>
        /// <exception cref="ArgumentNullException">If type is null.</exception>
        public NewItemTypesAttribute(Type type)
        {
            if (type == null)
                throw FxTrace.Exception.ArgumentNull("type");

            _factoryType = typeof(NewItemFactory);
            _types = new Type[] { type };
        }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        /// <param name="types">List of types that this attribute declares as being
        /// valid new item Types.</param>
        /// <exception cref="ArgumentNullException">If types is null or emtpy.</exception>
        public NewItemTypesAttribute(params Type[] types)
        {
            if (types == null || types.Length < 1)
                throw FxTrace.Exception.ArgumentNull("types");

            _factoryType = typeof(NewItemFactory);
            _types = types;
        }

        /// <summary>
        /// Gets a list of Types that this attribute declares as being valid new item Types.
        /// Guaranteed to be non-null.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Suppress to avoid unnecessary change.")]
        public Type Type
        {
            get
            {
                return _types[0];
            }
        }

        /// <summary>
        /// Gets a list of Types that this attribute declares as being valid new item Types.
        /// Guaranteed to be non-null.
        /// </summary>
        public IEnumerable<Type> Types
        {
            get
            {
                return _types;
            }
        }

        /// <summary>
        /// Gets or sets the factory Type associated with this attribute.  The Type is
        /// guaranteed to derive from NewItemFactory.
        /// </summary>
        /// <exception cref="ArgumentException">If type does not derive from NewItemFactory</exception>
        /// <exception cref="ArgumentNullException">If type is null.</exception>
        public Type FactoryType
        {
            get
            {
                return _factoryType;
            }
            set
            {
                if (value == null)
                    throw FxTrace.Exception.ArgumentNull("value");
                if (!typeof(NewItemFactory).IsAssignableFrom(value))
                    throw FxTrace.Exception.AsError(new ArgumentException(Resources.Error_InvalidFactoryType));

                _factoryType = value;
            }
        }

        /// <summary>
        /// Gets the TypeId for this attribute.  Returns an equality array unique to this attribute
        /// type and the contained factory type.  The order in which the type attributes are passed
        /// into the constructor of this class (if there are more than one) matters and is used in 
        /// determining the equality of two NewItemTypesAttribute instances.
        /// </summary>
        public override object TypeId
        {
            get
            {
                object[] typeId = new object[_types.Length + 2];
                for (int i = 0; i < _types.Length; i++)
                {
                    typeId[i + 2] = _types[i];
                }
                typeId[0] = typeof(NewItemTypesAttribute);
                typeId[1] = _factoryType;
                return new EqualityArray(typeId);
            }
        }
    }
}
