//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using Microsoft.VisualBasic.Activities;
    using System;
    using System.Activities.Expressions;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Text.RegularExpressions;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Xml.Linq;
    using Microsoft.VisualBasic.Activities.XamlIntegration;

    public sealed class ActivityWithResultConverter : TypeConverterBase
    {
        public ActivityWithResultConverter()
            : base(typeof(Activity<>), typeof(ExpressionConverterHelper<>))
        {
        }

        public ActivityWithResultConverter(Type type)
            : base(type, typeof(Activity<>), typeof(ExpressionConverterHelper<>))
        {
        }

        internal static object GetRootTemplatedActivity(IServiceProvider serviceProvider)
        {
            // For now, we only support references to the root Activity when we're inside an Activity.Body
            // Note that in the case of nested activity bodies, this gives us the outer activity
            IRootObjectProvider rootProvider =
                serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            if (rootProvider == null)
            {
                return null;
            }
            IAmbientProvider ambientProvider =
                serviceProvider.GetService(typeof(IAmbientProvider)) as IAmbientProvider;
            if (ambientProvider == null)
            {
                return null;
            }
            IXamlSchemaContextProvider schemaContextProvider =
                serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider;
            if (schemaContextProvider == null)
            {
                return null;
            }
            XamlMember activityBody = GetXamlMember(schemaContextProvider.SchemaContext, typeof(Activity), "Implementation");
            XamlMember dynamicActivityBody = GetXamlMember(schemaContextProvider.SchemaContext, typeof(DynamicActivity), "Implementation");
            if (activityBody == null || dynamicActivityBody == null)
            {
                return null;
            }
            if (ambientProvider.GetFirstAmbientValue(null, activityBody, dynamicActivityBody) == null)
            {
                return null;
            }
            object rootActivity = rootProvider.RootObject as Activity;
            return rootActivity;
        }

        static XamlMember GetXamlMember(XamlSchemaContext schemaContext, Type type, string memberName)
        {
            XamlType xamlType = schemaContext.GetXamlType(type);
            if (xamlType == null)
            {
                return null;
            }
            XamlMember xamlMember = xamlType.GetMember(memberName);
            return xamlMember;
        }

        internal sealed class ExpressionConverterHelper<T> : TypeConverterHelper<Activity<T>>
        {
            static Regex LiteralEscapeRegex = new Regex(@"^(%+\[)");
            static Type LocationHelperType = typeof(LocationHelper<>);

            TypeConverter baseConverter;
            Type valueType;
            LocationHelper locationHelper; // true if we're dealing with a Location

            public ExpressionConverterHelper()
                : this(TypeHelper.AreTypesCompatible(typeof(T), typeof(Location)))
            {
            }

            public ExpressionConverterHelper(bool isLocationType)
            {
                this.valueType = typeof(T);

                if (isLocationType)
                {
                    Fx.Assert(this.valueType.IsGenericType && this.valueType.GetGenericArguments().Length == 1, "Should only get Location<T> here");
                    this.valueType = this.valueType.GetGenericArguments()[0];
                    Type concreteHelperType = LocationHelperType.MakeGenericType(typeof(T), this.valueType);
                    this.locationHelper = (LocationHelper)Activator.CreateInstance(concreteHelperType);
                }
            }

            TypeConverter BaseConverter
            {
                get
                {
                    if (this.baseConverter == null)
                    {
                        this.baseConverter = TypeDescriptor.GetConverter(this.valueType);
                    }

                    return this.baseConverter;
                }
            }

            public override Activity<T> ConvertFromString(string text, ITypeDescriptorContext context)
            {
                if (IsExpression(text))
                {
                    // Expression.  Use the expression parser.
                    string expressionText = text.Substring(1, text.Length - 2);

                    if (this.locationHelper != null)
                    {
                        // 
                        return (Activity<T>)this.locationHelper.CreateExpression(expressionText);
                    }
                    else
                    {
                        // 
                        return new VisualBasicValue<T>()
                        {
                            ExpressionText = expressionText
                        };
                    }
                }
                else
                {
                    if (this.locationHelper != null)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidLocationExpression));
                    }

                    // look for "%[....]" escape pattern
                    if (text.EndsWith("]", StringComparison.Ordinal) && LiteralEscapeRegex.IsMatch(text))
                    {
                        // strip off the very front-most '%' from the original string
                        text = text.Substring(1, text.Length - 1);
                    }

                    T literalValue;
                    if (text is T)
                    {
                        literalValue = (T)(object)text;
                    }
                    else if (text == string.Empty) // workaround for System.Runtime.Xaml bug
                    {
                        literalValue = default(T);
                    }
                    else
                    {
                        // Literal value.  Invoke the base type converter.
                        literalValue = (T)BaseConverter.ConvertFromString(context, text);
                    }

                    return new Literal<T> { Value = literalValue };
                }
            }

            static bool IsExpression(string text)
            {
                return (text.StartsWith("[", StringComparison.Ordinal) && text.EndsWith("]", StringComparison.Ordinal));
            }            

            // to perform the generics dance around Locations we need these helpers
            abstract class LocationHelper
            {
                public abstract Activity CreateExpression(string expressionText);
            }

            class LocationHelper<TLocationValue> : LocationHelper
            {
                public override Activity CreateExpression(string expressionText)
                {
                    return new VisualBasicReference<TLocationValue>()
                    {
                        ExpressionText = expressionText
                    };
                }
            }
        } 
    }
}
