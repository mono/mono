#if NET_2_0
// Author.cs
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;

namespace Samples.AspNet.CS.Controls
{
    [
    TypeConverter(typeof(AuthorConverter))
    ]
    public class Author
    {
        private string firstnameValue;
        private string lastnameValue;
        private string middlenameValue;

        public Author()
            :
            this(String.Empty, String.Empty, String.Empty)
        {
        }

        public Author(string firstname, string lastname)
            :
            this(firstname, String.Empty, lastname)
        {
        }

        public Author(string firstname, 
                    string middlename, string lastname)
        {
            firstnameValue = firstname;
            middlenameValue = middlename;
            lastnameValue = lastname;
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("First name of author."),
        NotifyParentProperty(true),
        ]
        public virtual String FirstName
        {
            get
            {
                return firstnameValue;
            }
            set
            {
                firstnameValue = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("Last name of author."),
        NotifyParentProperty(true)
        ]
        public virtual String LastName
        {
            get
            {
                return lastnameValue;
            }
            set
            {
                lastnameValue = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("Middle name of author."),
        NotifyParentProperty(true)
        ]
        public virtual String MiddleName
        {
            get
            {
                return middlenameValue;
            }
            set
            {
                middlenameValue = value;
            }
        }

        public override string ToString()
        {
            return ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(CultureInfo culture)
        {
            return TypeDescriptor.GetConverter(
                GetType()).ConvertToString(null, culture, this);
        }
    }
}
#endif
