//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Hosting;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using System.Globalization;
    using System.Runtime;

    //DefaultCommandExtensionCallback - provides default key input gestures for most of the 
    //WF commands. user can either implmeent his own class or override this one and provide special 
    //handling for specific commands
    class DefaultCommandExtensionCallback : IWorkflowCommandExtensionCallback
    {
        Dictionary<ICommand, List<KeyGesture>> defaultGestures = new Dictionary<ICommand, List<KeyGesture>>();

        public DefaultCommandExtensionCallback()
        {
            defaultGestures.Add(DesignerView.GoToParentCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.P) });
            defaultGestures.Add(DesignerView.ExpandInPlaceCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.E) });
            defaultGestures.Add(DesignerView.ExpandAllCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.X) });
            defaultGestures.Add(DesignerView.CollapseCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.C) });
            defaultGestures.Add(DesignerView.CollapseAllCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.Y) });
            defaultGestures.Add(DesignerView.RestoreCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.R) });
            defaultGestures.Add(DesignerView.ZoomInCommand,
                new List<KeyGesture> { 
                    new KeyGesture(Key.OemPlus, ModifierKeys.Control, "Ctrl +"),
                    new KeyGesture(Key.Add, ModifierKeys.Control) });
            defaultGestures.Add(DesignerView.ZoomOutCommand,
                new List<KeyGesture> { 
                    new KeyGesture(Key.OemMinus, ModifierKeys.Control, "Ctrl -"),
                    new KeyGesture(Key.Subtract, ModifierKeys.Control) });
            defaultGestures.Add(DesignerView.ToggleArgumentDesignerCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.A) });
            defaultGestures.Add(DesignerView.ToggleVariableDesignerCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.V) });
            defaultGestures.Add(DesignerView.ToggleImportsDesignerCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.I) });
            defaultGestures.Add(DesignerView.ToggleMiniMapCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.O) });
            defaultGestures.Add(DesignerView.CreateVariableCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.N) });
            defaultGestures.Add(DesignerView.CycleThroughDesignerCommand,
                new List<KeyGesture> { new KeyGesture(Key.F6, ModifierKeys.Control | ModifierKeys.Alt, "Ctrl + Alt + F6") });
            defaultGestures.Add(ExpressionTextBox.CompleteWordCommand,
                new List<KeyGesture> { new KeyGesture(Key.Right, ModifierKeys.Alt, "Alt + Right Arrow") });
            defaultGestures.Add(ExpressionTextBox.GlobalIntellisenseCommand,
                new List<KeyGesture> { new KeyGesture(Key.J, ModifierKeys.Control, "Ctrl + J") });
            defaultGestures.Add(ExpressionTextBox.ParameterInfoCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.K, Key.P) });
            defaultGestures.Add(ExpressionTextBox.QuickInfoCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.K, Key.I) });
            defaultGestures.Add(DesignerView.MoveFocusCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.M) });
            defaultGestures.Add(DesignerView.ToggleSelectionCommand,
                new List<KeyGesture> { new ChordKeyGesture(Key.E, Key.S) });
            defaultGestures.Add(DesignerView.CutCommand,
               new List<KeyGesture> { new KeyGesture(Key.X, ModifierKeys.Control) });
            defaultGestures.Add(DesignerView.CopyCommand,
               new List<KeyGesture> { new KeyGesture(Key.C, ModifierKeys.Control) });
            defaultGestures.Add(DesignerView.PasteCommand,
               new List<KeyGesture> { new KeyGesture(Key.V, ModifierKeys.Control) });
            defaultGestures.Add(DesignerView.SelectAllCommand,
               new List<KeyGesture> { new KeyGesture(Key.A, ModifierKeys.Control) });
            defaultGestures.Add(DesignerView.UndoCommand,
               new List<KeyGesture> { new KeyGesture(Key.Z, ModifierKeys.Control) });
            defaultGestures.Add(DesignerView.RedoCommand,
               new List<KeyGesture> { new KeyGesture(Key.Y, ModifierKeys.Control) });
            defaultGestures.Add(ExpressionTextBox.IncreaseFilterLevelCommand,
                new List<KeyGesture> { new KeyGesture(Key.Decimal, ModifierKeys.Alt) });
            defaultGestures.Add(ExpressionTextBox.DecreaseFilterLevelCommand,
                new List<KeyGesture> { new KeyGesture(Key.OemComma, ModifierKeys.Alt) });


        }

        public void OnWorkflowCommandLoaded(CommandInfo commandInfo)
        {
            if (commandInfo == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("commandInfo"));
            }
            RoutedCommand cmd = commandInfo.Command as RoutedCommand;
            if (cmd != null)
            {
                List<KeyGesture> gestures = null;
                if (defaultGestures.TryGetValue(cmd, out gestures))
                {
                    gestures.ForEach((gesture) =>
                    {
                        if (!this.ContainsGesture(cmd, gesture))
                        {
                            cmd.InputGestures.Add(gesture);
                        }
                    });
                }
            }
        }

        protected bool ContainsGesture(RoutedCommand cmd, KeyGesture gesture)
        {
            return cmd.InputGestures.OfType<KeyGesture>().Any(p => string.Equals(p.DisplayString, gesture.DisplayString));
        }


        //ChordKeyGesture - class derived from KeyGesture - provides simple state machine implementation
        //to handle chord keyboard navigation. Invoke when ChordKey or ChordKey + Ctrl is pressed after
        //entering chord mode
        internal sealed class ChordKeyGesture : KeyGesture
        {
            bool isInKeyChordMode = false;
            Key chordKey;

            private WeakReference ownerReference;

            internal DesignerView Owner
            {
                get
                {
                    if (this.ownerReference != null)
                    {
                        return this.ownerReference.Target as DesignerView;
                    }

                    return null;
                }
                set
                {
                    if (value == null)
                    {
                        this.ownerReference = null;
                    }
                    else
                    {
                        this.ownerReference = new WeakReference(value);
                    }
                }
            }

            public ChordKeyGesture(Key key, Key chordKey) :
                base(key, ModifierKeys.Control, string.Format(CultureInfo.InvariantCulture, "Ctrl+{0}, {1}", key, chordKey))
            {
                this.chordKey = chordKey;
            }
            public void ResetChordMode()
            {
                this.isInKeyChordMode = false;
            }
            public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
            {
                bool result = false;
                KeyEventArgs keyArgs = inputEventArgs as KeyEventArgs;
                //lookup only for keyboard events
                if (null != keyArgs)
                {
                    //by default - check if we are entering double key navigation
                    if (!this.isInKeyChordMode)
                    {
                        //call base implementation to match Ctrl + actual key
                        if (base.Matches(targetElement, keyArgs))
                        {
                            this.isInKeyChordMode = true;
                        }
                    }
                    //if we are waiting for chord key
                    else if (keyArgs.Key == this.chordKey && (Keyboard.Modifiers == ModifierKeys.None || Keyboard.Modifiers == ModifierKeys.Control))
                    {
                        //ok - we found a match, reset state to default
                        System.Diagnostics.Debug.WriteLine(this.DisplayString);
                        result = true;
                        // reset all the chord key after this command
                        if (this.Owner != null)
                        {
                            this.Owner.ResetAllChordKeyGesturesMode();
                        }
                    }
                    //no, second key didn't match the chord
                    //if ctrl is pressed, just let it stay in chord mode
                    else if (Keyboard.Modifiers != ModifierKeys.Control)
                    {
                        this.isInKeyChordMode = false;
                    }
                }
                //any other input event resets state to default
                else
                {
                    this.isInKeyChordMode = false;
                }
                return result;
            }
        }

    }
}
