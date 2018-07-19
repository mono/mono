//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.ServiceModel;


    // <summary>
    // Helper class for visuaulizing the highlighted activity group
    // </summary>
    class ActivityDesignerHighlighter : IServiceProvider
    {
        private IDesignerGlyphProviderService glyphProviderService;
        private HighlightGlyphProvider highlightProvider = null;
        private ActivityDesigner owner;
        private WorkflowView workflowView;
        public ActivityDesignerHighlighter(ActivityDesigner owner)
        {
            this.owner = owner;
        }

        public object GetService(Type serviceType)
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

        public void Highlight(List<ActivityDesigner> highlightedDesigners)
        {
            if (highlightedDesigners == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("hightlightedDesigners");
            }

            glyphProviderService = this.GetService(typeof(IDesignerGlyphProviderService)) as IDesignerGlyphProviderService;
            workflowView = GetService(typeof(WorkflowView)) as WorkflowView;

            RemoveCurrentHighlight();

            IDesignerHost designerHost = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerHighlighterMesageFilter messageFilter = new DesignerHighlighterMesageFilter();
            highlightProvider = new HighlightGlyphProvider(designerHost.GetDesigner(designerHost.RootComponent) as ActivityDesigner, highlightedDesigners);
            glyphProviderService.AddGlyphProvider(highlightProvider);
            highlightProvider.MessageFilter = messageFilter;

            messageFilter.MouseDown += new EventHandler<System.Windows.Forms.MouseEventArgs>(messageFilter_MouseDown);
            messageFilter.KeyDown += new EventHandler<System.Windows.Forms.KeyEventArgs>(messageFilter_KeyDown);
            workflowView.AddDesignerMessageFilter(messageFilter);
            workflowView.FitToScreenSize();
        }

        public void RemoveCurrentHighlight()
        {
            HighlightGlyphProvider currentHightlightGlyhProvider = null;
            foreach (IDesignerGlyphProvider glyphProvider in glyphProviderService.GlyphProviders)
            {
                if (glyphProvider is HighlightGlyphProvider)
                {
                    currentHightlightGlyhProvider = (HighlightGlyphProvider) glyphProvider;
                    break;
                }
            }
            if (currentHightlightGlyhProvider != null)
            {
                //remove associated designerMessageFilter before removing currentGlyphProvider.
                workflowView.RemoveDesignerMessageFilter(currentHightlightGlyhProvider.MessageFilter);
                glyphProviderService.RemoveGlyphProvider(currentHightlightGlyhProvider);
            }

        }

        void messageFilter_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            glyphProviderService.RemoveGlyphProvider(highlightProvider);
            if (workflowView != null)
            {
                workflowView.RemoveDesignerMessageFilter(sender as WorkflowDesignerMessageFilter);
                Point scrollPosition = workflowView.ClientPointToLogical(owner.Location);
                workflowView.FitToWorkflowSize();
                // try to center the owner designer int the the workflowview
                Size viewSize = workflowView.ClientSizeToLogical(workflowView.ViewPortSize);
                if (scrollPosition.Y > viewSize.Height / 2)
                {
                    scrollPosition.Y -= viewSize.Height / 2;
                }
                if (scrollPosition.X > viewSize.Width / 2)
                {
                    scrollPosition.X -= viewSize.Width / 2;
                }
                workflowView.ScrollPosition = scrollPosition;
            }
        }

        void messageFilter_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            glyphProviderService.RemoveGlyphProvider(highlightProvider);
            if (workflowView != null)
            {
                workflowView.RemoveDesignerMessageFilter(sender as WorkflowDesignerMessageFilter);
                Point scrollPosition = workflowView.ClientPointToLogical(e.Location);
                workflowView.FitToWorkflowSize();
                // try to center the clicked portion of the workflow in the workflowview
                Size viewSize = workflowView.ClientSizeToLogical(workflowView.ViewPortSize);
                if (scrollPosition.Y > viewSize.Height / 2)
                {
                    scrollPosition.Y -= viewSize.Height / 2;
                }
                if (scrollPosition.X > viewSize.Width / 2)
                {
                    scrollPosition.X -= viewSize.Width / 2;
                }
                workflowView.ScrollPosition = scrollPosition;
            }
        }

        // this is the message filter inserted in the workflowview to escape back to the normal view
        // from the highlighted view. since glyphs cant take mouse events, this is the only way to 
        // detect mouseclicks when in highlighted view.

        internal sealed class DesignerHighlighterMesageFilter : WorkflowDesignerMessageFilter
        {
            public event EventHandler<System.Windows.Forms.KeyEventArgs> KeyDown;
            public event EventHandler<System.Windows.Forms.MouseEventArgs> MouseDown;

            protected override bool OnKeyDown(System.Windows.Forms.KeyEventArgs eventArgs)
            {
                if (KeyDown != null)
                {
                    KeyDown(this, eventArgs);
                }
                // let event pass down to others. we dont want to mark it as handled
                return false;
            }

            protected override bool OnMouseDown(System.Windows.Forms.MouseEventArgs eventArgs)
            {
                if (MouseDown != null)
                {
                    MouseDown(this, eventArgs);
                }
                // let event pass down to others. we dont want to mark it as handled
                return false;
            }
        }

        internal sealed class HighlightGlyphProvider : IDesignerGlyphProvider
        {
            private List<ActivityDesigner> highlightedDesigners;
            private DesignerHighlighterMesageFilter messageFilter;

            private ActivityDesigner rootDesigner;

            public HighlightGlyphProvider(ActivityDesigner rootDesigner, List<ActivityDesigner> highlightedDesigners)
            {

                this.RootDesigner = rootDesigner;
                this.HighlightedDesigners = highlightedDesigners;
            }

            public List<ActivityDesigner> HighlightedDesigners
            {
                get { return highlightedDesigners; }
                set { highlightedDesigners = value; }
            }

            public DesignerHighlighterMesageFilter MessageFilter
            {
                get { return messageFilter; }
                set { messageFilter = value; }
            }

            public ActivityDesigner RootDesigner
            {
                get { return rootDesigner; }
                set { rootDesigner = value; }
            }

            public ActivityDesignerGlyphCollection GetGlyphs(ActivityDesigner activityDesigner)
            {
                if (activityDesigner == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activityDesigner");
                }

                if (!activityDesigner.IsRootDesigner)
                {
                    return null;
                }
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
                glyphs.Add(new HighlightOverlayGlyph(activityDesigner.Bounds, HighlightedDesigners));
                return glyphs;
            }

        }

    }
}
