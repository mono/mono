namespace Test.Elements
{
    using System;
    using System.Text;
    using System.Xaml;
    using System.Windows.Markup;
    using System.ComponentModel;
    using System.Collections.Generic;

    public abstract class Vehicle
    {
    }

    public class BikeVS : ValueSerializer
    {
        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return true;
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            Bicycle bike = value as Bicycle;
            return "a " + bike.Brand + " bicycle made in " + bike.Year;
        }
    }

    [ValueSerializer(typeof(BikeVS))]
    [TypeConverter(typeof(FakeTC))]
    public class Bicycle : Vehicle
    {
        public string Brand { get; set; }
        public int Year { get; set; }
    }

    public class CarVS : ValueSerializer
    {
        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return (value is Sedan) || (value is Hatchback);
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            Car car = (Car)value;
            StringBuilder sb = new StringBuilder("a " + car.Brand + " " + car.Year + " " + car.Model);

            if (value is Sedan)
            {
                Sedan sedan = (Sedan)value;
                return sb.Append(" sedan " + sedan.Trim).ToString();
            }
            else
            {
                Hatchback hatchback = (Hatchback)value;
                string canCarryBikes = hatchback.CanCarryBikes ? "which can carry bikes" : "which cannot carry bikes";
                return sb.Append(" hatchback " + canCarryBikes).ToString();
            }
        }
    }

    [ValueSerializer(typeof(CarVS))]
    [TypeConverter(typeof(FakeTC))]
    public abstract class Car : Vehicle
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
    }

    public class Sedan : Car
    {
        public string Trim { get; set; }
    }

    public class Hatchback : Car
    {
        public bool CanCarryBikes { get; set; }
    }

    public class Coupe : Car
    {
        public string Description { get; set; }
    }

    public class MotorboatVS : ValueSerializer
    {
        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return value is SpeedBoat;
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            return "Speedboat with max speed set at " + ((SpeedBoat)value).MaxSpeed;
        }
    }

    public class FakeTC : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            throw new NotImplementedException();
        }
    }

    public class MotorboatTC : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            int len = "Speedboat with max speed set at ".Length;
            string speedboat = (string)value;
            return new SpeedBoat { MaxSpeed = Int32.Parse(speedboat.Substring(len, speedboat.Length - len)) };
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            throw new Exception();
        }
    }

    [ValueSerializer(typeof(MotorboatVS))]
    [TypeConverter(typeof(MotorboatTC))]
    public abstract class Motorboat : Vehicle
    {
    }

    public class Cruiser : Motorboat
    {
        public string Description { get; set; }
    }

    public class SpeedBoat : Motorboat
    {
        public int MaxSpeed { get; set; }
    }

    public class EmptyBoat : Motorboat
    {
    }

    public class MotorcycleTC : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            throw new Exception();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string description = (string) value;

            int len1 = "a motorcycle with great fuel economy at ".Length;
            int len2 = "MPG".Length;
            string MPG = description.Substring(len1, description.Length - len1 - len2);
            return new Motorcycle { MPG = Int32.Parse(MPG) };
        }
    }

    public class MotorcycleVS : ValueSerializer
    {
        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return true;
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            return "a motorcycle with great fuel economy at " + ((Motorcycle)value).MPG + "MPG";
        }

        public override bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            throw new Exception();
        }

        public override object ConvertFromString(string value, IValueSerializerContext context)
        {
            throw new Exception();
        }
    }

    [TypeConverter(typeof(MotorcycleTC))]
    [ValueSerializer(typeof(MotorcycleVS))]
    public class Motorcycle : Vehicle
    {
        public float MPG { get; set; }
    }

    public class Motorcycle2VS : ValueSerializer
    {
        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return true;
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            return "a motorcycle with great fuel economy at " + ((Motorcycle2)value).MPG + "MPG";
        }

        public override bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            return true;
        }

        public override object ConvertFromString(string value, IValueSerializerContext context)
        {
            return new Motorcycle2 { MPG = 100 };
        }
    }

    public class Motorcycle4VS : ValueSerializer
    {
        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return true;
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            return "a motorcycle with great fuel economy at " + ((Motorcycle4)value).MPG + "MPG";
        }

        public override bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            return true;
        }

        public override object ConvertFromString(string value, IValueSerializerContext context)
        {
            return new Motorcycle4 { MPG = 100 };
        }
    }

    [ValueSerializer(typeof(Motorcycle2VS))]
    [TypeConverter(typeof(FakeTC))]
    public class Motorcycle2 : Vehicle
    {
        public float MPG { get; set; }
    }


    public class Motorcycle3 : Vehicle
    {
        public float MPG { get; set; }
    }

    [TypeConverter(typeof(FakeTC))]
    public class Motorcycle4 : Vehicle
    {
        public float MPG { get; set; }
    }

    public class VSContainer
    {
        public Vehicle Vehicle { get; set; }
    }

    public class VehicleVS : ValueSerializer
    {
        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return value is Car;
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            Car car = (Car)value;
            return "Car: " + car.Brand;
        }
    }

    public class VSContainer2
    {
        [ValueSerializer(typeof(VehicleVS))]
        public Vehicle Vehicle { get; set; }
    }

    public class VSContainer3
    {
        public Vehicle[] Vehicles { get; set; }
    }

    public class VSContainer4
    {
        [ValueSerializer(typeof(Motorcycle2VS))]
        public Vehicle Vehicle { get; set; }
    }

    public class VSContainer5
    {
        [ValueSerializer(typeof(Motorcycle4VS))]
        public Vehicle Vehicle { get; set; }
    }

    public class AlwaysThrowVS : ValueSerializer
    {
        public override bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            return true;
        }

        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return true;
        }

        public override object ConvertFromString(string value, IValueSerializerContext context)
        {
            throw new InvalidOperationException("This ValueSerialiezr should never be called.");
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            throw new InvalidOperationException("This ValueSerializer should never be called.");
        }
    }

    public class VSOnStringPropertyContainer
    {
        [ValueSerializer(typeof(AlwaysThrowVS))]
        [TypeConverter(typeof(FakeTC))]
        public string Prop { get; set; }       
    }
}