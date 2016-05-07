//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System.Collections;
    using System.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Drawing;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.Activities.Design;
    using System.ServiceModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Windows.Forms;


    [ActivityDesignerTheme(typeof(SendActivityDesignerTheme))]
    class SendActivityDesigner : ActivityDesigner
    {
        static ActivityComparer<SendActivity> matchByContractCallback;
        static ActivityComparer<SendActivity> matchByOperationCallback;


        public override Size MinimumSize
        {
            get
            {
                return new Size(150, 80);
            }
        }

        public override string Text
        {
            get
            {
                if (this.SendActivity.ServiceOperationInfo != null)
                {
                    string operationName = this.SendActivity.ServiceOperationInfo.Name;
                    if (!String.IsNullOrEmpty(operationName))
                    {
                        return operationName;
                    }
                }
                return base.Text;
            }
        }

        protected override ActivityDesignerVerbCollection Verbs
        {
            get
            {
                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
                foreach (ActivityDesignerVerb verb in base.Verbs)
                {
                    verbs.Add(verb);
                }

                ActivityDesignerVerb findSimilarOperationsVerb = new FindSimilarActivitiesVerb<SendActivity>(
                    this, MatchByOperationCallback, SR2.GetString(SR2.ShowActivitiesWithSameOperation));
                verbs.Add(findSimilarOperationsVerb);

                ActivityDesignerVerb findSimilarContractVerb = new FindSimilarActivitiesVerb<SendActivity>(
                    this, MatchByContractCallback, SR2.GetString(SR2.ShowActivitiesWithSameContract));
                verbs.Add(findSimilarContractVerb);

                return verbs;
            }
        }

        static ActivityComparer<SendActivity> MatchByContractCallback
        {
            get
            {
                if (matchByContractCallback == null)
                {
                    matchByContractCallback = new ActivityComparer<SendActivity>(MatchByContract);
                }
                return matchByContractCallback;
            }
        }

        static ActivityComparer<SendActivity> MatchByOperationCallback
        {
            get
            {
                if (matchByOperationCallback == null)
                {
                    matchByOperationCallback = new ActivityComparer<SendActivity>(MatchByOperation);
                }
                return matchByOperationCallback;
            }
        }

        SendActivity SendActivity
        {
            get { return this.Activity as SendActivity; }

        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);
            if (e.Member != null && e.Member.Name == SendActivity.ServiceOperationInfoProperty.Name)
            {
                if (SendActivity != null)
                {
                    SendActivity.ParameterBindings.Clear();
                }
                TypeDescriptor.Refresh(e.Activity);
                PerformLayout();
            }
        }

        protected override void OnMouseDoubleClick(System.Windows.Forms.MouseEventArgs e)
        {
            // Do not allow editing if activity is locked
            if (this.IsLocked)
            {
                return;
            }

            OperationInfoBase pickedServiceOperation = null;
            if (ServiceOperationUIEditor.TryPickOperation(this.Activity.Site, this.Activity, this.SendActivity.ServiceOperationInfo, out pickedServiceOperation))
            {
                PropertyDescriptorUtils.SetPropertyValue(this.Activity.Site, ServiceOperationHelpers.GetServiceOperationInfoPropertyDescriptor(this.Activity), this.Activity, pickedServiceOperation);
            }
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle outgoingArrowRect = new Rectangle(this.Location.X + this.Size.Width - 24, this.Location.Y + 10, 24, 24);
            Rectangle incomingArrowRect = new Rectangle(this.Location.X + this.Size.Width, this.Location.Y + this.Size.Height - 35, -24, 24);
            e.Graphics.DrawImage(ImageResources.Arrow, outgoingArrowRect);
            bool isOneWay = false;


            if (SendActivity.ServiceOperationInfo != null)
            {
                OperationInfoBase operation = this.SendActivity.ServiceOperationInfo;
                // Refresh the contract type in the case the type is a Design Time type)
                //ServiceOperationHelpers.RefreshReferencedDesignTimeTypes(this.Activity.Site as IServiceProvider, operation);
                isOneWay = operation.GetIsOneWay(this.Activity.Site as IServiceProvider);
            }
            if (!isOneWay)
            {
                e.Graphics.DrawImage(ImageResources.Arrow, incomingArrowRect);
            }
        }
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (SendActivity != null)
            {
                SendActivity.GetParameterPropertyDescriptors(properties);
            }
        }

        static bool MatchByContract(SendActivity source, SendActivity target)
        {
            return ReceiveActivityDesigner.MatchByContract(source.ServiceOperationInfo, target.ServiceOperationInfo);
        }

        static bool MatchByOperation(SendActivity source, SendActivity target)
        {
            return ReceiveActivityDesigner.MatchByOperation(source.ServiceOperationInfo, target.ServiceOperationInfo);
        }

    }

}
