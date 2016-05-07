using System.Windows.Input;

//This class can  be removed if we make FlowchartDesigner public.
//The purpose of this is so that VS can raise the command defined for the flowchart designer.
namespace System.Activities.Core.Presentation
{
    public static class FlowchartDesignerCommands
    {
        public static readonly RoutedCommand ConnectNodesCommand = new RoutedCommand("ConnectNodes", typeof(FlowchartDesignerCommands));
    }
}
