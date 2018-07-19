//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Design;
    using System.Net.Security;
    using System.Reflection;
    using System.Workflow.Activities.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.Generic;

    [TypeConverter(typeof(ServiceOperationInfoTypeConverter))]
    [Editor(typeof(ServiceOperationUIEditor), typeof(UITypeEditor))]
    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class OperationInfoBase : DependencyObject
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty MethodInfoProperty =
            DependencyProperty.Register("MethodInfo",
            typeof(MethodInfo), typeof(OperationInfoBase),
            new PropertyMetadata(null, DependencyPropertyOptions.NonSerialized));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
            typeof(string), typeof(OperationInfoBase),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty PrincipalPermissionNameProperty =
            DependencyProperty.Register("PrincipalPermissionName",
            typeof(string), typeof(OperationInfoBase),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty PrincipalPermissionRoleProperty =
            DependencyProperty.Register("PrincipalPermissionRole",
            typeof(string), typeof(OperationInfoBase),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        public virtual string Name
        {
            get { return (string) this.GetValue(OperationInfoBase.NameProperty); }
            set { this.SetValue(OperationInfoBase.NameProperty, value); }
        }

        [DefaultValue(null)]
        public virtual string PrincipalPermissionName
        {
            get { return (string) this.GetValue(OperationInfoBase.PrincipalPermissionNameProperty); }
            set { this.SetValue(OperationInfoBase.PrincipalPermissionNameProperty, value); }
        }

        [DefaultValue(null)]
        public virtual string PrincipalPermissionRole
        {
            get { return (string) this.GetValue(OperationInfoBase.PrincipalPermissionRoleProperty); }
            set { this.SetValue(OperationInfoBase.PrincipalPermissionRoleProperty, value); }
        }

        internal bool IsReadOnly
        {
            get
            {
                return !this.DesignMode;
            }
        }

        public virtual OperationInfoBase Clone()
        {
            OperationInfoBase clonedOperation = (OperationInfoBase) Activator.CreateInstance(this.GetType());
            clonedOperation.Name = this.Name;
            clonedOperation.PrincipalPermissionName = this.PrincipalPermissionName;
            clonedOperation.PrincipalPermissionRole = this.PrincipalPermissionRole;

            return clonedOperation;
        }

        public override bool Equals(object obj)
        {
            OperationInfoBase operationInfo = obj as OperationInfoBase;
            if (operationInfo == null)
            {
                return false;
            }
            if (String.Compare(operationInfo.Name, this.Name, StringComparison.Ordinal) != 0)
            {
                return false;
            }
            if (String.Compare(operationInfo.PrincipalPermissionName, this.PrincipalPermissionName, StringComparison.Ordinal) != 0)
            {
                return false;
            }
            if (String.Compare(operationInfo.PrincipalPermissionRole, this.PrincipalPermissionRole, StringComparison.Ordinal) != 0)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal protected abstract string GetContractFullName(IServiceProvider provider);
        internal protected abstract Type GetContractType(IServiceProvider provider);
        internal protected abstract bool GetIsOneWay(IServiceProvider provider);
        internal protected abstract MethodInfo GetMethodInfo(IServiceProvider provider);
        internal protected abstract OperationParameterInfoCollection GetParameters(IServiceProvider provider);
    }
}
