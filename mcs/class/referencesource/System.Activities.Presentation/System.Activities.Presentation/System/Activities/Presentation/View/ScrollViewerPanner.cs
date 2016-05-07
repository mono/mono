//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    
    internal class ScrollViewerPanner
    {
        private ScrollViewer scrollViewer;
        private Point panningStartPosition;
        private PanState currentPanState;
        private bool inPanMode;
        private MouseButton draggingMouseButton;        

        public ScrollViewerPanner(ScrollViewer scrollViewer)
        {
            Fx.Assert(scrollViewer != null, "ScrollViewer should never be null");
            this.ScrollViewer = scrollViewer;
        }

        internal enum PanState
        {
            Normal,     // Normal editing mode
            ReadyToPan,
            Panning,
        }

        public ScrollViewer ScrollViewer
        {
            get
            {
                return this.scrollViewer;
            }

            set
            {
                if (value != this.scrollViewer)
                {
                    if (this.scrollViewer != null)
                    {
                        this.UnregisterEvents();
                    }

                    this.scrollViewer = value;
                    if (this.scrollViewer != null)
                    {
                        this.RegisterEvents();
                    }
                }
            }
        }

        public bool InPanMode
        {
            get
            {
                return this.inPanMode;
            }

            set
            {
                if (this.inPanMode != value)
                {
                    this.inPanMode = value;
                    if (this.inPanMode)
                    {
                        this.CurrentPanState = PanState.ReadyToPan;
                    }
                    else
                    {
                        this.CurrentPanState = PanState.Normal;
                    }
                }
            }
        }

        public Cursor Hand { get; set; }

        public Cursor DraggingHand { get; set; }

        internal PanState CurrentPanState
        {
            get
            {
                return this.currentPanState;
            }

            set
            {
                if (this.currentPanState != value)
                {
                    this.currentPanState = value;
                    switch (this.currentPanState)
                    {
                        case PanState.ReadyToPan:
                            this.scrollViewer.Cursor = this.Hand;
                            break;
                        case PanState.Panning:
                            this.scrollViewer.Cursor = this.DraggingHand;
                            break;
                        default:
                            this.scrollViewer.Cursor = null;
                            break;
                    }

                    this.UpdateForceCursorProperty();
                }
            }
        }

        internal bool IsInScrollableArea(Point mousePosition)
        {
            return mousePosition.X < this.scrollViewer.ViewportWidth && mousePosition.Y < this.scrollViewer.ViewportHeight;
        }

        internal void OnScrollViewerMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (this.CurrentPanState)
            {
                case PanState.Normal:
                    if (e.ChangedButton == MouseButton.Middle)
                    {
                        this.StartPanningIfNecessary(e);
                    }

                    break;
                case PanState.ReadyToPan:
                    switch (e.ChangedButton)
                    {
                        case MouseButton.Left:
                            this.StartPanningIfNecessary(e);
                            break;
                        case MouseButton.Middle:
                            this.StartPanningIfNecessary(e);
                            break;
                        case MouseButton.Right:
                            e.Handled = true;
                            break;
                    }

                    break;
                case PanState.Panning:
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        internal void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            this.StopPanning();
        }

        internal void OnScrollViewerMouseMove(object sender, MouseEventArgs e)
        {
            switch (this.CurrentPanState)
            {
                case PanState.Panning:
                    Point currentPosition = Mouse.GetPosition(this.scrollViewer);
                    Vector offset = Point.Subtract(currentPosition, this.panningStartPosition);
                    this.panningStartPosition = currentPosition;
                    this.scrollViewer.ScrollToHorizontalOffset(this.scrollViewer.HorizontalOffset - offset.X);
                    this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.VerticalOffset - offset.Y);
                    e.Handled = true;
                    break;
                case PanState.ReadyToPan:
                    this.UpdateForceCursorProperty();
                    break;
                default:
                    break;
            }
        }

        internal void OnScrollViewerMouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (this.CurrentPanState)
            {
                case PanState.ReadyToPan:
                    this.StopPanningIfNecessary(e);

                    // When the mouse is captured by other windows/views, that
                    // window/view needs this mouse-up message to release
                    // the capture.                    
                    if (!this.IsMouseCapturedByOthers())
                    {
                        e.Handled = true;
                    }

                    break;
                case PanState.Panning:
                    this.StopPanningIfNecessary(e);
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        internal void OnScrollViewerKeyDown(object sender, KeyEventArgs e)
        {
            switch (this.CurrentPanState)
            {
                // Don't change to ReadyToPan mode if the space is a 
                // repeated input, because repeated-key input may come 
                // from activity element on the scroll view.
                case PanState.Normal:
                    if (e.Key == Key.Space 
                        && !e.IsRepeat
                        && this.AllowSwitchToPanning()) 
                    {
                        this.CurrentPanState = PanState.ReadyToPan;
                    }

                    break;
                default:
                    break;
            }
        }

        internal void OnScrollViewerKeyUp(object sender, KeyEventArgs e)
        {
            switch (this.CurrentPanState)
            {
                case PanState.ReadyToPan:
                    if (e.Key == Key.Space && !this.InPanMode)
                    {
                        this.CurrentPanState = PanState.Normal;
                    }

                    break;
                default:
                    break;
            }
        }

        private void StartPanningIfNecessary(MouseButtonEventArgs e)
        {
            if (DesignerView.IsMouseInViewport(e, this.scrollViewer))
            {
                this.draggingMouseButton = e.ChangedButton;
                this.CurrentPanState = PanState.Panning;
                this.scrollViewer.Focus();
                this.panningStartPosition = Mouse.GetPosition(this.scrollViewer);
                Mouse.Capture(this.scrollViewer);
                e.Handled = true;
            }
        }

        private void StopPanningIfNecessary(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == this.draggingMouseButton && object.Equals(this.scrollViewer, Mouse.Captured))
            {
                // Trigers OnLostMouseCapture
                this.scrollViewer.ReleaseMouseCapture();
            }
        }

        private void StopPanning()
        {
            // stop panning
            if (this.InPanMode
                || (Keyboard.IsKeyDown(Key.Space) && this.CurrentPanState == PanState.Panning))
            {
                this.CurrentPanState = PanState.ReadyToPan;
            }
            else
            {
                this.CurrentPanState = PanState.Normal;
            }
        }

        private void UpdateForceCursorProperty()
        {
            Point pt = Mouse.GetPosition(this.ScrollViewer);
            if (this.IsInScrollableArea(pt))
            {
                this.scrollViewer.ForceCursor = true;
            }
            else
            {
                this.scrollViewer.ForceCursor = false;
            }
        }

        // Mouse is sometimes captured by ScrollViewer's children, like
        // RepeatButton in Scroll Bar.
        private bool IsMouseCapturedByOthers()
        {
            return (Mouse.Captured != null)
                && !object.Equals(Mouse.Captured, this.ScrollViewer);
        }

        private bool AllowSwitchToPanning()
        {
            return Mouse.LeftButton == MouseButtonState.Released
                && Mouse.RightButton == MouseButtonState.Released;
        }

        private void RegisterEvents()
        {
            this.scrollViewer.PreviewMouseDown  += new MouseButtonEventHandler(this.OnScrollViewerMouseDown);            
            this.scrollViewer.PreviewMouseMove  += new MouseEventHandler(this.OnScrollViewerMouseMove);
            this.scrollViewer.PreviewMouseUp    += new MouseButtonEventHandler(this.OnScrollViewerMouseUp);
            this.scrollViewer.LostMouseCapture  += new MouseEventHandler(this.OnLostMouseCapture);
            this.scrollViewer.KeyDown           += new KeyEventHandler(this.OnScrollViewerKeyDown);
            this.scrollViewer.KeyUp             += new KeyEventHandler(this.OnScrollViewerKeyUp);
        }

        private void UnregisterEvents()
        {
            this.scrollViewer.PreviewMouseDown  -= new MouseButtonEventHandler(this.OnScrollViewerMouseDown);
            this.scrollViewer.PreviewMouseMove  -= new MouseEventHandler(this.OnScrollViewerMouseMove);
            this.scrollViewer.PreviewMouseUp    -= new MouseButtonEventHandler(this.OnScrollViewerMouseUp);
            this.scrollViewer.LostMouseCapture  -= new MouseEventHandler(this.OnLostMouseCapture);
            this.scrollViewer.KeyDown           -= new KeyEventHandler(this.OnScrollViewerKeyDown);
            this.scrollViewer.KeyUp             -= new KeyEventHandler(this.OnScrollViewerKeyUp);
        }
    }
}
