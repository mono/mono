namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel.Design;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowMenuCommands : StandardCommands
    {
        //Debugger commmands in \\cpvsbuild\drops\whidbey\pd6\raw\current\sources\debugger\vsdebug\resource\VSDbgCmdBase.ctc

        public static readonly Guid WorkflowCommandSetId = new Guid("9aeb9524-82c6-40b9-9285-8d85d3dbd4c4");
        public static readonly Guid DebugCommandSetId = new Guid("C9DD4A59-47FB-11d2-83E7-00C04F9902C1");
        public static readonly Guid DebugWorkflowGroupId = new Guid("{e186451b-2313-42bd-84b9-815f1c923aef}");

        //standard vs command set ids
        internal static readonly Guid StandardCommandSet97Id = new Guid("{5efc7975-14bc-11cf-9b2b-00aa00573819}");
        internal static readonly Guid StandardCommandSet2kId = new Guid("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}");

        //toolbar
        public const int WorkflowToolBar = 0x050C;

        // command ids
        //Debug menu ids
        private const int cmdidInsertBreakpoint = 0x00000177;
        private const int cmdidInsertTracepoint = 0x00000041;
        private const int cmdidEnableBreakpoint = 0x00000178;
        private const int cmdidToggleBreakpoint = 0x000000FF;
        private const int cmdidShowNextStatement = 0x00000103;
        private const int cmdidRunToCursor = 0x000000FB;
        private const int cmdidSetNextStatement = 0x00000102;
        private const int cmdidGoToDisassembly = 0x00000107;
        private const int cmdidNewFileTracepoint = 0x00000140;
        private const int cmdidNewDataBreakpoint = 0x00000139;

        private const int cmdidClearBreakpoints = 0x00000100;
        private const int cmdidBreakpointLocation = 0x00000142;
        private const int cmdidBreakpointCondition = 0x00000143;
        private const int cmdidBreakpointHitCount = 0x00000144;
        private const int cmdidBreakpointConstraints = 0x00000145;
        private const int cmdidBreakpointAction = 0x00000146;
        private const int cmdidShowExecutionState = 0x00001002;


        //standard print command ids
        private const int cmdidPrint = 0x001b;  //27
        private const int cmdidPageSetup = 0x00e3;  //227
        private const int cmdidPrintPreview = 0x00e4;  //228

        //Properties command id
        private const int cmdidProperties = 0x1001;

        // zoom 
        private const int cmdidWOEZoom400 = 0x3100;
        private const int cmdidWOEZoom300 = 0x3101;
        private const int cmdidWOEZoom200 = 0x3102;
        private const int cmdidWOEZoom150 = 0x3103;
        private const int cmdidWOEZoom100 = 0x3104;
        private const int cmdidWOEZoom75 = 0x3105;
        private const int cmdidWOEZoom50 = 0x3106;
        private const int cmdidWOEShowAll = 0x3107;

        public const int FirstZoomCommand = cmdidWOEZoom400; //the first and last zoom commands
        public const int LastZoomCommand = cmdidWOEShowAll; //should be in [....] with the \private\Core\Tools\OrchestrationDesignerUI\PkgCmdID.h

        // page layout
        private const int cmdidDefaultPage = 0x3110;
        private const int cmdidAutoWidthPage = 0x3111;
        private const int cmdidAutoHeightPage = 0x3112;

        // Common commands
        private const int cmdidExpand = 0x3113;
        private const int cmdidCollapse = 0x3114;

        // pan / zoom in/out
        private const int cmdidZoomIn = 0x3119;
        private const int cmdidZoomOut = 0x311A;
        private const int cmdidPan = 0x311B;
        private const int cmdidDefaultFilter = 0x311C;

        private const int cmdidDisable = 0x3115;
        private const int cmdidEnable = 0x3116;
        private const int cmdidChangeTheme = 0x3117;
        private const int cmdidCreateTheme = 0x3118;

        private const int cmdidZoomLevelCombo = 0x311F;
        private const int cmdidZoomLevelListHandler = 0x3120;

        //non-toggling print preview (for the layout menu in the right bottom corner)
        private const int cmdidPrintPreviewPage = 0x3121;

        private const int cmdidSaveWorkflowAsImage = 0x3124;
        private const int cmdidCopyWorkflowToClipboard = 0x3125;

        private const int cmdidDebugWorkflowSteppingInstance = 0x3201;
        private const int cmdidDebugWorkflowSteppingBranch = 0x3202;

        private const int cmdidPageUp = 0x001B;
        private const int cmdidPageDn = 0x001D;

        // menu ids
        private const int mnuidSelection = 0x0500;
        private const int mnuidZoom = 0x0507;
        private const int mnuidPageLayout = 0x0508;
        private const int mnuidDesignerActions = 0x0509;
        private const int mnuidPan = 0x050B;

        public WorkflowMenuCommands()
        {
        }

        //verbids
        public static readonly int VerbGroupGeneral = StandardCommands.VerbFirst.ID;
        public static readonly int VerbGroupView = StandardCommands.VerbFirst.ID + 25;
        public static readonly int VerbGroupEdit = StandardCommands.VerbFirst.ID + 50;
        public static readonly int VerbGroupOptions = StandardCommands.VerbFirst.ID + 75;
        public static readonly int VerbGroupActions = StandardCommands.VerbFirst.ID + 100;
        public static readonly int VerbGroupMisc = StandardCommands.VerbFirst.ID + 125;
        public static readonly int VerbGroupDesignerActions = StandardCommands.VerbFirst.ID + 150;

        //Menuids
        public static readonly Guid MenuGuid = WorkflowCommandSetId;
        public static readonly CommandID SelectionMenu = new CommandID(WorkflowCommandSetId, mnuidSelection);
        public static readonly CommandID DesignerActionsMenu = new CommandID(WorkflowCommandSetId, mnuidDesignerActions);

        //debug
        public static readonly CommandID InsertBreakpointMenu = new CommandID(StandardCommandSet97Id, cmdidInsertBreakpoint);
        public static readonly CommandID EnableBreakpointMenu = new CommandID(StandardCommandSet97Id, cmdidEnableBreakpoint);
        public static readonly CommandID ToggleBreakpointMenu = new CommandID(StandardCommandSet97Id, cmdidToggleBreakpoint);
        public static readonly CommandID ClearBreakpointsMenu = new CommandID(StandardCommandSet97Id, cmdidClearBreakpoints);
        public static readonly CommandID ShowNextStatementMenu = new CommandID(StandardCommandSet97Id, cmdidShowNextStatement);
        public static readonly CommandID RunToCursorMenu = new CommandID(StandardCommandSet97Id, cmdidRunToCursor);
        public static readonly CommandID SetNextStatementMenu = new CommandID(StandardCommandSet97Id, cmdidSetNextStatement);
        public static readonly CommandID GotoDisassemblyMenu = new CommandID(DebugCommandSetId, cmdidGoToDisassembly);
        public static readonly CommandID NewFileTracePointMenu = new CommandID(DebugCommandSetId, cmdidNewFileTracepoint);
        public static readonly CommandID NewDataBreakpointMenu = new CommandID(DebugCommandSetId, cmdidNewDataBreakpoint);

        public static readonly CommandID InsertTracePointMenu = new CommandID(DebugCommandSetId, cmdidInsertTracepoint);
        public static readonly CommandID BreakpointLocationMenu = new CommandID(DebugCommandSetId, cmdidBreakpointLocation);
        public static readonly CommandID BreakpointConditionMenu = new CommandID(DebugCommandSetId, cmdidBreakpointCondition);
        public static readonly CommandID BreakpointHitCountMenu = new CommandID(DebugCommandSetId, cmdidBreakpointHitCount);
        public static readonly CommandID BreakpointConstraintsMenu = new CommandID(DebugCommandSetId, cmdidBreakpointConstraints);
        public static readonly CommandID BreakpointActionMenu = new CommandID(DebugCommandSetId, cmdidBreakpointAction);
        public static readonly CommandID ExecutionStateMenu = new CommandID(WorkflowCommandSetId, cmdidShowExecutionState);

        public static readonly CommandID DebugStepInstanceMenu = new CommandID(WorkflowCommandSetId, cmdidDebugWorkflowSteppingInstance);
        public static readonly CommandID DebugStepBranchMenu = new CommandID(WorkflowCommandSetId, cmdidDebugWorkflowSteppingBranch);

        //print 
        public static readonly CommandID Print = new CommandID(StandardCommandSet97Id, cmdidPrint);
        public static readonly CommandID PageSetup = new CommandID(StandardCommandSet97Id, cmdidPageSetup);
        public static readonly CommandID PrintPreview = new CommandID(StandardCommandSet97Id, cmdidPrintPreview);

        public static readonly CommandID PageUp = new CommandID(StandardCommandSet2kId, cmdidPageUp);
        public static readonly CommandID PageDown = new CommandID(StandardCommandSet2kId, cmdidPageDn);

        //Properties
        public static readonly CommandID DesignerProperties = new CommandID(WorkflowCommandSetId, cmdidProperties);

        // zoom menu
        public static readonly CommandID ZoomMenu = new CommandID(WorkflowCommandSetId, mnuidZoom);
        public static readonly CommandID PageLayoutMenu = new CommandID(WorkflowCommandSetId, mnuidPageLayout);
        public static readonly CommandID PanMenu = new CommandID(WorkflowCommandSetId, mnuidPan);

        // zoom commands
        public static readonly CommandID Zoom400Mode = new CommandID(WorkflowCommandSetId, cmdidWOEZoom400);
        public static readonly CommandID Zoom300Mode = new CommandID(WorkflowCommandSetId, cmdidWOEZoom300);
        public static readonly CommandID Zoom200Mode = new CommandID(WorkflowCommandSetId, cmdidWOEZoom200);
        public static readonly CommandID Zoom150Mode = new CommandID(WorkflowCommandSetId, cmdidWOEZoom150);
        public static readonly CommandID Zoom100Mode = new CommandID(WorkflowCommandSetId, cmdidWOEZoom100);
        public static readonly CommandID Zoom75Mode = new CommandID(WorkflowCommandSetId, cmdidWOEZoom75);
        public static readonly CommandID Zoom50Mode = new CommandID(WorkflowCommandSetId, cmdidWOEZoom50);
        public static readonly CommandID ShowAll = new CommandID(WorkflowCommandSetId, cmdidWOEShowAll);

        // page layout
        public static readonly CommandID DefaultPage = new CommandID(WorkflowCommandSetId, cmdidDefaultPage);
        public static readonly CommandID PrintPreviewPage = new CommandID(WorkflowCommandSetId, cmdidPrintPreviewPage);

        //Common commands
        public static readonly CommandID Expand = new CommandID(WorkflowCommandSetId, cmdidExpand);
        public static readonly CommandID Collapse = new CommandID(WorkflowCommandSetId, cmdidCollapse);
        public static readonly CommandID Disable = new CommandID(WorkflowCommandSetId, cmdidDisable);
        public static readonly CommandID Enable = new CommandID(WorkflowCommandSetId, cmdidEnable);

        public static readonly CommandID ChangeTheme = new CommandID(WorkflowCommandSetId, cmdidChangeTheme);
        public static readonly CommandID CreateTheme = new CommandID(WorkflowCommandSetId, cmdidCreateTheme);

        // zoom In/Out and panning
        public static readonly CommandID ZoomIn = new CommandID(WorkflowCommandSetId, cmdidZoomIn);
        public static readonly CommandID ZoomOut = new CommandID(WorkflowCommandSetId, cmdidZoomOut);
        public static readonly CommandID Pan = new CommandID(WorkflowCommandSetId, cmdidPan);
        public static readonly CommandID DefaultFilter = new CommandID(WorkflowCommandSetId, cmdidDefaultFilter);

        //zoom level combo
        public static readonly CommandID ZoomLevelCombo = new CommandID(WorkflowCommandSetId, cmdidZoomLevelCombo);
        public static readonly CommandID ZoomLevelListHandler = new CommandID(WorkflowCommandSetId, cmdidZoomLevelListHandler);

        //Save workflow commands
        public static readonly CommandID SaveAsImage = new CommandID(WorkflowCommandSetId, cmdidSaveWorkflowAsImage);
        public static readonly CommandID CopyToClipboard = new CommandID(WorkflowCommandSetId, cmdidCopyWorkflowToClipboard);
    }
}
