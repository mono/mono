//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime;
    using System.ServiceModel;
    using System.Workflow.ComponentModel;


    // <summary>
    // This the the class that implements the Search by menu item displayed on the activityDesigner
    // this is responsible for finding the matching activity designers and displaying them using
    // the acticvitydesigner hightlighter.
    // </summary>
    internal class FindSimilarActivitiesVerb<TActivity> : ActivityDesignerVerb where TActivity : Activity
    {
        List<ActivityDesigner> matchingActivityDesigner;
        ActivityComparer<TActivity> matchMaker;
        ActivityDesigner owner;

        public FindSimilarActivitiesVerb(ActivityDesigner designer, ActivityComparer<TActivity> matchMaker, string displayText)
            : base(designer, DesignerVerbGroup.Misc, displayText, new EventHandler(OnInvoke))
        {
            Fx.Assert(designer != null,
                "Received null for designer parameter to FindSimilarActivitiesVerb ctor.");
            Fx.Assert(matchMaker != null,
                "Received null for matchMaker parameter to FindSimilarActivitiesVerb ctor.");
            this.owner = designer;
            this.matchMaker = matchMaker;
        }

        private static void OnInvoke(object source, EventArgs e)
        {
            FindSimilarActivitiesVerb<TActivity> designerVerb = source as FindSimilarActivitiesVerb<TActivity>;
            ActivityDesigner activityDesigner = designerVerb.owner;
            List<ActivityDesigner> highlightedDesigners = designerVerb.GetMatchingActivityDesigners(activityDesigner);
            ActivityDesignerHighlighter hightlighter = new ActivityDesignerHighlighter(activityDesigner);
            hightlighter.Highlight(highlightedDesigners);
        }

        private ActivityDesigner GetDesigner(Activity activity)
        {

            IDesignerHost designerHost = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
            return designerHost.GetDesigner(activity as IComponent) as ActivityDesigner;
        }

        private List<ActivityDesigner> GetMatchingActivityDesigners(ActivityDesigner activityDesigner)
        {
            CompositeActivityDesigner rootDesigner = DesignerPainter.GetRootDesigner(activityDesigner);
            matchingActivityDesigner = new List<ActivityDesigner>();

            Walker activityTreeWalker = new Walker();
            activityTreeWalker.FoundActivity += new WalkerEventHandler(OnWalkerFoundActivity);
            activityTreeWalker.Walk(rootDesigner.Activity);

            return matchingActivityDesigner;
        }

        private object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceType");
            }

            if (owner.Activity != null && owner.Activity.Site != null)
            {
                return owner.Activity.Site.GetService(serviceType);
            }
            else
            {
                return null;
            }
        }

        private void OnWalkerFoundActivity(Walker walker, WalkerEventArgs eventArgs)
        {
            TActivity foundActivity = eventArgs.CurrentActivity as TActivity;
            if (foundActivity != null)
            {
                if (this.matchMaker((TActivity) owner.Activity, foundActivity))
                {
                    matchingActivityDesigner.Add(GetDesigner(eventArgs.CurrentActivity));
                }
            }
        }
    }
}
