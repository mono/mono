using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Xaml;

namespace Test.Elements
{
    public class SimpleMe: MarkupExtension
    {
        private string _display;

        public SimpleMe()
        {
            _display = "No Display Provided";
        }

        public SimpleMe(string display)
        {
            _display = display;
        }

        public SimpleMe(string display, string prefix)
        {
            _display = prefix + display;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Display;
        }

        public string Display
        {
            get { return _display; }
            set { _display = value; }
        }
    }

    public class ResourceLookupExtension: MarkupExtension
    {
        string _key;

        public ResourceLookupExtension()
        {
        }

        public ResourceLookupExtension(string key)
        {
            _key = key;
        }

        public string ResourceKey
        {
            get { return _key; }
            set { _key = value; }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IXamlSchemaContextProvider schemaContextProvider;
            IAmbientProvider ambient;
            schemaContextProvider = (IXamlSchemaContextProvider)serviceProvider.GetService(typeof(IXamlSchemaContextProvider));
            ambient = (IAmbientProvider)serviceProvider.GetService(typeof(IAmbientProvider));
            XamlType RwHolderType = schemaContextProvider.SchemaContext.GetXamlType(typeof(RWElementResourceHolder));
            XamlMember resourceProperty2 = RwHolderType.GetMember("Resources");
            XamlType RoHolderType = schemaContextProvider.SchemaContext.GetXamlType(typeof(ElementResourceHolder));
            XamlMember resourceProperty1 = RoHolderType.GetMember("Resources");
            IEnumerable<AmbientPropertyValue> aEnumerable = ambient.GetAllAmbientValues(null, resourceProperty1, resourceProperty2);
            if (aEnumerable == null)
            {
                return null;
            }
            foreach (AmbientPropertyValue aValue in aEnumerable)
            {
                if (resourceProperty1.Equals(aValue.RetrievedProperty) || resourceProperty2.Equals(aValue.RetrievedProperty))
                {
                    Dictionary<string, object> resources = (Dictionary<string, object>)aValue.Value;
                    object value;
                    if (resources.TryGetValue(_key, out value))
                    {
                        return value;
                    }
                }
            }
            string err = String.Format("Resource key {0} not found.", _key);
            throw new InvalidOperationException(err);
        }
    }

    [MarkupExtensionReturnType(typeof(int))]
    public class IntResourceExtension : MarkupExtension
    {

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return 1;
        }
    }

    public class DuplicateArityExtension : MarkupExtension
    {
        public DuplicateArityExtension(int n) { _value = n; }
        public DuplicateArityExtension(bool b) { _value = b; }

        private object _value;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _value.ToString();
        }
    }

    public class ColorElementExtension : MarkupExtension
    {
        public string MEColorName { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new ColorElement { ColorName = MEColorName };
        }
    }

    public class GenericMarkupExtension<T> : MarkupExtension
    {
        public T Value { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }

    public static class GenericMarkupExtension
    {
        public static void SetAP(object target, Element value)
        {
        }

        public static Element GetAP(object target)
        {
            return new Element();
        }
    }
}
