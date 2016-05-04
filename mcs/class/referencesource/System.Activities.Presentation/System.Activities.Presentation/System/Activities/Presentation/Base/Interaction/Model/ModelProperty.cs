//------------------------------------------------------------------------------
// <copyright file="ModelProperty.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Presentation.Model {

    using System.Activities.Presentation.Internal.Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;

    /// <summary>
    /// An ModelProperty represents a property on an item.  ModelProperties are 
    /// associated with an instance of an item, which allows them to have simple 
    /// Value get/set properties instead of the more cumbersome GetValue/SetValue 
    /// mechanism of PropertyDescriptor.
    /// 
    /// A ModelProperty’s value may come from a locally set value, or it may be 
    /// inherited from somewhere higher up in the property mechanism.  Because 
    /// all items in the tree contain Source properties, you can 
    /// easily find out the real source of a property value simply using the 
    /// following code:
    /// 
    ///     Console.WriteLine(property.Value.Source);
    /// 
    /// Value will return null if the property is not set anywhere in the hierarchy.
    /// 
    /// Type converters and editors defined on the underlying data model are 
    /// wrapped so that they accept ModelItems as parameters.
    /// </summary>
    public abstract class ModelProperty {

        /// <summary>
        /// Creates a new ModelProperty.
        /// </summary>
        protected ModelProperty() { }

        /// <summary>
        /// Returns the attributes declared on this property.
        /// </summary>
        public abstract AttributeCollection Attributes { get; }

        /// <summary>
        /// Returns Value cast as a ModelItemCollection.  This property allows you to 
        /// access collection properties easily without cluttering your code with casts:
        /// 
        ///     Property.Collection.Add(myItem);
        /// 
        /// If the property value is not a collection, this property will return null.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public abstract ModelItemCollection Collection { get; }

        /// <summary>
        /// Returns the currently computed value for this property.  Setting a value
        /// on this property is the same as calling SetValue, but can be used in
        /// data binding expressions.
        /// </summary>
        public abstract object ComputedValue { get; set; }

        /// <summary>
        /// Returns the type converter to use with this property.  Underlying type 
        /// converters are all wrapped so they accept Item objects.  When performing 
        /// a conversion to a particular value, the type converter’s return type is 
        /// not wrapped.  Type converters which return standard values also return 
        /// values as Item objects.
        /// </summary>
        public abstract TypeConverter Converter { get; }

        /// <summary>
        /// Returns the type which defines this property if IsAttached returns true.
        /// Otherwhise, returns null.
        /// </summary>
        public abstract Type AttachedOwnerType { get; }

        /// <summary>
        /// Returns the default value for this property.  If the property does not 
        /// define a default value this will return null.
        /// </summary>
        public abstract object DefaultValue { get; }

        /// <summary>
        /// Returns Value cast as a ItemDictionary.  This property allows you to 
        /// access dictionary properties easily without cluttering your code with casts:
        /// 
        ///     Property.Dictionary[key] = value;
        /// 
        /// If the property value is not a dictionary, this property will return null.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public abstract ModelItemDictionary Dictionary { get; }

        /// <summary>
        /// Returns true if the property can be shown in a property window.
        /// </summary>
        public abstract bool IsBrowsable { get; }

        /// <summary>
        /// Returns true if the value contained in the property is a ItemCollection.
        /// </summary>
        public abstract bool IsCollection { get; }

        /// <summary>
        /// Returns true if the value contained in the property is a ItemDictionary.
        /// </summary>
        public abstract bool IsDictionary { get; }

        /// <summary>
        /// Returns true if the property is read only.
        /// </summary>
        public abstract bool IsReadOnly { get; }

        /// <summary>
        /// Returns true if the property’s value is set locally.
        /// </summary>
        public abstract bool IsSet { get; }

        /// <summary>
        /// Returns true if the property represents an attached property from a different type.
        /// </summary>
        public abstract bool IsAttached { get; }

        /// <summary>
        /// Returns the value set into this property.  A property may return a 
        /// value that is inherited further up the element hierarchy, in which 
        /// case this property will return a value whose source != this.  
        /// If no value has ever been set for the property Value will return null.
        /// </summary>
        public abstract ModelItem Value { get; }

        /// <summary>
        /// Returns the name of this property.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Returns the parent of this property.  All properties have 
        /// parents, so this never returns null.
        /// </summary>
        public abstract ModelItem Parent { get; }

        /// <summary>
        /// The data type of the property.
        /// </summary>
        public abstract Type PropertyType { get; }

        /// <summary>
        /// Clears the local value for the property.
        /// </summary>
        public abstract void ClearValue();

        /// <summary>
        /// Sets a local value on a property.  If this value is already 
        /// a ModelItem, it will be used directly.  If it isn’t, a ModelItem 
        /// will be created.  Setting null into a property is valid, but 
        /// this is not the same as calling ClearValue().
        /// </summary>
        /// <param name="value">
        /// The new value to set.
        /// </param>
        /// <returns>
        /// The input value, if the value is already a ModelItem, or a newly
        /// created ModelItem wrapping the value.
        /// </returns>
        public abstract ModelItem SetValue(object value);

        internal virtual string Reference
        {
            get
            {
                return null;
            }
        }

        internal virtual void ClearReference()
        {
        }

        internal virtual void SetReference(string sourceProperty)
        {
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        // FXCop: args are validated; fxcop does not seem to understand ReferenceEquals.
        
        public static bool operator ==(ModelProperty first, ModelProperty second) {
            if (object.ReferenceEquals(first, second)) return true;
            if (object.ReferenceEquals(first, null) || object.ReferenceEquals(second, null)) return false;
            return (first.Parent == second.Parent && first.Name.Equals(second.Name));
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        // FXCop: args are validated; fxcop does not seem to understand ReferenceEquals.
        
        public static bool operator !=(ModelProperty first, ModelProperty second) {
            if (object.ReferenceEquals(first, second)) return false;
            if (object.ReferenceEquals(first, null) || object.ReferenceEquals(second, null)) return true;
            return (first.Parent != second.Parent || !first.Name.Equals(second.Name));
        }

        /// <summary>
        /// Equality for properties.  Properties are equal if
        /// they have the same name and parent.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (object.ReferenceEquals(obj, this)) return true;
            ModelProperty prop = obj as ModelProperty;
            if (object.ReferenceEquals(prop, null)) return false;
            if (prop.Parent != Parent) return false;
            return prop.Name.Equals(Name);
        }

        /// <summary>
        /// Standard hashcode implementation.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return Parent.GetHashCode() ^ Name.GetHashCode();
        }
    }
}
