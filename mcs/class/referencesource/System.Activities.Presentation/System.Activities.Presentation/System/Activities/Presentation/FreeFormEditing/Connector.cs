//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    internal class Connector : UserControl
    {
        public const double ArrowShapeWidth = 5;
        
        public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(
            "Points", 
            typeof(PointCollection), 
            typeof(Connector), 
            new FrameworkPropertyMetadata(new PointCollection()));
        
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", 
            typeof(bool), 
            typeof(Connector), 
            new FrameworkPropertyMetadata(false));
        
        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
            "LabelText", 
            typeof(string), 
            typeof(Connector), 
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty HighlightOnHoverProperty = DependencyProperty.Register(
            "HighlightOnHover",
            typeof(bool),
            typeof(Connector),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsHighlightedForAutoSplitProperty = DependencyProperty.Register(
            "IsHighlightedForAutoSplit",
            typeof(bool),
            typeof(Connector),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IdentityProperty = DependencyProperty.Register(
            "Identity", 
            typeof(Guid), 
            typeof(Connector));

        // Label will be shown only if there is one segment in the connector whose length is greater than this.
        internal const int MinConnectorSegmentLengthForLabel = 30;

        private DesignerConfigurationService designerConfigurationService = null;

        private FreeFormPanel panel = null;
        
        public Connector()
        {
            this.Loaded += (sender, e) =>
            {
                this.Identity = Guid.NewGuid();
            };
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "PointCollection is a special WPF class and got special Clone logic, the setter of this property is used several places.")]
        public PointCollection Points
        {
            get { return (PointCollection)GetValue(Connector.PointsProperty); }
            set { SetValue(Connector.PointsProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(Connector.IsSelectedProperty); }
            set { SetValue(Connector.IsSelectedProperty, value); }
        }

        public string LabelText
        {
            get { return (string)GetValue(Connector.LabelTextProperty); }
            set { SetValue(Connector.LabelTextProperty, value); }
        }

        public bool HighlightOnHover
        {
            get { return (bool)GetValue(Connector.HighlightOnHoverProperty); }
            set { SetValue(Connector.HighlightOnHoverProperty, value); }
        }

        public bool IsHighlightedForAutoSplit
        {
            get { return (bool)GetValue(Connector.IsHighlightedForAutoSplitProperty); }
            set { SetValue(Connector.IsHighlightedForAutoSplitProperty, value); }
        }

        public Guid Identity
        {
            get { return (Guid)GetValue(Connector.IdentityProperty); }
            set { SetValue(Connector.IdentityProperty, value); }
        }

        public UIElement SourceShape
        {
            get
            {
                ConnectionPoint sourceConnectionPoint = FreeFormPanel.GetSourceConnectionPoint(this);
                if (sourceConnectionPoint != null)
                {
                    return sourceConnectionPoint.ParentDesigner;
                }

                return null;
            }
        }

        public UIElement DestinationShape
        {
            get
            {
                ConnectionPoint destinationConnectionPoint = FreeFormPanel.GetDestinationConnectionPoint(this);
                if (destinationConnectionPoint != null)
                {
                    return destinationConnectionPoint.ParentDesigner;
                }

                return null;
            }
        }

        public IAutoSplitContainer AutoSplitContainer
        {
            get;
            set;
        }

        public virtual FrameworkElement StartDot
        {
            get
            {
                return null;
            }
        }

        private FreeFormPanel Panel
        {
            get
            {
                if (this.panel == null)
                {
                    this.panel = VisualTreeUtils.FindVisualAncestor<FreeFormPanel>(this);
                }

                return this.panel;
            }
        }

        private bool AutoSplitEnabled
        {
            get
            {
                if (this.designerConfigurationService == null)
                {
                    DesignerView view = VisualTreeUtils.FindVisualAncestor<DesignerView>(this);
                    if (view != null)
                    {
                        this.designerConfigurationService = view.Context.Services.GetService<DesignerConfigurationService>();
                        return this.designerConfigurationService.AutoSplitEnabled;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return this.designerConfigurationService.AutoSplitEnabled;
                }
            }
        }

        public virtual void SetLabelToolTip(object toolTip)
        {
            // subclass should be able to override this method to provide ToolTip of connector
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ConnectorAutomationPeer(this, base.OnCreateAutomationPeer());
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            if (this.AutoSplitEnabled && this.AutoSplitContainer != null && DragDropHelper.GetDraggedObjectCount(e) == 1 && this.AutoSplitContainer.CanAutoSplit(e))
            {
                this.HighlightForAutoSplit();
                this.Panel.RemoveAutoConnectAdorner();
                this.Panel.CurrentAutoSplitTarget = this;
            }

            base.OnDragEnter(e);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            if (this.IsHighlightedForAutoSplit)
            {
                this.DehighlightForAutoSplit();
                this.Panel.CurrentAutoSplitTarget = null;
            }

            base.OnDragLeave(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (this.AutoSplitEnabled && this.IsHighlightedForAutoSplit)
            {
                try
                {
                    this.AutoSplitContainer.DoAutoSplit(e, this);
                }
                finally
                {
                    this.DehighlightForAutoSplit();
                    this.Panel.CurrentAutoSplitTarget = null;
                    e.Handled = true;
                }
            }

            base.OnDrop(e);
        }

        private void AddAutoSplitAdorner(UIElement shape)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(shape);
            Fx.Assert(adornerLayer != null, "AdornerLayer should not be null.");
            adornerLayer.Add(new AutoSplitAdorner(shape));
        }

        private void RemoveAutoSplitAdorner(UIElement shape)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(shape);
            Fx.Assert(adornerLayer != null, "AdornerLayer should not be null.");
            Adorner[] adorners = adornerLayer.GetAdorners(shape);
            foreach (Adorner adorner in adorners)
            {
                if (adorner is AutoSplitAdorner)
                {
                    adornerLayer.Remove(adorner);
                    return;
                }
            }
        }

        private void HighlightForAutoSplit()
        {
            this.IsHighlightedForAutoSplit = true;
            this.AddAutoSplitAdorner(this.SourceShape);
            this.AddAutoSplitAdorner(this.DestinationShape);
        }

        private void DehighlightForAutoSplit()
        {
            this.IsHighlightedForAutoSplit = false;
            this.RemoveAutoSplitAdorner(this.SourceShape);
            this.RemoveAutoSplitAdorner(this.DestinationShape);
        }

        private class ConnectorAutomationPeer : UIElementAutomationPeer
        {
            private AutomationPeer wrappedAutomationPeer;

            public ConnectorAutomationPeer(FrameworkElement owner, AutomationPeer wrappedAutomationPeer)
                : base(owner)
            {
                this.wrappedAutomationPeer = wrappedAutomationPeer;
            }

            protected override string GetItemStatusCore()
            {
                UIElement sourceDesigner = VirtualizedContainerService.TryGetVirtualizedElement(FreeFormPanel.GetSourceConnectionPoint(this.Owner).ParentDesigner);
                string sourceId = sourceDesigner.GetValue(AutomationProperties.ItemStatusProperty) as string;
                UIElement destinationDesigner = VirtualizedContainerService.TryGetVirtualizedElement(FreeFormPanel.GetDestinationConnectionPoint(this.Owner).ParentDesigner);
                string destinationId = destinationDesigner.GetValue(AutomationProperties.ItemStatusProperty) as string;
                return string.Format(CultureInfo.InvariantCulture, "Source={0} Destination={1} Points={2}", sourceId, destinationId, ((Connector)this.Owner).Points);
            }

            protected override string GetClassNameCore()
            {
                return this.wrappedAutomationPeer.GetClassName();
            }

            protected override string GetNameCore()
            {
                return this.wrappedAutomationPeer.GetName();
            }

            protected override string GetAutomationIdCore()
            {
                return this.wrappedAutomationPeer.GetAutomationId();
            }
        }
    }
}
