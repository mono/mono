//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Security;
    using System.Reflection;
    using System.ServiceModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;

    [Browsable(true)]
    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class OperationParameterInfo : DependencyObject
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty AttributesProperty =
            DependencyProperty.Register("Attributes",
            typeof(ParameterAttributes), typeof(OperationParameterInfo),
            new PropertyMetadata(DependencyPropertyOptions.Metadata));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
            typeof(string), typeof(OperationParameterInfo),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty ParameterTypeProperty =
            DependencyProperty.Register("ParameterType",
            typeof(Type), typeof(OperationParameterInfo),
            new PropertyMetadata(typeof(void), DependencyPropertyOptions.Metadata));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position",
            typeof(int), typeof(OperationParameterInfo),
            new PropertyMetadata(-1, DependencyPropertyOptions.Metadata));

        public OperationParameterInfo()
        {
        }

        public OperationParameterInfo(string parameterName)
        {
            SetValue(NameProperty, parameterName);
        }

        internal OperationParameterInfo(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameter");
            }

            SetValue(OperationParameterInfo.NameProperty, parameter.Name);
            SetValue(OperationParameterInfo.PositionProperty, parameter.Position);
            SetValue(OperationParameterInfo.AttributesProperty, parameter.Attributes);
            SetValue(OperationParameterInfo.ParameterTypeProperty, parameter.ParameterType);
        }

        public ParameterAttributes Attributes
        {
            get
            {
                return (ParameterAttributes) GetValue(AttributesProperty);
            }
            set
            {
                SetValue(AttributesProperty, value);
            }
        }

        public bool IsIn
        {
            get
            {
                return ((this.Attributes & ParameterAttributes.In) != 0);
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public bool IsLcid
        {
            get
            {
                return ((this.Attributes & ParameterAttributes.Lcid) != 0);
            }
        }

        public bool IsOptional
        {
            get
            {
                return ((this.Attributes & ParameterAttributes.Optional) != 0);
            }
        }

        public bool IsOut
        {
            get
            {
                return ((this.Attributes & ParameterAttributes.Out) != 0);
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public bool IsRetval
        {
            get
            {
                return ((this.Attributes & ParameterAttributes.Retval) != 0);
            }
        }

        public string Name
        {
            get
            {
                return (string) GetValue(NameProperty);
            }
            set
            {
                SetValue(NameProperty, value);
            }
        }

        public Type ParameterType
        {
            get
            {
                return (Type) GetValue(ParameterTypeProperty);
            }
            set
            {
                SetValue(ParameterTypeProperty, value);
            }
        }

        public int Position
        {
            get
            {
                return (int) GetValue(PositionProperty);
            }
            set
            {
                SetValue(PositionProperty, value);
            }
        }

        public OperationParameterInfo Clone()
        {
            OperationParameterInfo clonedParameter = new OperationParameterInfo();
            clonedParameter.Name = this.Name;
            clonedParameter.Attributes = this.Attributes;
            clonedParameter.Position = this.Position;
            clonedParameter.ParameterType = this.ParameterType;

            return clonedParameter;
        }

        public override bool Equals(object obj)
        {
            OperationParameterInfo parameter = obj as OperationParameterInfo;
            if (parameter == null)
            {
                return false;
            }
            if (String.Compare(parameter.Name, this.Name, StringComparison.Ordinal) != 0)
            {
                return false;
            }
            if (parameter.Attributes != this.Attributes)
            {
                return false;
            }
            if (parameter.Position != this.Position)
            {
                return false;
            }
            if (parameter.ParameterType != this.ParameterType)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
