//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Drawing.Design;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Windows.Forms;

    internal class ServiceOperationUIEditor : UITypeEditor
    {

        public static bool TryPickOperation(IServiceProvider serviceProvider, Activity activity, OperationInfoBase currentOperation, out OperationInfoBase selectedOperation)
        {
            selectedOperation = null;
            bool isReceiveActivity = activity is ReceiveActivity;
            try
            {
                using (OperationPickerDialog operationPicker = new OperationPickerDialog(serviceProvider, isReceiveActivity))
                {
                    Walker activityTreeWalker = new Walker();
                    Type allowedActivityType = null;

                    if (isReceiveActivity)
                    {
                        allowedActivityType = typeof(ReceiveActivity);
                    }
                    else
                    {
                        allowedActivityType = typeof(SendActivity);
                    }

                    activityTreeWalker.FoundActivity += delegate(Walker walker, WalkerEventArgs eventArgs)
                    {
                        Activity foundActivity = eventArgs.CurrentActivity;
                        if (!(allowedActivityType.IsAssignableFrom(foundActivity.GetType())))
                        {
                            return;
                        }

                        if (!foundActivity.Enabled)
                        {
                            return;
                        }

                        if (foundActivity is ReceiveActivity)
                        {
                            ReceiveActivity reciveActivity = foundActivity as ReceiveActivity;
                            if (reciveActivity.ServiceOperationInfo != null)
                            {
                                operationPicker.AddServiceOperation(reciveActivity.ServiceOperationInfo, reciveActivity);
                            }
                        }
                        if (foundActivity is SendActivity)
                        {
                            SendActivity sendActivity = foundActivity as SendActivity;
                            if (sendActivity.ServiceOperationInfo != null)
                            {
                                operationPicker.AddServiceOperation(sendActivity.ServiceOperationInfo, sendActivity);
                            }
                        }
                    };
                    activityTreeWalker.Walk(activity.RootActivity);
                    OperationInfoBase currentServiceOperationInfo = currentOperation as OperationInfoBase;
                    if (currentServiceOperationInfo != null)
                    {
                        operationPicker.SelectedOperation = currentServiceOperationInfo;
                    }
                    DialogResult dialogResult = operationPicker.ShowDialog();
                    if ((operationPicker.SelectedOperation != null) && (dialogResult == DialogResult.OK) && !operationPicker.SelectedOperation.Equals(currentServiceOperationInfo))
                    {
                        selectedOperation = operationPicker.SelectedOperation.Clone();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                DesignerHelpers.ShowMessage(serviceProvider, e.Message, DR.GetString(DR.WorkflowDesignerTitle), MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                throw;
            }

            return false;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (provider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("provider");
            }

            OperationInfoBase pickedServiceOperation = null;
            if (TryPickOperation(provider, (Activity) context.Instance, (OperationInfoBase) value, out pickedServiceOperation))
            {
                return pickedServiceOperation;
            }
            else
            {
                return base.EditValue(context, provider, value);
            }

        }


        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
