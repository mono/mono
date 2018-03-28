namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.Diagnostics;
    using System.Collections;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Drawing.Imaging;
    using System.Collections.Generic;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;

    #region Class DragDropManager
    //This behavior needs the logical coordinates
    internal class DragDropManager : WorkflowDesignerMessageFilter
    {
        #region Members and Constructor
        private const string CF_DESIGNERSTATE = "CF_WINOEDESIGNERCOMPONENTSSTATE";

        private List<Activity> draggedActivities = new List<Activity>();
        private List<Activity> existingDraggedActivities = new List<Activity>();

        private Image dragImage = null;
        private Point dragImagePointInClientCoOrd = Point.Empty;
        private bool dragImageSnapped = false;

        private ActivityDesigner dropTargetDesigner = null;
        private bool wasCtrlKeyPressed = false;

        private ActivityDesigner draggedDesigner = null;
        private Point dragInitiationPoint = Point.Empty;
        private bool dragStarted = false;

        private bool exceptionInDragDrop = false;

        internal DragDropManager()
        {
        }
        #endregion

        #region WorkflowDesignerMessageFilter Overrides

        #region Within Designer DragDrop Operation
        protected override void Initialize(WorkflowView parentView)
        {
            base.Initialize(parentView);

            IServiceContainer serviceContainer = GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (serviceContainer != null)
            {
                serviceContainer.RemoveService(typeof(DragDropManager));
                serviceContainer.AddService(typeof(DragDropManager), this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    IServiceContainer serviceContainer = GetService(typeof(IServiceContainer)) as IServiceContainer;
                    if (serviceContainer != null)
                        serviceContainer.RemoveService(typeof(DragDropManager));
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            Debug.Assert(this.draggedDesigner == null);
            Debug.Assert(this.dropTargetDesigner == null);

            WorkflowView parentView = ParentView;
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);

            //If the point is not a valid point on layout then return
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
                return false;

            //Cache the point where the mouse was clicked
            if (eventArgs.Button == MouseButtons.Left)
            {
                this.dragInitiationPoint = parentView.ClientPointToLogical(clientPoint);
                this.dragStarted = true;
            }

            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            Debug.Assert(this.dropTargetDesigner == null);

            WorkflowView parentView = ParentView;
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);

            //If the point is not a valid point on layout then return
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
                return false;

            if (eventArgs.Button == MouseButtons.Left)
            {
                Point logicalPoint = parentView.ClientPointToLogical(clientPoint);
                HitTestInfo hitTestInfo = MessageHitTestContext;

                if (this.draggedDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink)this.draggedDesigner).OnMouseDragMove(eventArgs);
                }
                else if (parentView.RootDesigner != null && this.dragStarted && (eventArgs.Button & MouseButtons.Left) > 0 && (Math.Abs(this.dragInitiationPoint.X - logicalPoint.X) > SystemInformation.DragSize.Width || Math.Abs(this.dragInitiationPoint.Y - logicalPoint.Y) > SystemInformation.DragSize.Height))
                {
                    //Test if the mouse click was on the designer
                    ActivityDesigner potentialDraggedDesigner = hitTestInfo.AssociatedDesigner;
                    if (potentialDraggedDesigner != null)
                    {
                        //If we can intitiate the drag then do so otherwise just indicate that the designer isbeing dragged
                        if (CanInitiateDragDrop())
                        {
                            InitiateDragDrop();
                            this.dragStarted = false;
                        }
                        else
                        {
                            this.draggedDesigner = potentialDraggedDesigner;
                            ((IWorkflowDesignerMessageSink)this.draggedDesigner).OnMouseDragBegin(this.dragInitiationPoint, eventArgs);
                            parentView.Capture = true;
                        }
                    }
                }
            }
            else
            {
                if (this.draggedDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.draggedDesigner).OnMouseDragEnd();
                this.draggedDesigner = null;
            }

            return (this.draggedDesigner != null);
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            Debug.Assert(this.dropTargetDesigner == null);

            if (this.draggedDesigner != null)
            {
                ((IWorkflowDesignerMessageSink)this.draggedDesigner).OnMouseDragEnd();
                this.draggedDesigner = null;
                this.dragStarted = false;
                ParentView.Capture = false;
                return true;
            }

            return false;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            return (this.draggedDesigner != null);
        }

        protected override bool OnMouseHover(MouseEventArgs eventArgs)
        {
            return (this.draggedDesigner != null);
        }

        protected override bool OnMouseLeave()
        {
            return (this.draggedDesigner != null);
        }

        protected override bool OnMouseCaptureChanged()
        {
            if (!ParentView.Capture)
            {
                if (this.draggedDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.draggedDesigner).OnMouseDragEnd();
                this.draggedDesigner = null;
                this.dragStarted = false;
            }

            return false;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (this.draggedDesigner != null)
            {
                if (eventArgs.KeyValue == (int)Keys.Escape)
                {
                    ((IWorkflowDesignerMessageSink)this.draggedDesigner).OnMouseDragEnd();
                    this.draggedDesigner = null;
                    this.dragStarted = false;
                    ParentView.Capture = false;
                }
                else
                {
                    Debug.Assert(this.dropTargetDesigner == null);
                    ((IWorkflowDesignerMessageSink)this.draggedDesigner).OnKeyDown(eventArgs);
                    eventArgs.Handled = true;
                }

                return true;
            }

            return false;
        }

        protected override bool OnKeyUp(KeyEventArgs eventArgs)
        {
            if (this.draggedDesigner != null)
            {
                Debug.Assert(this.dropTargetDesigner == null);
                ((IWorkflowDesignerMessageSink)this.draggedDesigner).OnKeyUp(eventArgs);
                eventArgs.Handled = true;
                return true;
            }

            return false;
        }
        #endregion

        #region DragDrop Operations
        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            //We purposely pass the DragEnter thru to the next behavior so that the WindowingBehavior can clear the
            //active designer
            Debug.Assert(this.dropTargetDesigner == null);

            //Invalidate the entire rectangle so that we draw active placement glyphs on connectors
            WorkflowView parentView = ParentView;
            parentView.InvalidateClientRectangle(Rectangle.Empty);

            //By default we do not allow any drag drop operation
            eventArgs.Effect = DragDropEffects.None;
            this.wasCtrlKeyPressed = false;

            //Now cache the components which are getting dragged so that we don't need to create them again and again
            if (this.existingDraggedActivities.Count > 0)
            {
                this.draggedActivities.AddRange(this.existingDraggedActivities);
            }
            else
            {
                try
                {
                    Activity[] activities = CompositeActivityDesigner.DeserializeActivitiesFromDataObject(ParentView, eventArgs.Data);
                    if (activities != null)
                        this.draggedActivities.AddRange(activities);
                }
                catch
                {
                    this.exceptionInDragDrop = true;
                }
            }

            //Get the coordinates
            Point clientPoint = parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
            Point logicalPoint = parentView.ScreenPointToLogical(new Point(eventArgs.X, eventArgs.Y));

            //Now try to create the drag image and invalidate the area so that we can draw the dragged image
            Debug.Assert(this.dragImage == null);
            CreateDragFeedbackImages(this.draggedActivities);
            if (this.dragImage != null)
                this.dragImagePointInClientCoOrd = new Point(clientPoint.X + SystemInformation.CursorSize.Width / 4, clientPoint.Y + SystemInformation.CursorSize.Height / 4);

            //If the hit is not in the layouts then we need to bail out, this is very important
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
                return false;

            //Now we have a potential for successful drag drop, so construct drag event arguments with logical coordinates
            this.wasCtrlKeyPressed = ((eventArgs.KeyState & 8) == 8);
            ActivityDragEventArgs dragdropEventArgs = new ActivityDragEventArgs(eventArgs, this.dragInitiationPoint, logicalPoint, this.draggedActivities);

            //Now check which designer is under the cursor, if there is no designer then we return
            HitTestInfo hitTestInfo = MessageHitTestContext;
            ActivityDesigner potentialDropTargetDesigner = hitTestInfo.AssociatedDesigner;
            if (potentialDropTargetDesigner == null)
                return false;

            //Now that we found a potential droptarget designer, make sure that we can start drag drop
            //If the drag drop can not be performed then return.
            if (!this.wasCtrlKeyPressed && IsRecursiveDropOperation(potentialDropTargetDesigner))
                return false;

            CompositeActivityDesigner compositeDesigner = potentialDropTargetDesigner as CompositeActivityDesigner;
            if (compositeDesigner != null && !compositeDesigner.IsEditable)
                return false;

            //Now that we can truely perform drag and drop operation we can pump in the message
            this.dropTargetDesigner = potentialDropTargetDesigner;
            ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragEnter(dragdropEventArgs);

            //Check the return value, if this is a potential snap location then we need to snap the image
            if (!dragdropEventArgs.DragImageSnapPoint.IsEmpty)
            {
                Point midPointInClientCoOrd = parentView.LogicalPointToClient(dragdropEventArgs.DragImageSnapPoint);
                Size dragImageIconSize = parentView.LogicalSizeToClient(AmbientTheme.DragImageIconSize);
                this.dragImagePointInClientCoOrd = new Point(midPointInClientCoOrd.X - dragImageIconSize.Width / 2, midPointInClientCoOrd.Y - dragImageIconSize.Height / 2);
                this.dragImageSnapped = true;
            }

            eventArgs.Effect = dragdropEventArgs.Effect;

            if (eventArgs.Effect == DragDropEffects.None && this.exceptionInDragDrop)
                eventArgs.Effect = (this.wasCtrlKeyPressed) ? DragDropEffects.Copy : DragDropEffects.Move;

            return true;
        }

        protected override bool OnDragOver(DragEventArgs eventArgs)
        {
            //By default we do not allow any drag drop operation
            eventArgs.Effect = DragDropEffects.None;
            this.wasCtrlKeyPressed = false;
            this.dragImageSnapped = false;

            //Get the coordinates
            WorkflowView parentView = ParentView;
            Point clientPoint = parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
            Point logicalPoint = parentView.ScreenPointToLogical(new Point(eventArgs.X, eventArgs.Y));

            //Update the drag image position
            Point oldDragImagePoint = this.dragImagePointInClientCoOrd;
            this.dragImagePointInClientCoOrd = new Point(clientPoint.X + SystemInformation.CursorSize.Width / 4, clientPoint.Y + SystemInformation.CursorSize.Height / 4);

            //Now check if the drag point is in active layout if not then clear the designer
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
            {
                if (this.dropTargetDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragLeave();
                this.dropTargetDesigner = null;
            }
            else
            {
                //Now we have a potential for successful drag drop, so construct drag event arguments with logical coordinates
                this.wasCtrlKeyPressed = ((eventArgs.KeyState & 8) == 8);
                ActivityDragEventArgs dragdropEventArgs = new ActivityDragEventArgs(eventArgs, this.dragInitiationPoint, logicalPoint, this.draggedActivities);

                //Now check which designer is under the cursor, if there is no designer then we return
                HitTestInfo hitTestInfo = MessageHitTestContext;
                ActivityDesigner potentialDropTargetDesigner = hitTestInfo.AssociatedDesigner;
                if (potentialDropTargetDesigner != null)
                {
                    CompositeActivityDesigner compositeDesigner = potentialDropTargetDesigner as CompositeActivityDesigner;
                    if ((!this.wasCtrlKeyPressed && IsRecursiveDropOperation(potentialDropTargetDesigner)) ||
                        (compositeDesigner != null && !compositeDesigner.IsEditable))
                    {
                        dragdropEventArgs.Effect = DragDropEffects.None;
                        potentialDropTargetDesigner = null;
                    }
                }

                //If the designers differ then send appropriate messages
                if (this.dropTargetDesigner != potentialDropTargetDesigner)
                {
                    if (this.dropTargetDesigner != null)
                        ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragLeave();

                    this.dropTargetDesigner = potentialDropTargetDesigner;

                    if (this.dropTargetDesigner != null)
                        ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragEnter(dragdropEventArgs);
                }
                else
                {
                    //Looks like we got the same designer
                    if (this.dropTargetDesigner != null)
                        ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragOver(dragdropEventArgs);

                    //Check if there is a potential for the drag image to be snapped
                    if (DragDropEffects.None != dragdropEventArgs.Effect && !dragdropEventArgs.DragImageSnapPoint.IsEmpty)
                    {
                        Point midPointInClientCoOrd = parentView.LogicalPointToClient(dragdropEventArgs.DragImageSnapPoint);
                        Size dragImageIconSize = parentView.LogicalSizeToClient(AmbientTheme.DragImageIconSize);
                        this.dragImagePointInClientCoOrd = new Point(midPointInClientCoOrd.X - dragImageIconSize.Width / 2, midPointInClientCoOrd.Y - dragImageIconSize.Height / 2);
                        this.dragImageSnapped = true;
                    }
                }

                eventArgs.Effect = dragdropEventArgs.Effect;
            }

            //


            if (this.dragImage != null)
            {
                parentView.InvalidateClientRectangle(new Rectangle(oldDragImagePoint, this.dragImage.Size));
                parentView.InvalidateClientRectangle(new Rectangle(this.dragImagePointInClientCoOrd, this.dragImage.Size));
            }

            if (eventArgs.Effect == DragDropEffects.None && this.exceptionInDragDrop)
                eventArgs.Effect = (this.wasCtrlKeyPressed) ? DragDropEffects.Copy : DragDropEffects.Move;

            return true;
        }

        protected override bool OnDragDrop(DragEventArgs eventArgs)
        {
            //Invalidate the entire rectangle so that we draw active placement glyphs on connectors
            WorkflowView parentView = ParentView;
            parentView.InvalidateClientRectangle(Rectangle.Empty);

            //By default we do not allow any drag drop operation
            eventArgs.Effect = DragDropEffects.None;

            DestroyDragFeedbackImages();

            //Get the coordinates
            Point clientPoint = parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
            Point logicalPoint = parentView.ScreenPointToLogical(new Point(eventArgs.X, eventArgs.Y));

            //Now we check if the drag drop was in any valid area, if not then do not proceed further
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
            {
                if (this.dropTargetDesigner != null)
                    ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragLeave();
                this.wasCtrlKeyPressed = false;
                this.dropTargetDesigner = null;
                this.draggedActivities.Clear();
                return false;
            }

            //Now we have a potential for successful drag drop, so construct drag event arguments with logical coordinates
            this.wasCtrlKeyPressed = ((eventArgs.KeyState & 8) == 8);
            ActivityDragEventArgs dragdropEventArgs = new ActivityDragEventArgs(eventArgs, this.dragInitiationPoint, logicalPoint, this.draggedActivities);

            //Now check which designer is under the cursor, if we have the same designer as the old one
            //If not then we set the new one as drop target and pump in messages
            HitTestInfo hitTestInfo = MessageHitTestContext;
            if (this.dropTargetDesigner != hitTestInfo.AssociatedDesigner)
            {
                if (this.dropTargetDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragLeave();
                    this.dropTargetDesigner = null;
                }

                if (hitTestInfo.AssociatedDesigner != null)
                {
                    this.dropTargetDesigner = hitTestInfo.AssociatedDesigner;
                    if (this.dropTargetDesigner != null)
                        ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragEnter(dragdropEventArgs);
                }
            }

            //We now have appropriate droptarget designer
            try
            {
                if (this.dropTargetDesigner != null)
                {
                    //We do not allow recursive drag and drop
                    if (!this.wasCtrlKeyPressed && IsRecursiveDropOperation(this.dropTargetDesigner) ||
                        (this.dropTargetDesigner is CompositeActivityDesigner && !((CompositeActivityDesigner)this.dropTargetDesigner).IsEditable))
                    {
                        ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragLeave();
                        dragdropEventArgs.Effect = DragDropEffects.None;
                    }
                    else
                    {
                        // IMPORTANT: Don't use draggedActivities variable, because components which are
                        // there may not be created using the assembly references  added to ITypeResultionService
                        // this.workflowView.time the components will be created using the assembly references got added to the project
                        List<Activity> droppedActivities = new List<Activity>();
                        string transactionDescription = SR.GetString(SR.DragDropActivities);

                        //This means that we are trying to move activities so we use the same activities for drop
                        if (!this.wasCtrlKeyPressed && this.existingDraggedActivities.Count > 0)
                        {
                            droppedActivities.AddRange(this.existingDraggedActivities);
                            if (droppedActivities.Count > 1)
                                transactionDescription = SR.GetString(SR.MoveMultipleActivities, droppedActivities.Count);
                            else if (droppedActivities.Count == 1)
                                transactionDescription = SR.GetString(SR.MoveSingleActivity, droppedActivities[0].GetType());
                        }
                        else
                        {
                            droppedActivities.AddRange(CompositeActivityDesigner.DeserializeActivitiesFromDataObject(ParentView, eventArgs.Data, true));
                            if (droppedActivities.Count > 0)
                                transactionDescription = SR.GetString(SR.CreateActivityFromToolbox, droppedActivities[0].GetType());
                        }

                        //Now that we have what needs to be dropped, we start the actual drag and drop
                        IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
                        DesignerTransaction transaction = null;
                        if (droppedActivities.Count > 0)
                            transaction = designerHost.CreateTransaction(transactionDescription);

                        dragdropEventArgs = new ActivityDragEventArgs(eventArgs, this.dragInitiationPoint, logicalPoint, droppedActivities);

                        try
                        {
                            ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragDrop(dragdropEventArgs);

                            if (dragdropEventArgs.Effect == DragDropEffects.Move)
                                this.existingDraggedActivities.Clear();

                            if (transaction != null)
                                transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            if (transaction != null)
                                transaction.Cancel();
                            throw e;
                        }

                        //We deserialize the designers and try to store the designer states
                        if (droppedActivities.Count > 0)
                        {
                            Stream componentStateStream = eventArgs.Data.GetData(DragDropManager.CF_DESIGNERSTATE) as Stream;
                            if (componentStateStream != null)
                                Helpers.DeserializeDesignersFromStream(droppedActivities, componentStateStream);

                            //Set the current selection
                            ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
                            if (selectionService != null)
                                selectionService.SetSelectedComponents(droppedActivities, SelectionTypes.Replace);
                        }

                        //Active the design surface
                        if (designerHost != null)
                            designerHost.Activate();
                    }
                }
            }
            catch (Exception ex)
            {
                //We purposely consume application thrown exception which are result of user cancelling the action
                //during dragdrop where we popup UI Wizards during drag drop. Ref: InvokeWebService
                ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragLeave();
                dragdropEventArgs.Effect = DragDropEffects.None;

                string dragDropException = ex.Message;
                if (ex.InnerException != null && !String.IsNullOrEmpty(ex.InnerException.Message))
                    dragDropException = ex.InnerException.Message;

                string errorMessage = DR.GetString(DR.Error_FailedToDeserializeComponents);
                errorMessage += "\r\n" + DR.GetString(DR.Error_Reason, dragDropException);
                DesignerHelpers.ShowError(ParentView, errorMessage);

                if (ex != CheckoutException.Canceled)
                    throw new Exception(errorMessage, ex);
            }
            finally
            {
                //Make sure that mouse over designer is set to null
                this.wasCtrlKeyPressed = false;
                this.draggedActivities.Clear();
                this.dropTargetDesigner = null;
                this.exceptionInDragDrop = false;
                eventArgs.Effect = dragdropEventArgs.Effect;
            }

            return true;
        }

        protected override bool OnDragLeave()
        {
            //Invalidate so that we can clear the drag image and active placement glyphs
            WorkflowView parentView = ParentView;
            parentView.InvalidateClientRectangle(Rectangle.Empty);

            DestroyDragFeedbackImages();

            //Clear the control key flag
            this.wasCtrlKeyPressed = false;

            //Now we fire the drag leave event
            if (this.dropTargetDesigner != null)
                ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnDragLeave();

            //Clear the buffered designer as the drag drop has ended
            this.dropTargetDesigner = null;
            this.draggedActivities.Clear();
            this.exceptionInDragDrop = false;

            return true;
        }

        protected override bool OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            if (this.dropTargetDesigner != null)
                ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnGiveFeedback(gfbevent);
            return true;
        }

        protected override bool OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent)
        {
            if (this.dropTargetDesigner != null)
                ((IWorkflowDesignerMessageSink)this.dropTargetDesigner).OnQueryContinueDrag(qcdevent);
            return true;
        }
        #endregion

        #region Drawing
        protected override bool OnPaintWorkflowAdornments(PaintEventArgs e, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            if (this.dragImage != null)
                ActivityDesignerPaint.DrawImage(e.Graphics, this.dragImage, new Rectangle(this.dragImagePointInClientCoOrd, this.dragImage.Size), new Rectangle(0, 0, this.dragImage.Width, this.dragImage.Height), DesignerContentAlignment.Center, (this.dragImageSnapped) ? 1.0f : 0.5f, WorkflowTheme.CurrentTheme.AmbientTheme.DrawGrayscale);
            return false;
        }
        #endregion

        #endregion

        #region Properties
        public ActivityDesigner DropTargetDesigner
        {
            get
            {
                return this.dropTargetDesigner;
            }
        }

        public ActivityDesigner DraggedDesigner
        {
            get
            {
                return this.draggedDesigner;
            }
        }

        public IList<Activity> DraggedActivities
        {
            get
            {
                return this.draggedActivities.AsReadOnly();
            }
        }

        public Point DragInitiationPoint
        {
            get
            {
                return this.dragInitiationPoint;
            }
        }
        #endregion

        #region Protected Methods
        protected virtual void CreateDragFeedbackImages(IList<Activity> draggedActivities)
        {
            Bitmap draggedImage = null;
            if (draggedActivities.Count > 0)
            {
                Bitmap image = null;
                String description = String.Empty;
                if (draggedActivities.Count > 1)
                {
                    image = DR.GetImage(DR.Activities) as Bitmap;
                    description = DR.GetString(DR.ActivitiesDesc);
                }
                else
                {
                    ToolboxBitmapAttribute toolboxBitmapAttribute = (ToolboxBitmapAttribute)TypeDescriptor.GetAttributes(draggedActivities[0].GetType())[typeof(ToolboxBitmapAttribute)];
                    image = toolboxBitmapAttribute.GetImage(draggedActivities[0].GetType()) as Bitmap;
                    description = draggedActivities[0].GetType().Name;
                }

                if (image != null && description.Length > 0)
                {
                    //Start creating a bitmap
                    WorkflowView parentView = ParentView;
                    Rectangle imageRectangle = (image != null) ? new Rectangle(Point.Empty, image.Size) : Rectangle.Empty;
                    Rectangle descriptionRectangle = (description.Length > 0) ? new Rectangle(Point.Empty, new Size(AmbientTheme.DragImageTextSize.Width, parentView.Font.Height + 2)) : Rectangle.Empty;
                    if (!imageRectangle.IsEmpty)
                        descriptionRectangle.Offset(imageRectangle.Width + AmbientTheme.DragImageMargins.Width, 0);

                    Size draggedImageSize = parentView.LogicalSizeToClient(new Size(imageRectangle.Width + descriptionRectangle.Width, Math.Max(imageRectangle.Height, descriptionRectangle.Height)));
                    draggedImage = new Bitmap(draggedImageSize.Width, draggedImageSize.Height, PixelFormat.Format32bppArgb);
                    using (Graphics draggedImageGraphics = Graphics.FromImage(draggedImage))
                    using (Brush backgroundBrush = new SolidBrush(Color.FromArgb(0, 255, 0, 255)))
                    {
                        draggedImageGraphics.ScaleTransform(ScaleZoomFactor, ScaleZoomFactor);

                        draggedImageGraphics.FillRectangle(backgroundBrush, new Rectangle(0, 0, draggedImage.Width, draggedImage.Height));
                        if (image != null)
                            draggedImageGraphics.DrawImage(image, new Rectangle(Point.Empty, image.Size));

                        if (description.Length > 0)
                        {
                            StringFormat stringFormat = new StringFormat();
                            stringFormat.Alignment = StringAlignment.Near;
                            stringFormat.Trimming = StringTrimming.EllipsisCharacter;
                            stringFormat.LineAlignment = StringAlignment.Center;
                            draggedImageGraphics.DrawString(description, parentView.Font, SystemBrushes.WindowText, descriptionRectangle, stringFormat);
                        }
                    }
                }
            }

            this.dragImage = draggedImage;
        }

        protected virtual void DestroyDragFeedbackImages()
        {
            //Dispose the drag image if it is created
            if (this.dragImage != null)
            {
                this.dragImage.Dispose();
                this.dragImage = null;
            }
        }
        #endregion

        #region Helpers
        internal bool IsValidDropContext(HitTestInfo dropLocation)
        {
            if (this.draggedActivities.Count == 0)
                return false;

            if (dropLocation == null || dropLocation.AssociatedDesigner == null)
                return false;

            CompositeActivityDesigner compositeDesigner = dropLocation.AssociatedDesigner as CompositeActivityDesigner;
            if (compositeDesigner == null)
                return false;

            if (!compositeDesigner.IsEditable || !compositeDesigner.CanInsertActivities(dropLocation, new List<Activity>(this.draggedActivities).AsReadOnly()))
                return false;

            if (!this.wasCtrlKeyPressed && this.existingDraggedActivities.Count > 0)
            {
                //We are trying to move the actvities with designer
                if (!DesignerHelpers.AreAssociatedDesignersMovable(this.draggedActivities))
                    return false;

                if (IsRecursiveDropOperation(dropLocation.AssociatedDesigner))
                    return false;

                IDictionary commonParentActivities = Helpers.PairUpCommonParentActivities(this.draggedActivities);
                foreach (DictionaryEntry entry in commonParentActivities)
                {
                    CompositeActivityDesigner compositeActivityDesigner = ActivityDesigner.GetDesigner(entry.Key as Activity) as CompositeActivityDesigner;
                    Activity[] activitiesToMove = (Activity[])((ArrayList)entry.Value).ToArray(typeof(Activity));
                    if (compositeActivityDesigner != null && !compositeActivityDesigner.CanMoveActivities(dropLocation, new List<Activity>(activitiesToMove).AsReadOnly()))
                        return false;
                }
            }

            return true;
        }

        private float ScaleZoomFactor
        {
            get
            {
                WorkflowView parentView = ParentView;
                return ((float)parentView.Zoom / 100.0f * parentView.ActiveLayout.Scaling);
            }
        }

        private bool IsRecursiveDropOperation(ActivityDesigner dropTargetDesigner)
        {
            if (dropTargetDesigner == null)
                return false;

            ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
            CompositeActivity dropTargetComponent = dropTargetDesigner.Activity as CompositeActivity;
            if (dropTargetComponent == null || selectionService == null)
                return false;

            // First check for activity designer specific recursion - possible recursion when drag-n-drop from outside the current 
            // designer such toolbox or other activity designers.
            WorkflowView workflowView = GetService(typeof(WorkflowView)) as WorkflowView;
            IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
            WorkflowDesignerLoader loader = GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;

            // When drag-n-drop within the same designer, if the drag drop is not within designer or no valid droptarget, we do not do anything
            if (this.draggedActivities.Count == 0 || this.existingDraggedActivities.Count == 0)
                return false;

            //Go thru all the components in dragged components and check for recursive dragdrop
            //Get all the top level activities being dragged dropped
            ArrayList topLevelActivities = new ArrayList(Helpers.GetTopLevelActivities(selectionService.GetSelectedComponents()));
            CompositeActivity parentActivity = dropTargetComponent;
            while (parentActivity != null)
            {
                if (topLevelActivities.Contains(parentActivity))
                    return true;

                parentActivity = parentActivity.Parent;
            }


            return false;
        }

        private bool CanInitiateDragDrop()
        {
            //Go thru all the selected components and make sure that they can participate in drag drop
            ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
            IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (selectionService == null || designerHost == null)
                return false;

            // check if we are cutting root component
            ICollection components = selectionService.GetSelectedComponents();
            if (components == null || components.Count < 1 || selectionService.GetComponentSelected(designerHost.RootComponent) || !Helpers.AreAllActivities(components))
                return false;

            return true;
        }

        private void InitiateDragDrop()
        {
            WorkflowView parentView = ParentView;
            ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
            IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (selectionService == null || designerHost == null)
                return;

            // check if we are cutting root component
            ICollection components = selectionService.GetSelectedComponents();
            if (components == null || components.Count < 1 || selectionService.GetComponentSelected(designerHost.RootComponent) || !Helpers.AreAllActivities(components))
                return;

            DragDropEffects effects = DragDropEffects.None;
            try
            {
                // get component serialization service
                this.existingDraggedActivities.AddRange(Helpers.GetTopLevelActivities(components));

                //IMPORTANT: FOR WITHIN DESIGNER COMPONENT MOVE WE REMOVE THE ACTIVITIES BEFORE WE ADD THEM WHICH IS IN 
                //ONDRAGDROP FUNCTION. ALTHOUGH THIS VIOLATES THE DODRAGDROP FUNCTION SIMANTICS, WE NEED TO DO THIS
                //SO THAT WE CAN USE THE SAME IDS FOR THE ACTIVITIES
                DragDropEffects allowedEffects = (DesignerHelpers.AreAssociatedDesignersMovable(this.existingDraggedActivities)) ? DragDropEffects.Move | DragDropEffects.Copy : DragDropEffects.Copy;
                IDataObject dataObject = CompositeActivityDesigner.SerializeActivitiesToDataObject(ParentView, this.existingDraggedActivities.ToArray());
                effects = parentView.DoDragDrop(dataObject, allowedEffects);

                // 
            }
            catch (Exception e)
            {
                DesignerHelpers.ShowError(ParentView, e.Message);
            }
            finally
            {
                //This means drag drop occurred across designer
                if (effects == DragDropEffects.Move && this.existingDraggedActivities.Count > 0)
                {
                    string transactionDescription = String.Empty;
                    if (this.existingDraggedActivities.Count > 1)
                        transactionDescription = SR.GetString(SR.MoveMultipleActivities, this.existingDraggedActivities.Count);
                    else
                        transactionDescription = SR.GetString(SR.MoveSingleActivity, this.existingDraggedActivities[0].GetType());

                    CompositeActivityDesigner.RemoveActivities(ParentView, this.existingDraggedActivities.AsReadOnly(), transactionDescription);
                }

                this.existingDraggedActivities.Clear();
            }
        }
        #endregion
    }
    #endregion
}
