using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
namespace System.Management.Instrumentation
{
    #region CommonUMPAttributes


    /// <summary>
    /// This attribute declares a class to be exposed as a management
    /// interface.   
    /// 
    /// It declares the noun to expose in Monad and
    /// optionally the XML Namespace to expose the class 
    /// through WMI.NET and WS-Management.
    /// 
    /// </summary>
    /// 

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false,Inherited=false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementEntityAttribute : Attribute
    {
        public ManagementEntityAttribute()
        {
        }

        public string Name
        {
            get { return _nounName; }
            set
            {
                _nounName = value;
            }
        }

        public bool External
        {
            get { return _isExternalClass; }
            set
            {
                _isExternalClass = value;
            }
        }
        public bool Singleton
        {
            get { return _isSingleton; }
            set
            {
                _isSingleton = value;
            }
        }
            

        private string _nounName;
        private bool   _isExternalClass = false;
        private bool _isSingleton = false;

/*
        /// <summary>
        /// Reference to the Type which acts as a factory for instances
        /// of this class.
        /// </summary>
        /// 
        public Type Factory
        {
            get { return _factory; }
            set { _factory = value; }
        }
        private Type _factory;

        public Type FactoryFor
        {
            get { return _factoryfor; }
            set { _factoryfor = value; }
        }
        private Type _factoryfor;
*/
    }



    #endregion CommonUMPAttributes
    /// <remarks>
    /// WMI is able to deal with Decoupled and Hosted providers. 
    /// UserHosted for component loaded inproc to the client is not allowed for .NET extension providers.

    public enum ManagementHostingModel
    {
        Decoupled,
        NetworkService,
        LocalService,
        LocalSystem
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class WmiConfigurationAttribute : Attribute
    {
        private string _Scope = null;
        private string _SecurityRestriction = null;
        private string _NamespaceSecurity = null;
        private ManagementHostingModel _HostingModel = ManagementHostingModel.Decoupled;
        private string _HostingGroup = null;
        private bool _IdentifyLevel = true;

        public WmiConfigurationAttribute(string scope) 
        { 
            string namespaceName = scope;
            if (namespaceName != null)
                namespaceName = namespaceName.Replace('/', '\\');

            if (namespaceName == null || namespaceName.Length == 0)
                namespaceName = "root\\default";


            bool once = true;
            foreach (string namespacePart in namespaceName.Split('\\'))
            {
                if (namespacePart.Length == 0
                    || (once && String.Compare(namespacePart, "root", StringComparison.OrdinalIgnoreCase) != 0)  // Must start with 'root'
                    || !Regex.Match(namespacePart, @"^[a-z,A-Z]").Success // All parts must start with letter
                    || Regex.Match(namespacePart, @"_$").Success // Must not end with an underscore
                    || Regex.Match(namespacePart, @"[^a-z,A-Z,0-9,_,\u0080-\uFFFF]").Success) // Only letters, digits, or underscores
                {
                    //ManagementException.ThrowWithExtendedInfo(ManagementStatus.InvalidNamespace);
                }
                once = false;
            }

            _Scope = namespaceName;

        }

        /// <remarks>
        /// The security descriptor used by instrumentation to filter the providers
        public string SecurityRestriction
        {
            get { return _SecurityRestriction; }
            set { _SecurityRestriction = value; }
        }
        public string NamespaceSecurity
        {
            get { return _NamespaceSecurity; }
            set { _NamespaceSecurity = value; }
        }
        public bool IdentifyLevel
        {
            get { return _IdentifyLevel; }
            set { _IdentifyLevel = value; }
        }
        public ManagementHostingModel HostingModel
        {
            get { return _HostingModel; }
            set { _HostingModel = value; }
        }

        /// <remarks>
        /// To support provider separation
        public string HostingGroup
        {
            get { return _HostingGroup; }
            set { _HostingGroup = value; }
        }
        /// <remarks>
        /// Scope of the assembly in the target instrumentation space
        /// In WMI speak is the namespace
        public string Scope
        {
            get { return _Scope; }
        }
    }

