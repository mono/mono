//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;
    using System.Security;
    using System.Security.Permissions;
    using System.Diagnostics.CodeAnalysis;

    [Serializable]
    public class RedirectionException : CommunicationException
    {
        public RedirectionException(RedirectionType type, RedirectionDuration duration, RedirectionScope scope, params RedirectionLocation[] locations)
            : this(GetDefaultMessage(type, locations), type, duration, scope, null, locations)
        {
        }

        public RedirectionException(RedirectionType type, RedirectionDuration duration, RedirectionScope scope, Exception innerException, params RedirectionLocation[] locations)
            : this(GetDefaultMessage(type, locations), type, duration, scope, innerException, locations)
        {
        }

        public RedirectionException(string message, RedirectionType type, RedirectionDuration duration, RedirectionScope scope, params RedirectionLocation[] locations)
            : this(message, type, duration, scope, null, locations)
        {
        }

        public RedirectionException(string message, RedirectionType type, RedirectionDuration duration, RedirectionScope scope, Exception innerException, params RedirectionLocation[] locations)
            : base(message, innerException)
        {
            if (type == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("type");
            }

            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            else if (message.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("message",
                    SR.GetString(SR.ParameterCannotBeEmpty));

            }

            if (type.InternalType == RedirectionType.InternalRedirectionType.UseIntermediary
                || type.InternalType == RedirectionType.InternalRedirectionType.Resource)
            {
                if (locations == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("locations", SR.GetString(SR.RedirectMustProvideLocation));
                }
                else if (locations.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("locations", SR.GetString(SR.RedirectMustProvideLocation));
                }
            }

            if (type.InternalType == RedirectionType.InternalRedirectionType.Cache && locations != null && locations.Length > 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.RedirectCacheNoLocationAllowed));
            }

            if (locations == null)
            {
                //if we got here, then the redirect type doesn't care if there are locations...
                locations = EmptyArray<RedirectionLocation>.Instance;
            }

            this.Locations = new ReadOnlyCollection<RedirectionLocation>(locations);
            this.Type = type;
            this.Scope = scope;
            this.Duration = duration;
        }

        // Serialization
        private RedirectionException() : base() { }

        protected RedirectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Type = (RedirectionType)info.GetValue("Type", typeof(RedirectionType));
            this.Duration = (RedirectionDuration)info.GetValue("Duration", typeof(RedirectionDuration));
            this.Scope = (RedirectionScope)info.GetValue("Scope", typeof(RedirectionScope));
            RedirectionLocation[] locations = (RedirectionLocation[])info.GetValue("Locations", typeof(RedirectionLocation[]));
            this.Locations = new ReadOnlyCollection<RedirectionLocation>(locations);
        }

        // The analysis tool used for the security signoff (runcodeanalysis /sdl) is getting confused, reporting that we need 
        // to put the [SecurityCritical] attribute here, when we have it already and it matches the attribute from the base class.
        [SuppressMessage(FxCop.Category.Security, "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "The SecurityCritical attribute is used.")]
        [Fx.Tag.SecurityNote(Critical = "Overrides the base.GetObjectData which is critical, as well as calling this method.")]
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Type", this.Type, typeof(RedirectionType));
            info.AddValue("Duration", this.Duration, typeof(RedirectionDuration));
            info.AddValue("Scope", this.Scope, typeof(RedirectionScope));
            info.AddValue("Locations", this.Locations.ToArray<RedirectionLocation>(), typeof(RedirectionLocation[]));
        }

        public RedirectionDuration Duration { get; private set; }
        public IEnumerable<RedirectionLocation> Locations { get; private set; }
        public RedirectionScope Scope { get; private set; }
        public RedirectionType Type { get; private set; }

        static string FormatLocations(RedirectionLocation[] locations)
        {
            string result = String.Empty;
            if (locations != null && locations.Length > 0)
            {
                StringBuilder builder = new StringBuilder();
                int nonNullCount = 0;
                for (int i = 0; i < locations.Length; i++)
                {
                    if (locations[i] != null)
                    {
                        nonNullCount++;
                        if (nonNullCount > 1)
                        {
                            builder.AppendLine();
                        }
                        builder.AppendFormat("    {0}", locations[i].Address.AbsoluteUri);
                    }
                }

                result = builder.ToString();
            }

            return result;
        }

        static string GetDefaultMessage(RedirectionType type, RedirectionLocation[] locations)
        {
            string message = string.Empty;

            if (type != null)
            {
                if (type.InternalType == RedirectionType.InternalRedirectionType.Cache)
                {
                    message = SR.GetString(SR.RedirectCache);
                }
                else if (type.InternalType == RedirectionType.InternalRedirectionType.Resource)
                {
                    message = SR.GetString(SR.RedirectResource, FormatLocations(locations));
                }
                else if (type.InternalType == RedirectionType.InternalRedirectionType.UseIntermediary)
                {
                    message = SR.GetString(SR.RedirectUseIntermediary, FormatLocations(locations));
                }
                else
                {
                    //this an unknown extension redirection type...
                    message = SR.GetString(SR.RedirectGenericMessage);
                }
            }

            return message;
        }
    }
}
