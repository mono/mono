// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xaml;

namespace System.Windows.Markup
{
    /// <summary>
    ///  Class for Xaml markup extension for static field and property references.
    /// </summary>
    [TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    [TypeConverter(typeof(StaticExtensionConverter))]
    [MarkupExtensionReturnType(typeof(object))]
    public class StaticExtension : MarkupExtension 
    {
        /// <summary>
        ///  Constructor that takes no parameters
        /// </summary>
        public StaticExtension()
        {
        }
        
        /// <summary>
        ///  Constructor that takes the member that this is a static reference to.  
        ///  This string is of the format 
        ///     Prefix:ClassName.FieldOrPropertyName.  The Prefix is 
        ///  optional, and refers to the XML prefix in a Xaml file.
        /// </summary>
        public StaticExtension(
            string   member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            _member = member;
        }

        /// <summary>
        ///  Return an object that should be set on the targetObject's targetProperty
        ///  for this markup extension.  For a StaticExtension this is a static field
        ///  or property value.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        ///  The object to set on this property.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_member == null)
            {
                throw new InvalidOperationException(SR.Get(SRID.MarkupExtensionStaticMember));
            }

            object value;
            Type type = MemberType;
            string fieldString = null;
            string memberFullName = null;
            if (type != null)
            {
                fieldString = _member;
                memberFullName = type.FullName + "." + _member;
            }
            else
            {
                memberFullName = _member;

                // Validate the _member

                int dotIndex = _member.IndexOf('.');
                if (dotIndex < 0)
                {
                    throw new ArgumentException(SR.Get(SRID.MarkupExtensionBadStatic, _member));
                }

                // Pull out the type substring (this will include any XML prefix, e.g. "av:Button")

                string typeString = _member.Substring(0, dotIndex);
                if (typeString == string.Empty)
                {
                    throw new ArgumentException(SR.Get(SRID.MarkupExtensionBadStatic, _member));
                }

                // Get the IXamlTypeResolver from the service provider

                if (serviceProvider == null)
                {
                    throw new ArgumentNullException("serviceProvider");
                }

                IXamlTypeResolver xamlTypeResolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
                if (xamlTypeResolver == null)
                {
                    throw new ArgumentException(SR.Get(SRID.MarkupExtensionNoContext, GetType().Name, "IXamlTypeResolver"));
                }

                // Use the type resolver to get a Type instance

                type = xamlTypeResolver.Resolve(typeString);

                // Get the member name substring

                fieldString = _member.Substring(dotIndex + 1, _member.Length - dotIndex - 1);
                if (fieldString == string.Empty)
                {
                    throw new ArgumentException(SR.Get(SRID.MarkupExtensionBadStatic, _member));
                }
            }

            // Use the built-in parser for enum types
            
            if (type.IsEnum)
            {
                return Enum.Parse(type, fieldString);
            }

            // For other types, reflect
            if (GetFieldOrPropertyValue(type, fieldString, out value))
            {
                return value;
            }
            else
            {
                throw new ArgumentException(SR.Get(SRID.MarkupExtensionBadStatic, memberFullName));
            }
        }

        // return false if a public static field or property with the same name cannot be found.
        private bool GetFieldOrPropertyValue(Type type, string name, out object value)
        {
            FieldInfo field = null;
            Type temp = type;

            do
            {
                field = temp.GetField(name, BindingFlags.Public | BindingFlags.Static);
                if (field != null)
                {
                    value = field.GetValue(null);
                    return true;
                }

                temp = temp.BaseType;
            } while(temp != null);


            PropertyInfo prop = null;
            temp = type;

            do
            {
                prop = temp.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
                if (prop != null)
                {
                    value = prop.GetValue(null,null);
                    return true;
                }

                temp = temp.BaseType;
            } while(temp != null);

            value = null;
            return false;
        }

        /// <summary>
        ///  The static field or property represented by a string.  This string is
        ///  of the format Prefix:ClassName.FieldOrPropertyName.  The Prefix is 
        ///  optional, and refers to the XML prefix in a Xaml file.
        /// </summary>
        [ConstructorArgument("member")]
        public string Member
        {
            get { return _member; }
            set 
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _member = value;
            }
        }

        [DefaultValue(null)]
        public Type MemberType
        {
            get { return _memberType; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _memberType = value;
            }
        }

        private string _member;
        private Type _memberType;
    }
}

