namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Activities.Presentation;

    /// <summary>
    /// Attribute that is used to attach a PropertyOrder to a property.
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    [AttributeUsage(AttributeTargets.Property)]
    sealed class PropertyOrderAttribute : Attribute {
        private PropertyOrder _order;
        
        /// <summary>
        /// Creates a PropertyOrderAttribute.
        /// </summary>
        /// <param name="order">The PropertyOrder to attach to the property</param>
        /// <exception cref="ArgumentNullException">When order is null</exception>
        public PropertyOrderAttribute(PropertyOrder order)
        {
            if (order == null)
                throw FxTrace.Exception.ArgumentNull("order");

            _order = order;
        }

        /// <summary>
        /// Gets the associated PropertyOrder
        /// </summary>
        public PropertyOrder Order {
            get { return _order; }
        }
    }
}
