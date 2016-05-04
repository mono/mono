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


    [ActivityDesignerTheme(typeof(ReceiveActivityDesignerTheme))]
    class ReceiveActivityDesigner : SequenceDesigner
    {
        static ActivityComparer<ReceiveActivity> matchByContractCallback;
        static ActivityComparer<ReceiveActivity> matchByOperationCallback;

        public override Size MinimumSize
        {
            get
            {
                return new Size(170, 80);
            }
        }

        public override string Text
        {
            get
            {
                if (this.ReceiveActivity.ServiceOperationInfo != null)
                {
                    string operationName = this.ReceiveActivity.ServiceOperationInfo.Name;
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

                ActivityDesignerVerb findSimilarOperationsVerb = new FindSimilarActivitiesVerb<ReceiveActivity>(
                    this, MatchByOperationCallback, SR2.GetString(SR2.ShowActivitiesWithSameOperation));
                verbs.Add(findSimilarOperationsVerb);

                ActivityDesignerVerb findSimilarContractVerb = new FindSimilarActivitiesVerb<ReceiveActivity>(
                    this, MatchByContractCallback, SR2.GetString(SR2.ShowActivitiesWithSameContract));
                verbs.Add(findSimilarContractVerb);

                return verbs;
            }
        }

        static ActivityComparer<ReceiveActivity> MatchByContractCallback
        {
            get
            {
                if (matchByContractCallback == null)
                {
                    matchByContractCallback = new ActivityComparer<ReceiveActivity>(MatchByContract);
                }
                return matchByContractCallback;
            }
        }

        static ActivityComparer<ReceiveActivity> MatchByOperationCallback
        {
            get
            {
                if (matchByOperationCallback == null)
                {
                    matchByOperationCallback = new ActivityComparer<ReceiveActivity>(MatchByOperation);
                }
                return matchByOperationCallback;
            }
        }

        ReceiveActivity ReceiveActivity
        {
            get { return this.Activity as ReceiveActivity; }

        }

        internal static bool MatchByContract(OperationInfoBase source, OperationInfoBase target)
        {
            if ((source == null) || (target == null))
            {
                return false;
            }
            string sourceContract = source.GetContractFullName(null);
            string targetContract = target.GetContractFullName(null);
            return MatchNames(sourceContract, targetContract);
        }

        internal static bool MatchByOperation(OperationInfoBase source, OperationInfoBase target)
        {
            if (!MatchByContract(source, target))
            {
                return false;
            }
            return MatchNames(source.Name, target.Name);
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            IExtenderListService extenderListService = (IExtenderListService)GetService(typeof(IExtenderListService));
            if (extenderListService != null)
            {
                bool foundExtender = false;
                foreach (IExtenderProvider extenderProvider in extenderListService.GetExtenderProviders())
                {
                    if (extenderProvider.GetType() == typeof(WorkflowServiceAttributesPropertyProviderExtender))
                    {
                        foundExtender = true;
                    }
                }

                if (!foundExtender)
                {
                    IExtenderProviderService extenderProviderService = (IExtenderProviderService)GetService(typeof(IExtenderProviderService));
                    if (extenderProviderService != null)
                    {
                        extenderProviderService.AddExtenderProvider(new WorkflowServiceAttributesPropertyProviderExtender());
                    }
                }
            }
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);
            if (e.Member != null && e.Member.Name == ReceiveActivity.ServiceOperationInfoProperty.Name)
            {
                ReceiveActivity receiveActivity = e.Activity as ReceiveActivity;
                if (receiveActivity != null)
                {
                    receiveActivity.ParameterBindings.Clear();
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
            if (ServiceOperationUIEditor.TryPickOperation(this.Activity.Site, this.Activity, this.ReceiveActivity.ServiceOperationInfo, out pickedServiceOperation))
            {
                PropertyDescriptorUtils.SetPropertyValue(this.Activity.Site, ServiceOperationHelpers.GetServiceOperationInfoPropertyDescriptor(this.Activity), this.Activity, pickedServiceOperation);
            }
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle incomingArrowRect = new Rectangle(this.Location.X - 2, this.Location.Y + 20, 24, 24);
            Rectangle outgoingArrowRect = new Rectangle(this.Location.X + 22, this.Location.Y + this.Size.Height - 45, -24, 24);
            e.Graphics.DrawImage(ImageResources.Arrow, incomingArrowRect);
            bool isOneWay = false;


            if (ReceiveActivity.ServiceOperationInfo != null)
            {
                // Refresh the referenced design time types in the serviceoperationInfo object properties;
                //ServiceOperationHelpers.RefreshReferencedDesignTimeTypes(this.Activity.Site as IServiceProvider, operation);
                isOneWay = this.ReceiveActivity.ServiceOperationInfo.GetIsOneWay(this.Activity.Site as IServiceProvider);
            }
            if (!isOneWay)
            {
                e.Graphics.DrawImage(ImageResources.Arrow, outgoingArrowRect);
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            ReceiveActivity receiveActivity = this.Activity as ReceiveActivity;
            if (receiveActivity != null)
            {
                receiveActivity.GetParameterPropertyDescriptors(properties);
            }
        }

        static bool MatchByContract(ReceiveActivity source, ReceiveActivity target)
        {
            return MatchByContract(source.ServiceOperationInfo, target.ServiceOperationInfo);
        }

        static bool MatchByOperation(ReceiveActivity source, ReceiveActivity target)
        {
            return MatchByOperation(source.ServiceOperationInfo, target.ServiceOperationInfo);
        }

        static bool MatchNames(string sourceName, string targetName)
        {
            if (string.IsNullOrEmpty(sourceName) || string.IsNullOrEmpty(targetName))
            {
                return false;
            }
            return (sourceName == targetName);
        }

    }

}