    /// <summary>
    /// This is the base class for all attribute which can be applied
    /// to members of the Automation class.
    /// </summary>
    /// 
    /// <remarks>
    /// The Exception member tells Monad which exception coming from the
    /// member can be treated as non-fatal errors for the pipeline.
    /// </remarks>
    /// 
    [AttributeUsage(AttributeTargets.All)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public abstract class ManagementMemberAttribute : Attribute
    {
        /// <summary>
        /// The exceptions that can be thrown by the member.
        /// </summary>
        /// 
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        private string _Name;
    }

    /// <summary>
    /// This abstract attribute determines how one would get an instance of the class.
    /// You can get an instance by:
    /// 1) Binding to an instance [Bind]
    /// 2) Creating an instance [Create]
    /// 3) Using a factory to get an instance [Factory]
    /// For any particular NOUN, there can only be ONE way to get instances.
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public abstract class ManagementNewInstanceAttribute : ManagementMemberAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementBindAttribute : ManagementNewInstanceAttribute
    {
        /// <summary>
        /// Declares the type that the output should be 
        /// treated as even if the return value is of
        /// type System.Object.
        /// </summary>
        /// 
        public ManagementBindAttribute() { }

        public Type Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }
        private Type _schema;
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementCreateAttribute : ManagementNewInstanceAttribute
    {
        ///// <summary>
        ///// Declares the type that the output should be 
        ///// treated as even if the return value is of
        ///// type System.Object.
        ///// </summary>
        /////         
    }


    /// <summary>
    /// This attribute determines how one would remove a real object
    /// </summary>
    /// 
    /// 
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementRemoveAttribute : ManagementMemberAttribute
    {
        /// <summary>
        /// Declares the type that the output should be 
        /// treated as even if the return value is of
        /// type System.Object.
        /// </summary>
        /// 
        public Type Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }
        private Type _schema;
    }

    /// <summary>
    /// This attribute defines the enumerator of instances of the class
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementEnumeratorAttribute : ManagementNewInstanceAttribute
    {
/*        /// <summary>
        /// Declares the member as an enumerator for other classes.  The other
        /// Type must specify the Factory property of the AutomationAttribute to
        /// be this Type.
        /// </summary>
        /// 
        public Type FactoryFor
        {
            get { return _factoryFor; }
            set { _factoryFor = value; }
        }
        private Type _factoryFor;

*/        /// <summary>
        /// Declares the type that the output should be 
        /// treated as even if the return value is of
        /// type System.Object.
        /// </summary>
        /// 
        public Type Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }
        private Type _schema;
    }

