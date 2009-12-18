#if NET_2_0
// AuthorConverter.cs
using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Samples.AspNet.CS.Controls
{
    public class AuthorConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(
            ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(
            ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext 
            context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return new Author();
            }

            if (value is string)
            {
                string s = (string)value;
                if (s.Length == 0)
                {
                    return new Author();
                }

                string[] parts = s.Split(' ');

                        // Determine if name is stored as first and 
                        // last; first, middle, and last;
                        // or is in error.
                if ((parts.Length < 2) || (parts.Length > 3))
                {
                    throw new ArgumentException(
                        "Name must have 2 or 3 parts.", "value");
                }

                if (parts.Length == 2)
                {
                    return new Author(parts[0], parts[1]);
                }

                if (parts.Length == 3)
                {
                    return new Author(parts[0], parts[1], parts[2]);
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (value != null)
            {
                if (!(value is Author))
                {
                    throw new ArgumentException(
                        "Invalid Author", "value");
                }
            }

            if (destinationType == typeof(string))
            {
                if (value == null)
                {
                    return String.Empty;
                }

                Author auth = (Author)value;

                if (auth.MiddleName != String.Empty)
                {
                    return String.Format("{0} {1} {2}",
                        auth.FirstName,
                        auth.MiddleName,
                        auth.LastName);
                }
                else
                {
                    return String.Format("{0} {1}",
                         auth.FirstName,
                        auth.LastName);
                }
            }

            return base.ConvertTo(context, culture, value, 
                destinationType);
        }
    }
}
#endif
