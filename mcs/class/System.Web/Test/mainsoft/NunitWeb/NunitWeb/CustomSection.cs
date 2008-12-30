#if NET_2_0

using System;
using System.Configuration;

namespace MonoTests.SystemWeb.Framework
{
    public class CustomSection : ConfigurationSection
    {
        [ConfigurationProperty ("sections", IsRequired = true)]
        public CustomSubSectionCollection AreaSections {
            get {
                return (CustomSubSectionCollection) base["sections"];
            }
            set {
                base["sections"] = value;
            }
        }
    }

    public class CustomSubSectionCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement () {
            return new CustomTagCollection ();
        }

        protected override object GetElementKey (ConfigurationElement element) {
            return (element as CustomTagCollection).Area;
        }
    }

    public class CustomTagCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement () {
            return new CustomTagElement ();
        }

        protected override object GetElementKey (ConfigurationElement element) {
            return ((CustomTagElement) element).Name;
        }

        [ConfigurationProperty ("area", DefaultValue = "UndefinedArea", IsKey = true, IsRequired = true)]
        public string Area {
            get {
                return (string) base["area"];
            }
        }
    }

    public class CustomTagElement : ConfigurationElement
    {
        [ConfigurationProperty ("name", DefaultValue = "CustomName", IsKey = true, IsRequired = true)]
        public string Name {
            get {
                return (string) base["name"];
            }
        }
    }
}

#endif
