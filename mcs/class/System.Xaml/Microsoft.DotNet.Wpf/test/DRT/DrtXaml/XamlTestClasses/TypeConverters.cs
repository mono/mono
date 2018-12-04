using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Xaml;

namespace Test.Elements
{
    public class ColorElementConverter: TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            // XAML only converts from String to targettype.
            return (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
        {
            // XAML only converts targettype to string.
            return (destinationType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value)
        {
            String s = value as string;

            if (String.IsNullOrEmpty(s))
            {
                throw new ArgumentException("XAML can only convert from Strings");
            }

            int dashIdx = s.IndexOf('-');
            string color = s;
            string colorType = String.Empty;
            if(dashIdx > 1)
            {
                color = s.Substring(0, dashIdx);
                colorType = s.Substring(dashIdx+1);
            }

            ColorElement e=null;
            if (colorType == "Duel")
            {
                e = new ColorElementDuel();
                e.ColorName = color;
                return e;
            }
            else
            {
                e = new ColorElement();
            }

            IXamlSchemaContextProvider schemaContextProvider = (IXamlSchemaContextProvider)typeDescriptorContext.GetService(typeof(IXamlSchemaContextProvider));
            IAmbientProvider iAmbient = (IAmbientProvider)typeDescriptorContext.GetService(typeof(IAmbientProvider));

            XamlType ambientLabel = schemaContextProvider.SchemaContext.GetXamlType(typeof(HasAmbientLabel));
            XamlMember label = ambientLabel.GetMember("Label");
            AmbientPropertyValue apVal = iAmbient.GetFirstAmbientValue(null, label);
            if (apVal == null)
            {
                e.ColorName = color;
            }
            else
            {
                e.ColorName = color + "-" + apVal.Value.ToString();
            }
            return e;
        }

        public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
        {
            ColorElement e = value as ColorElement;

            return e.ColorName;
        }
    }

    public class ColorElement2Converter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            // XAML only converts from String to targettype.
            return (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
        {
            // XAML only converts targettype to string.
            return (destinationType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value)
        {
            String s = value as string;

            if (null == s)
            {
                throw new ArgumentException("XAML can only convert from Strings");
            }

            ColorElement2 e = new ColorElement2();

            IXamlSchemaContextProvider schemaContextProvider = (IXamlSchemaContextProvider)typeDescriptorContext.GetService(typeof(IXamlSchemaContextProvider));
            IAmbientProvider iAmbient = (IAmbientProvider)typeDescriptorContext.GetService(typeof(IAmbientProvider));

            XamlType ambientLabel = schemaContextProvider.SchemaContext.GetXamlType(typeof(DerivedFromHasAmbientLabel));
            XamlMember label = ambientLabel.GetMember("Num");
            AmbientPropertyValue apVal = iAmbient.GetFirstAmbientValue(null, label);
            if (apVal == null)
            {
                e.ColorName = s;
            }
            else
            {
                e.ColorName = s + "-" + apVal.Value.ToString();
            }
            return e;
        }

        public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
        {
            ColorElement2 e = value as ColorElement2;

            return e.ColorName;
        }
    }

    public class ColorListConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new ColorListTC() { MainColor = (string)value };
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return ((ColorListTC)value).MainColor;
        }
    }

    public class NamespacePrefixValidation
    {
        public BarContainer BarContainer { get; set; }
        public FooContainer FooContainer { get; set; }
        [System.Windows.Markup.XamlDeferLoad(typeof(FooContainerTemplateConverter), typeof(FooContainer))]       
        public Func<FooContainer> FooContainerTemplate { get; set; }
    }

    public class FooContainer
    {
        [TypeConverter(typeof(NamespacePrefixValidatingConverter))]
        public int Foo { get; set; }
    }

    public class BarContainer
    {
        public int Bar { get; set; }
    }

