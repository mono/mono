namespace Mfconsulting.General.Prj2Make.Schema.Csproj02 {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Build {
        
        /// <remarks/>
        public Settings Settings;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable=false)]
        public Reference[] References;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Settings {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Config")]
        public Config[] Config;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ApplicationIcon;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AssemblyKeyContainerName;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AssemblyName;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AssemblyOriginatorKeyFile;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DefaultClientScript;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DefaultHTMLPageLayout;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DefaultTargetSchema;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool DelaySign;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OutputType;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RootNamespace;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StartupObject;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Config {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ConfigName Name;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool AllowUnsafeBlocks;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BaseAddress;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool CheckForOverflowUnderflow;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ConfigurationOverrideFile;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DefineConstants;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DocumentationFile;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool DebugSymbols;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public short FileAlignment;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool IncrementalBuild;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Optimize;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OutputPath;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool RegisterForComInterop;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool RemoveIntegerChecks;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool TreatWarningsAsErrors;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.SByte WarningLevel;
    }
    
    /// <remarks/>
    public enum ConfigName {
        
        /// <remarks/>
        Debug,
        
        /// <remarks/>
        Release,
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Reference {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AssemblyName;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string HintPath;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class CSHARP {
        
        /// <remarks/>
        public Build Build;
        
        /// <remarks/>
        public Files Files;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ProjectType;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ProductVersion;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal SchemaVersion;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ProjectGuid;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Files {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(typeof(Folder), IsNullable=false)]
        [System.Xml.Serialization.XmlArrayItemAttribute(typeof(File), IsNullable=false)]
        public object[] Include;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Folder {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RelPath;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
        public string WebReferenceUrl;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UrlBehavior;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string WebReferences;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class File {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RelPath;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SubType;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public FileBuildAction BuildAction;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DependentUpon;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Generator;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LastGenOutput;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DesignTime;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AutoGen;
    }
    
    /// <remarks/>
    public enum FileBuildAction {
        
        /// <remarks/>
        Compile,
        
        /// <remarks/>
        Content,
        
        /// <remarks/>
        EmbeddedResource,
        
        /// <remarks/>
        None,
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Include {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Folder", typeof(Folder))]
        [System.Xml.Serialization.XmlElementAttribute("File", typeof(File))]
        public object[] Items;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class References {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Reference")]
        public Reference[] Reference;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class VisualStudioProject {
        
        /// <remarks/>
        public CSHARP CSHARP;
    }
}
