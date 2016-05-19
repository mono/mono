//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;

    internal sealed class BookmarkUndoUnit : UndoUnit
    {
        private ModelTreeManager modelTreeManager;

        public BookmarkUndoUnit(EditingContext context, ModelTreeManager modelTreeManager)
            : base(context)
        {
            Fx.Assert(modelTreeManager != null, "modelTreeManager cannot be null");

            this.modelTreeManager = modelTreeManager;
            this.DoList = new List<UndoUnit>();
            this.RedoList = new List<UndoUnit>();
        }

        internal List<UndoUnit> DoList
        {
            get;
            private set;
        }

        internal List<UndoUnit> RedoList
        {
            get;
            private set;
        }

        public override void Redo()
        {
            this.modelTreeManager.StopTracking();
            try
            {
                this.DoList = this.RedoList.Reverse<UndoUnit>().ToList();
                using (EditingScope redoEditingScope = this.modelTreeManager.CreateEditingScope(this.Description, true))
                {
                    this.DoList.ForEach(unit => unit.Redo());
                    redoEditingScope.Complete();
                }
                this.RedoList.Clear();
            }
            finally
            {
                this.modelTreeManager.StartTracking();
            }
        }

        public override void Undo()
        {
            this.modelTreeManager.StopTracking();
            try
            {
                this.RedoList = this.DoList.Reverse<UndoUnit>().ToList();
                using (EditingScope undoEditingScope = this.modelTreeManager.CreateEditingScope(this.Description, true))
                {
                    this.RedoList.ForEach(unit => unit.Undo());
                    undoEditingScope.Complete();
                }
                this.DoList.Clear();
            }
            finally
            {
                this.modelTreeManager.StartTracking();
            }
        }
    }
}
