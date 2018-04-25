//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System.Collections.Generic;
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Xaml;
    using XamlBuildTask;

    // This class is modal. The following properties are set in different modes:
    //                 Type  TextValue   Value   ArrayContents
    // x:Null 
    // x:Array          x                             x
    // Known Value      x        x         x
    // Unknown Value    x        x
    //
    // While this modality is not ideal, it's preferable to having multiple concrete classes
    // and requiring the user to down-cast.
    // However, we can't make the class mutable and expect users to maintain the modality.
    // So it's publicly immutable and has constructors for various modes (except for x:Null,
    // which the user can replicate by passing value == null and type == object).
    public sealed class AttributeParameterData
    {
        List<AttributeParameterData> arrayContents;

        public AttributeParameterData(XamlType type, string textValue)
        {
            ValidateType(type);
            if (textValue == null && type.UnderlyingType.IsValueType)
            {
                throw FxTrace.Exception.Argument("value", SR.AttributeValueNotNullable(type.UnderlyingType));
            }

            this.Type = type;
            if (textValue != null)
            {
                this.Value = AttributeData.GetParameterValue(ref textValue, type);
                this.TextValue = textValue;
            }
        }

        public AttributeParameterData(XamlType type, object value)
        {
            ValidateType(type);
            if (value == null && type.UnderlyingType.IsValueType)
            {
                throw FxTrace.Exception.Argument("value", SR.AttributeValueNotNullable(type.UnderlyingType));
            }
            if (value != null && !type.UnderlyingType.IsAssignableFrom(value.GetType()))
            {
                throw FxTrace.Exception.Argument("value", SR.AttributeValueNotAssignableToType(value.GetType(), type.UnderlyingType));
            }

            this.Type = type;
            if (value != null)
            {
                if (type.UnderlyingType.IsArray)
                {
                    Array array = (Array)value;
                    XamlType elementType = type.SchemaContext.GetXamlType(type.UnderlyingType.GetElementType());
                    this.arrayContents = new List<AttributeParameterData>();
                    foreach (object item in array)
                    {
                        this.arrayContents.Add(new AttributeParameterData(elementType, item));
                    }
                    this.IsArray = true;
                }
                else
                {
                    this.Value = value;
                    this.TextValue = AttributeData.GetParameterText(value, type);
                }
            }
        }

        public AttributeParameterData(XamlType type, IEnumerable<AttributeParameterData> arrayContents)
        {
            ValidateType(type);
            if (!type.UnderlyingType.IsArray)
            {
                throw FxTrace.Exception.Argument("type", SR.AttributeTypeIsNotArray(type));
            }
            if (arrayContents == null)
            {
                throw FxTrace.Exception.ArgumentNull("arrayContents");
            }

            this.Type = type;
            this.arrayContents = new List<AttributeParameterData>(arrayContents);
            this.IsArray = true;
        }

        internal AttributeParameterData()
        {
            TextValue = String.Empty;
            IsArray = false;
        }

        public string TextValue 
        { 
            get;
            internal set; 
        }

        public object Value
        {
            get;
            internal set;
        }
        
        public XamlType Type 
        { 
            get; 
            internal set;
        }

        public ReadOnlyCollection<AttributeParameterData> ArrayContents
        {
            get
            {
                return this.arrayContents == null ? null : this.arrayContents.AsReadOnly();
            }
        }

        internal void AddArrayContentsEntry(AttributeParameterData arrayContentsEntry)
        {
            if (arrayContentsEntry != null)
            {
                if (this.arrayContents == null)
                {
                    this.arrayContents = new List<AttributeParameterData>();
                }
                this.arrayContents.Add(arrayContentsEntry);
            }
        }      

        internal bool IsArray { get; set; }

        private static void ValidateType(XamlType type)
        {
            if (type == null)
            {
                throw FxTrace.Exception.ArgumentNull("type");
            }
            if (type.IsUnknown)
            {
                throw FxTrace.Exception.Argument("type", SR.AttributeParameterTypeUnknownNoErrNum(type));
            }
            if (!AttributeData.IsSupportedParameterType(type.UnderlyingType))
            {
                throw FxTrace.Exception.Argument("type", SR.AttributeParamTypeNotSupportedNoErrNum(type));
            }
        }
    }
}
