using System.Windows.Markup;
using System.ComponentModel;
using System;
using System.Globalization;
using System.Xaml;
using System.Xaml.Schema;
using System.Collections.Generic;

namespace Test.Elements
{
    public class TemplateClass1 : Element
    {
        public TestTemplate Template { get; set; }

        public string Text { get; set; }
    }

    public class TemplateClass2: Element
    {
        public TestTemplate Template { get; set; }

        [Ambient]
        public string Suffix { get; set; }

        [TypeConverter(typeof(AppendAmbientStringToStringConverter))]
        public string AppendWithSuffix { get; set; }
    }

    public class TemplateClassWithNameResolver : Element
    {
        public TemplateWithNameResolver Template { get; set; }

        public string Text { get; set; }
    }

    public class AppendAmbientStringToStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            IXamlSchemaContextProvider schemaContextProvider =
                (IXamlSchemaContextProvider)context.GetService(typeof(IXamlSchemaContextProvider));
            IAmbientProvider ambientValueProvider =
                (IAmbientProvider)context.GetService(typeof(IAmbientProvider));
            IXamlNamespaceResolver namespaceResolver =
                (IXamlNamespaceResolver)context.GetService(typeof(IXamlNamespaceResolver));

            XamlTypeName typeName = XamlTypeName.Parse("TemplateClass2", namespaceResolver);
            XamlType xamlType = schemaContextProvider.SchemaContext.GetXamlType(typeName);
            XamlMember ambientProperty = xamlType.GetMember("Suffix");
            IEnumerable<AmbientPropertyValue> propVals = ambientValueProvider.GetAllAmbientValues(null, ambientProperty);
            string s = (string)value;
            foreach (AmbientPropertyValue val in propVals)
            {
                s += (string)val.Value;
            }
            return s;
        }
    }
}
