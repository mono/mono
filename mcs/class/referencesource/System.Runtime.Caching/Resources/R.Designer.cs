﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.20414.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace System.Runtime.Caching.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class R {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Grandfathered suppression from original caching code checkin")]
        internal R() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("System.Runtime.Caching.Resources.R", typeof(R).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; must be greater than or equal to &apos;{1}&apos; and less than or equal to &apos;{2}&apos;..
        /// </summary>
        internal static string Argument_out_of_range {
            get {
                return ResourceManager.GetString("Argument_out_of_range", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The collection &apos;{0}&apos; contains a null element..
        /// </summary>
        internal static string Collection_contains_null_element {
            get {
                return ResourceManager.GetString("Collection_contains_null_element", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The collection &apos;{0}&apos; contains a null or empty string..
        /// </summary>
        internal static string Collection_contains_null_or_empty_string {
            get {
                return ResourceManager.GetString("Collection_contains_null_or_empty_string", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to retrieve configuration section &apos;{0}&apos;..
        /// </summary>
        internal static string Config_unable_to_get_section {
            get {
                return ResourceManager.GetString("Config_unable_to_get_section", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Default is a reserved MemoryCache name..
        /// </summary>
        internal static string Default_is_reserved {
            get {
                return ResourceManager.GetString("Default_is_reserved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The collection &apos;{0}&apos; is empty..
        /// </summary>
        internal static string Empty_collection {
            get {
                return ResourceManager.GetString("Empty_collection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Initialization has not completed yet.  The InitializationComplete method must be invoked before Dispose is invoked..
        /// </summary>
        internal static string Init_not_complete {
            get {
                return ResourceManager.GetString("Init_not_complete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to One of the following parameters must be specified: dependencies, absoluteExpiration, slidingExpiration..
        /// </summary>
        internal static string Invalid_argument_combination {
            get {
                return ResourceManager.GetString("Invalid_argument_combination", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only one callback can be specified.  Either RemovedCallback or UpdateCallback must be null..
        /// </summary>
        internal static string Invalid_callback_combination {
            get {
                return ResourceManager.GetString("Invalid_callback_combination", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to AbsoluteExpiration must be DateTimeOffset.MaxValue or SlidingExpiration must be TimeSpan.Zero..
        /// </summary>
        internal static string Invalid_expiration_combination {
            get {
                return ResourceManager.GetString("Invalid_expiration_combination", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid state..
        /// </summary>
        internal static string Invalid_state {
            get {
                return ResourceManager.GetString("Invalid_state", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method has already been invoked, and can only be invoked once..
        /// </summary>
        internal static string Method_already_invoked {
            get {
                return ResourceManager.GetString("Method_already_invoked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property has already been set, and can only be set once..
        /// </summary>
        internal static string Property_already_set {
            get {
                return ResourceManager.GetString("Property_already_set", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid configuration: {0}=&quot;{1}&quot;.  The {0} value must be a time interval that can be parsed by System.TimeSpan.Parse..
        /// </summary>
        internal static string TimeSpan_invalid_format {
            get {
                return ResourceManager.GetString("TimeSpan_invalid_format", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CacheItemUpdateCallback must be null..
        /// </summary>
        internal static string Update_callback_must_be_null {
            get {
                return ResourceManager.GetString("Update_callback_must_be_null", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid configuration: {0}=&quot;{1}&quot;.  The {0} value must be a non-negative 32-bit integer..
        /// </summary>
        internal static string Value_must_be_non_negative_integer {
            get {
                return ResourceManager.GetString("Value_must_be_non_negative_integer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid configuration: {0}=&quot;{1}&quot;.  The {0} value must be a positive 32-bit integer..
        /// </summary>
        internal static string Value_must_be_positive_integer {
            get {
                return ResourceManager.GetString("Value_must_be_positive_integer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid configuration: {0}=&quot;{1}&quot;.  The {0} value cannot be greater than &apos;{2}&apos;..
        /// </summary>
        internal static string Value_too_big {
            get {
                return ResourceManager.GetString("Value_too_big", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid configuration: {0}=&quot;{1}&quot;.  The {0} value must be a boolean..
        /// </summary>
        internal static string Value_must_be_boolean {
            get {
                return ResourceManager.GetString("Value_must_be_boolean", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to An empty string is invalid..
        /// </summary>
        internal static string Empty_string_invalid {
            get {
                return ResourceManager.GetString("Empty_string_invalid", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The parameter regionName must be null..
        /// </summary>
        internal static string RegionName_not_supported {
            get {
                return ResourceManager.GetString("RegionName_not_supported", resourceCulture);
            }
        }

    }
}
