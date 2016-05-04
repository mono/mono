#region Using Directives
using System;
using System.Resources;
using System.Drawing;
using System.Workflow.ComponentModel.Design;
#endregion

namespace System.Workflow.Activities
{
    #region Class ActivityDesignerResources (DR)
    internal static class DR
    {
        internal static Color TransparentColor = Color.FromArgb(255, 0, 255);
        internal const string ResourceSet = "System.Workflow.Activities.ActivityDesignerResources";
        private static ResourceManager resourceManager = new ResourceManager(ResourceSet, System.Reflection.Assembly.GetExecutingAssembly());

        internal const string DropActivityHere = "DropActivityHere";
        internal const string MoveLeftDesc = "MoveLeftDesc";
        internal const string MoveRightDesc = "MoveRightDesc";
        internal const string DropEventsHere = "DropEventsHere";
        internal const string InvokeWebServiceDisplayName = "InvokeWebServiceDisplayName";
        internal const string ScopeDesc = "ScopeDesc";
        internal const string EventsDesc = "EventsDesc";
        internal const string SequentialWorkflowHelpText = "SequentialWorkflowHelpText";
        internal const string StartSequentialWorkflow = "StartSequentialWorkflow";
        internal const string WorkflowExceptions = "WorkflowExceptions";
        internal const string WorkflowEvents = "WorkflowEvents";
        internal const string WorkflowCompensation = "WorkflowCompensation";
        internal const string WorkflowCancellation = "WorkflowCancellation";
        internal const string EventBasedWorkFlow = "EventBasedWorkFlow";
        internal const string AddNewEvent = "AddNewEvent";
        internal const string DeleteEvent = "DeleteEvent";
        internal const string ViewPreviousEvent = "ViewPreviousEvent";
        internal const string ViewNextEvent = "ViewNextEvent";
        internal const string NavigateToEvent = "NavigateToEvent";
        internal const string View = "View";
        internal const string AddNewEventDesc = "AddNewEventDesc";
        internal const string DeleteEventDesc = "DeleteEventDesc";
        internal const string NavigateToEventDesc = "NavigateToEventDesc";
        internal const string ViewPreviousEventDesc = "ViewPreviousEventDesc";
        internal const string ViewNextEventDesc = "ViewNextEventDesc";
        internal const string WebServiceReceiveDisplayName = "WebServiceReceiveDisplayName";
        internal const string WebServiceResponseDisplayName = "WebServiceResponseDisplayName";
        internal const string WebServiceFaultDisplayName = "WebServiceFaultDisplayName";
        internal const string AddState = "AddState";
        internal const string AddEventDriven = "AddEventDriven";
        internal const string AddStateInitialization = "AddStateInitialization";
        internal const string AddStateFinalization = "AddStateFinalization";
        internal const string AddingChild = "AddingChild";
        internal const string StateHelpText = "StateHelpText";
        internal const string StateMachineWorkflowHelpText = "StateMachineWorkflowHelpText";
        internal const string StateMachineView = "StateMachineView";
        internal const string SetAsInitialState = "SetAsInitialState";
        internal const string SetAsCompletedState = "SetAsCompletedState";
        internal const string SendToBack = "SendToBack";
        internal const string BringToFront = "BringToFront";
        internal const string ImageFileFilter = "ImageFileFilter";

        //Bitmaps
        internal const string Compensation = "Compensation";
        internal const string SequenceArrow = "SequenceArrow";
        internal const string Event = "Event";
        internal const string Exception = "Exception";
        internal const string NewEvent = "NewEvent";
        internal const string Delete = "Delete";
        internal const string NextEvent = "NextEvent";
        internal const string PreviousEvent = "PreviousEvent";
        internal const string NavigateEvent = "NavigateEvent";
        internal const string WorkflowView = "WorkflowView";
        internal const string ExceptionsView = "ExceptionsView";
        internal const string EventsView = "EventsView";
        internal const string CompensationView = "CompensationView";
        internal const string InitialState = "InitialState";
        internal const string CompletedState = "CompletedState";
        internal const string ThemePropertyReadOnly = "ThemePropertyReadOnly";
        internal const string Error_InvalidImageResource = "Error_InvalidImageResource";

        internal static string GetString(string resID)
        {
            return DR.resourceManager.GetString(resID);
        }

        internal static Image GetImage(string resID)
        {
            Image image = DR.resourceManager.GetObject(resID) as Image;

            //Please note that the default version of make transparent uses the color of pixel at left bottom of the image
            //as the transparent color to make the bitmap transparent. Hence we do not use it
            Bitmap bitmap = image as Bitmap;
            if (bitmap != null)
                bitmap.MakeTransparent(DR.TransparentColor);

            return image;
        }
    }
    #endregion
}
