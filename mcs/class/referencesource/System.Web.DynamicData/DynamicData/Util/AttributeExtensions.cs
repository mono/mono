namespace System.Web.DynamicData.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;    

    internal static class AttributeExtensions {
        /// <summary>
        /// Gets the first attribute of a given time on the target AttributeCollection, or null.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type</typeparam>
        /// <param name="attributes">The AttributeCollection object</param>
        /// <returns></returns>
        internal static TAttribute FirstOrDefault<TAttribute>(this System.ComponentModel.AttributeCollection attributes) where TAttribute : Attribute {
            return attributes.OfType<TAttribute>().FirstOrDefault();
        }

        internal static TResult GetAttributePropertyValue<TAttribute, TResult>(this System.ComponentModel.AttributeCollection attributes, Func<TAttribute, TResult> propertyGetter)
            where TResult : class
            where TAttribute : Attribute {

            return attributes.GetAttributePropertyValue(propertyGetter, null);
        }

        internal static TResult GetAttributePropertyValue<TAttribute, TResult>(this System.ComponentModel.AttributeCollection attributes, Func<TAttribute, TResult> propertyGetter, TResult defaultValue)
            where TAttribute : Attribute {

            var attribute = attributes.FirstOrDefault<TAttribute>();

            return attribute.GetPropertyValue<TAttribute, TResult>(propertyGetter, defaultValue);
        }

        /// <summary>
        /// Gets the property for a given attribute reference or returns null if the reference is null.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type</typeparam>
        /// <typeparam name="TResult">The type of the attribute's property</typeparam>
        /// <param name="attribute">The attribute reference</param>
        /// <param name="propertyGetter">The function to evaluate on the attribute</param>
        /// <returns></returns>
        internal static TResult GetPropertyValue<TAttribute, TResult>(this TAttribute attribute, Func<TAttribute, TResult> propertyGetter)
            where TResult : class
            where TAttribute : Attribute {

            return attribute.GetPropertyValue(propertyGetter, null);
        }

        /// <summary>
        /// Gets the property for a given attribute reference or returns the default value if the reference is null.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type</typeparam>
        /// <typeparam name="TResult">The type of the attribute's property</typeparam>
        /// <param name="attribute">The attribute reference</param>
        /// <param name="propertyGetter">The function to evaluate on the attribute</param>
        /// <param name="defaultValue">The default value to return if the attribute is null</param>
        /// <returns></returns>
        internal static TResult GetPropertyValue<TAttribute, TResult>(this TAttribute attribute, Func<TAttribute, TResult> propertyGetter, TResult defaultValue)
            where TAttribute : Attribute {

            if (attribute != null) {
                return propertyGetter(attribute);
            }
            else {
                return defaultValue;
            }
        }
    }
}
