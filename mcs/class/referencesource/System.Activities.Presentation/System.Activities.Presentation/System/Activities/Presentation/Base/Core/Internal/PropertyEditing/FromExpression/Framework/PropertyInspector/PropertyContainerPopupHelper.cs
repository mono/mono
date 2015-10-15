// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Properties
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Controls;

    // <summary>
    // Acrylic needs a way of knowing when we begin and end extended edit mode,
    // so this class fires commands when those two events take place.
    // </summary>
    internal class PropertyContainerPopup : WorkaroundPopup
    {
        // these events allow Acrylic to implement its workaround for Avalon/MFC interop
        // focus issues (WinOS 
        public static readonly RoutedCommand OnBeginExtendedEdit = new RoutedCommand("OnBeginExtendedEdit", typeof(PropertyContainerPopup));
        public static readonly RoutedCommand OnEndExtendedEdit = new RoutedCommand("OnEndExtendedEdit", typeof(PropertyContainerPopup));

        public static CustomPopupPlacementCallback RightAlignedPopupPlacement
        {
            get { return new CustomPopupPlacementCallback(PropertyContainerPopup.RightAlignedPopupPlacementCallback); }
        }

        protected override void OnOpened(EventArgs e)
        {
            // Fire OnBeginExtendedEdit command (for Acrylic)
            PropertyContainer owningPropertyContainer = (PropertyContainer)this.GetValue(PropertyContainer.OwningPropertyContainerProperty);
            PropertyContainerPopup.OnBeginExtendedEdit.Execute(this, owningPropertyContainer);

            base.OnOpened(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            PropertyContainer owningPropertyContainer = (PropertyContainer)this.GetValue(PropertyContainer.OwningPropertyContainerProperty);

            // Revert back to Inline when the popup is dismissed and we haven't already switched
            // to the pinned mode
            if (owningPropertyContainer != null && owningPropertyContainer.ActiveEditMode == PropertyContainerEditMode.ExtendedPopup)
            {
                DependencyObject potentialDescendant = Mouse.Captured as DependencyObject;
                if (potentialDescendant != null && owningPropertyContainer.IsAncestorOf(potentialDescendant))
                {
                    // v1 38479: This is a mitigation for Windows OS 



                    Mouse.Capture(null);
                }

                owningPropertyContainer.ActiveEditMode = PropertyContainerEditMode.Inline;
            }

            // Fire OnEndExtendedEdit command (for Acrylic)
            OnEndExtendedEdit.Execute(this, owningPropertyContainer);
        }

        public static CustomPopupPlacement[] RightAlignedPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            return new CustomPopupPlacement[] { new CustomPopupPlacement(new Point(targetSize.Width - popupSize.Width, targetSize.Height), PopupPrimaryAxis.Horizontal) };
        }
    }
}
