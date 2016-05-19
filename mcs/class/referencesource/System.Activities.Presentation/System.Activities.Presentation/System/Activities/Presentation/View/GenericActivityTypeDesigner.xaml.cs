// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    internal partial class GenericActivityTypeDesigner
    {
        private static AttachedProperty<string> displayNameProperty = new AttachedProperty<string>
            {
                Name = "DisplayName",
                OwnerType = typeof(ActivityBuilder<>),
                Getter = (modelItem) => ViewUtilities.GetActivityBuilderDisplayName(modelItem)
            };

        public GenericActivityTypeDesigner()
        {
            this.InitializeComponent();
            DesignerView.SetCommandMenuMode(this, CommandMenuMode.NoCommandMenu);
        }

        protected override void OnContextMenuLoaded(ContextMenu menu)
        {
            menu.IsOpen = false;
        }

        private void RegisterDisplayNameProperty()
        {
            AttachedPropertiesService attachedPropertiesService = 
                this.Context.Services.GetService<AttachedPropertiesService>();
            if (attachedPropertiesService == null)
            {
                return;
            }

            IEnumerable<AttachedProperty> properties = attachedPropertiesService.GetAttachedProperties(typeof(ActivityBuilder<>));
            if (properties != null && properties.Contains(displayNameProperty))
            {
                return;
            }

            attachedPropertiesService.AddProperty(displayNameProperty);
        }
    }
}
