using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace System.Activities.Presentation.View
{
    public interface IExpressionEditorInstance
    {
        //Properties
        Control HostControl { get; } // Returns a Control to be used to display in the ExpressionTextBox
        string Text { get; set; }

        ScrollBarVisibility VerticalScrollBarVisibility { get; set; }
        ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }
        int MinLines { get; set; }
        int MaxLines { get; set; }
        bool HasAggregateFocus { get; }
        bool AcceptsReturn { get; set; }
        bool AcceptsTab { get; set; }

        //Methods
        void Close(); // For closing and getting rid of the editor items - closes the specific expression editor
        void Focus(); // For setting focus on the editor
        void ClearSelection(); // Clear the selection in the editor
        bool Cut();
        bool Copy();
        bool Paste();
        bool Undo();
        bool Redo();
        bool CompleteWord();
        bool GlobalIntellisense();
        bool ParameterInfo();
        bool QuickInfo();
        bool IncreaseFilterLevel();
        bool DecreaseFilterLevel();

        bool CanCut();
        bool CanCopy();
        bool CanPaste();
        bool CanUndo();
        bool CanRedo();
        bool CanCompleteWord();
        bool CanGlobalIntellisense();
        bool CanParameterInfo();
        bool CanQuickInfo();
        bool CanIncreaseFilterLevel();
        bool CanDecreaseFilterLevel();

        string GetCommittedText();

        // Events
        event EventHandler TextChanged; // An event which is raised when the text in an expression editor is changed
        event EventHandler LostAggregateFocus; // An event which is raised when the expression editor lost aggregate focus
        event EventHandler GotAggregateFocus; // An event which is raised when the expression editor got aggregate focus
        event EventHandler Closing; // An event which is raised when the expression editor is closing
    }
}
