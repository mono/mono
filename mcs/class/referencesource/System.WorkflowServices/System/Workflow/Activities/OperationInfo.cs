//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Security;
    using System.Reflection;
    using System.ServiceModel;
    using System.Workflow.Activities.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class OperationInfo : OperationInfoBase
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty ContractNameProperty =
            DependencyProperty.Register("ContractName",
            typeof(string), typeof(OperationInfo),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty IsOneWayProperty =
            DependencyProperty.Register("IsOneWay",
            typeof(bool), typeof(OperationInfo),
            new PropertyMetadata(false, DependencyPropertyOptions.Metadata));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register("Parameters",
            typeof(OperationParameterInfoCollection), typeof(OperationInfo),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly,
            new Attribute[] {
                new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content)
            }
            ));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty ProtectionLevelProperty =
            DependencyProperty.Register("ProtectionLevel",
            typeof(ProtectionLevel?), typeof(OperationInfo),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        public OperationInfo()
        {
            this.SetReadOnlyPropertyValue(OperationInfo.ParametersProperty,
                new OperationParameterInfoCollection(this));
        }

        public string ContractName
        {
            get
            {
                return (string) this.GetValue(OperationInfo.ContractNameProperty);
            }
            set
            {
                this.SetValue(OperationInfo.ContractNameProperty, value);
            }
        }

        [DefaultValue(false)]
        public bool HasProtectionLevel
        {
            get
            {
                return (this.ProtectionLevel != null);
            }
        }

        [DefaultValue(false)]
        public bool IsOneWay
        {
            get
            {
                return (bool) this.GetValue(OperationInfo.IsOneWayProperty);
            }
            set
            {
                this.SetValue(OperationInfo.IsOneWayProperty, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public OperationParameterInfoCollection Parameters
        {
            get
            {
                return (OperationParameterInfoCollection) this.GetValue(OperationInfo.ParametersProperty);
            }
        }

        [DefaultValue(null)]
        public ProtectionLevel? ProtectionLevel
        {
            get
            {
                return (ProtectionLevel?) this.GetValue(OperationInfo.ProtectionLevelProperty);
            }
            set
            {
                this.SetValue(OperationInfo.ProtectionLevelProperty, value);
            }
        }

        public override OperationInfoBase Clone()
        {
            OperationInfo clonedOperation = (OperationInfo) base.Clone();
            clonedOperation.ContractName = this.ContractName;
            clonedOperation.IsOneWay = this.IsOneWay;
            if (this.HasProtectionLevel)
            {
                clonedOperation.ProtectionLevel = this.ProtectionLevel;
            }

            foreach (OperationParameterInfo parameter in this.Parameters)
            {
                clonedOperation.Parameters.Add(parameter.Clone());
            }

            return clonedOperation;
        }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
            {
                return false;
            }

            OperationInfo operationInfo = obj as OperationInfo;
            if (operationInfo == null)
            {
                return false;
            }
            if (String.Compare(operationInfo.ContractName, this.ContractName, StringComparison.Ordinal) != 0)
            {
                return false;
            }
            if (operationInfo.IsOneWay != this.IsOneWay)
            {
                return false;
            }
            if (operationInfo.HasProtectionLevel != this.HasProtectionLevel)
            {
                return false;
            }
            if (operationInfo.ProtectionLevel != this.ProtectionLevel)
            {
                return false;
            }

            if (operationInfo.Parameters.Count != this.Parameters.Count)
            {
                return false;
            }

            foreach (OperationParameterInfo parameter in operationInfo.Parameters)
            {
                OperationParameterInfo correspondingParameter = this.Parameters[parameter.Name];
                if (correspondingParameter == null)
                {
                    return false;
                }

                if (!parameter.Equals(correspondingParameter))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string returnValue = string.Empty;
            if (!string.IsNullOrEmpty(this.Name))
            {
                returnValue = this.Name;

                if (!string.IsNullOrEmpty(this.ContractName))
                {
                    returnValue = this.ContractName + "." + returnValue;
                }
            }

            return returnValue;
        }

        protected internal override string GetContractFullName(IServiceProvider provider)
        {
            return this.ContractName;
        }

        protected internal override Type GetContractType(IServiceProvider provider)
        {
            Type contractType = DynamicContractTypeBuilder.GetContractType(this, this.ParentDependencyObject as ReceiveActivity);
            if (contractType == null && !this.IsReadOnly)
            {
                Activity owner = this.ParentDependencyObject as Activity;
                if (owner != null)
                {
                    owner.RootActivity.RemoveProperty(DynamicContractTypeBuilder.DynamicContractTypesProperty);
                }

                contractType = DynamicContractTypeBuilder.GetContractType(this, this.ParentDependencyObject as ReceiveActivity);
            }
            return contractType;
        }

        internal protected override bool GetIsOneWay(IServiceProvider provider)
        {
            return this.IsOneWay;
        }

        internal protected override MethodInfo GetMethodInfo(IServiceProvider provider)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return null;
            }

            MethodInfo methodInfo = null;
            if (this.IsReadOnly)
            {
                if (this.UserData.Contains(OperationInfoBase.MethodInfoProperty))
                {
                    methodInfo = this.UserData[OperationInfoBase.MethodInfoProperty] as MethodInfo;
                }

                if (methodInfo != null)
                {
                    return methodInfo;
                }
            }

            methodInfo = InternalGetMethodInfo(provider);
            if (methodInfo == null && !this.IsReadOnly)
            {
                Activity owner = this.ParentDependencyObject as Activity;
                if (owner != null)
                {
                    owner.RootActivity.RemoveProperty(DynamicContractTypeBuilder.DynamicContractTypesProperty);
                }
                methodInfo = InternalGetMethodInfo(provider);
            }

            if (this.IsReadOnly)
            {
                this.UserData[OperationInfoBase.MethodInfoProperty] = methodInfo;
            }

            return methodInfo;
        }

        internal protected override OperationParameterInfoCollection GetParameters(IServiceProvider provider)
        {
            return this.Parameters;
        }

        internal void ResetProtectionLevel()
        {
            this.ProtectionLevel = null;
        }

        MethodInfo InternalGetMethodInfo(IServiceProvider provider)
        {
            Type type = this.GetContractType(provider);
            if (type != null)
            {
                foreach (MethodInfo methodInfo in type.GetMethods())
                {
                    object[] operationContractAttribs =
                        methodInfo.GetCustomAttributes(typeof(OperationContractAttribute), true);

                    if (operationContractAttribs != null && operationContractAttribs.Length > 0)
                    {
                        string operationName =
                            ((OperationContractAttribute) operationContractAttribs[0]).Name;

                        if (string.IsNullOrEmpty(operationName) &&
                            string.Compare(methodInfo.Name, this.Name, StringComparison.Ordinal) == 0)
                        {
                            return methodInfo;
                        }
                        else if (string.Compare(operationName, this.Name, StringComparison.Ordinal) == 0)
                        {
                            return methodInfo;
                        }
                    }
                }
            }

            return null;
        }
    }
}