    public class FooContainerTemplateConverter : XamlDeferringLoader
    {
        public override object Load(XamlReader reader, IServiceProvider context)
        {
            IXamlObjectWriterFactory objectWriterFactory = (IXamlObjectWriterFactory) context.GetService(typeof(IXamlObjectWriterFactory));
            FooContainerFactory fooContainerFactory = new FooContainerFactory(objectWriterFactory, reader);
            return new Func<FooContainer>(() => fooContainerFactory.GetFooContainer());
        }

        public override XamlReader Save(object value, IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }

    public class FooContainerFactory
    {
        XamlNodeList nodeList;
        IXamlObjectWriterFactory objectWriterFactory;

        public FooContainerFactory(IXamlObjectWriterFactory objectWriterFactory, XamlReader xamlReader)
        {
            this.nodeList = new XamlNodeList(xamlReader.SchemaContext);
            XamlServices.Transform(xamlReader, nodeList.Writer);

            this.objectWriterFactory = objectWriterFactory;
        }

        public FooContainer GetFooContainer()
        {
            XamlObjectWriter objectWriter = this.objectWriterFactory.GetXamlObjectWriter(new XamlObjectWriterSettings());
            XamlServices.Transform(nodeList.GetReader(), objectWriter);
            return (FooContainer)objectWriter.Result;
        }
    }

    public class NamespacePrefixValidatingConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() != typeof(string))
            {
                return base.ConvertFrom(context, culture, value);
            }

            IXamlNamespaceResolver namespaceResolver = (IXamlNamespaceResolver)context.GetService(typeof(IXamlNamespaceResolver));

            int expectedNamespaceCount = int.Parse(value.ToString());
            int count = 0;
            foreach (NamespaceDeclaration namespaceDeclaration in namespaceResolver.GetNamespacePrefixes())
            {
                count++;
            }

            if (count != expectedNamespaceCount)
            {
                throw new Exception("Count of items returned from IXamlNamespaceResolver.GetNamespacePrefixes should be " + value);
            }

            return expectedNamespaceCount;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return value.ToString();
        }
    }

    //Classes to ensure that we still call type converters, even if they don't properly report that they CanConvertFrom.
    public class ConverterTestElement
    {
        [TypeConverter(typeof(ConverterWhichSaysItCannotConvertFromStringButReallyCan))]
        public string String { get; set; }
    }

    public class ConverterWhichSaysItCannotConvertFromStringButReallyCan : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string strValue = value as string;
            return strValue + " has been converted.";
        }
    }


    public class GenericTypeConverter<T> : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() != typeof(string))
            {
                return base.ConvertFrom(context, culture, value);
            }

            return Activator.CreateInstance(typeof(T), value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return value.ToString();
        }

    }

    [TypeConverter(typeof(GenericTypeConverter<ClassWithGenericConverter>))]
    public class ClassWithGenericConverter
    {
        public ClassWithGenericConverter(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public override string ToString()
        {
            return Value;
        }
    }

    internal class InternalTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new ElementWithTitle { Title = value.ToString() };
        }
    }


    internal class AnimalListConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                AnimalList animals = new AnimalList();
                string[] animalStrings = (value as string).Split(new char[] { '#', '$' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string animalString in animalStrings)
                {
                    string[] animalInfo = animalString.Split(':');
                    AnimalInfo animal = new AnimalInfo();
                    animal.Name = animalInfo[0];
                    animal.Number = Int32.Parse(animalInfo[1]);
                    animals.Add(animal);
                }

                return animals;
            }
            else
            {
                throw new ArgumentException("In ConvertFrom: can not convert to List<AnimalInfo>.");
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is AnimalList)
            {
                AnimalList animals = value as AnimalList;
                StringBuilder text = new StringBuilder();

                for (int i = 0; i < animals.Count; i++)
                {
                    AnimalInfo animal = animals[i];
                    text.Append(animal.Name + ":" + animal.Number);

                    if (i != animals.Count - 1)
                    {
                        text.Append("#");
                    }
                }
                return text.ToString();
            }
            else
            {
                throw new ArgumentException(string.Format("In ConvertTo: can not convert type '{0}' to string.", value.GetType().AssemblyQualifiedName));
            }
        }
    }
}
