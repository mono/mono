namespace Mfconsulting.General.Prj2Make.Schema.Cmbx {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Combine {
        
        /// <remarks/>
        public StartMode StartMode;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable=false)]
        public Entry[] Entries;
        
        /// <remarks/>
        public Configurations Configurations;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal fileversion;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string description;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class StartMode {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Execute")]
        public Execute[] Execute;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string startupentry;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string single;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Execute {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string entry;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Configuration {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Entry")]
        public Entry[] Entry;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Entry {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string filename;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string configurationname;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string build;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Configurations {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Configuration")]
        public Configuration[] Configuration;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string active;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Entries {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Entry")]
        public Entry[] Entry;
    }
}
