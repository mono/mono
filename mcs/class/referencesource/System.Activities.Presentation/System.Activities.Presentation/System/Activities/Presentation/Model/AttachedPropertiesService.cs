//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Runtime;
    
    public class AttachedPropertiesService
    {
        List<AttachedProperty> properties;

        public AttachedPropertiesService()
        {
            this.properties = new List<AttachedProperty>();
        }

        public void AddProperty(AttachedProperty property)
        {
            if (property == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("property"));
            }
            if (string.IsNullOrEmpty(property.Name))
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.AttachedPropertyNameShouldNotBeEmpty));
            }
            this.properties.Add(property);
        }

        internal IEnumerable<AttachedProperty> GetAttachedProperties(Type modelItemType)
        {
            var properties = from property in this.properties
                where property.OwnerType.IsAssignableFrom(modelItemType) select property;

            if (modelItemType.IsGenericType)
            {
                var propertiesFromGenericRoot = from property in this.properties
                                                where property.OwnerType.IsAssignableFrom(modelItemType.GetGenericTypeDefinition())
                                                select property;
                properties = properties.Concat(propertiesFromGenericRoot).Distinct();
            }

            return properties;
        }
    }

}