    /// <summary>
    /// Exposes a method or property as a Probe
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementProbeAttribute : ManagementMemberAttribute
    {
        /// <summary>
        /// Declares the type that the output should be 
        /// treated as even if the return value is of
        /// type System.Object.
        /// </summary>
        /// 
        public Type Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }
        private Type _schema;
    }

    #region Task
    /// <summary>
    /// Exposes a method as a task.
    /// </summary>
    /// 
    /// <remarks>
    /// The TaskAttribute is placed on a method to expose it as a management task.
    /// 
    /// If the task enumerates manageable objects, the task declaration should set
    /// the Enumeration option to true.
    /// 
    /// ISSUE-2005/06/08-jeffjon
    /// Does the task need a Schema parameter or should we have a separate Probe
    /// attribute?
    /// </remarks>
    /// 
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementTaskAttribute : ManagementMemberAttribute
    {
        public ManagementTaskAttribute()
        {
        }

        /// <summary>
        /// Declares the type that the output should be 
        /// treated as even if the return value is of
        /// type System.Object.
        /// </summary>
        /// 
        public Type Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }
        private Type _schema;
    }
    #endregion Task

    #region Naming

    /// <summary>
    /// This attribute defines the ID (key) property of the class.
    /// </summary>
    /// 
    /// <remarks>
    /// For Monad, this property is used to do filtering of enumerations.
    /// 
    /// If used on a parameter, then the attribute must also exist on a property in
    /// the class.
    /// </remarks>
    /// 
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementKeyAttribute : ManagementMemberAttribute
    {
        public ManagementKeyAttribute()
        {
        }
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementReferenceAttribute : Attribute
    {
        public ManagementReferenceAttribute()
        {
        }
        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        private string _Type;
    }

    #endregion Naming

    #region Configuration

    /// <summary>
    /// Defines a property as the storage for configuration data.
    /// </summary>
    /// 
    public enum ManagementConfigurationType { Apply, OnCommit };

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementConfigurationAttribute : ManagementMemberAttribute
    {
        /// <summary>
        /// Declares the type that the output should be 
        /// treated as even if the return value is of
        /// type System.Object.
        /// </summary>
        /// 

        public ManagementConfigurationAttribute() 
        {
            updateMode = ManagementConfigurationType.Apply;
        }

        public ManagementConfigurationType Mode
        {
            get { return updateMode; }
            set { updateMode = value; }
        }

        public Type Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }
        private ManagementConfigurationType updateMode; 
        private Type _schema;

    }

    [AttributeUsage(AttributeTargets.Method)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementCommitAttribute : ManagementMemberAttribute
    {
    }
    /// <summary>
    /// This attribute defines the naming (user friendly name) of method parameters
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    sealed public class ManagementNameAttribute : Attribute
    {

        public ManagementNameAttribute(string name)
        {
            _Name = name;

        }
        public string Name
        {
            get { return _Name; }
        }
        private string _Name;
    }

    #endregion Configuration

    /*
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class FactoryAttribute : NewInstanceAttribute
    {
        /// <summary>
        /// Declares the type that the output should be 
        /// treated as even if the return value is of
        /// type System.Object.
        /// </summary>
        /// 
        public FactoryAttribute() { }
        public FactoryAttribute(Type t) { }

        public Type Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }
        private Type _schema;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class FactoryForAttribute : ManagementMemberAttribute
    {
        /// <summary>
        /// Declares the type that the output should be 
        /// treated as even if the return value is of
        /// type System.Object.
        /// </summary>
        /// 
        public FactoryForAttribute(Type t) { }

        public Type Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }
        private Type _schema;
    }


     #region Constraints

    /// <summary>
    /// Constraints the member/option to a minimum and/or maximum length.
    /// </summary>
    /// 
    /// <remarks>
    /// This can be used for strings or collections.
    /// </remarks>
    /// 
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class ValidateLengthAttribute : Attribute
    {
        /// <summary>
        /// The minimum length
        /// </summary>
        /// 
        public int Min
        {
            get { return _min; }
            set { _min = value; }
        }
        private int _min = int.MinValue;

        /// <summary>
        /// The maximum length
        /// </summary>
        /// 
        public int Max
        {
            get { return _max; }
            set { _max = value; }
        }
        private int _max = int.MaxValue;
    }

    /// <summary>
    /// Constraints the member/option to a range of values.
    /// </summary>
    /// 
    /// <remarks>
    /// This can be used for strings or collections.
    /// </remarks>
    /// 
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class ValidateRangeAttribute : Attribute
    {
        /// <summary>
        /// Defines the range for the constraint
        /// </summary>
        /// 
        /// <param name="lower">
        /// The minimum of the range.
        /// </param>
        /// 
        /// <param name="upper">
        /// The maximum of the range.
        /// </param>
        /// 
        public ValidateRangeAttribute(object lower, object upper)
        {
            this._lower = lower;
            this._upper = upper;
        }

        /// <summary>
        /// The lower bound for the range
        /// </summary>
        /// 
        public object Lower
        {
            get { return _lower; }
            set { _lower = value; }
        }
        private object _lower;

        /// <summary>
        /// The upper bound for the range
        /// </summary>
        /// 
        public object Upper
        {
            get { return _upper; }
            set { _upper = value; }
        }
        private object _upper;
    }

    /// <summary>
    /// Constraints the member/option to a pattern represented by a regular expression
    /// </summary>
    /// 
    /// <remarks>
    /// This can be used for strings or collections.
    /// </remarks>
    /// 
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class ValidatePatternAttribute : Attribute
    {
        /// <summary>
        /// Defines the pattern for the constraint
        /// </summary>
        /// 
        /// <param name="pattern">
        /// The minimum of the range.
        /// </param>
        /// 
        public ValidatePatternAttribute(string pattern)
        {
            this._pattern = pattern;
        }

        /// <summary>
        /// The pattern which defines the constraint
        /// </summary>
        /// 
        public string Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }
        private string _pattern;

        /// <summary>
        /// The options for the regular expression defined by the pattern.
        /// </summary>
        /// 
        public RegexOptions Options
        {
            get { return _options; }
            set { _options = value; }
        }
        private RegexOptions _options = RegexOptions.IgnoreCase;
    }

    /// <summary>
    /// Constraints the member/option to a number of values.
    /// </summary>
    /// 
    /// <remarks>
    /// This can be used for strings or collections.
    /// </remarks>
    /// 
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class ValidateCountAttribute : Attribute
    {
        /// <summary>
        /// Defines the minimum and maximum number of elements.
        /// </summary>
        /// 
        /// <param name="minimum">
        /// The minimum minimum number of elements.
        /// </param>
        /// 
        /// <param name="maximum">
        /// The maximum number of elements.
        /// </param>
        /// 
        public ValidateCountAttribute(int minimum, int maximum)
        {
        }

        /// <summary>
        /// The minimum number of elements
        /// </summary>
        /// 
        public int Minimum;

        /// <summary>
        /// The maximum number of elements
        /// </summary>
        /// 
        public int Maximum;
    }
    
    /// <summary>
    /// Constraints the member/option to a set of values.
    /// </summary>
    /// 
    
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
        [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
        public class ValidateSetAttribute : Attribute
        {
            /// <summary>
            /// Defines the range for the constraint
            /// </summary>
            /// 
            /// <param name="validValues">
            /// The valid values for the set.
            /// </param>
            /// 
            public ValidateSetAttribute(params string[] validValues)
            {
            }

            /// <summary>
            /// The valid values for the set.
            /// </summary>
            /// 
            public string[] ValidValues
            {
                get { return null; }
                set {  }
            }
        
            /// <summary>
            /// If true, the values are compared in a case-insensitive way.
            /// If false, the set is constrained to exact matches.
            /// </summary>
            /// 
            public bool IgnoreCase
            {
                get { return _ignoreCase; }
                set { _ignoreCase = value; }
            }
            private bool _ignoreCase = true;
        }
    
    #endregion Constraints
    /// <summary>
    /// Specifies the options for a task.
    /// </summary>
    /// 
    /// <remarks>
    /// When placed on a parameter of a task method, this attribute
    /// describes the options for the parameter.
    /// </remarks>
    /// 
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class ManagementTaskOptionAttribute : Attribute
    {
        /// <summary>
        /// If true, the option must be specified.
        /// </summary>
        /// 
        /// <remarks>
        /// If false, and the InitialValue is not specified, then
        /// an initial value will be deduced using the "default" 
        /// keyword in C#.
        /// </remarks>
        /// 
        public bool Mandatory
        {
            get { return _mandatory; }
            set { _mandatory = value; }
        }
        private bool _mandatory = true;

        /// <summary>
        /// The initial value of the parameter. Used if Mandatory=false.
        /// </summary>
        /// 
        public object InitialValue
        {
            get { return _initialValue; }
            set { _initialValue = value; }
        }
        private object _initialValue;

        /// <summary>
        /// Monad specific - provides mapping of the pipeline object
        /// to the parameter value.
        /// </summary>
        /// 
        public bool ValueFromPipeline
        {
            get { return _valueFromPipeline; }
            set { _valueFromPipeline = value; }
        }
        private bool _valueFromPipeline;

        /// <summary>
        /// Monad specific - provides mapping of the pipeline object's
        /// property with the same name as the parameter to the parameter
        /// value.
        /// </summary>
        public bool ValueFromPipelineByPropertyName
        {
            get { return _valueFromPipelineByPropertyName; }
            set { _valueFromPipelineByPropertyName = value; }
        }
        private bool _valueFromPipelineByPropertyName;

    }    
    */
};
