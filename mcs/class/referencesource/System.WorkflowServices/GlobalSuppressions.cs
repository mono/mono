//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

// Code copied from WCF V1
[module: SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Scope = "member", Target = "System.ServiceModel.Diagnostics.ExceptionUtility+InternalException..ctor(System.Boolean)", MessageId = "System.SystemException.#ctor(System.String)")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.ServiceModel.Diagnostics.ExceptionUtility.ThrowHelperArgumentNull(System.String,System.String):System.ArgumentNullException")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.ServiceModel.Diagnostics.ExceptionUtility.ThrowHelperCallback(System.Exception):System.Exception")]
[module: SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Scope = "member", Target = "System.ServiceModel.Diagnostics.ExceptionUtility.ThrowHelperCallback(System.Exception):System.Exception", MessageId = "System.ServiceModel.Diagnostics.ExceptionUtility.ThrowHelperCallback(System.String,System.Exception)")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.ServiceModel.Diagnostics.ExceptionUtility.ThrowHelperFatal(System.String,System.Exception):System.Exception")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.ServiceModel.Diagnostics.ExceptionUtility.ThrowHelperWarning(System.Exception):System.Exception")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.ServiceModel.Diagnostics.ExceptionUtility.ThrowHelperArgument(System.String):System.ArgumentException")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.ServiceModel.Diagnostics.FatalException..ctor(System.String)")]

// Code copied from WF V1
[module: SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "System.Workflow.ComponentModel.Walker.WalkProperties(System.Workflow.ComponentModel.Activity,System.Object):System.Boolean")]
[module: SuppressMessage("Reliability", "Reliability104:CaughtAndHandledExceptionsRule", Scope = "member", Target = "System.Workflow.ComponentModel.Walker.WalkProperties(System.Workflow.ComponentModel.Activity,System.Object):System.Boolean")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Workflow.ComponentModel.Walker..ctor(System.Boolean)")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Workflow.ComponentModel.Walker..ctor(System.Object)")]


// Added by [....],  This warning is issued on the dialog resources of the System.Workflow.Activities dialgos, which hold types from System.WorkflowServices.dll , this warning complains that the assembly version does not match the Mscorlib assemblyversion of 4.0.0.0 , thats why this message is suppressed globally.

[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.ServiceContractDetailViewControl.resources", MessageId = ">>$this.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.ServiceContractViewControl.resources", MessageId = ">>backgroundPanel.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.ServiceContractViewControl.resources", MessageId = ">>$this.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.ServiceContractViewControl.resources", MessageId = ">>contractNameLabel.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.OperationPickerDialog.resources", MessageId = ">>footerPanel.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.OperationPickerDialog.resources", MessageId = ">>detailsViewPanel.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.OperationPickerDialog.resources", MessageId = ">>operationsPanel.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.OperationPickerDialog.resources", MessageId = ">>operationsListBox.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.OperationPickerDialog.resources", MessageId = ">>contentPanel.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.ServiceOperationDetailViewControl.resources", MessageId = ">>$this.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.ServiceOperationDetailViewControl.resources", MessageId = ">>editableLabelControl1.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.ServiceOperationViewControl.resources", MessageId = ">>backgroundPanel.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.ServiceOperationViewControl.resources", MessageId = ">>$this.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.ServiceOperationViewControl.resources", MessageId = ">>operationNameLabel.Type")]
[module: SuppressMessage("Microsoft.Usage", "CA2228:DoNotShipUnreleasedResourceFormats", Scope = "resource", Target = "System.Workflow.Activities.Design.ReflectedServiceOperationDetailViewControl.resources", MessageId = ">>$this.Type")]

// Added by [....] for jself. These suppressions are for violations in red bits that are temporarily copied over.

[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Workflow.ComponentModel.WalkerEventArgs.get_CurrentValue():System.Object")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Workflow.ComponentModel.WalkerEventArgs.get_UserData():System.Object")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Workflow.ComponentModel.WalkerEventArgs.get_CurrentPropertyName():System.String")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Workflow.ComponentModel.WalkerEventArgs.get_CurrentPropertyOwner():System.Object")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Workflow.ComponentModel.WalkerEventArgs.get_CurrentProperty():System.Reflection.PropertyInfo")]
[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Workflow.ComponentModel.WalkerEventArgs.set_Action(System.Workflow.ComponentModel.WalkerAction):System.Void")]
[module: SuppressMessage("Reliability", "Reliability102:WrapExceptionsRule", Scope = "member", Target = "System.Workflow.Activities.ParameterInfoBasedPropertyDescriptor..ctor(System.Type,System.Reflection.ParameterInfo,System.Boolean,System.Attribute[])")]
