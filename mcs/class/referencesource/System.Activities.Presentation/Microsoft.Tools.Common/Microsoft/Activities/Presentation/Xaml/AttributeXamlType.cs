// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.ComponentModel;
    using System.Xaml;
    using System.Xaml.Schema;

    internal class AttributeXamlType<TAttribute, TAttributeInfo> : XamlType
        where TAttribute : Attribute
        where TAttributeInfo : AttributeInfo<TAttribute>, new()
    {
        private TAttributeInfo attributeInfo = new TAttributeInfo();

        public AttributeXamlType(XamlSchemaContext xamlSchemaContext)
            : base(typeof(TAttribute), xamlSchemaContext)
        {
        }

        protected override XamlValueConverter<TypeConverter> LookupTypeConverter()
        {
            return new XamlValueConverter<TypeConverter>(typeof(AttributeConverter<TAttribute, TAttributeInfo>), this);
        }

        protected override bool LookupConstructionRequiresArguments()
        {
            return this.attributeInfo.LookupConstructionRequiresArguments;
        }

        protected override XamlTypeInvoker LookupInvoker()
        {
            if (this.attributeInfo.Invoker != null)
            {
                return this.attributeInfo.Invoker;
            }
            else
            {
                return base.LookupInvoker();
            }
        }
    }
}
