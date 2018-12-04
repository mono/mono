using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Xaml;

namespace Test.Elements
{
    public class DestinationTypeConverter : TypeConverter
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

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                IDestinationTypeProvider targetService = context.GetService(typeof(IDestinationTypeProvider)) as IDestinationTypeProvider;
                Type targetType = targetService.GetDestinationType();
                return Activator.CreateInstance(targetType);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return value.GetType().ToString();
        }
    }

    public class DestinationTypeProviderTestContainer
    {
        List<DestinationType> destinationList = new List<DestinationType>();

        public DestinationType<int> DestinationType { get; set; }
        
        [TypeConverter(typeof(DestinationTypeConverter))]
        public DestinationType DestinationProperty { get; set; }

        public List<DestinationType> DestinationTypeList 
        {
            get { return this.destinationList; }
        }
    }

    [TypeConverter(typeof(DestinationTypeConverter))]
    public class DestinationType
    {
    }

    public class DestinationType<T> : DestinationType
    {
    }
}
