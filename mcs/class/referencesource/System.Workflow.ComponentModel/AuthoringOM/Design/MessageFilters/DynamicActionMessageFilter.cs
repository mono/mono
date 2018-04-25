namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Text;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;
    using System.ComponentModel.Design;

    #region Class DynamicActionMessageFilter
    //Behavior needs coordinates in client coordinate system
    internal sealed class DynamicActionMessageFilter : WorkflowDesignerMessageFilter
    {
        #region Members, Construction and Destruction
        private List<DynamicAction> actions = new List<DynamicAction>();

        private int draggedButtonIndex = -1;
        private int draggedActionIndex = -1;

        private bool infoTipSet = false;

        internal DynamicActionMessageFilter()
        {
        }
        #endregion

        #region Properties and Methods
        internal void AddAction(DynamicAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (!this.actions.Contains(action))
            {
                if (IsButtonDragged)
                    SetDraggedButton(-1, -1);

                this.actions.Add(action);
                RefreshAction(action);
            }
        }

        internal bool ActionExists(DynamicAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            return this.actions.Contains(action);
        }

        internal void RemoveAction(DynamicAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (this.actions.Contains(action))
            {
                if (IsButtonDragged)
                    SetDraggedButton(-1, -1);

                RefreshAction(action);
                this.actions.Remove(action);
            }
        }

        internal void RefreshAction(DynamicAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            int actionIndex = this.actions.IndexOf(action);
            if (actionIndex >= 0)
                ParentView.InvalidateClientRectangle(GetActionBounds(actionIndex));
        }
        #endregion

        #region Behavior Overrides
        protected override void Initialize(WorkflowView parentView)
        {
            base.Initialize(parentView);

            IServiceContainer serviceContainer = GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (serviceContainer != null)
            {
                serviceContainer.RemoveService(typeof(DynamicActionMessageFilter));
                serviceContainer.AddService(typeof(DynamicActionMessageFilter), this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                IServiceContainer serviceContainer = GetService(typeof(IServiceContainer)) as IServiceContainer;
                if (serviceContainer != null)
                    serviceContainer.RemoveService(typeof(DynamicActionMessageFilter));
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            UpdateTransparency(new Point(eventArgs.X, eventArgs.Y));
            Refresh();
            return false;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);

            Refresh();
            UpdateTransparency(clientPoint);

            bool retval = false;
            if ((eventArgs.Button & MouseButtons.Left) > 0)
            {
                for (int i = this.actions.Count - 1; i >= 0; i--)
                {
                    DynamicAction action = this.actions[i];
                    Rectangle actionBounds = GetActionBounds(i);
                    if (actionBounds.Contains(clientPoint))
                    {
                        //If we clicked on disabled button then no further action is needed
                        for (int j = 0; j < action.Buttons.Count; j++)
                        {
                            Rectangle buttonBounds = GetButtonBounds(i, j);
                            if (buttonBounds.Contains(clientPoint) && action.Buttons[j].State == ActionButton.States.Disabled)
                                return true;
                        }

                        //Now check all the buttons and update their states
                        for (int j = 0; j < action.Buttons.Count; j++)
                        {
                            ActionButton actionButton = action.Buttons[j];
                            if (actionButton.State != ActionButton.States.Disabled)
                            {
                                Rectangle buttonBounds = GetButtonBounds(i, j);
                                if (buttonBounds.Contains(clientPoint))
                                {
                                    actionButton.State = ActionButton.States.Pressed;
                                    if (action.ActionType != DynamicAction.ActionTypes.TwoState)
                                        SetDraggedButton(i, j);
                                }
                                else if (action.ActionType == DynamicAction.ActionTypes.TwoState)
                                {
                                    actionButton.State = ActionButton.States.Normal;
                                }
                            }
                        }

                        retval = true;
                    }
                }
            }

            return retval;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);

            Refresh();
            UpdateTransparency(clientPoint);

            string infoTip = String.Empty;
            bool retval = IsButtonDragged;
            if (!IsButtonDragged)
            {
                for (int i = this.actions.Count - 1; i >= 0; i--)
                {
                    DynamicAction action = this.actions[i];
                    Rectangle actionBounds = GetActionBounds(i);

                    for (int j = 0; j < action.Buttons.Count; j++)
                    {
                        ActionButton actionButton = action.Buttons[j];

                        if (actionBounds.Contains(clientPoint))
                        {
                            Rectangle buttonBounds = GetButtonBounds(i, j);
                            bool buttonContainsPoint = buttonBounds.Contains(clientPoint);

                            if (buttonContainsPoint && infoTip.Length == 0)
                                infoTip = actionButton.Description;

                            if (actionButton.State != ActionButton.States.Disabled &&
                                actionButton.State != ActionButton.States.Pressed)
                            {
                                if (buttonContainsPoint)
                                    actionButton.State = ActionButton.States.Highlight;
                                else
                                    actionButton.State = ActionButton.States.Normal;
                            }

                            retval = true;
                        }
                        else
                        {
                            if (actionButton.State == ActionButton.States.Highlight)
                                actionButton.State = ActionButton.States.Normal;
                        }
                    }
                }
            }

            WorkflowView parentView = ParentView;
            if (infoTip.Length > 0)
            {
                this.infoTipSet = true;
                parentView.ShowInfoTip(infoTip);
            }
            else if (this.infoTipSet)
            {
                parentView.ShowInfoTip(String.Empty);
                this.infoTipSet = false;
            }

            return retval;
        }

        protected override bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            for (int i = this.actions.Count - 1; i >= 0; i--)
            {
                DynamicAction action = this.actions[i];
                Rectangle actionBounds = GetActionBounds(i);
                if (actionBounds.Contains(new Point(eventArgs.X, eventArgs.Y)))
                    return true;
            }

            return false;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);

            Refresh();
            UpdateTransparency(clientPoint);

            bool retval = false;
            if ((eventArgs.Button & MouseButtons.Left) > 0)
            {
                for (int i = this.actions.Count - 1; i >= 0; i--)
                {
                    DynamicAction action = this.actions[i];
                    Rectangle actionBounds = GetActionBounds(i);
                    if (actionBounds.Contains(clientPoint))
                    {
                        for (int j = 0; j < action.Buttons.Count; j++)
                        {
                            ActionButton actionButton = action.Buttons[j];

                            if (actionButton.State != ActionButton.States.Disabled)
                            {
                                Rectangle buttonBounds = GetButtonBounds(i, j);

                                if (buttonBounds.Contains(clientPoint) && action.ActionType != DynamicAction.ActionTypes.TwoState)
                                    actionButton.State = ActionButton.States.Highlight;
                                else if (actionButton.State == ActionButton.States.Highlight)
                                    actionButton.State = ActionButton.States.Normal;
                            }
                        }

                        retval = true;
                    }
                }
            }

            if (IsButtonDragged)
                SetDraggedButton(-1, -1);

            return retval;
        }

        protected override bool OnMouseLeave()
        {
            ParentView.ShowInfoTip(String.Empty);
            UpdateTransparency(Point.Empty);
            Refresh();
            return false;
        }

        protected override bool OnMouseCaptureChanged()
        {
            if (IsButtonDragged)
                SetDraggedButton(-1, -1);
            return false;
        }

        protected override bool OnPaintWorkflowAdornments(PaintEventArgs e, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            for (int i = 0; i < this.actions.Count; i++)
            {
                GraphicsContainer graphicsState = e.Graphics.BeginContainer();
                Point actionLocation = GetActionBounds(i).Location;
                e.Graphics.TranslateTransform(actionLocation.X, actionLocation.Y);
                this.actions[i].Draw(e.Graphics);
                e.Graphics.EndContainer(graphicsState);
            }
            return false;
        }
        #endregion

        #region Helpers
        private void Refresh()
        {
            WorkflowView parentView = ParentView;
            for (int i = 0; i < this.actions.Count; i++)
                parentView.InvalidateClientRectangle(GetActionBounds(i));
        }

        private Rectangle GetActionBounds(int actionIndex)
        {
            Rectangle bounds = new Rectangle(Point.Empty, ParentView.ViewPortSize);
            DynamicAction action = this.actions[actionIndex];

            bounds.Inflate(-action.DockMargin.Width, -action.DockMargin.Height);
            return new Rectangle(ActivityDesignerPaint.GetRectangleFromAlignment(action.DockAlignment, bounds, action.Bounds.Size).Location, action.Bounds.Size);
        }

        private Rectangle GetButtonBounds(int actionIndex, int buttonIndex)
        {
            Rectangle bounds = GetActionBounds(actionIndex);
            Rectangle buttonBounds = this.actions[actionIndex].GetButtonBounds(buttonIndex);
            buttonBounds.Offset(bounds.Location);
            return buttonBounds;
        }

        private void UpdateTransparency(Point point)
        {
            for (int i = 0; i < this.actions.Count; i++)
            {
                float transparency = 0;
                if (!point.IsEmpty)
                {
                    Rectangle actionBounds = GetActionBounds(i);
                    if (actionBounds.Contains(point) || this.draggedActionIndex == i)
                    {
                        transparency = 1.0f;
                    }
                    else
                    {
                        Rectangle rectangle = ParentView.ViewPortRectangle;
                        double distance = DesignerGeometryHelper.DistanceFromPointToRectangle(point, actionBounds);
                        if (distance > rectangle.Width / 3 || distance > rectangle.Height / 3)
                        {
                            transparency = 0.3f;
                        }
                        else
                        {
                            //Uncomment the following code for fluctuating transparency
                            //1.0f - ((float)Convert.ToInt32(distance)) / Math.Max(ParentView.ViewPortSize.Width, ParentView.ViewPortSize.Height);
                            transparency = 1.0f;
                        }
                    }
                }

                this.actions[i].Transparency = transparency;
            }
        }

        private bool IsButtonDragged
        {
            get
            {
                return (this.draggedActionIndex >= 0 && this.draggedButtonIndex >= 0);
            }
        }

        private void SetDraggedButton(int actionIndex, int buttonIndex)
        {
            if (this.draggedActionIndex == actionIndex && this.draggedButtonIndex == buttonIndex)
                return;

            WorkflowView parentView = ParentView;
            if (this.draggedActionIndex >= 0 && this.draggedButtonIndex >= 0)
            {
                if (this.draggedActionIndex < this.actions.Count)
                    this.actions[this.draggedActionIndex].Buttons[this.draggedButtonIndex].State = ActionButton.States.Highlight;

                this.draggedActionIndex = -1;
                this.draggedButtonIndex = -1;
                parentView.Capture = false;
                UpdateTransparency(parentView.PointToClient(Control.MousePosition));
            }

            this.draggedActionIndex = actionIndex;
            this.draggedButtonIndex = buttonIndex;

            if (this.draggedActionIndex >= 0 && this.draggedButtonIndex >= 0)
                parentView.Capture = true;
        }
        #endregion
    }
    #endregion

    #region Class DynamicAction
    internal class DynamicAction : IDisposable
    {
        #region Members and constructor
        private static float DefaultTransparency = 0.0f;
        private static Size[] Sizes = new Size[] { new Size(20, 20), new Size(24, 24), new Size(28, 28), new Size(32, 32), new Size(36, 36) };
        private static Size[] Margins = new Size[] { new Size(1, 1), new Size(1, 1), new Size(2, 2), new Size(2, 2), new Size(3, 3) };
        internal enum ButtonSizes { Small = 0, SmallMedium = 1, Medium = 2, MediumLarge = 3, Large = 4 };
        internal enum ActionTypes { Standard = 1, TwoState = 2 };

        private ItemList<ActionButton> buttons = null;
        private ButtonSizes buttonSizeType = ButtonSizes.Medium;
        private DesignerContentAlignment dockAlignment = DesignerContentAlignment.TopLeft;
        private float minimumTransparency = DynamicAction.DefaultTransparency;
        private float transparency = DynamicAction.DefaultTransparency;
        private ActionTypes actionType = ActionTypes.Standard;

        private Size borderSize = new Size(2, 2);
        private Size dockMargin = DynamicAction.Sizes[(int)ButtonSizes.Medium];
        private Size buttonSize = DynamicAction.Sizes[(int)ButtonSizes.Medium];
        private Size margin = DynamicAction.Margins[(int)ButtonSizes.Medium];

        internal DynamicAction()
        {
            this.buttons = new ItemList<ActionButton>(this);
        }

        ~DynamicAction()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (ActionButton button in this.buttons)
                ((IDisposable)button).Dispose();
            this.buttons.Clear();
        }
        #endregion

        #region Properties and Methods
        internal IList<ActionButton> Buttons
        {
            get
            {
                return this.buttons;
            }
        }

        internal Size DockMargin
        {
            get
            {
                return this.dockMargin;
            }

            set
            {
                this.dockMargin = value;
            }
        }

        internal ActionTypes ActionType
        {
            get
            {
                return this.actionType;
            }
        }

        internal ButtonSizes ButtonSize
        {
            get
            {
                return this.buttonSizeType;
            }

            set
            {
                if (this.buttonSizeType == value)
                    return;

                this.buttonSizeType = value;
                this.buttonSize = DynamicAction.Sizes[(int)this.buttonSizeType];
                this.margin = DynamicAction.Margins[(int)this.buttonSizeType];
            }
        }

        internal DesignerContentAlignment DockAlignment
        {
            get
            {
                return this.dockAlignment;
            }

            set
            {
                if (this.dockAlignment == value)
                    return;

                this.dockAlignment = value;
            }
        }

        internal float Transparency
        {
            get
            {
                return this.transparency;
            }

            set
            {
                if (this.transparency == value)
                    return;

                this.transparency = Math.Max(DynamicAction.DefaultTransparency, value);
            }
        }

        internal void Draw(Graphics graphics)
        {
            if (this.transparency == 0 || this.buttons.Count == 0)
                return;

            ActivityDesignerPaint.Draw3DButton(graphics, null, Bounds, this.transparency - 0.1f, ButtonState.Normal);

            for (int i = 0; i < this.buttons.Count; i++)
            {
                Rectangle buttonBounds = GetButtonBounds(i);
                ActionButton button = this.buttons[i];
                if (button.StateImages.Length == 1)
                {
                    Image buttonImage = button.StateImages[0];
                    if (button.State == ActionButton.States.Normal || button.State == ActionButton.States.Disabled)
                    {
                        buttonBounds.Inflate(-2, -2);
                        ActivityDesignerPaint.DrawImage(graphics, buttonImage, buttonBounds, new Rectangle(Point.Empty, buttonImage.Size), DesignerContentAlignment.Fill, transparency, (button.State == ActionButton.States.Disabled));
                    }
                    else
                    {
                        ButtonState state = (button.State == ActionButton.States.Highlight) ? ButtonState.Normal : ButtonState.Pushed;
                        ActivityDesignerPaint.Draw3DButton(graphics, buttonImage, buttonBounds, this.transparency, state);
                    }
                }
                else
                {
                    Image buttonImage = this.buttons[i].StateImages[(int)this.buttons[i].State];
                    buttonBounds.Inflate(-2, -2);
                    ActivityDesignerPaint.DrawImage(graphics, buttonImage, buttonBounds, new Rectangle(Point.Empty, buttonImage.Size), DesignerContentAlignment.Fill, this.transparency, false);
                }
            }
        }

        internal Rectangle Bounds
        {
            get
            {
                Size size = Size.Empty;
                int buttonCount = Math.Max(1, this.buttons.Count);
                size.Width = (2 * borderSize.Width) + (buttonCount * this.buttonSize.Width) + ((buttonCount + 1) * this.margin.Width);
                size.Height = (2 * borderSize.Height) + this.buttonSize.Height + (2 * this.margin.Height);
                return new Rectangle(Point.Empty, size);
            }
        }

        internal Rectangle GetButtonBounds(int buttonIndex)
        {
            if (buttonIndex < 0 || buttonIndex >= this.buttons.Count)
                throw new ArgumentOutOfRangeException("buttonIndex");

            Rectangle rectangle = Rectangle.Empty;
            rectangle.X = this.borderSize.Width + (buttonIndex * this.buttonSize.Width) + ((buttonIndex + 1) * this.margin.Width);
            rectangle.Y = this.borderSize.Height + this.margin.Height;
            rectangle.Size = this.buttonSize;
            return rectangle;
        }
        #endregion
    }
    #endregion

    #region Class ActionButton
    internal sealed class ActionButton : IDisposable
    {
        #region Members, Constructor and Destruction
        internal enum States { Normal = 0, Highlight = 1, Pressed = 2, Disabled = 3 };

        internal event EventHandler StateChanged;

        private Image[] stateImages = null;
        private string description = String.Empty;
        private States buttonState = States.Normal;

        internal ActionButton(Image[] stateImages)
        {
            StateImages = stateImages;
        }

        void IDisposable.Dispose()
        {
        }
        #endregion

        #region Properties and Methods
        internal Image[] StateImages
        {
            get
            {
                return this.stateImages;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Length != 1 && value.Length != 4)
                    throw new ArgumentException(SR.GetString(SR.Error_InvalidStateImages), "value");

                this.stateImages = value;
                foreach (Image image in this.stateImages)
                {
                    Bitmap bitmap = image as Bitmap;
                    if (bitmap != null)
                        bitmap.MakeTransparent(AmbientTheme.TransparentColor);
                }
            }
        }

        internal States State
        {
            get
            {
                return this.buttonState;
            }

            set
            {
                if (this.buttonState == value)
                    return;

                this.buttonState = value;

                if (StateChanged != null)
                    StateChanged(this, EventArgs.Empty);
            }
        }

        internal string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = value;
            }
        }
        #endregion
    }
    #endregion
}
