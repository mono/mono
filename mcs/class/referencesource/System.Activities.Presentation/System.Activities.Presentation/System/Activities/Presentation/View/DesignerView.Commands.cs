//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Debugger;
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.Debug;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Sqm;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Microsoft.Win32;
    using System.Windows.Documents;

    public partial class DesignerView
    {
        public const string CustomMenuItemsSeparatorCommand = "6F455692-EA19-4ac9-ABEE-57F6DF20A687";

        public static readonly DependencyProperty CommandMenuModeProperty =
            DependencyProperty.RegisterAttached("CommandMenuMode", typeof(CommandMenuMode), typeof(DesignerView), new UIPropertyMetadata(CommandMenuMode.FullCommandMenu));

        static readonly DependencyProperty MenuItemOriginProperty =
            DependencyProperty.RegisterAttached("MenuItemOrigin", typeof(FrameworkElement), typeof(DesignerView));

        public static readonly DependencyProperty MenuItemStyleProperty =
            DependencyProperty.Register("MenuItemStyle", typeof(Style), typeof(DesignerView), new UIPropertyMetadata(null));

        public static readonly DependencyProperty MenuSeparatorStyleProperty =
            DependencyProperty.Register("MenuSeparatorStyle", typeof(Style), typeof(DesignerView), new UIPropertyMetadata(null));

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes,
            Justification = "The class RoutedCommand just has readonly properties, hence the referencetype instance cannot be modified")]
        public static readonly ICommand GoToParentCommand = new RoutedCommand("GoToParentCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ExpandCommand = new RoutedCommand("ExpandCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ExpandAllCommand = new RoutedCommand("ExpandAllCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CollapseCommand = new RoutedCommand("CollapseCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CollapseAllCommand = new RoutedCommand("CollapseAllCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand RestoreCommand = new RoutedCommand("RestoreCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ExpandInPlaceCommand = new RoutedCommand("ExpandInPlaceCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand InsertBreakpointCommand = new RoutedCommand("InsertBreakpointCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand DeleteBreakpointCommand = new RoutedCommand("DeleteBreakpointParentCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand EnableBreakpointCommand = new RoutedCommand("EnableBreakpointCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand DisableBreakpointCommand = new RoutedCommand("DisableBreakpointCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand SaveAsImageCommand = new RoutedCommand("SaveAsImageCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CopyAsImageCommand = new RoutedCommand("CopyAsImageCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ZoomInCommand = new RoutedCommand("ZoomInCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ZoomOutCommand = new RoutedCommand("ZoomOutCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ToggleArgumentDesignerCommand = new RoutedCommand("ToggleArgumentDesignerCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ToggleImportsDesignerCommand = new RoutedCommand("ToggleImportsDesignerCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ToggleVariableDesignerCommand = new RoutedCommand("ToggleVariableDesignerCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CreateVariableCommand = new RoutedCommand("CreateVariableCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ToggleMiniMapCommand = new RoutedCommand("ToggleMinimapCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CycleThroughDesignerCommand = new RoutedCommand("CycleThroughDesignerCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CreateWorkflowElementCommand = new RoutedCommand("CreateWorkflowElementCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CreateArgumentCommand = new RoutedCommand("CreateArgumentCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CommitCommand = new RoutedCommand("CommitCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand FitToScreenCommand = new RoutedCommand("FitToScreenCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ResetZoomCommand = new RoutedCommand("ResetZoomCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand MoveFocusCommand = new RoutedCommand("MoveFocusCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ToggleSelectionCommand = new RoutedCommand("ToggleSelectionCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CutCommand = new RoutedCommand("CutCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CopyCommand = new RoutedCommand("CopyCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand PasteCommand = new RoutedCommand("PasteCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand SelectAllCommand = new RoutedCommand("SelectAllCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand UndoCommand = new RoutedCommand("UndoCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand RedoCommand = new RoutedCommand("RedoCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand AddAnnotationCommand = new RoutedCommand("AddAnnotationCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand EditAnnotationCommand = new RoutedCommand("EditAnnotationCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand DeleteAnnotationCommand = new RoutedCommand("DeleteAnnotationCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ShowAllAnnotationCommand = new RoutedCommand("ShowAllAnnotationCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand HideAllAnnotationCommand = new RoutedCommand("HideAllAnnotationCommand", typeof(DesignerView));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand DeleteAllAnnotationCommand = new RoutedCommand("DeleteAllAnnotationCommand", typeof(DesignerView));

        WorkflowViewElement contextMenuTarget;
        HashSet<CommandBinding> ignoreCommands = new HashSet<CommandBinding>();

        Func<WorkflowViewElement, bool, Visibility> navigateToParentFunction;
        Func<WorkflowViewElement, bool, Visibility> navigateToChildFunction;
        Func<WorkflowViewElement, BreakpointTypes> getBreakpointType;
        Func<bool> isCommandServiceEnabled;
        Func<bool> areBreakpointServicesEnabled;
        Func<int, bool> isCommandSupported;

        bool ContainsChordKeyGestures(InputGestureCollection collection)
        {
            if (collection == null)
            {
                return false;
            }
            else
            {
                foreach (KeyGesture gesture in collection)
                {
                    if (gesture.GetType() == typeof(DefaultCommandExtensionCallback.ChordKeyGesture))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        // Set chordkey gestures owner to this class, so that the chordkey can clear other chord key's mode
        // after executing.
        void SetChordKeyGesturesOwner()
        {
            foreach (CommandBinding binding in this.CommandBindings)
            {
                RoutedCommand cmd = binding.Command as RoutedCommand;
                foreach (KeyGesture gesture in cmd.InputGestures)
                {
                    if (gesture.GetType() == typeof(DefaultCommandExtensionCallback.ChordKeyGesture))
                    {
                        ((DefaultCommandExtensionCallback.ChordKeyGesture)gesture).Owner = this;
                    }
                }
            }
        }

        // Let the chord command in front, so that the chord command (e.g. Ctrl+E,Ctrl+V) will match
        // before the (e.g. Ctrl+V).
        void ReorderCommandBindings()
        {
            CommandBindingCollection chordCommandBindings = new CommandBindingCollection();
            CommandBindingCollection basicCommandBindings = new CommandBindingCollection();
            foreach (CommandBinding binding in this.CommandBindings)
            {
                RoutedCommand cmd = binding.Command as RoutedCommand;
                if (ContainsChordKeyGestures(cmd.InputGestures))
                {
                    chordCommandBindings.Add(binding);
                }
                else
                {
                    basicCommandBindings.Add(binding);
                }
            }
            this.CommandBindings.Clear();
            this.CommandBindings.AddRange(chordCommandBindings);
            this.CommandBindings.AddRange(basicCommandBindings);
        }

        internal void ResetAllChordKeyGesturesMode()
        {
            foreach (CommandBinding binding in this.CommandBindings)
            {
                RoutedCommand cmd = binding.Command as RoutedCommand;
                foreach (KeyGesture gesture in cmd.InputGestures)
                {
                    if (gesture.GetType() == typeof(DefaultCommandExtensionCallback.ChordKeyGesture))
                    {
                        ((DefaultCommandExtensionCallback.ChordKeyGesture)gesture).ResetChordMode();
                    }
                }
            }
        }

        void InitializeMenuActions()
        {
            this.isCommandServiceEnabled = () =>
                {
                    return null != this.Context.Services.GetService<ICommandService>();
                };

            this.areBreakpointServicesEnabled = () =>
                {
                    return null != this.Context.Services.GetService<IDesignerDebugView>() && this.isCommandServiceEnabled();
                };

            this.isCommandSupported = commandId =>
                {
                    return this.Context.Services.GetService<ICommandService>().IsCommandSupported(commandId);
                };


            this.navigateToParentFunction = (selection, shouldExecute) =>
                {
                    bool result = false;
                    ModelItem target = null;
                    if (null != selection && selection.Equals(this.RootDesigner))
                    {
                        if (null != selection.ModelItem.Parent)
                        {
                            WorkflowViewService viewService = (WorkflowViewService)this.Context.Services.GetService<ViewService>();
                            target = selection.ModelItem;
                            do
                            {
                                target = target.Parent;
                                if (target == null)
                                {
                                    break;
                                }
                            }
                            while (!viewService.ShouldAppearOnBreadCrumb(target, true));
                            result = (null != target);
                        }
                    }
                    if (shouldExecute && result)
                    {
                        this.MakeRootDesigner(target);
                    }
                    return result ? Visibility.Visible : Visibility.Collapsed;
                };

            this.navigateToChildFunction = (selection, shouldExecute) =>
                {
                    bool result = false;
                    ModelItem target = null;
                    if (null != selection && !selection.Equals(this.RootDesigner))
                    {
                        target = selection.ModelItem;
                        WorkflowViewService viewService = (WorkflowViewService)this.Context.Services.GetService<ViewService>();
                        result = viewService.ShouldAppearOnBreadCrumb(target, true);
                    }
                    if (shouldExecute && result)
                    {
                        this.MakeRootDesigner(target);
                    }
                    return result ? Visibility.Visible : Visibility.Collapsed;
                };

            this.getBreakpointType = selection =>
                {
                    IDesignerDebugView debugView = this.Context.Services.GetService<IDesignerDebugView>();
                    var breakpoints = debugView.GetBreakpointLocations();
                    BreakpointTypes result = BreakpointTypes.None;
                    if (null != breakpoints && null != debugView.SelectedLocation)
                    {
                        breakpoints.TryGetValue(debugView.SelectedLocation, out result);
                    }
                    return result;
                };
            this.ContextMenu.ClipToBounds = false;

            //workflow command extension callback invoker
            Action<WorkflowCommandExtensionItem> updateCommands = (item) =>
                {
                    //if there are any commands which were ignored - add them back to bindings collections
                    foreach (CommandBinding binding in this.ignoreCommands)
                    {
                        this.CommandBindings.Add(binding);
                    }
                    this.ignoreCommands.Clear();

                    if (null != item.CommandExtensionCallback)
                    {
                        foreach (CommandBinding cb in this.CommandBindings)
                        {
                            //if callback returns false, it means that user will handle the command, add it to ingore list
                            CommandInfo ci = new CommandInfo(cb.Command);
                            item.CommandExtensionCallback.OnWorkflowCommandLoaded(ci);
                            if (!ci.IsBindingEnabledInDesigner)
                            {
                                this.ignoreCommands.Add(cb);
                            }

                        }
                        //remove all commands from ignore list from bindings - let the commands bubble up to the client
                        foreach (CommandBinding cb in this.ignoreCommands)
                        {
                            this.CommandBindings.Remove(cb);
                        }

                        if (null != this.ContextMenu && this.ContextMenu.HasItems)
                        {
                            foreach (MenuItem menuItem in this.ContextMenu.Items.OfType<MenuItem>())
                            {
                                this.RefreshContextMenu(menuItem);
                            }
                        }
                    }
                    if (item.CommandExtensionCallback.GetType() == typeof(DefaultCommandExtensionCallback))
                    {
                        this.ReorderCommandBindings();
                        this.SetChordKeyGesturesOwner();
                    }
                };

            //subscribe for command extension callback changes;
            this.context.Items.Subscribe<WorkflowCommandExtensionItem>(new SubscribeContextCallback<WorkflowCommandExtensionItem>(updateCommands));
            //if entry already exists - invoke update; if entry doesn't exist - do nothing. perhaps it will be added later by the user
            if (this.context.Items.Contains<WorkflowCommandExtensionItem>())
            {
                updateCommands(this.context.Items.GetValue<WorkflowCommandExtensionItem>());
            }
        }

        public Style MenuItemStyle
        {
            get { return (Style)GetValue(MenuItemStyleProperty); }
            set { SetValue(MenuItemStyleProperty, value); }
        }

        public Style MenuSeparatorStyle
        {
            get { return (Style)GetValue(MenuSeparatorStyleProperty); }
            set { SetValue(MenuSeparatorStyleProperty, value); }
        }

        void RefreshContextMenu(MenuItem menuItem)
        {
            if (null != menuItem.Command)
            {
                //update MenuItem's Command property - if user did update any keyboard shortcuts, then it would be reflected by that change
                ICommand cmd = menuItem.Command;
                menuItem.Command = null;
                menuItem.Command = cmd;
            }
            if (menuItem.HasItems)
            {
                foreach (MenuItem subItem in menuItem.Items.OfType<MenuItem>())
                {
                    this.RefreshContextMenu(subItem);
                }
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            bool shouldDisplayMenu = false;
            DependencyObject target = null;
            DependencyObject source = e.OriginalSource as DependencyObject;
            if (source is Adorner)
            {
                source = ((Adorner)source).AdornedElement;
            }

            DependencyObject designerPresenterSource = this.designerPresenter.FindCommonVisualAncestor(source);
            DependencyObject extensionSurfaceSource = this.wfViewExtensionSurface.FindCommonVisualAncestor(source);

            //check, who is trying to open context menu - i limit context menu to only elements which are visible inside
            //core of the workflow designer - no breadcrumb or toolbox
            if (designerPresenterSource == this.designerPresenter || extensionSurfaceSource == this.wfViewExtensionSurface)
            {
                Selection currentSelection = this.Context.Items.GetValue<Selection>();
                if (null != currentSelection && currentSelection.SelectionCount >= 1)
                {
                    if (currentSelection.SelectionCount == 1) // single selection
                    {
                        //check original source, to see if it prevents displaying context menu
                        if (!CommandMenuMode.Equals(CommandMenuMode.NoCommandMenu, GetCommandMenuMode((DependencyObject)e.OriginalSource)))
                        {
                            if (null != currentSelection.PrimarySelection.View)
                            {
                                target = currentSelection.PrimarySelection.View;
                                shouldDisplayMenu = true;
                            }
                        }
                    }
                    else // multi-selection
                    {
                        // If Multi-selection's context menu was not enabled, don't display menu.
                        shouldDisplayMenu = Context.Services.GetService<DesignerConfigurationService>().MultipleItemsContextMenuEnabled;
                    }
                }
            }
            e.Handled = true;
            base.OnContextMenuOpening(e);

            if (shouldDisplayMenu)
            {
                // if the context menu is triggered by a context menu key, open context menu at the center of the designer;
                // else open the context menu at mouse point
                this.LoadContextMenu((UIElement)target, (e.CursorLeft < 0 && e.CursorTop < 0));
            }
        }

        void LoadContextMenu(UIElement sender, bool openedByKeyboard)
        {
            if (this.ContextMenu == null)
            {
                return;
            }

            if (!Selection.MultipleObjectsSelected(this.Context) && sender != null && CommandMenuMode.Equals(CommandMenuMode.NoCommandMenu, GetCommandMenuMode(sender)))
            {
                return;
            }

            //clear context menu state
            this.UnloadContextMenu(this.ContextMenu);

            this.contextMenuTarget = sender as WorkflowViewElement;

            //because of WPF caching behaviour, i have to create new instance of context menu each time it is shown
            ContextMenu newMenu = new ContextMenu() { ItemContainerStyleSelector = new ContextMenuItemStyleSelector(this) };
            newMenu.Loaded += this.OnWorkflowViewContextMenuLoaded;
            newMenu.Unloaded += this.OnWorkflowViewContextMenuClosed;
            foreach (var entry in this.ContextMenu.Items.OfType<Control>().Reverse())
            {
                this.ContextMenu.Items.Remove(entry);
                entry.Visibility = Visibility.Visible;
                newMenu.Items.Insert(0, entry);
            }
            this.ContextMenu = newMenu;

            if (!Selection.MultipleObjectsSelected(this.Context))
            {
                if (null != this.contextMenuTarget && null != this.contextMenuTarget.ContextMenu)
                {
                    var items = this.contextMenuTarget.ContextMenu.Items.OfType<Control>().Reverse();
                    int insertIndex = this.ContextMenu.Items.Count;

                    foreach (var item in items)
                    {
                        this.contextMenuTarget.ContextMenu.Items.Remove(item);
                        DesignerView.SetMenuItemOrigin(item, this.contextMenuTarget);
                        this.ContextMenu.Items.Insert(insertIndex, item);
                    }
                }

                Fx.Assert(this.contextMenuTarget.IsVisible, string.Format(CultureInfo.InvariantCulture, "ContextMenuTarget {0} is not visible", this.contextMenuTarget.GetType()));
                this.ContextMenu.Placement = openedByKeyboard ? PlacementMode.Relative : PlacementMode.MousePoint;
                this.ContextMenu.PlacementTarget = this.contextMenuTarget;
            }
            else
            {
                this.ContextMenu.Placement = openedByKeyboard ? PlacementMode.Center : PlacementMode.MousePoint;
                this.ContextMenu.PlacementTarget = this;
            }

            this.ContextMenu.IsOpen = true;
        }

        void UnloadContextMenu(ContextMenu contextMenuToUnload)
        {
            if (null != contextMenuToUnload && null != this.contextMenuTarget)
            {
                // this should happen only for single selection
                //select all menu items which do not belong to DesignerView
                var items = contextMenuToUnload.Items.OfType<Control>()
                    .Where(p => DesignerView.GetMenuItemOrigin(p) != null)
                    .Reverse();

                foreach (Control item in items)
                {
                    //remove item from designer menu's location
                    contextMenuToUnload.Items.Remove(item);

                    //and add it back to activity designer
                    DesignerView.GetMenuItemOrigin(item).ContextMenu.Items.Insert(0, item);
                    DesignerView.SetMenuItemOrigin(item, null);
                }
                this.contextMenuTarget = null;
                contextMenuToUnload.Loaded -= this.OnWorkflowViewContextMenuLoaded;
                contextMenuToUnload.Unloaded -= this.OnWorkflowViewContextMenuClosed;
            }
        }

        void OnWorkflowViewContextMenuLoaded(object sender, RoutedEventArgs e)
        {
            this.ContextMenu.MinWidth = 0;
            this.ContextMenu.MinWidth = this.ContextMenu.DesiredSize.Width;

            Action<WorkflowViewElement, FrameworkElement> contextMenuLoaded =
                (designer, menuSource) =>
                {
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} DesignerView.OnWorkflowViewContextMenuLoaded()", DateTime.Now.ToLocalTime()));
                    if (null != menuSource && null != menuSource.ContextMenu)
                    {
                        if (null != designer) // for single selection, designer is the current selected item, we set CommandTarget to current selection
                        {
                            foreach (var item in menuSource.ContextMenu.Items.OfType<MenuItem>())
                            {
                                item.CommandTarget = designer;
                            }
                            designer.NotifyContextMenuLoaded(menuSource.ContextMenu);
                        }
                        else // for multiple selection, designer is null, we set the CommandTarget to DesignerView
                        {
                            foreach (var item in menuSource.ContextMenu.Items.OfType<MenuItem>())
                            {
                                item.CommandTarget = this;
                            }
                        }
                    }
                };
            //if context menu is loaded, deffer notification untill its all menu items have been loaded
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, contextMenuLoaded, this.contextMenuTarget, this);
        }

        void OnWorkflowViewContextMenuClosed(object sender, RoutedEventArgs e)
        {
            this.UnloadContextMenu((ContextMenu)sender);
        }

        void OnMenuItemSeparatorLoaded(object sender, RoutedEventArgs e)
        {
            //define a delegate which will handle separator load events
            Action<Separator> action = (separator) =>
            {
                if (null != separator && null != separator.Tag && null != this.ContextMenu)
                {
                    //check the separator tags - it should contain command names in a tag property
                    string[] commands = separator.Tag.ToString().Split(';');
                    //check if this separator has a special tag value - CustomMenuItemsSeparatorCommand
                    //it means, it has to be displayed only, when there are custom menu items added to the menu
                    if (commands.Length == 1 && string.Equals(CustomMenuItemsSeparatorCommand, commands[0]))
                    {
                        // The CustomMenuItemsSeparator should be visible only if it has visible child.
                        int index = this.ContextMenu.Items.IndexOf(separator);
                        bool visible = false;
                        for (int i = index + 1; i < this.ContextMenu.Items.Count && !visible; i++)
                        {
                            object contextMenuItem = this.ContextMenu.Items[i];
                            if (contextMenuItem is MenuItem)
                            {
                                visible = ((MenuItem)contextMenuItem).Visibility == Visibility.Visible;
                            }
                            else
                            {
                                // For safety sake, someone might put other things into the ContextMenu.Items other than MenuItem.
                                visible = true;
                            }
                        }
                        separator.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else if (commands.Length != 0)
                    {
                        //set this separator visibility, if associated menu items have either a name or command.name property set to a passed 
                        //value, and at least one of the menu item is visible
                        separator.Visibility = this.ContextMenu.Items
                            .OfType<MenuItem>()
                            .Where(item => commands.Any(
                                    cmd => string.Equals(cmd, null != item.Command && item.Command is RoutedCommand ? ((RoutedCommand)item.Command).Name : item.Name)))
                                    .Any(item => item.Visibility == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            };
            //postpone separator show/hide logic until all menu items are loaded and their visibility is calculated
            this.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, action, sender);
        }

        void OnGoToParentMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                if (Selection.MultipleObjectsSelected(this.Context))
                {
                    item.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                    return;
                }
                item.Visibility = this.navigateToParentFunction(this.contextMenuTarget, false);
            }
            e.Handled = true;
        }

        void OnGoToParentCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void OnGoToParentCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.navigateToParentFunction(this.contextMenuTarget ?? this.FocusedViewElement, true);
            e.Handled = true;
            FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.ViewParent);
        }

        void OnCollapseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.FocusedViewElement != null)
            {
                e.CanExecute = this.FocusedViewElement.ShowExpanded;
            }
            e.Handled = true;
        }

        void OnCollapseCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.FocusedViewElement.ExpandState = false;
            if (this.ShouldExpandAll)
            {
                this.FocusedViewElement.PinState = true;
            }
            e.Handled = true;
        }

        void OnCollapseAllCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.ShouldCollapseAll;
            e.Handled = true;
        }

        void OnCollapseAllCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.ShouldCollapseAll = true;
            e.Handled = true;
            FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.CollapseAll);
        }

        void OnRestoreCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ShouldExpandAll || this.ShouldCollapseAll;
            e.Handled = true;
        }

        void OnRestoreCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.ShouldExpandAll = false;
            this.ShouldCollapseAll = false;
            e.Handled = true;
            FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.Restore);
        }

        void OnExpandAllCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.ShouldExpandAll;
            e.Handled = true;
        }

        void OnExpandAllCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.ShouldExpandAll = true;
            e.Handled = true;
            FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.ExpandAll);
        }

        void OnCollapseExpandInPlaceMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                if (Selection.MultipleObjectsSelected(this.Context))
                {
                    item.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                    return;
                }
                item.Visibility = ExpandButtonVisibilityConverter.GetExpandCollapseButtonVisibility(this.contextMenuTarget);
            }
            e.Handled = true;
        }

        void OnExpandInPlaceCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.FocusedViewElement != null)
            {
                e.CanExecute = !this.FocusedViewElement.ShowExpanded;
            }
            e.Handled = true;
        }

        void OnExpandInPlaceCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.FocusedViewElement.ExpandState = true;
            if (this.ShouldCollapseAll)
            {
                this.FocusedViewElement.PinState = true;
            }
            e.Handled = true;
        }

        void OnExpandMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                if (Selection.MultipleObjectsSelected(this.Context))
                {
                    item.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                    return;
                }
                item.Visibility = this.navigateToChildFunction(this.contextMenuTarget, false);
            }
            e.Handled = true;
        }

        void OnExpandCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void OnExpandCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.navigateToChildFunction(this.contextMenuTarget ?? this.FocusedViewElement, true);
            e.Handled = true;
            FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.OpenChild);
        }

        void OnCopyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.IsReadOnly && CutCopyPasteHelper.CanCopy(this.Context);
            e.ContinueRouting = false;
            e.Handled = true;
        }

        void OnCopyCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.Context.Services.GetService<DesignerPerfEventProvider>().WorkflowDesignerCopyStart();
            CutCopyPasteHelper.DoCopy(this.Context);
            e.Handled = true;
            this.Context.Services.GetService<DesignerPerfEventProvider>().WorkflowDesignerCopyEnd();
        }

        void OnPasteCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.IsReadOnly && CutCopyPasteHelper.CanPaste(this.Context);
            e.ContinueRouting = false;
            e.Handled = true;
        }

        void OnPasteCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            context.Services.GetService<DesignerPerfEventProvider>().WorkflowDesignerPasteStart();
            WorkflowViewElement sourceElement = e.OriginalSource as WorkflowViewElement;
            if (sourceElement != null)
            {
                Point contextMenuTopLeft = this.ContextMenu.TranslatePoint(new Point(0, 0), sourceElement);
                CutCopyPasteHelper.DoPaste(this.Context, contextMenuTopLeft, sourceElement);
            }
            else
            {
                CutCopyPasteHelper.DoPaste(this.Context);
            }
            e.Handled = true;
            context.Services.GetService<DesignerPerfEventProvider>().WorkflowDesignerPasteEnd();
        }

        void OnCutCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //handle CutCommand only if Cut reffers to WF designer context menu, otherwise - let target element handle it
            e.CanExecute = !this.IsReadOnly && CutCopyPasteHelper.CanCut(this.Context);
            e.ContinueRouting = false;
            e.Handled = true;
        }

        void OnCutCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            CutCopyPasteHelper.DoCut(this.Context);
            e.Handled = true;
        }

        void OnDeleteCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //query DeleteHelper if delete can occur - i.e. there is no root element selected
            e.CanExecute = !this.IsReadOnly && DeleteHelper.CanDelete(this.Context);
            e.ContinueRouting = false;
            e.Handled = true;
        }

        void OnDeleteCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteHelper.Delete(this.Context);
            e.Handled = true;
        }

        void OnPropertiesMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                item.Visibility = this.isCommandServiceEnabled() && this.isCommandSupported(CommandValues.ShowProperties) ?
                    Visibility.Visible : Visibility.Collapsed;
            }
            e.Handled = true;
        }

        void OnShowPropertiesCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void OnShowPropertiesCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            //execute ShowProperties command
            ExecuteCommand(CommandValues.ShowProperties);
            e.Handled = true;
        }

        void OnBreakpointMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                if (Selection.MultipleObjectsSelected(this.Context))
                {
                    item.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                    return;
                }
                item.Visibility =
                    this.areBreakpointServicesEnabled() && (this.contextMenuTarget is ActivityDesigner) ?
                    Visibility.Visible : Visibility.Collapsed;
            }
            e.Handled = true;
        }

        void OnInsertBreakpointMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                item.Visibility =
                    this.areBreakpointServicesEnabled() &&
                    this.isCommandSupported(CommandValues.InsertBreakpoint) &&
                    this.getBreakpointType(this.FocusedViewElement) == BreakpointTypes.None ?
                    Visibility.Visible : Visibility.Collapsed;
            }
            e.Handled = true;
        }

        void OnInsertBreakpointCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.FocusedViewElement != null && this.FocusedViewElement.ModelItem != null &&
                AllowBreakpointAttribute.IsBreakpointAllowed(this.FocusedViewElement.ModelItem.ItemType);
            e.Handled = true;
        }

        void OnInsertBreakpointCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteCommand(CommandValues.InsertBreakpoint);
            e.Handled = true;
        }

        void OnDeleteBreakpointMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                item.Visibility =
                    this.areBreakpointServicesEnabled() &&
                    this.isCommandSupported(CommandValues.DeleteBreakpoint) &&
                    this.getBreakpointType(this.FocusedViewElement) != BreakpointTypes.None ?
                    Visibility.Visible : Visibility.Collapsed;
            }
            e.Handled = true;
        }

        void OnDeleteBreakpointCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.FocusedViewElement != null && this.FocusedViewElement.ModelItem != null &&
                AllowBreakpointAttribute.IsBreakpointAllowed(this.FocusedViewElement.ModelItem.ItemType);
            e.Handled = true;
        }

        void OnDeleteBreakpointCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteCommand(CommandValues.DeleteBreakpoint);
            e.Handled = true;
        }

        void OnEnableBreakpointMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                Visibility visibility = Visibility.Collapsed;
                if (this.areBreakpointServicesEnabled() && this.isCommandSupported(CommandValues.EnableBreakpoint))
                {
                    BreakpointTypes breakpoint = this.getBreakpointType(this.FocusedViewElement);
                    visibility = ((breakpoint & BreakpointTypes.Bounded) != 0 && (breakpoint & BreakpointTypes.Enabled) == 0) ?
                        Visibility.Visible : Visibility.Collapsed;
                }
                item.Visibility = visibility;
            }
            e.Handled = true;
        }

        void OnEnableBreakpointCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void OnEnableBreakpointCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteCommand(CommandValues.EnableBreakpoint);
            e.Handled = true;
        }

        void OnDisableBreakpointMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                Visibility visibility = Visibility.Collapsed;
                if (this.areBreakpointServicesEnabled() && this.isCommandSupported(CommandValues.DisableBreakpoint))
                {
                    BreakpointTypes breakpoint = this.getBreakpointType(this.FocusedViewElement);
                    visibility = ((breakpoint & BreakpointTypes.Bounded) != 0 && (breakpoint & BreakpointTypes.Enabled) != 0) ?
                        Visibility.Visible : Visibility.Collapsed;
                }
                item.Visibility = visibility;
            }
            e.Handled = true;
        }


        void OnDisableBreakpointCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void OnDisableBreakpointCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteCommand(CommandValues.DisableBreakpoint);
            e.Handled = true;
        }

        void OnUndoCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.Context.Services.GetService<UndoEngine>().Undo();
            e.Handled = true;
        }

        void OnUndoCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void OnRedoCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.Context.Services.GetService<UndoEngine>().Redo();
            e.Handled = true;
        }

        void OnRedoCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void OnCommitCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            TextBox textBox = e.OriginalSource as TextBox;
            if (textBox != null)
            {
                BindingExpression textBinding = textBox.GetBindingExpression(TextBox.TextProperty);
                if (textBinding != null)
                {
                    textBinding.UpdateSource();
                    e.Handled = true;
                }
            }
        }

        void OnCommitCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void OnSelectAllCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.SelectAll();
            e.Handled = true;
        }

        void OnCopyAsImageMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                if (Selection.MultipleObjectsSelected(this.Context))
                {
                    item.Visibility = Visibility.Collapsed;
                }
                else
                {
                    item.Visibility = Visibility.Visible;
                }
            }
            e.Handled = true;
        }

        void OnCopyAsImageCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = null != this.RootDesigner;
            e.Handled = true;
        }

        void OnCopyAsImageCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            VirtualizedContainerService virtualizingContainerService = this.Context.Services.GetService<VirtualizedContainerService>();
            virtualizingContainerService.BeginPopulateAll((Action)(() =>
            {
                BitmapSource screenShot = this.CreateScreenShot();
                try
                {
                    RetriableClipboard.SetImage(screenShot);
                }
                catch (COMException err)
                {
                    ErrorReporting.ShowErrorMessage(err.Message);
                }
                e.Handled = true;
                FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.CopyAsImage);
            }));
        }

        void OnSaveAsImageMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                if (Selection.MultipleObjectsSelected(this.Context))
                {
                    item.Visibility = Visibility.Collapsed;
                }
                else
                {
                    item.Visibility = Visibility.Visible;
                }
            }
            e.Handled = true;
        }

        void OnSaveAsImageCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = null != this.RootDesigner;
            e.Handled = true;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "Catching all exceptions to avoid VS Crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Catching all exceptions to avoid VS Crash")]
        void OnSaveAsImageCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            ModelItem rootItem = ((WorkflowViewElement)this.RootDesigner).ModelItem;
            PropertyDescriptor displayNameProperty = TypeDescriptor.GetProperties(rootItem)["DisplayName"];
            // default to root item's typename
            string name = rootItem.ItemType.Name;
            // if there is a display name property on root use that as the file name.
            if (displayNameProperty != null && displayNameProperty.PropertyType.Equals(typeof(string)))
            {
                name = (string)displayNameProperty.GetValue(rootItem);
            }
            SaveFileDialog dlg = new SaveFileDialog()
            {
                Filter = @"JPG|*.jpg|PNG|*.png|GIF|*.gif|XPS|*.xps",
                FileName = name
            };
            bool? showDialogResult = false;
            try
            {
                showDialogResult = dlg.ShowDialog();
            }
            catch (ArgumentException)
            {
                dlg.FileName = null;
                showDialogResult = dlg.ShowDialog();
            }
            if (true == showDialogResult && !string.IsNullOrEmpty(dlg.FileName))
            {
                VirtualizedContainerService virtualizingContainerService = this.Context.Services.GetService<VirtualizedContainerService>();
                virtualizingContainerService.BeginPopulateAll((Action)(() =>
                {
                    try
                    {
                        switch (dlg.FilterIndex)
                        {
                            case 1:
                                this.CreateImageFile(dlg.FileName, typeof(JpegBitmapEncoder));
                                break;

                            case 2:
                                this.CreateImageFile(dlg.FileName, typeof(PngBitmapEncoder));
                                break;

                            case 3:
                                this.CreateImageFile(dlg.FileName, typeof(GifBitmapEncoder));
                                break;

                            case 4:
                                this.CreateXPSDocument(dlg.FileName);
                                break;

                            default:
                                throw FxTrace.Exception.AsError(new InvalidOperationException("Not supported file type"));
                        }
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.Message, err.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
            e.Handled = true;
            FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.SaveAsImage);
        }

        void OnZoomInCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.zoomToTicksConverter.CanZoomIn();
            e.Handled = true;
        }

        void OnZoomInCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.zoomToTicksConverter.ZoomIn();
            e.Handled = true;
        }

        void OnMoveFocusCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        void OnMoveFocusCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            UIElement focusedElement = Keyboard.FocusedElement as UIElement;
            if (focusedElement != null)
            {
                this.IsMultipleSelectionMode = true;
                focusedElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                this.IsMultipleSelectionMode = false;
            }
            e.Handled = true;
        }

        void OnToggleSelectionCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        void OnToggleSelectionCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            WorkflowViewElement focusedElement = Keyboard.FocusedElement as WorkflowViewElement;
            if (focusedElement != null && focusedElement.ModelItem != null)
            {
                Selection.Toggle(this.Context, focusedElement.ModelItem);
            }
            e.Handled = true;
        }

        void OnZoomOutCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.zoomToTicksConverter.CanZoomOut();
            e.Handled = true;
        }

        void OnZoomOutCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.zoomToTicksConverter.ZoomOut();
            e.Handled = true;
        }

        void OnToggleArgumentDesignerCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute =
                ((this.shellBarItemVisibility & ShellBarItemVisibility.Arguments) == ShellBarItemVisibility.Arguments) &&
                null != this.ActivitySchema &&
                typeof(ActivityBuilder).IsAssignableFrom(this.ActivitySchema.ItemType);
            e.Handled = true;

        }

        void OnToggleArgumentDesignerCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.buttonArguments1.IsChecked = !this.buttonArguments1.IsChecked;
            this.FocusShellBarDesigner(this.arguments1);
            e.Handled = true;
        }

        void OnToggleVariableDesignerCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((this.shellBarItemVisibility & ShellBarItemVisibility.Variables) == ShellBarItemVisibility.Variables);
            e.Handled = true;
        }

        void OnToggleVariableDesignerCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.buttonVariables1.IsChecked = !this.buttonVariables1.IsChecked;
            this.FocusShellBarDesigner(this.variables1);
            e.Handled = true;
        }

        void OnToggleImportsDesignerCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((this.shellBarItemVisibility & ShellBarItemVisibility.Imports) == ShellBarItemVisibility.Imports)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        void OnToggleImportsDesignerCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.buttonImports1.IsChecked = !this.buttonImports1.IsChecked;
            this.FocusShellBarDesigner(this.imports1);
            e.Handled = true;
        }

        void FocusShellBarDesigner(UIElement designer)
        {
            // Focus the Argument/Variable/Imports designer when it is turn on.
            if (designer.IsEnabled)
            {
                if (!designer.IsKeyboardFocusWithin)
                {
                    Keyboard.Focus(designer);
                }
            }
            // Focus an activity designer when the Argument/Variable/Imports designer is turned off and has keyboard focus within.
            else if (designer.IsKeyboardFocusWithin)
            {
                Keyboard.Focus(this.GetDesignerToFocus());
            }
        }

        void OnCreateVariableMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                if (Selection.MultipleObjectsSelected(this.Context))
                {
                    item.Visibility = Visibility.Collapsed;
                    return;
                }
                item.Visibility = variablesStatusBarItem.Visibility;
            }
        }

        void OnCreateVariableCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute =
                ((this.shellBarItemVisibility & ShellBarItemVisibility.Variables) == ShellBarItemVisibility.Variables) &&
                !this.IsReadOnly && null != this.variables1.CurrentVariableScope;
            e.Handled = true;
        }

        void OnCreateVariableCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.buttonVariables1.IsChecked = true;
            this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { this.variables1.CreateNewVariableWrapper(); }));
            e.Handled = true;
        }

        void OnCreateArgumentCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute =
                ((this.shellBarItemVisibility & ShellBarItemVisibility.Arguments) == ShellBarItemVisibility.Arguments) &&
                !this.IsReadOnly && null != this.arguments1.ActivitySchema;
            e.Handled = true;
        }

        void OnCreateArgumentCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.buttonArguments1.IsChecked = true;
            this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { this.arguments1.CreateNewArgumentWrapper(); }));
            e.Handled = true;
        }

        void OnToggleMiniMapCommandExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = e.CanExecute = true;
        }

        void OnToggleMiniMapCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            this.miniMap.IsEnabled = !this.miniMap.IsEnabled;
        }

        void OnCycleThroughDesignerCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Handled = true;
        }

        UIElement GetDesignerToFocus()
        {
            Selection selection = this.Context.Items.GetValue<Selection>();
            if (selection.SelectionCount != 0 && selection.PrimarySelection.View != null)
            {
                return (UIElement)selection.PrimarySelection.View;
            }
            else
            {
                return this.RootDesigner;
            }
        }

        void OnCycleThroughDesignerCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            UIElement toFocus = this.breadCrumbListBox;

            if (this.BreadCrumbBarLayout.IsKeyboardFocusWithin)
            {
                toFocus = this.GetDesignerToFocus();
            }
            else if (this.scrollViewer.IsKeyboardFocusWithin)
            {
                if ((bool)this.buttonVariables1.IsChecked)
                {
                    toFocus = this.variables1;
                }
                else if ((bool)this.buttonArguments1.IsChecked)
                {
                    toFocus = this.arguments1;
                }
                else if ((bool)this.buttonImports1.IsChecked)
                {
                    toFocus = this.imports1;
                }
                else
                {
                    toFocus = this.buttonVariables1;
                }
            }
            else if ((bool)this.buttonVariables1.IsChecked && this.variables1.IsKeyboardFocusWithin)
            {
                toFocus = this.buttonVariables1;
            }
            else if ((bool)this.buttonArguments1.IsChecked && this.arguments1.IsKeyboardFocusWithin)
            {
                toFocus = this.buttonVariables1;
            }
            else if ((bool)this.buttonImports1.IsChecked && this.imports1.IsKeyboardFocusWithin)
            {
                toFocus = this.buttonVariables1;
            }
            this.Dispatcher.BeginInvoke(new Action<IInputElement>((target) =>
                {
                    System.Diagnostics.Debug.WriteLine(target.GetType().Name + " " + target.GetHashCode());
                    Keyboard.Focus(target);
                }), DispatcherPriority.ApplicationIdle, toFocus);
            e.Handled = true;
        }

        void OnCreateWorkflowElementCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            string typeName = null;
            // e.Parameter is IDictionary<string, string> when the designer is hosted
            // inside Visual Studio. It is IDataObject otherwise.
            if (e.Parameter is IDictionary<string, string>)
            {
                IDictionary<string, string> context = e.Parameter as IDictionary<string, string>;
                context.TryGetValue("TypeName", out typeName);
            }
            else
            {
                IDataObject context = e.Parameter as IDataObject;
                if (context != null)
                {
                    typeName = context.GetData(DragDropHelper.WorkflowItemTypeNameFormat) as string;
                }
            }

            bool precondition = !this.IsReadOnly && !string.IsNullOrWhiteSpace(typeName);

            if (precondition)
            {
                Type activityType = Type.GetType(typeName, false);
                if (null != activityType)
                {
                    Selection selection = this.Context.Items.GetValue<Selection>();
                    if (selection.SelectionCount == 1 && selection.PrimarySelection.View is WorkflowViewElement)
                    {
                        WorkflowViewElement viewElement = (WorkflowViewElement)selection.PrimarySelection.View;
                        ICompositeView container = viewElement.ActiveCompositeView;
                        if (null != container)
                        {
                            List<object> itemsToPaste = new List<object>(1);
                            Type factoryType;
                            if (activityType.TryGetActivityTemplateFactory(out factoryType))
                            {
                                itemsToPaste.Add(factoryType);
                            }
                            else
                            {
                                itemsToPaste.Add(activityType);
                            }

                            e.CanExecute = container.CanPasteItems(itemsToPaste);
                        }
                    }
                }
            }
            e.Handled = true;
        }

        void OnCreateWorkflowElementCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Selection selection = this.Context.Items.GetValue<Selection>();
            Fx.Assert(selection.SelectionCount == 1, "selection.SelectionCount should be 1.");
            Fx.Assert(selection.PrimarySelection.View is WorkflowViewElement, "selection.PrimarySelection.View should be WorkflowViewElement type.");

            WorkflowViewElement viewElement = (WorkflowViewElement)selection.PrimarySelection.View;

            Type activityType = null;
            IDataObject dataObject = null;

            // e.Parameter is IDictionary<string, string> when the designer is hosted
            // inside Visual Studio. It is IDataObject otherwise.
            if (e.Parameter is IDictionary<string, string>)
            {
                IDictionary<string, string> context = e.Parameter as IDictionary<string, string>;
                activityType = Type.GetType(context["TypeName"]);

                // For the VisualStudio 11 hosted designer case data object corresponding to the toolbox item is passed in
                // through AppDomain level data by EditorPane.IToolboxUser.ItemPicked method.
                string dataObjectKey = typeof(System.Runtime.InteropServices.ComTypes.IDataObject).FullName;
                object data = AppDomain.CurrentDomain.GetData(dataObjectKey);
                if (data is IntPtr)
                {
                    IntPtr dataObjectPointer = (IntPtr)data;
                    dataObject = new DataObject((System.Runtime.InteropServices.ComTypes.IDataObject)Marshal.GetObjectForIUnknown(dataObjectPointer));
                    Marshal.Release(dataObjectPointer);
                    AppDomain.CurrentDomain.SetData(dataObjectKey, null);
                }
            }
            else
            {
                dataObject = e.Parameter as IDataObject;
                activityType = Type.GetType((string)dataObject.GetData(DragDropHelper.WorkflowItemTypeNameFormat));
            }

            object instance = DragDropHelper.GetDroppedObjectInstance(viewElement, this.Context, activityType, dataObject);
            if (instance != null)
            {
                List<object> itemsToPaste = new List<object>(1);
                List<object> metaData = new List<object>(1);
                if (instance is FlowNode)
                {
                    List<FlowNode> flowchartMetaData = new List<FlowNode>(1);
                    flowchartMetaData.Add(instance as FlowNode);
                    metaData.Add(flowchartMetaData);
                }
                else
                {
                    itemsToPaste.Add(instance);
                }

                viewElement.ActiveCompositeView.OnItemsPasted(itemsToPaste, metaData, new Point(), null);
            }
            e.Handled = true;
        }

        void OnFitToScreenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (null != this.zoomToTicksConverter);
        }

        void OnFitToScreenCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.zoomToTicksConverter.FitToScreen();
            e.Handled = true;
            FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.FitToScreen);
        }

        void OnResetZoomCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (null != this.zoomToTicksConverter);
        }

        void OnResetZoomCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.zoomToTicksConverter.ResetZoom();
            e.Handled = true;
            FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.ResetZoom);
        }

        void OnZoomPickerUndoRedoCommandPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        void OnAnnotationsMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (this.context.Services.GetService<DesignerConfigurationService>().AnnotationEnabled == true)
            {
                if (!Selection.MultipleObjectsSelected(this.Context))
                {
                    item.Visibility = Visibility.Visible;
                    e.Handled = true;
                    return;
                }
            }

            item.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        void OnAddAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ContextMenuUtilities.OnAddAnnotationCommandCanExecute(e, this.Context);
        }

        void OnAddAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ContextMenuUtilities.OnAddAnnotationCommandExecuted(e, this.Context.Items.GetValue<Selection>().PrimarySelection);
        }

        void OnEditAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // call the same method as delete annotation command
            ContextMenuUtilities.OnDeleteAnnotationCommandCanExecute(e, this.Context);
        }

        void OnEditAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ContextMenuUtilities.OnEditAnnotationCommandExecuted(e, this.Context.Items.GetValue<Selection>().PrimarySelection);
        }

        void OnDeleteAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ContextMenuUtilities.OnDeleteAnnotationCommandCanExecute(e, this.Context);
        }

        void OnDeleteAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ContextMenuUtilities.OnDeleteAnnotationCommandExecuted(e, this.Context.Items.GetValue<Selection>().PrimarySelection);
        }

        void OnShowAllAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.context.Services.GetService<DesignerConfigurationService>().AnnotationEnabled != true)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = true;
            e.Handled = true;
        }

        void OnShowAllAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ModelTreeManager modelTreeManager = this.Context.Services.GetService<ModelTreeManager>();
            AnnotationAdornerService annotationService = this.Context.Services.GetService<AnnotationAdornerService>();
            ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();
            foreach (ModelItem item in ModelTreeManager.Find(modelTreeManager.Root, new Predicate<ModelItem>(ModelItemExtensions.HasAnnotation), false))
            {
                viewStateService.StoreViewState(item, Annotation.IsAnnotationDockedViewStateName, true);
            }
            e.Handled = true;
        }

        void OnHideAllAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.context.Services.GetService<DesignerConfigurationService>().AnnotationEnabled != true)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = true;
            e.Handled = true;
        }

        void OnHideAllAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ModelTreeManager modelTreeManager = this.Context.Services.GetService<ModelTreeManager>();
            AnnotationAdornerService annotationService = this.Context.Services.GetService<AnnotationAdornerService>();
            ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();
            foreach (ModelItem item in ModelTreeManager.Find(modelTreeManager.Root, new Predicate<ModelItem>(ModelItemExtensions.HasAnnotation), false))
            {
                viewStateService.StoreViewState(item, Annotation.IsAnnotationDockedViewStateName, false);
            }
            e.Handled = true;
        }

        void OnDeleteAllAnnotationCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ContextMenuUtilities.OnDeleteAllAnnotationCommandCanExecute(e, this.Context);
        }

        void OnDeleteAllAnnotationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ModelTreeManager modelTreeManager = this.Context.Services.GetService<ModelTreeManager>();
            AnnotationAdornerService annotationService = this.Context.Services.GetService<AnnotationAdornerService>();
            ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();

            MessageBoxResult result = MessageBox.Show(SR.DeleteAllAnnotationMessage, SR.DeleteAnnotationTitle, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
            {
                e.Handled = true;
                return;
            }

            ModelEditingScope editingScope = null;
            bool isModified = false;
            try
            {
                foreach (ModelItem item in ModelTreeManager.Find(modelTreeManager.Root, new Predicate<ModelItem>(ModelItemExtensions.HasAnnotation), false))
                {
                    isModified = true;
                    if (editingScope == null)
                    {
                        editingScope = item.BeginEdit(SR.DeleteAllAnnotationsDescription);
                    }
                    item.Properties[Annotation.AnnotationTextPropertyName].ClearValue();
                    viewStateService.StoreViewStateWithUndo(item, Annotation.IsAnnotationDockedViewStateName, null);
                }

                if (isModified)
                {
                    modelTreeManager.AddToCurrentEditingScope(new NotifyArgumentVariableAnnotationTextChanged()
                    {
                        ArgumentDesigner = this.arguments1,
                        VariableDesigner = this.variables1,
                    });
                }
            }
            finally
            {
                if (editingScope != null)
                {
                    editingScope.Complete();
                }
            }

            e.Handled = true;
        }

        void ExecuteCommand(int command)
        {
            IDesignerDebugView debuggerService = this.Context.Services.GetService<IDesignerDebugView>();
            ICommandService commandService = this.Context.Services.GetService<ICommandService>();
            if (null != commandService)
            {
                //setup parameters
                var commandParameters = (Dictionary<string, object>)null;
                if (null != debuggerService &&
                    (command == CommandValues.InsertBreakpoint ||
                    command == CommandValues.DeleteBreakpoint ||
                    command == CommandValues.EnableBreakpoint ||
                    command == CommandValues.DisableBreakpoint))
                {
                    commandParameters = new Dictionary<string, object>();
                    if (command == CommandValues.InsertBreakpoint)
                    {
                        commandParameters.Add(typeof(BreakpointTypes).Name, BreakpointTypes.Bounded);
                    }

                    commandParameters.Add(typeof(SourceLocation).Name, debuggerService.SelectedLocation);
                }

                //execute command
                commandService.ExecuteCommand(command, commandParameters);
            }
        }

        static FrameworkElement GetMenuItemOrigin(DependencyObject obj)
        {
            return (FrameworkElement)obj.GetValue(MenuItemOriginProperty);
        }

        static void SetMenuItemOrigin(DependencyObject obj, FrameworkElement value)
        {
            obj.SetValue(MenuItemOriginProperty, value);
        }

        public static CommandMenuMode GetCommandMenuMode(DependencyObject obj)
        {
            return (CommandMenuMode)obj.GetValue(CommandMenuModeProperty);
        }

        public static void SetCommandMenuMode(DependencyObject obj, CommandMenuMode value)
        {
            obj.SetValue(CommandMenuModeProperty, value);
        }

        sealed class ContextMenuItemStyleSelector : StyleSelector
        {
            DesignerView owner;

            public ContextMenuItemStyleSelector(DesignerView owner)
            {
                this.owner = owner;
            }

            public override Style SelectStyle(object item, DependencyObject container)
            {
                if (item is MenuItem && null != this.owner.MenuItemStyle)
                {
                    ((MenuItem)item).ItemContainerStyleSelector = this;
                    return this.owner.MenuItemStyle;
                }
                else if (item is Separator && null != this.owner.MenuSeparatorStyle)
                {
                    return this.owner.MenuSeparatorStyle;
                }
                return base.SelectStyle(item, container);
            }
        }
    }

    [Fx.Tag.XamlVisible(false)]
    public sealed class CommandMenuMode
    {
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes,
            Justification = "The class being an empty class, the readonly mutable referencetypes cannot be modified")]
        public static readonly CommandMenuMode NoCommandMenu = new CommandMenuMode();
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly CommandMenuMode FullCommandMenu = new CommandMenuMode();

        private CommandMenuMode()
        {
        }
    }
}
