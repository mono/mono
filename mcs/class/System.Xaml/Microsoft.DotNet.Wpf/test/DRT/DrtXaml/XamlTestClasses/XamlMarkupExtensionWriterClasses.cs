namespace Test.Elements
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows.Markup;
    using System.Xml;
    using System.Collections.Generic;

    public class AddressContainer
    {
        public Address AddressInst
        { get; set; }
    }
    [TypeConverter(typeof(AddressConverter))]
    public class Address
    {
        public string Street
        { get; set; }
        public string City
        { get; set; }
        public string State
        { get; set; }
    }
    public class AddressConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            Address obj = value as Address;

            if (null == obj)
            {
                throw new ArgumentException();
            }

            AddressExtension extension = new AddressExtension();
            extension.Avenue = obj.Street;
            extension.Town = obj.City;
            extension.Province = obj.State;
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class AddressExtension : MarkupExtension
    {
        public string Avenue
        { get; set; }
        public string Town
        { get; set; }
        public string Province
        { get; set; }
    
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Address obj = new Address();
            obj.Street = Avenue;
            obj.City = Town;
            obj.State = Province;
            return obj;
        }
    }

    public class SimpleAttributableMEContainer
    {
        public SimpleAttributableMEContainer()
        {
            this.InstanceOfClassWithMEConverter = new ClassWithMEConverter();
        }
        public ClassWithMEConverter InstanceOfClassWithMEConverter
        { get; set; }
    }
    [TypeConverter(typeof(ClassWithMEConverterConverter))]
    public class ClassWithMEConverter
    {
        public int IntData
        { get; set; }
    }
    public class ClassWithMEConverterConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ClassWithMEConverter instance = value as ClassWithMEConverter;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ClassWithMEExtension extension = new ClassWithMEExtension();
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class ClassWithMEExtension : MarkupExtension
    {
        public ClassWithMEExtension()
        {
            IntData = 1; StringData = "hello";
        }

        public int IntData
        { get; set; }
        public string StringData
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ClassWithMEConverter c = new ClassWithMEConverter();
            return c;
        }
    }

    public class SimpleNonAttributableMEContainer
    {
        public SimpleNonAttributableMEContainer()
        {
            this.InstanceOfClassWithMEConverter = new ClassWithNonAttributableMEConverter();
        }
        public ClassWithNonAttributableMEConverter InstanceOfClassWithMEConverter
        { get; set; }
    }
    [TypeConverter(typeof(ClassWithNonAttributableMEConverterConverter))]
    public class ClassWithNonAttributableMEConverter
    {
        public int IntData
        { get; set; }
    }
    public class ClassWithNonAttributableMEConverterConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ClassWithNonAttributableMEConverter instance = value as ClassWithNonAttributableMEConverter;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ClassWithNonAttributableMEExtension extension = new ClassWithNonAttributableMEExtension();
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class ClassWithNonAttributableMEExtension : MarkupExtension
    {
        public ClassWithNonAttributableMEExtension()
        {
            IntData = 1;
            StringData = "hello";
            ComplexTypeInstance = new ComplexType();
        }

        public int IntData
        { get; set; }
        public string StringData
        { get; set; }
        public ComplexType ComplexTypeInstance
        { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ClassWithNonAttributableMEConverter c = new ClassWithNonAttributableMEConverter();
            return c;
        }
    }
    public class ComplexType
    {
        public ComplexType()
        {
            IntData = 2;
        }
        public int IntData
        { get; set; }
    }

    public class NestedMEContainer
    {
        public NestedMEContainer()
        {
            this.InstanceOfClassWithMEConverter = new ClassWithNestedMEConverter();
        }
        public ClassWithNestedMEConverter InstanceOfClassWithMEConverter
        { get; set; }
    }
    [TypeConverter(typeof(ClassWithNestedMEConverterConverter))]
    public class ClassWithNestedMEConverter
    {
        public int IntData
        { get; set; }
    }
    public class ClassWithNestedMEConverterConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ClassWithNestedMEConverter instance = value as ClassWithNestedMEConverter;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ClassWithNestedMEExtension extension = new ClassWithNestedMEExtension();
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class ClassWithNestedMEExtension : MarkupExtension
    {
        public ClassWithNestedMEExtension()
        {
            IntData = 1;
            StringData = "hello";
            ComplexTypeInstance = new ComplexType();
        }

        public int IntData
        { get; set; }
        public string StringData
        { get; set; }

        [TypeConverter(typeof(ComplexTypeConverter))]
        public ComplexType ComplexTypeInstance
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ClassWithNestedMEConverter c = new ClassWithNestedMEConverter();
            return c;
        }
    }
    public class ComplexTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ComplexType instance = value as ComplexType;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ComplexTypeExtension extension = new ComplexTypeExtension();
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class ComplexTypeExtension : MarkupExtension
    {
        public ComplexTypeExtension()
        {
            IntData = 100; StringData = "complex";
        }

        public int IntData
        { get; set; }
        public string StringData
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ComplexType c = new ComplexType();
            return c;
        }
    }


    public class MEWithStringConvertersContainer
    {
        public MEWithStringConvertersContainer()
        {
            InstanceOfClassWithStringConverter = new ClassWithStringConverter();
            this.InstanceOfClassWithMEConverter = new ClassWithNestedMEConverter2();
        }
        public ClassWithStringConverter InstanceOfClassWithStringConverter
        { get; set; }
        public ClassWithNestedMEConverter2 InstanceOfClassWithMEConverter
        { get; set; }
    }
    [TypeConverter(typeof(ClassWithNestedMEConverter2Converter))]
    public class ClassWithNestedMEConverter2
    {
        public int IntData
        { get; set; }
    }
    public class ClassWithNestedMEConverter2Converter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ClassWithNestedMEConverter2 instance = value as ClassWithNestedMEConverter2;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ClassWithNestedMEExtension2 extension = new ClassWithNestedMEExtension2();
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class ClassWithNestedMEExtension2 : MarkupExtension
    {
        public ClassWithNestedMEExtension2()
        {
            IntData = 1;
            InstanceOfClassWithStringConverter = new ClassWithStringConverter();
            ComplexTypeInstance = new ComplexType();
        }

        public int IntData
        { get; set; }

        public ClassWithStringConverter InstanceOfClassWithStringConverter
        { get; set; }

        [TypeConverter(typeof(ComplexTypeConverter2))]
        public ComplexType ComplexTypeInstance
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ClassWithNestedMEConverter2 c = new ClassWithNestedMEConverter2();
            return c;
        }
    }
    public class ComplexTypeConverter2 : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ComplexType instance = value as ComplexType;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ComplexTypeExtension2 extension = new ComplexTypeExtension2();
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class ComplexTypeExtension2 : MarkupExtension
    {
        public ComplexTypeExtension2()
        {
            IntData = 100;
            InstanceOfClassWithStringConverter = new ClassWithStringConverter();
        }

        public int IntData
        { get; set; }
        public ClassWithStringConverter InstanceOfClassWithStringConverter
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ComplexType c = new ComplexType();
            return c;
        }
    }
    [TypeConverter(typeof(MyStringConverter))]
    public class ClassWithStringConverter
    {
        public ClassWithStringConverter()
        {
            X = 1; Y = 2;
        }
        public int X
        { get; set; }
        public int Y
        { get; set; }
    }
    public class MyStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string s = value as string;
            if (s == null)
            {
                throw new ArgumentException();
            }

            return new ClassWithTypeConverter();
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                ClassWithStringConverter c = value as ClassWithStringConverter;
                if (c == null)
                {
                    throw new ArgumentException();
                }
                return "(" + c.X.ToString() + "," + c.Y.ToString() + ")";
            }
            return false;
        }
    }

    public class SimpleMEWithCstrArgsContainer
    {
        public SimpleMEWithCstrArgsContainer()
        {
            this.InstanceOfClassWithMEConverter = new ClassWithMEConverterWithCstrArgs();
        }
        public ClassWithMEConverterWithCstrArgs InstanceOfClassWithMEConverter
        { get; set; }
    }
    [TypeConverter(typeof(ClassWithMEConverterWithCstrArgsConverter))]
    public class ClassWithMEConverterWithCstrArgs
    {
        internal ClassWithMEConverterWithCstrArgs()
        {
            IntData = 3;
        }
        public int IntData
        { get; set; }
    }
    public class ClassWithMEConverterWithCstrArgsConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ClassWithMEConverterWithCstrArgs instance = value as ClassWithMEConverterWithCstrArgs;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ClassWithMEWithCstrArgsExtension extension = new ClassWithMEWithCstrArgsExtension();
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class ClassWithMEWithCstrArgsExtension : MarkupExtension
    {
        internal ClassWithMEWithCstrArgsExtension()
        {
            IntData = 1; StringData = "hello";
        }
        public ClassWithMEWithCstrArgsExtension(int intData, string stringData)
        {
            IntData = intData;
            StringData = stringData;
        }
        [ConstructorArgument("intData")]
        public int IntData
        { get; set; }
        [ConstructorArgument("stringData")]
        public string StringData
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ClassWithMEConverterWithCstrArgs c = new ClassWithMEConverterWithCstrArgs();
            return c;
        }
    }

    public class MEWithParamCstrArgsContainer
    {
        public MEWithParamCstrArgsContainer()
        {
            this.InstanceOfClassWithMEConverter = new ClassWithMEConverterWithParamCstrArgs();
        }
        public ClassWithMEConverterWithParamCstrArgs InstanceOfClassWithMEConverter
        { get; set; }
    }
    [TypeConverter(typeof(ClassWithMEConverterWithParamCstrArgsConverter))]
    public class ClassWithMEConverterWithParamCstrArgs
    {
        public int IntData
        { get; set; }
    }
    public class ClassWithMEConverterWithParamCstrArgsConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ClassWithMEConverterWithParamCstrArgs instance = value as ClassWithMEConverterWithParamCstrArgs;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ClassWithMEWithParamCstrArgsExtension extension = new ClassWithMEWithParamCstrArgsExtension(new int[] { 1, 2, 3 });
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class ClassWithMEWithParamCstrArgsExtension : MarkupExtension
    {
        public ClassWithMEWithParamCstrArgsExtension(params int[] paramData)
        {
            ParamData = paramData;
        }
        [ConstructorArgument("paramData")]
        public int[] ParamData
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ClassWithMEConverterWithParamCstrArgs c = new ClassWithMEConverterWithParamCstrArgs();
            return c;
        }
    }

    public class NestedMEWithCstrArgsContainer
    {
        public NestedMEWithCstrArgsContainer()
        {
            this.InstanceOfClassWithMEConverter = new ClassWithNestedMEConverterWithCstrArgs();
        }
        public ClassWithNestedMEConverterWithCstrArgs InstanceOfClassWithMEConverter
        { get; set; }
    }
    [TypeConverter(typeof(ClassWithNestedMEConverterWithCstrArgsConverter))]
    public class ClassWithNestedMEConverterWithCstrArgs
    {
        public int IntData
        { get; set; }
    }
    public class ClassWithNestedMEConverterWithCstrArgsConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ClassWithNestedMEConverterWithCstrArgs instance = value as ClassWithNestedMEConverterWithCstrArgs;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ClassWithNestedMEWithCstrArgsExtension extension = new ClassWithNestedMEWithCstrArgsExtension();
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class ClassWithNestedMEWithCstrArgsExtension : MarkupExtension
    {
        public ClassWithNestedMEWithCstrArgsExtension()
        {
            IntData = 1;
            StringData = "hello";
            ComplexTypeInstanceForCstrArgs = new ComplexType();
            ComplexTypeInstance = new ComplexType();
        }

        public ClassWithNestedMEWithCstrArgsExtension(int intData, ComplexType compData)
        {
            IntData = intData;
            ComplexTypeInstanceForCstrArgs = compData;
            ComplexTypeInstance = new ComplexType();
        }

        [ConstructorArgument("intData")]
        public int IntData
        { get; set; }
        public string StringData
        { get; set; }

        [ConstructorArgument("compData")]
        [TypeConverter(typeof(ComplexTypeConverter3))]
        public ComplexType ComplexTypeInstanceForCstrArgs
        { get; set; }

        [TypeConverter(typeof(ComplexTypeConverter3))]
        public ComplexType ComplexTypeInstance
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ClassWithNestedMEConverterWithCstrArgs c = new ClassWithNestedMEConverterWithCstrArgs();
            return c;
        }
    }

    public class ClassWithNestedMEWithCstrArgsContainer2
    {
        public ClassWithNestedMEWithCstrArgsExtension2 property { get; set; }
    }

    public class ClassWithNestedMEWithCstrArgsExtension2 : MarkupExtension
    {
        public ClassWithNestedMEWithCstrArgsExtension2(int intData, ComplexTypeExtension3 compData)
        {
            IntData = intData;
            ComplexTypeInstanceForCstrArgs = compData;
            ComplexTypeInstance = new ComplexType();
        }

        [ConstructorArgument("intData")]
        public int IntData
        { get; set; }
        public string StringData
        { get; set; }

        [ConstructorArgument("compData")]
        public ComplexTypeExtension3 ComplexTypeInstanceForCstrArgs
        { get; set; }

        [TypeConverter(typeof(ComplexTypeConverter3))]
        public ComplexType ComplexTypeInstance
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ClassWithNestedMEConverterWithCstrArgs c = new ClassWithNestedMEConverterWithCstrArgs();
            return c;
        }
    }
    public class ComplexTypeConverter3 : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ComplexType instance = value as ComplexType;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ComplexTypeExtension3 extension = new ComplexTypeExtension3();
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class ComplexTypeExtension3 : MarkupExtension
    {
        public ComplexTypeExtension3()
        {
            IntData = 100; StringData = "complex";
        }

        public int IntData
        { get; set; }
        public string StringData
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ComplexType c = new ComplexType();
            return c;
        }
    }

    public class ContainingRefClass
    {
        public ContainingRefClass()
        {
            Be = new MERefTestExtension();
            B = (MERefTest)Be.ProvideValue(null);
            Be.Value = B;
        }

        public MERefTest B
        { get; set; }
        public MERefTestExtension Be
        { get; set; }
    }


    public class Developer
    {
        public string Name { get; set; }
        public DeveloperExtension Friend { get; set; }
    }

    public class DeveloperExtension : MarkupExtension
    {
        public string Name { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new DeveloperExtension();
        }
    }

    public class SimpleMEContainer2
    {
        public SimpleMEContainer2()
        {
            this.InstanceOfClassWithMEConverter = new ClassWithMEConverter();
        }
        [TypeConverter(typeof(ClassWithMEConverterConverter2))]
        public ClassWithMEConverter InstanceOfClassWithMEConverter
        { get; set; }
    }

    public class ClassWithMEConverterConverter2 : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            ClassWithMEConverter instance = value as ClassWithMEConverter;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            ClassWithMEExtension2 extension = new ClassWithMEExtension2();
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }

    public class ClassWithMEExtension2 : MarkupExtension
    {
        public ClassWithMEExtension2()
        {
            IntData = 1;
            StringData1 = "Has a Space";
            StringData2 = @"I'm ""cool"", 1 = 2.  {Math is \magic\} ";
        }

        public int IntData
        { get; set; }
        public string StringData1
        { get; set; }
        public string StringData2
        { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ClassWithMEConverter c = new ClassWithMEConverter();
            return c;
        }
    }

    public class MEWithCollectionContainer
    {
        public MEWithCollectionContainer()
        {
            this.InstanceOfClassWithMEWithCollectionConverter = new ClassWithMEWithCollectionConverter();
        }
        public ClassWithMEWithCollectionConverter InstanceOfClassWithMEWithCollectionConverter
        { get; set; }
    }
    [TypeConverter(typeof(ClassWithMEWithCollectionConverterConverter))]
    public class ClassWithMEWithCollectionConverter
    {
        public int IntData
        { get; set; }
    }
    public class ClassWithMEWithCollectionConverterConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException();
            }

            var instance = value as ClassWithMEWithCollectionConverter;

            if (null == instance)
            {
                throw new ArgumentException();
            }

            CollectionME extension = new CollectionME() { IntData = 2 };
            return extension;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }
    public class CollectionME : MarkupExtension
    {
        public CollectionME()
        {
            listData = new List<int>();
        }

        List<int> listData;
        public int IntData
        { get; set; }

        public List<int> ListData
        {
            get
            {
                return this.listData;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var c = new ClassWithMEWithCollectionConverter();
            return c;
        }
    }

    [TypeConverter(typeof(CustomMEXargsConverter))]
    public class CustomMEXargsClass
    {
        public CustomMEXargsClass()
        {
        }

        public object Content { get; set; }
    }

    public class CustomMEXargsConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupExtension);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(MarkupExtension))
            {
                throw new ArgumentException("destinationType is not a MarkupExtension", "destinationType");
            }

            CustomMEXargsClass customMEXargsClass = value as CustomMEXargsClass;

            if (null == customMEXargsClass)
            {
                throw new ArgumentException();
            }

            CustomMEWithXArgs extension = new CustomMEWithXArgs(customMEXargsClass.Content);

            return extension;
        }


        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            throw new NotSupportedException();
        }
    }


    public class CustomMEWithXArgs : MarkupExtension
    {
        //private CustomMEClass CustomMEClass;

        public CustomMEWithXArgs(object content)
        {
        }

        [ConstructorArgument("content")]
        public object Content { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return null;
        }
    }
}
