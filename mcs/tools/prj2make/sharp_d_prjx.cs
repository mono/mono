namespace Mfconsulting.General.Prj2Make.Schema.Prjx {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class CodeGeneration {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string runtime;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string compiler;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.SByte warninglevel;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string nowarn;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public CodeGenerationIncludedebuginformation includedebuginformation;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string optimize;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public CodeGenerationUnsafecodeallowed unsafecodeallowed;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string generateoverflowchecks;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string mainclass;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string target;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string definesymbols;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string generatexmldocumentation;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string win32Icon;
    }
    
    /// <remarks/>
    public enum CodeGenerationIncludedebuginformation {
        
        /// <remarks/>
        False,
        
        /// <remarks/>
        True,
    }
    
    /// <remarks/>
    public enum CodeGenerationUnsafecodeallowed {
        
        /// <remarks/>
        False,
        
        /// <remarks/>
        True,
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Configuration {
        
        /// <remarks/>
        public CodeGeneration CodeGeneration;
        
        /// <remarks/>
        public Execution Execution;
        
        /// <remarks/>
        public Output Output;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string runwithwarnings;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Execution {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string commandlineparameters;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string consolepause;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Output {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string directory;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string assembly;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string executeScript;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string executeBeforeBuild;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string executeAfterBuild;
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
    public class Contents {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("File")]
        public File[] File;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class File {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public FileSubtype subtype;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public FileBuildaction buildaction;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string dependson;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string data;
    }
    
    /// <remarks/>
    public enum FileSubtype {
        
        /// <remarks/>
        Code,
        
        /// <remarks/>
        Directory,
    }
    
    /// <remarks/>
    public enum FileBuildaction {
        
        /// <remarks/>
        Nothing,
        
        /// <remarks/>
        Compile,
        
        /// <remarks/>
        EmbedAsResource,
        
        /// <remarks/>
        Exclude,
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class DeploymentInformation {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string target;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string script;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string strategy;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Project {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable=false)]
        public File[] Contents;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable=false)]
        public Reference[] References;
        
        /// <remarks/>
        public DeploymentInformation DeploymentInformation;
        
        /// <remarks/>
        public Configuration Configuration;
        
        /// <remarks/>
        public Configurations Configurations;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string description;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string newfilesearch;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string enableviewstate;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Decimal version;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string projecttype;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class Reference {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ReferenceType type;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string refto;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ReferenceLocalcopy localcopy;
    }
    
    /// <remarks/>
    public enum ReferenceType {
        
        /// <remarks/>
        Assembly,
        
        /// <remarks/>
        Gac,
        
        /// <remarks/>
        Project,
    }
    
    /// <remarks/>
    public enum ReferenceLocalcopy {
        
        /// <remarks/>
        False,
        
        /// <remarks/>
        True,
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public class References {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Reference")]
        public Reference[] Reference;
    }
}
