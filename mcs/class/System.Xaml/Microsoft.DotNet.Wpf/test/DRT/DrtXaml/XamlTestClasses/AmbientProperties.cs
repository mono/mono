using System;
using System.Collections.Generic;
using System.Text;
using System.Xaml;
using System.Windows.Markup;
using System.ComponentModel;
using System.Globalization;

namespace Test.Elements
{
    [ContentPropertyAttribute("Child")]
    public class HasAmbientLabel: Element
    {
        [Ambient]
        public string Label { get; set; }

        public Element Child { get; set; }
    }

    public class DerivedFromHasAmbientLabel : HasAmbientLabel
    {
        [Ambient]
        public int Num { get; set; }
    }

    public class ElementResourceDictionary : Dictionary<string, object> { }
    public class ElementChildren : List<Element> { }

    [ContentPropertyAttribute("Children")]
    public class ElementResourceHolder : Element
    {
        private ElementResourceDictionary _resources;
        private ElementChildren _children;

        public ElementResourceHolder()
        {
            _resources = new ElementResourceDictionary();
            _children = new ElementChildren();
        }

        [Ambient]
        public ElementResourceDictionary Resources{ get { return _resources; } }
        public ElementChildren Children { get { return _children; } }
    }

    [ContentPropertyAttribute("Children")]
    public class RWElementResourceHolder : Element
    {
        private ElementResourceDictionary _resources;
        private ElementChildren _children;

        public RWElementResourceHolder()
        {
            _resources = new ElementResourceDictionary();
            _children = new ElementChildren();
        }

        [Ambient]
        public ElementResourceDictionary Resources
        {
            get { return _resources; }
            set { _resources = value; }
        }
        public ElementChildren Children { get { return _children; } }
    }

    [ContentProperty("Content")]
    public class AmbientMe : MarkupExtension
    {
        public int Content { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IAmbientProvider ambient = serviceProvider.GetService(typeof(IAmbientProvider)) as IAmbientProvider;
            ambient.GetAllAmbientValues(null);
            return new object();
        }
    }

    public class FooAmbient
    {
        [Ambient]
        public string Text { get; set; }

        [Ambient]
        public AmbientTC AmbientTC { get; set; }
    }

    [TypeConverter(typeof(AmbientTCConverter))]
    public class AmbientTC
    {
        public AmbientTC()
        {
        }

        public string Text { get; set; }
    }

    /// <summary>
    /// AmbientTCConverter class
    /// </summary>
    public class AmbientTCConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            // XAML only converts from String to targettype.
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
        {
            // XAML only converts targettype to string.
            return destinationType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value)
        {
            string s = value as string;
            if (null == s)
            {
                throw new ArgumentException("XAML can only convert from Strings");
            }

            AmbientTC atc = new AmbientTC();
            IXamlSchemaContextProvider xscProvider = (IXamlSchemaContextProvider)typeDescriptorContext.GetService(typeof(IXamlSchemaContextProvider));
            IAmbientProvider iAmbient = (IAmbientProvider)typeDescriptorContext.GetService(typeof(IAmbientProvider));

            XamlType ambientSP = xscProvider.SchemaContext.GetXamlType(typeof(FooAmbient));
            XamlMember textProp = ambientSP.GetMember("AmbientTC");
            AmbientPropertyValue ambientPropVal = iAmbient.GetFirstAmbientValue(null, textProp);
            if (ambientPropVal == null)
            {
                atc.Text = s;
            }
            else
            {
                atc.Text = s + "-" + (ambientPropVal.Value as string);
            }

            return atc;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [Ambient]
    public class AmbientType
    {
        public string Prop1 { get; set; }
        public Object Prop2 { get; set; }
    }

    public class AmbientTypeME : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            XamlSchemaContext xsc = (serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider).SchemaContext;
            IAmbientProvider ambient = serviceProvider.GetService(typeof(IAmbientProvider)) as IAmbientProvider;
            return (ambient.GetFirstAmbientValue(xsc.GetXamlType(typeof(AmbientType))) as AmbientType).Prop1;
        }
    }
}
