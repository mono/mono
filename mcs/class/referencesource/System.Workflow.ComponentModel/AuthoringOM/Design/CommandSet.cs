namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Resources;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Printing;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    // IMPORTANT: 
    //KEYBOARD: You need to goto <Document and settings\<user name>\ApplicationData\Microsoft\VisualStudio\8.0\" and delete 
    // all of your *.vsk file, becuase VS always picks up keyboard bindings from that file, and also on deveenv.exe /.setup
    // he does not clean that up. 
    //MENUS: You need to goto <Document and settings\<user name>\ApplicationData\Microsoft\VisualStudio\8.0\1033" and delete 
    // all of your *.prf file, becuase VS always picks up menus from that file, and also on deveenv.exe /.setup
    // he does not clean that up. 
    internal sealed class CommandSet : IDisposable
    {
        internal static CommandID[] NavigationToolCommandIds = new CommandID[] { WorkflowMenuCommands.ZoomIn, WorkflowMenuCommands.ZoomOut, WorkflowMenuCommands.Pan, WorkflowMenuCommands.DefaultFilter };

        private IServiceProvider serviceProvider;
        private IMenuCommandService menuCommandService;
        private ISelectionService selectionService;
        private WorkflowView workflowView;

        private List<CommandSetItem> commandSet;
        private CommandSetItem[] zoomCommands;
        private CommandSetItem[] layoutCommands;
        private CommandSetItem[] navigationToolCommands;

        private const string CF_DESIGNER = "CF_WINOEDESIGNERCOMPONENTS";
        private const string CF_DESIGNERSTATE = "CF_WINOEDESIGNERCOMPONENTSSTATE";

        private WorkflowDesignerMessageFilter activeFilter;

        public CommandSet(IServiceProvider serviceProvider)
        {
            Debug.Assert(serviceProvider != null);
            this.serviceProvider = serviceProvider;

            this.menuCommandService = (IMenuCommandService)this.serviceProvider.GetService(typeof(IMenuCommandService));
            if (this.menuCommandService == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IMenuCommandService).FullName));

            this.workflowView = serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (this.workflowView == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(WorkflowView).FullName));

            this.selectionService = (ISelectionService)this.serviceProvider.GetService(typeof(ISelectionService));
            if (this.selectionService == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ISelectionService).FullName));

            this.commandSet = new List<CommandSetItem>();
            this.commandSet.AddRange(new CommandSetItem[] {
                        //Save commands
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnMenuSaveWorkflowAsImage), WorkflowMenuCommands.SaveAsImage), 
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnMenuCopyToClipboard), WorkflowMenuCommands.CopyToClipboard), 

                        // Printing commands
                        new CommandSetItem(new EventHandler(OnStatusPrint), new EventHandler(OnMenuPrint), WorkflowMenuCommands.Print), 
                        new CommandSetItem(new EventHandler(OnStatusPageSetup), new EventHandler(OnMenuPageSetup), WorkflowMenuCommands.PageSetup), 

                        // Editing commands
                        new CommandSetItem(new EventHandler(OnStatusDelete), new EventHandler(OnMenuDelete), MenuCommands.Delete), 
                        new CommandSetItem(new EventHandler(OnStatusCopy), new EventHandler(OnMenuCopy), MenuCommands.Copy), 
                        new CommandSetItem(new EventHandler(OnStatusCut), new EventHandler(OnMenuCut), MenuCommands.Cut), 
                        new CommandSetItem(new EventHandler(OnStatusPaste), new EventHandler(OnMenuPaste), MenuCommands.Paste, true),
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnMenuSelectAll), MenuCommands.SelectAll),

                        // Properties
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnMenuDesignerProperties), WorkflowMenuCommands.DesignerProperties),

                        // IMPORTANT: Microsoft does not handle this command, so VS.NET sends it to solution explorer
                        // window, which enables this meu item on the for the current file node
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnViewCode), new CommandID(StandardCommands.Cut.Guid, 333)),

                        // Keyboard commands
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnKeyCancel), MenuCommands.KeyCancel), 
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnKeyCancel), MenuCommands.KeyReverseCancel), 
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnKeyMove), MenuCommands.KeyMoveUp), 
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnKeyMove), MenuCommands.KeyMoveDown), 
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnKeyMove), MenuCommands.KeyMoveLeft), 
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnKeyMove), MenuCommands.KeyMoveRight),
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnKeyMove), MenuCommands.KeySelectNext), 
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnKeyMove), MenuCommands.KeySelectPrevious),
                        new CommandSetItem(new EventHandler(OnStatusExpandCollapse), new EventHandler(OnExpandCollapse), WorkflowMenuCommands.Expand),
                        new CommandSetItem(new EventHandler(OnStatusExpandCollapse), new EventHandler(OnExpandCollapse), WorkflowMenuCommands.Collapse),
                        new CommandSetItem(new EventHandler(OnStatusEnable), new EventHandler(OnEnable), WorkflowMenuCommands.Disable, true),
                        new CommandSetItem(new EventHandler(OnStatusEnable), new EventHandler(OnEnable), WorkflowMenuCommands.Enable, true),
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnCreateTheme), WorkflowMenuCommands.CreateTheme),
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnChangeTheme), WorkflowMenuCommands.ChangeTheme),
                        new CommandSetItem(new EventHandler(OnStatusAnySelection), new EventHandler(OnKeyDefault), MenuCommands.KeyDefaultAction),
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnKeyPageDnUp), WorkflowMenuCommands.PageUp),
                        new CommandSetItem(new EventHandler(OnStatusAlways), new EventHandler(OnKeyPageDnUp), WorkflowMenuCommands.PageDown),

                    });


            //WorkflowView commands
            this.zoomCommands = new CommandSetItem[] 
                    {
                        new CommandSetItem(new EventHandler(OnStatusZoom), new EventHandler(OnZoom), WorkflowMenuCommands.Zoom400Mode, DR.GetString(DR.Zoom400Mode)),
                        new CommandSetItem(new EventHandler(OnStatusZoom), new EventHandler(OnZoom), WorkflowMenuCommands.Zoom300Mode, DR.GetString(DR.Zoom300Mode)),
                        new CommandSetItem(new EventHandler(OnStatusZoom), new EventHandler(OnZoom), WorkflowMenuCommands.Zoom200Mode, DR.GetString(DR.Zoom200Mode)),
                        new CommandSetItem(new EventHandler(OnStatusZoom), new EventHandler(OnZoom), WorkflowMenuCommands.Zoom150Mode, DR.GetString(DR.Zoom150Mode)),
                        new CommandSetItem(new EventHandler(OnStatusZoom), new EventHandler(OnZoom), WorkflowMenuCommands.Zoom100Mode, DR.GetString(DR.Zoom100Mode)),
                        new CommandSetItem(new EventHandler(OnStatusZoom), new EventHandler(OnZoom), WorkflowMenuCommands.Zoom75Mode, DR.GetString(DR.Zoom75Mode)),
                        new CommandSetItem(new EventHandler(OnStatusZoom), new EventHandler(OnZoom), WorkflowMenuCommands.Zoom50Mode, DR.GetString(DR.Zoom50Mode)),
                        new CommandSetItem(new EventHandler(OnStatusZoom), new EventHandler(OnZoom), WorkflowMenuCommands.ShowAll, DR.GetString(DR.ZoomShowAll)),
                    };
            this.commandSet.AddRange(this.zoomCommands);

            this.layoutCommands = new CommandSetItem[] 
                    {
                        new CommandSetItem(new EventHandler(OnStatusLayout), new EventHandler(OnPageLayout), WorkflowMenuCommands.DefaultPage), 
                        new CommandSetItem(new EventHandler(OnStatusLayout), new EventHandler(OnPageLayout), WorkflowMenuCommands.PrintPreviewPage), 
                        new CommandSetItem(new EventHandler(OnStatusLayout), new EventHandler(OnPageLayout), WorkflowMenuCommands.PrintPreview),
                    };
            this.commandSet.AddRange(this.layoutCommands);

            this.navigationToolCommands = new CommandSetItem[] 
                    {
                        new CommandSetItem(new EventHandler(OnStatusMessageFilter), new EventHandler(OnMessageFilterChanged), NavigationToolCommandIds[0]), 
                        new CommandSetItem(new EventHandler(OnStatusMessageFilter), new EventHandler(OnMessageFilterChanged), NavigationToolCommandIds[1]),
                        new CommandSetItem(new EventHandler(OnStatusMessageFilter), new EventHandler(OnMessageFilterChanged), NavigationToolCommandIds[2]), 
                        new CommandSetItem(new EventHandler(OnStatusMessageFilter), new EventHandler(OnMessageFilterChanged), NavigationToolCommandIds[3]),
                    };

            this.commandSet.AddRange(this.navigationToolCommands);

            // add all menu commands
            for (int i = 0; i < this.commandSet.Count; i++)
            {
                if (this.menuCommandService.FindCommand(this.commandSet[i].CommandID) == null)
                    this.menuCommandService.AddCommand(this.commandSet[i]);
            }

            IComponentChangeService changeService = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (changeService != null)
                changeService.ComponentChanged += new ComponentChangedEventHandler(OnComponentChanged);

            // Now setup the default command GUID for this designer.  This GUID is also used in our toolbar
            // definition file to identify toolbars we own.  We store the GUID in a command ID here in the
            // dictionary of the root component.  Our host may pull this GUID out and use it.
            IDictionaryService ds = this.serviceProvider.GetService(typeof(IDictionaryService)) as IDictionaryService;
            if (ds != null)
                ds.SetValue(typeof(CommandID), new CommandID(new Guid("5f1c3c8d-60f1-4b98-b85b-8679f97e8eac"), 0));
        }

        #region IDisposable Members
        public void Dispose()
        {
            IComponentChangeService changeService = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (changeService != null)
                changeService.ComponentChanged -= new ComponentChangedEventHandler(OnComponentChanged);

            if (this.activeFilter != null)
            {
                this.workflowView.RemoveDesignerMessageFilter(this.activeFilter);
                this.activeFilter.Dispose();
                this.activeFilter = null;
            }

            this.selectionService = null;

            for (int i = 0; i < this.commandSet.Count; i++)
                this.menuCommandService.RemoveCommand(this.commandSet[i]);
            this.menuCommandService = null;
        }
        #endregion

        #region Helper fucntions
        internal void UpdateCommandSet()
        {
            // whip through all of the commands and ask them to update.
            for (int i = 0; i < this.commandSet.Count; i++)
                this.commandSet[i].UpdateStatus();
        }

        internal void UpdateZoomCommands(bool enable)
        {
            int commandID = this.ConvertToZoomCommand(this.workflowView.Zoom);
            foreach (MenuCommand menuCommand in this.zoomCommands)
            {
                menuCommand.Enabled = enable;
                menuCommand.Checked = (commandID == menuCommand.CommandID.ID);
            }
        }

        internal void UpdatePageLayoutCommands(bool enable)
        {
            //we might have two commands checked at the same (PrintPreviewPage and PrintPreview - since they have sligtly different logic (one is toggle and the other is not))
            foreach (MenuCommand menuCommand in this.layoutCommands)
            {
                menuCommand.Enabled = enable;
                menuCommand.Checked = this.workflowView.PrintPreviewMode ? (menuCommand.CommandID == WorkflowMenuCommands.PrintPreview || menuCommand.CommandID == WorkflowMenuCommands.PrintPreviewPage) : menuCommand.CommandID == WorkflowMenuCommands.DefaultPage;
            }
        }

        internal void UpdatePanCommands(bool enable)
        {
            CommandID commandID = ConvertMessageFilterToCommandID();
            foreach (MenuCommand menuCommand in this.navigationToolCommands)
            {
                menuCommand.Enabled = enable;
                menuCommand.Checked = (commandID == menuCommand.CommandID);
            }
        }

        private CommandID ConvertMessageFilterToCommandID()
        {
            if (this.activeFilter is PanningMessageFilter)
            {
                return WorkflowMenuCommands.Pan;
            }
            else if (this.activeFilter is ZoomingMessageFilter)
            {
                if (((ZoomingMessageFilter)this.activeFilter).ZoomingIn)
                    return WorkflowMenuCommands.ZoomIn;
                else
                    return WorkflowMenuCommands.ZoomOut;
            }
            else
            {
                return WorkflowMenuCommands.DefaultFilter;
            }
        }

        private int ConvertToZoomLevel(int commandId)
        {
            int zoomLevel = 100;
            if (commandId == WorkflowMenuCommands.Zoom400Mode.ID) zoomLevel = 400;
            else if (commandId == WorkflowMenuCommands.Zoom300Mode.ID) zoomLevel = 300;
            else if (commandId == WorkflowMenuCommands.Zoom200Mode.ID) zoomLevel = 200;
            else if (commandId == WorkflowMenuCommands.Zoom150Mode.ID) zoomLevel = 150;
            else if (commandId == WorkflowMenuCommands.Zoom100Mode.ID) zoomLevel = 100;
            else if (commandId == WorkflowMenuCommands.Zoom75Mode.ID) zoomLevel = 75;
            else if (commandId == WorkflowMenuCommands.Zoom50Mode.ID) zoomLevel = 50;

            return zoomLevel;
        }

        private int ConvertToZoomCommand(int zoomLevel)
        {
            int commandID = 0; //do not select anything if the zoom level is not one of the standard ones
            if (zoomLevel == 400) commandID = WorkflowMenuCommands.Zoom400Mode.ID;
            else if (zoomLevel == 300) commandID = WorkflowMenuCommands.Zoom300Mode.ID;
            else if (zoomLevel == 200) commandID = WorkflowMenuCommands.Zoom200Mode.ID;
            else if (zoomLevel == 150) commandID = WorkflowMenuCommands.Zoom150Mode.ID;
            else if (zoomLevel == 100) commandID = WorkflowMenuCommands.Zoom100Mode.ID;
            else if (zoomLevel == 75) commandID = WorkflowMenuCommands.Zoom75Mode.ID;
            else if (zoomLevel == 50) commandID = WorkflowMenuCommands.Zoom50Mode.ID;

            return commandID;
        }
        #endregion

        #region Status Handlers
        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (this.activeFilter != null)
            {
                this.workflowView.RemoveDesignerMessageFilter(this.activeFilter);
                this.activeFilter = null;
                UpdatePanCommands(true);
            }
        }

        private void OnStatusZoom(object sender, EventArgs e)
        {
            UpdateZoomCommands(true);
        }

        private void OnZoom(object sender, EventArgs e)
        {
            MenuCommand menuCommand = (MenuCommand)sender;
            if (menuCommand.CommandID.ID == WorkflowMenuCommands.ShowAll.ID)
            {
                int newZoom = (int)(100.0f / this.workflowView.ActiveLayout.Scaling * Math.Min((float)this.workflowView.ViewPortSize.Width / (float)this.workflowView.ActiveLayout.Extent.Width, (float)this.workflowView.ViewPortSize.Height / (float)this.workflowView.ActiveLayout.Extent.Height));
                this.workflowView.Zoom = Math.Min(Math.Max(newZoom, AmbientTheme.MinZoom), AmbientTheme.MaxZoom);
            }
            else
            {
                this.workflowView.Zoom = ConvertToZoomLevel(menuCommand.CommandID.ID);
            }

            UpdateZoomCommands(true);
        }

        private void OnStatusLayout(object sender, EventArgs e)
        {
            UpdatePageLayoutCommands(true);
        }

        private void OnPageLayout(object sender, EventArgs e)
        {
            MenuCommand menuCommand = (MenuCommand)sender;
            this.workflowView.PrintPreviewMode = (menuCommand.CommandID == WorkflowMenuCommands.PrintPreview) ? !this.workflowView.PrintPreviewMode : (menuCommand.CommandID == WorkflowMenuCommands.PrintPreviewPage);
            UpdatePageLayoutCommands(true);
        }

        private void OnStatusMessageFilter(object sender, EventArgs e)
        {
            UpdatePanCommands(true);
        }

        private void OnMessageFilterChanged(object sender, EventArgs e)
        {
            if (this.activeFilter != null)
            {
                this.workflowView.RemoveDesignerMessageFilter(this.activeFilter);
                this.activeFilter = null;
            }

            MenuCommand menuCommand = (MenuCommand)sender;
            int commandId = menuCommand.CommandID.ID;
            if (WorkflowMenuCommands.ZoomIn.ID == commandId)
                this.activeFilter = new ZoomingMessageFilter(true);
            else if (WorkflowMenuCommands.ZoomOut.ID == commandId)
                this.activeFilter = new ZoomingMessageFilter(false);
            else if (WorkflowMenuCommands.Pan.ID == commandId)
                this.activeFilter = new PanningMessageFilter();

            if (this.activeFilter != null)
                this.workflowView.AddDesignerMessageFilter(this.activeFilter);

            this.workflowView.Focus();
            UpdatePanCommands(true);
        }

        private void OnStatusPrint(object sender, EventArgs e)
        {
            OnStatusAlways(sender, e);
        }

        private void OnStatusPageSetup(object sender, EventArgs e)
        {
            OnStatusAlways(sender, e);
        }

        private void OnStatusCopy(object sender, EventArgs e)
        {
            MenuCommand cmd = (MenuCommand)sender;
            bool enable = false;

            IDesignerHost designerHost = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost != null && !designerHost.Loading)
            {
                ArrayList selectedComponents = new ArrayList(this.selectionService.GetSelectedComponents());
                enable = Helpers.AreAllActivities(selectedComponents);

                if (enable)
                {
                    foreach (Activity activity in selectedComponents)
                    {
                        if (activity.Site != null)
                        {
                            designerHost = activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                            if (designerHost != null && this.selectionService.GetComponentSelected(designerHost.RootComponent))
                            {
                                enable = false;
                                break;
                            }
                        }
                    }
                }
            }

            cmd.Enabled = enable;
        }

        private void OnStatusCut(object sender, EventArgs e)
        {
            OnStatusDelete(sender, e);
        }

        private void OnStatusDelete(object sender, EventArgs e)
        {
            MenuCommand cmd = (MenuCommand)sender;
            cmd.Enabled = false;

            // check if we are cutting root component
            IDesignerHost designerHost = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost != null && designerHost.RootComponent != null && this.selectionService.GetComponentSelected(designerHost.RootComponent))
                return;

            //Check that we are cutting all activities
            //Check if we are in writable context
            ICollection components = this.selectionService.GetSelectedComponents();
            if (!DesignerHelpers.AreComponentsRemovable(components))
                return;

            // check if we can delete these
            Activity[] topLevelActivities = Helpers.GetTopLevelActivities(components);
            IDictionary commonParentActivities = Helpers.PairUpCommonParentActivities(topLevelActivities);
            foreach (DictionaryEntry entry in commonParentActivities)
            {
                CompositeActivityDesigner compositeActivityDesigner = ActivityDesigner.GetDesigner(entry.Key as Activity) as CompositeActivityDesigner;
                if (compositeActivityDesigner != null && !compositeActivityDesigner.CanRemoveActivities(new List<Activity>((Activity[])((ArrayList)entry.Value).ToArray(typeof(Activity))).AsReadOnly()))
                {
                    cmd.Enabled = false;
                    return;
                }
            }

            cmd.Enabled = true;
        }

        private void OnStatusPaste(object sender, EventArgs e)
        {
            MenuCommand cmd = (MenuCommand)sender;
            cmd.Enabled = false;

            //Check if we are in writtable context
            object selectedObject = this.selectionService.PrimarySelection;
            CompositeActivityDesigner compositeDesigner = ActivityDesigner.GetDesigner(selectedObject as Activity) as CompositeActivityDesigner;
            if (compositeDesigner == null)
                compositeDesigner = ActivityDesigner.GetParentDesigner(selectedObject);

            if (compositeDesigner == null || !compositeDesigner.IsEditable)
                return;

            //Check if data object format is valid
            IDesignerHost designerHost = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            IToolboxService ts = (IToolboxService)this.serviceProvider.GetService(typeof(IToolboxService));
            IDataObject dataObj = Clipboard.GetDataObject();
            if (dataObj == null || designerHost == null || (!dataObj.GetDataPresent(CF_DESIGNER) && (ts != null && !ts.IsSupported(dataObj, designerHost))))
                return;

            //Get the drop target and check if it is valid
            HitTestInfo hitInfo = null;
            if (selectedObject is HitTestInfo)
            {
                hitInfo = (HitTestInfo)selectedObject;
            }
            else if (selectedObject is CompositeActivity)
            {
                hitInfo = new HitTestInfo(compositeDesigner, HitTestLocations.Designer);
            }
            else if (selectedObject is Activity)
            {
                Activity selectedActivity = selectedObject as Activity;
                CompositeActivity parentActivity = selectedActivity.Parent;
                CompositeActivityDesigner parentDesigner = ActivityDesigner.GetDesigner(parentActivity) as CompositeActivityDesigner;
                if (parentDesigner != null)
                    hitInfo = new ConnectorHitTestInfo(parentDesigner, HitTestLocations.Designer, parentActivity.Activities.IndexOf(selectedActivity) + 1);
            }

            //Deserialize activities
            ICollection components = null;
            try
            {
                components = CompositeActivityDesigner.DeserializeActivitiesFromDataObject(this.serviceProvider, dataObj);
            }
            catch (CheckoutException ex)
            {
                if (ex != CheckoutException.Canceled)
                    throw ex;
            }

            cmd.Enabled = (components != null && hitInfo != null && compositeDesigner.CanInsertActivities(hitInfo, new List<Activity>(Helpers.GetTopLevelActivities(components)).AsReadOnly()));
        }

        private void OnStatusAnySelection(object sender, EventArgs e)
        {
            // any selection means that except the root component, if any of the activity is
            // selected then enable it
            MenuCommand cmd = (MenuCommand)sender;
            IDesignerHost designerHost = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            cmd.Enabled = (designerHost != null && this.selectionService.GetSelectedComponents().Count > 0 &&
                            !this.selectionService.GetComponentSelected(designerHost.RootComponent));
        }

        private void OnStatusAlways(object sender, EventArgs e)
        {
            MenuCommand cmd = (MenuCommand)sender;
            cmd.Enabled = true;
        }

        private void OnStatusExpandCollapse(object sender, EventArgs e)
        {
            MenuCommand menuCommand = (MenuCommand)sender;

            int expandCollapseItems = 0;
            foreach (object obj in this.selectionService.GetSelectedComponents())
            {
                Activity activity = obj as Activity;
                if (activity != null)
                {
                    CompositeActivityDesigner compositeDesigner = ActivityDesigner.GetDesigner(activity) as CompositeActivityDesigner;
                    if (compositeDesigner != null && compositeDesigner.CanExpandCollapse &&
                        ((menuCommand.CommandID == WorkflowMenuCommands.Expand && !compositeDesigner.Expanded) ||
                        (menuCommand.CommandID == WorkflowMenuCommands.Collapse && compositeDesigner.Expanded)))
                    {
                        expandCollapseItems += 1;
                    }
                }
            }

            menuCommand.Visible = menuCommand.Enabled = (expandCollapseItems == this.selectionService.SelectionCount);
        }

        private void OnStatusEnable(object sender, EventArgs e)
        {
            MenuCommand menuCommand = (MenuCommand)sender;

            bool enabledPropertyValue = true;
            bool enabled = true;
            ArrayList selectedObjects = new ArrayList(this.selectionService.GetSelectedComponents());
            for (int i = 0; i < selectedObjects.Count && enabled; i++)
            {
                Activity activity = selectedObjects[i] as Activity;
                if (activity != null)
                {
                    ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(activity);
                    if (activityDesigner == null || activityDesigner.IsLocked ||
                        (i > 0 && enabledPropertyValue != activity.Enabled) ||
                        (this.workflowView.RootDesigner != null && this.workflowView.RootDesigner.Activity == activity))
                    {
                        enabled = false;
                    }
                    else
                    {
                        enabledPropertyValue = activity.Enabled;
                    }
                }
                else
                {
                    enabled = false;
                }
            }

            menuCommand.Visible = menuCommand.Enabled = (enabled && ((menuCommand.CommandID == WorkflowMenuCommands.Enable && !enabledPropertyValue) || (menuCommand.CommandID == WorkflowMenuCommands.Disable && enabledPropertyValue)));
        }

        #endregion

        #region Execute Handlers
        private void OnKeyDefault(object sender, EventArgs e)
        {
            SendKeyDownCommand(Keys.Enter);
        }

        //sends specified key to the wf view, returns the .Handled flag
        private bool SendKeyDownCommand(Keys key)
        {
            IDesignerHost host = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null)
            {
                IRootDesigner rootDesigner = ActivityDesigner.GetDesigner(host.RootComponent as Activity) as IRootDesigner;
                if (rootDesigner != null)
                {
                    WorkflowView view = rootDesigner.GetView(ViewTechnology.Default) as WorkflowView;
                    if (view != null)
                    {
                        //because the some key presses are not coming into the Microsoft OnKeyDown
                        //we need to do this work around to manually send the keypress into the designer

                        KeyEventArgs eventArgs = new KeyEventArgs(key);
                        view.OnCommandKey(eventArgs);
                        return eventArgs.Handled;
                    }
                }
            }

            return false;
        }

        private void OnKeyMove(object sender, EventArgs e)
        {
            object selectedObject = this.selectionService.PrimarySelection;
            if (selectedObject == null)
                return;

            MenuCommand menuCommand = (MenuCommand)sender;

            Keys key = Keys.Left;

            if (menuCommand.CommandID.ID == MenuCommands.KeyMoveDown.ID)
                key = Keys.Down;
            else if (menuCommand.CommandID.ID == MenuCommands.KeyMoveUp.ID)
                key = Keys.Up;
            else if (menuCommand.CommandID.ID == MenuCommands.KeyMoveLeft.ID)
                key = Keys.Left;
            else if (menuCommand.CommandID.ID == MenuCommands.KeyMoveRight.ID)
                key = Keys.Right;
            else if (menuCommand.CommandID.ID == MenuCommands.KeySelectNext.ID)
                key = Keys.Tab;
            else if (menuCommand.CommandID.ID == MenuCommands.KeySelectPrevious.ID)
            { key = Keys.Tab | Keys.Shift; }

            SendKeyDownCommand(key);
        }

        private void OnExpandCollapse(object sender, EventArgs e)
        {
            // on enter key we want to do DoDefault of the designer
            MenuCommand menuCommand = (MenuCommand)sender;

            foreach (object obj in this.selectionService.GetSelectedComponents())
            {
                Activity activity = obj as Activity;
                if (activity != null)
                {
                    CompositeActivityDesigner designer = ActivityDesigner.GetDesigner(activity) as CompositeActivityDesigner;
                    if (designer != null)
                        designer.Expanded = (menuCommand.CommandID.ID == WorkflowMenuCommands.Expand.ID);
                }
            }

            MenuCommand expandCommand = this.menuCommandService.FindCommand(WorkflowMenuCommands.Expand);
            if (expandCommand != null)
                OnStatusExpandCollapse(expandCommand, EventArgs.Empty);

            MenuCommand collapseCommand = this.menuCommandService.FindCommand(WorkflowMenuCommands.Collapse);
            if (collapseCommand != null)
                OnStatusExpandCollapse(collapseCommand, EventArgs.Empty);
        }

        private void OnEnable(object sender, EventArgs e)
        {
            // on enter key we want to do DoDefault of the designer
            MenuCommand menuCommand = (MenuCommand)sender;

            DesignerTransaction trans = null;
            IComponent selectedComponent = this.selectionService.PrimarySelection as IComponent;
            if (selectedComponent != null && selectedComponent.Site != null)
            {
                IDesignerHost host = selectedComponent.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null)
                    trans = host.CreateTransaction(SR.GetString(SR.ChangingEnabled));
            }

            try
            {
                foreach (object obj in this.selectionService.GetSelectedComponents())
                {
                    Activity activity = obj as Activity;
                    if (activity != null)
                    {
                        ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(activity);
                        if (activityDesigner != null && !activityDesigner.IsLocked)
                        {
                            PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(activity)["Enabled"];
                            if (propertyDescriptor != null)
                                propertyDescriptor.SetValue(activity, !activity.Enabled);
                        }
                    }
                }

                if (trans != null)
                    trans.Commit();
            }
            finally
            {
                if (trans != null)
                    ((IDisposable)trans).Dispose();
            }

            MenuCommand commentCommand = this.menuCommandService.FindCommand(WorkflowMenuCommands.Disable);
            if (commentCommand != null)
                OnStatusEnable(commentCommand, EventArgs.Empty);

            MenuCommand uncommentCommand = this.menuCommandService.FindCommand(WorkflowMenuCommands.Enable);
            if (uncommentCommand != null)
                OnStatusEnable(uncommentCommand, EventArgs.Empty);
        }

        private void OnCreateTheme(object sender, EventArgs e)
        {
            ThemeConfigurationDialog themeConfigDialog = new ThemeConfigurationDialog(this.serviceProvider);
            if (themeConfigDialog.ShowDialog() == DialogResult.OK)
            {
                WorkflowTheme themeToApply = themeConfigDialog.ComposedTheme.Clone() as WorkflowTheme;
                if (themeToApply != null)
                {
                    WorkflowTheme.CurrentTheme = themeToApply;
                    WorkflowTheme.SaveThemeSettingToRegistry();
                }
            }
        }

        private void OnChangeTheme(object sender, EventArgs e)
        {
            IExtendedUIService extUIService = this.serviceProvider.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (extUIService != null)
                extUIService.ShowToolsOptions();
        }

        private void OnKeyCancel(object sender, EventArgs e)
        {
            SendKeyDownCommand(Keys.Escape);
        }

        private void OnKeyPageDnUp(object sender, EventArgs e)
        {
            MenuCommand menuCommand = (MenuCommand)sender;
            SendKeyDownCommand((menuCommand.CommandID == WorkflowMenuCommands.PageUp) ? Keys.PageUp : Keys.PageDown);
        }

        private void OnViewCode(object sender, EventArgs e)
        {
            IDesignerHost designerHost = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            IComponent rootComponent = (designerHost != null) ? designerHost.RootComponent : null;
            if (rootComponent != null)
            {
                IMemberCreationService memberCreationService = rootComponent.Site.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
                if (memberCreationService != null)
                    memberCreationService.ShowCode();
            }
        }

        private void OnMenuPageSetup(object sender, EventArgs e)
        {
            PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;
            if (printers.Count < 1)
            {
                DesignerHelpers.ShowError(this.serviceProvider, DR.GetString(DR.ThereIsNoPrinterInstalledErrorMessage));
                return;
            }

            WorkflowPageSetupDialog pageSetupDialog = new WorkflowPageSetupDialog(this.serviceProvider);
            if (DialogResult.OK == pageSetupDialog.ShowDialog())
                this.workflowView.PerformLayout(false);
        }

        private void OnMenuSaveWorkflowAsImage(object sender, EventArgs e)
        {
            List<SupportedImageFormats> supportedFormats = new List<SupportedImageFormats>();
            supportedFormats.Add(new SupportedImageFormats(DR.GetString(DR.BMPImageFormat), ImageFormat.Bmp));
            supportedFormats.Add(new SupportedImageFormats(DR.GetString(DR.JPEGImageFormat), ImageFormat.Jpeg));
            supportedFormats.Add(new SupportedImageFormats(DR.GetString(DR.PNGImageFormat), ImageFormat.Png));
            supportedFormats.Add(new SupportedImageFormats(DR.GetString(DR.TIFFImageFormat), ImageFormat.Tiff));
            supportedFormats.Add(new SupportedImageFormats(DR.GetString(DR.WMFImageFormat), ImageFormat.Wmf));
            supportedFormats.Add(new SupportedImageFormats(DR.GetString(DR.EXIFImageFormat), ImageFormat.Exif));
            supportedFormats.Add(new SupportedImageFormats(DR.GetString(DR.EMFImageFormat), ImageFormat.Emf));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = DR.GetString(DR.SaveWorkflowImageDialogTitle);
            saveFileDialog.DefaultExt = "bmp";

            string filter = String.Empty;
            foreach (SupportedImageFormats format in supportedFormats)
                filter += (filter.Length > 0) ? "|" + format.Description : format.Description;

            saveFileDialog.Filter = filter;
            saveFileDialog.FilterIndex = 0;
            if (saveFileDialog.ShowDialog() == DialogResult.OK && saveFileDialog.FilterIndex > 0 && saveFileDialog.FilterIndex <= supportedFormats.Count)
                this.workflowView.SaveWorkflowImage(saveFileDialog.FileName, supportedFormats[saveFileDialog.FilterIndex - 1].Format);
        }

        private void OnMenuCopyToClipboard(object sender, EventArgs e)
        {
            this.workflowView.SaveWorkflowImageToClipboard();
        }

        private void OnMenuPrint(object sender, EventArgs e)
        {
            //check if the printers are installed
            PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;
            if (printers.Count < 1)
            {
                DesignerHelpers.ShowError(this.serviceProvider, DR.GetString(DR.ThereIsNoPrinterInstalledErrorMessage));
                return;
            }

            //check printer selection before actually printing
            PrintDocument printDoc = this.workflowView.PrintDocument;
            PrintDialog printDialog = new System.Windows.Forms.PrintDialog();
            printDialog.AllowPrintToFile = false;
            printDialog.Document = printDoc;

            try
            {
                if (DialogResult.OK == printDialog.ShowDialog())
                {
                    //cache main settings
                    PrinterSettings cachedPrinterSettings = printDoc.PrinterSettings;
                    PageSettings cachedPageSettings = printDoc.DefaultPageSettings;

                    //set the user selected settings
                    //The printer dialog itself calls print on print document we do not have to call it.
                    printDoc.PrinterSettings = printDialog.PrinterSettings;
                    printDoc.DefaultPageSettings = printDialog.Document.DefaultPageSettings;

                    //print it...
                    printDoc.Print();

                    //and restore the main settings back
                    printDoc.PrinterSettings = cachedPrinterSettings;
                    printDoc.DefaultPageSettings = cachedPageSettings;
                }
                else
                {
                    //todo: copy updated settings from the dialog to the print doc
                    //in the worst case it's a no-op, in case user clicked apply/cancel it's the only way to
                    //update the settings (see Winoe#3129 and VSWhidbey#403124 for more details)
                }
            }
            catch (Exception exception)
            {
                string errorString = DR.GetString(DR.SelectedPrinterIsInvalidErrorMessage);
                errorString += "\n" + exception.Message;
                DesignerHelpers.ShowError(this.serviceProvider, errorString);
            }
        }

        private void OnMenuDesignerProperties(object sender, EventArgs e)
        {
            if (this.menuCommandService != null)
                this.menuCommandService.GlobalInvoke(MenuCommands.PropertiesWindow);
        }

        private void OnMenuCut(object sender, EventArgs e)
        {
            //check if we are cutting root component
            IDesignerHost designerHost = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost != null && this.selectionService.GetComponentSelected(designerHost.RootComponent))
                return;

            //Check that we are cutting all activities
            //Check if we are in writable context
            ICollection components = this.selectionService.GetSelectedComponents();
            if (!Helpers.AreAllActivities(components) || !DesignerHelpers.AreAssociatedDesignersMovable(components))
                return;

            // copy the selected component to clipboard
            OnMenuCopy(sender, e);

            // Set transaction description string based on number of activities being moved
            string description = String.Empty;

            if (components.Count > 1)
            {
                description = SR.GetString(SR.CutMultipleActivities, components.Count);
            }
            else
            {
                ArrayList componentList = new ArrayList(components);
                if (componentList.Count > 0)
                    description = SR.GetString(SR.CutSingleActivity, (componentList[0] as Activity).Name);
                else
                    description = SR.GetString(SR.CutActivity);
            }

            DesignerTransaction cutTransaction = designerHost.CreateTransaction(description);

            try
            {
                OnMenuDelete(sender, e);
                cutTransaction.Commit();
            }
            catch
            {
                cutTransaction.Cancel();
            }
        }

        private void OnMenuCopy(object sender, EventArgs e)
        {
            //Make sure that we are copying activities
            if (!Helpers.AreAllActivities(this.selectionService.GetSelectedComponents()))
                return;

            // serialize all top level activities to the store
            Activity[] topLevelActivities = Helpers.GetTopLevelActivities(this.selectionService.GetSelectedComponents());
            IDataObject dataObject = CompositeActivityDesigner.SerializeActivitiesToDataObject(this.serviceProvider, topLevelActivities);
            Clipboard.SetDataObject(dataObject);
        }

        private void OnMenuPaste(object sender, EventArgs e)
        {
            object selectedObject = this.selectionService.PrimarySelection;
            CompositeActivityDesigner compositeDesigner = ActivityDesigner.GetDesigner(selectedObject as Activity) as CompositeActivityDesigner;
            if (compositeDesigner == null)
                compositeDesigner = ActivityDesigner.GetParentDesigner(selectedObject);

            if (compositeDesigner == null || !compositeDesigner.IsEditable)
                return;

            // deserialize activities
            IDataObject dataObj = Clipboard.GetDataObject();
            ICollection components = null;

            try
            {
                components = CompositeActivityDesigner.DeserializeActivitiesFromDataObject(this.serviceProvider, dataObj, true);
            }
            catch (Exception ex)
            {
                if (ex != CheckoutException.Canceled)
                    throw new Exception(DR.GetString(DR.ActivityInsertError) + "\n" + ex.Message, ex);
            }

            if (components == null)
                throw new InvalidOperationException(DR.GetString(DR.InvalidOperationBadClipboardFormat));

            // get the drop target 
            HitTestInfo hitInfo = null;
            if (selectedObject is HitTestInfo)
            {
                hitInfo = (HitTestInfo)selectedObject;
            }
            else if (selectedObject is CompositeActivity)
            {
                hitInfo = new HitTestInfo(compositeDesigner, HitTestLocations.Designer);
            }
            else if (selectedObject is Activity)
            {
                Activity selectedActivity = selectedObject as Activity;
                CompositeActivity parentActivity = selectedActivity.Parent;
                CompositeActivityDesigner parentDesigner = ActivityDesigner.GetDesigner(parentActivity) as CompositeActivityDesigner;
                if (parentDesigner != null)
                    hitInfo = new ConnectorHitTestInfo(parentDesigner, HitTestLocations.Designer, parentActivity.Activities.IndexOf(selectedActivity) + 1);
            }

            List<Activity> topLevelActivities = new List<Activity>(Helpers.GetTopLevelActivities(components));

            // check if we can insert or not
            // I know  I should have disabled the paste menu it-self, but doing status check for paste gives a big performance hit. I am working on it.
            if (hitInfo == null || !compositeDesigner.CanInsertActivities(hitInfo, topLevelActivities.AsReadOnly()))
                throw new Exception(SR.GetString(SR.Error_NoPasteSupport));

            // Make sure the project has references to all inserted activities (in the case
            // where an activity is copied from another project
            IExtendedUIService extendedUIService = this.serviceProvider.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (extendedUIService != null)
            {
                foreach (Activity pastedActivity in components)
                    extendedUIService.AddAssemblyReference(pastedActivity.GetType().Assembly.GetName());
            }

            CompositeActivityDesigner.InsertActivities(compositeDesigner, hitInfo, topLevelActivities.AsReadOnly(), SR.GetString(SR.PastingActivities));
            Stream componentStateStream = dataObj.GetData(CF_DESIGNERSTATE) as Stream;
            if (componentStateStream != null)
                Helpers.DeserializeDesignersFromStream(components, componentStateStream);

            // set something on selections service
            this.selectionService.SetSelectedComponents(topLevelActivities.ToArray(), SelectionTypes.Replace);
            this.workflowView.EnsureVisible(this.selectionService.PrimarySelection);
        }

        private void OnMenuSelectAll(object sender, EventArgs e)
        {
            ActivityDesigner rootDesigner = ActivityDesigner.GetSafeRootDesigner(this.serviceProvider) as ActivityDesigner;
            if (rootDesigner != null)
            {
                List<Activity> activities = new List<Activity>();
                if (rootDesigner.Activity is CompositeActivity)
                    activities.AddRange(Helpers.GetNestedActivities(rootDesigner.Activity as CompositeActivity));
                this.selectionService.SetSelectedComponents(activities.ToArray(), SelectionTypes.Replace);
            }
        }

        private void OnMenuDelete(object sender, EventArgs e)
        {
            SendKeyDownCommand(Keys.Delete);
        }
        #endregion
    }

    #region Class SupportedImageFormats
    internal class SupportedImageFormats
    {
        public string Description;
        public ImageFormat Format;

        public SupportedImageFormats(string description, ImageFormat imageFormat)
        {
            Description = description;
            Format = imageFormat;
        }
    }
    #endregion

    #region Class CommandSetItem
    internal sealed class CommandSetItem : MenuCommand
    {
        private EventHandler statusHandler;
        private bool immidiateStatusUpdate = false;

        public CommandSetItem(EventHandler statusHandler, EventHandler invokeHandler, CommandID id)
            : base(invokeHandler, id)
        {
            this.statusHandler = statusHandler;
        }

        public CommandSetItem(EventHandler statusHandler, EventHandler invokeHandler, CommandID id, string text)
            : this(statusHandler, invokeHandler, id)
        {
            Properties["Text"] = text;
        }

        public CommandSetItem(EventHandler statusHandler, EventHandler invokeHandler, CommandID id, bool immidiateStatusUpdate)
            : this(statusHandler, invokeHandler, id)
        {
            this.immidiateStatusUpdate = immidiateStatusUpdate;
        }

        public override int OleStatus
        {
            get
            {
                if (this.immidiateStatusUpdate)
                    UpdateStatus();
                return base.OleStatus;
            }
        }

        public void UpdateStatus()
        {
            if (statusHandler != null)
            {
                try
                {
                    statusHandler(this, EventArgs.Empty);
                }
                catch
                {
                }
            }
        }
    }
    #endregion
}
