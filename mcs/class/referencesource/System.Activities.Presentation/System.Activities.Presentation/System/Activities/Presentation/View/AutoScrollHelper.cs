//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using System.Windows.Input;

    internal static class AutoScrollHelper
    {
        const int scrollBuffer = 30;

        public static void AutoScroll(MouseEventArgs e, DependencyObject element, double offsetPerScroll)
        {
            FrameworkElement logicalView = element as FrameworkElement;
            while (element != null)
            {
                element = VisualTreeHelper.GetParent(element);
                if (element != null && element is ScrollViewer)
                {
                    break;
                }
            }
            ScrollViewer scrollViewer = element as ScrollViewer;
            if (scrollViewer != null)
            {                
                AutoScroll(e.GetPosition(scrollViewer), scrollViewer, logicalView != null ? e.GetPosition(logicalView) : (Point?)null, logicalView,
                     25, 25, offsetPerScroll);
            }
        }

        public static void AutoScroll(DragEventArgs e, ScrollViewer scrollViewer, double offsetPerScroll)
        {
            AutoScroll(e.GetPosition(scrollViewer), scrollViewer, offsetPerScroll);
        }

        public static void AutoScroll(Point position, ScrollViewer scrollViewer, double offsetPerScroll)
        {
            AutoScroll(position, scrollViewer, null, null,
                50, 50, offsetPerScroll);
        }

        static void AutoScroll(Point positionInScrollViewer, ScrollViewer scrollViewer, Point? positionInLogicalView, FrameworkElement logicalView, double scrollOnDragThresholdX, double scrollOnDragThresholdY, double scrollOnDragOffset)
        {
            double scrollViewerWidth = scrollViewer.ActualWidth;
            double scrollViewerHeight = scrollViewer.ActualHeight;
            
            double logicalViewWidth = 0;
            double logicalViewHeight = 0;
            if (logicalView != null)
            {
                logicalViewWidth = logicalView.ActualWidth;
                logicalViewHeight = logicalView.ActualHeight;
            }             
            
            double heightToScroll = 0;
            double widthToScroll = 0;

            if (positionInScrollViewer.X > (scrollViewerWidth - scrollOnDragThresholdX)
                && (positionInLogicalView == null
                   || positionInLogicalView.Value.X < (logicalViewWidth - scrollBuffer)))
            {
                widthToScroll = scrollOnDragOffset;
            }
            else if (positionInScrollViewer.X < scrollOnDragThresholdX
                && (positionInLogicalView == null
                   || positionInLogicalView.Value.X > scrollBuffer))
            {
                widthToScroll = -scrollOnDragOffset;
            }

            if (positionInScrollViewer.Y > (scrollViewerHeight - scrollOnDragThresholdY)
                && (positionInLogicalView == null
                    || positionInLogicalView.Value.Y < logicalViewHeight - scrollBuffer))
            {
                heightToScroll = scrollOnDragOffset;
            }
            else if (positionInScrollViewer.Y < scrollOnDragThresholdY
                && (positionInLogicalView == null
                   || positionInLogicalView.Value.Y > scrollBuffer))
            {
                heightToScroll = -scrollOnDragOffset;
            }

            if (widthToScroll != 0 || heightToScroll != 0)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + heightToScroll);
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + widthToScroll);
            }
        }
    }
}
