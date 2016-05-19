//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class UndoEngine : IUndoEngineOperations
    {
        const int capacity = 100;
        List<UndoUnit> undoBuffer;
        List<UndoUnit> redoBuffer;
        EditingContext context;
        Bookmark bookmark;
        //interface which contains actual implementation of AddUndoUnit, Undo and Redo operations
        IUndoEngineOperations undoEngineImpl = null;

        public event EventHandler<UndoUnitEventArgs> UndoUnitAdded;
        public event EventHandler<UndoUnitEventArgs> UndoCompleted;
        public event EventHandler<UndoUnitEventArgs> RedoCompleted;
        public event EventHandler<UndoUnitEventArgs> UndoUnitCancelled;
        public event EventHandler UndoUnitDiscarded;
        public event EventHandler UndoRedoBufferChanged;

        public UndoEngine(EditingContext context)
        {
            this.context = context;
            undoBuffer = new List<UndoUnit>(capacity);
            redoBuffer = new List<UndoUnit>(capacity);
            this.undoEngineImpl = this;
        }

        internal bool IsBookmarkInPlace { get { return this.bookmark != null; } }

        // CreateImmediateEditingScope - creates a new ImmediateEditingScope which gatters all edits in its  
        // undo unit list. all changes in ImmediateEditingScope appear as a one change and can be 
        // undoned or redoned as a one set.
        internal ImmediateEditingScope CreateImmediateEditingScope(string bookmarkName, ModelTreeManager modelTreeManager)
        {
            Fx.Assert(modelTreeManager != null, "modelTreeManager should not be null.");

            //only one bookmark is supported
            Fx.Assert(this.bookmark == null, "Nested bookmarks are not supported.");

            //create bookmark undo unit, and give it a description
            BookmarkUndoUnit unit = new BookmarkUndoUnit(this.context, modelTreeManager)
            {
                Description = bookmarkName ?? string.Empty,
            };
            //create bookmark, and pass bookmark undo unit to it.
            this.bookmark = new Bookmark(this, unit);
            //switch implementation of AddUndoUnit, Undo, Redo to be delegated through bookmark
            this.undoEngineImpl = bookmark;
            return new ImmediateEditingScope(modelTreeManager, this.bookmark);
        }

        public IEnumerable<string> GetUndoActions()
        {
            return this.undoBuffer.Select(p => p.Description);
        }

        public IEnumerable<string> GetRedoActions()
        {
            return this.redoBuffer.Select(p => p.Description);
        }

        public void AddUndoUnit(UndoUnit unit)
        {
            if (unit == null)
            {
                throw FxTrace.Exception.ArgumentNull("unit");
            }
            this.undoEngineImpl.AddUndoUnitCore(unit);
        }

        public bool Undo()
        {
            this.IsUndoRedoInProgress = true;
            bool succeeded = this.undoEngineImpl.UndoCore();
            this.IsUndoRedoInProgress = false;
            if (succeeded)
            {
                this.NotifyUndoRedoBufferChanged();
            }
            return succeeded;
        }

        public bool Redo()
        {
            this.IsUndoRedoInProgress = true;
            bool succeeded = this.undoEngineImpl.RedoCore();
            this.IsUndoRedoInProgress = false;
            if (succeeded)
            {
                this.NotifyUndoRedoBufferChanged();
            }
            return succeeded;
        }

        public bool IsUndoRedoInProgress
        {
            get;
            private set;
        }

        void IUndoEngineOperations.AddUndoUnitCore(UndoUnit unit)
        {
            undoBuffer.Add(unit);
            this.NotifyUndoUnitAdded(unit);

            if (undoBuffer.Count > capacity)
            {
                undoBuffer.RemoveAt(0);
                NotifyUndoUnitDiscarded();
            }

            redoBuffer.Clear();
            this.NotifyUndoRedoBufferChanged();
        }

        bool IUndoEngineOperations.UndoCore()
        {
            bool succeeded = false;
            if (undoBuffer.Count > 0)
            {
                UndoUnit unitToUndo = undoBuffer.Last();
                undoBuffer.RemoveAt(undoBuffer.Count - 1);
                unitToUndo.Undo();
                redoBuffer.Add(unitToUndo);
                NotifyUndoExecuted(unitToUndo);
                succeeded = true;
            }
            return succeeded;
        }

        bool IUndoEngineOperations.RedoCore()
        {
            bool succeeded = false;
            if (redoBuffer.Count > 0)
            {
                UndoUnit unitToRedo = redoBuffer.Last();
                redoBuffer.RemoveAt(redoBuffer.Count - 1);
                unitToRedo.Redo();
                undoBuffer.Add(unitToRedo);
                NotifyRedoExecuted(unitToRedo);
                succeeded = true;
            }
            return succeeded;
        }

        private void NotifyUndoUnitAdded(UndoUnit unit)
        {
            if (this.UndoUnitAdded != null)
            {
                this.UndoUnitAdded(this, new UndoUnitEventArgs() { UndoUnit = unit });
            }
        }

        private void NotifyUndoExecuted(UndoUnit unit)
        {
            if (this.UndoCompleted != null)
            {
                this.UndoCompleted(this, new UndoUnitEventArgs() { UndoUnit = unit });
            }
        }

        private void NotifyRedoExecuted(UndoUnit unit)
        {
            if (this.RedoCompleted != null)
            {
                this.RedoCompleted(this, new UndoUnitEventArgs() { UndoUnit = unit });
            }
        }

        private void NotifyUndoUnitCancelled(UndoUnit unit)
        {
            if (this.UndoUnitCancelled != null)
            {
                this.UndoUnitCancelled(this, new UndoUnitEventArgs() { UndoUnit = unit });
            }
        }

        private void NotifyUndoUnitDiscarded()
        {
            if (this.UndoUnitDiscarded != null)
            {
                this.UndoUnitDiscarded(this, null);
            }
        }

        private void NotifyUndoRedoBufferChanged()
        {
            if (null != this.UndoRedoBufferChanged)
            {
                this.UndoRedoBufferChanged(this, EventArgs.Empty);
            }
        }

        //Bookmark implementation - implements core UndoEngine operations + IDisposable - 
        //default bookmark behavior is to Rollback changes, unless committed explicitly. 
        //usage of IDisposable enables usage of pattern:
        // using (Bookmark b = new Bookmark())....
        internal sealed class Bookmark : IDisposable, IUndoEngineOperations
        {
            BookmarkUndoUnit containerUndoUnit;
            UndoEngine undoEngine;
            bool isCommitted = false;
            bool isRolledBack = false;
            bool isDisposed = false;

            internal Bookmark(UndoEngine undoEngine, BookmarkUndoUnit undoUnit)
            {
                this.undoEngine = undoEngine;
                this.containerUndoUnit = undoUnit;
            }
        
            public void CommitBookmark()
            {
                //cannot commit more than once...
                if (this.isDisposed)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, SR.UndoEngine_OperationNotAllowed, "CommitBookmark")));
                }

                this.isCommitted = true;
                //restore original undo engine implementation
                this.undoEngine.undoEngineImpl = this.undoEngine;
                //get rid of the bookmark
                this.undoEngine.bookmark = null;
                //check if bookmark has any changes
                if (this.containerUndoUnit.DoList.Count != 0 || this.containerUndoUnit.RedoList.Count != 0)
                {
                    //add all changes in bookmark into a undo list as a one element
                    this.undoEngine.AddUndoUnit(this.containerUndoUnit);
                }
                //dispose bookmark
                this.Dispose();
            }

            public void RollbackBookmark()
            {
                //cannot rollback more than once
                if (this.isDisposed)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, SR.UndoEngine_OperationNotAllowed, "RollbackBookmark")));
                }
                this.isRolledBack = true;
                //get through the list of all accumulated changes and reverse each of them
                foreach (UndoUnit unit in this.containerUndoUnit.DoList.Reverse<UndoUnit>())
                {
                    unit.Undo();
                }
                //clear the lists
                this.containerUndoUnit.DoList.Clear();
                this.containerUndoUnit.RedoList.Clear();

                //restore original undo engine implementation
                this.undoEngine.undoEngineImpl = this.undoEngine;
                //get rid of the bookmark
                this.undoEngine.bookmark = null;
                this.undoEngine.NotifyUndoUnitCancelled(this.containerUndoUnit);
                //dispose bookmark
                this.Dispose();
            }

            public void Dispose()
            {
                if (!this.isDisposed)
                {
                    GC.SuppressFinalize(this);
                    DisposeInternal();
                }
            }

            void DisposeInternal()
            {
                if (!this.isDisposed)
                {
                    //if not committed or rolled back - rollback by default  
                    if (!this.isCommitted && !this.isRolledBack)
                    {
                        this.RollbackBookmark();
                    }
                    this.isDisposed = true;
                }
            }

            void IUndoEngineOperations.AddUndoUnitCore(UndoUnit unit)
            {
                //add element to Undo list
                this.containerUndoUnit.DoList.Add(unit);
                //clear redo list
                this.containerUndoUnit.RedoList.Clear();
            }

            bool IUndoEngineOperations.UndoCore()
            {
                //if there is anything to undo
                bool succeeded = false;
                if (this.containerUndoUnit.DoList.Count > 0)
                {
                    //get the last element done
                    UndoUnit unitToUndo = this.containerUndoUnit.DoList.Last();
                    //remove it
                    this.containerUndoUnit.DoList.RemoveAt(this.containerUndoUnit.DoList.Count - 1);
                    //undo it
                    unitToUndo.Undo();
                    //and insert to the head of redo list
                    this.containerUndoUnit.RedoList.Insert(0, unitToUndo);
                    succeeded = true;
                }
                return succeeded;
            }

            bool IUndoEngineOperations.RedoCore()
            {
                //if there is anything to redo
                bool succeeded = false;
                if (this.containerUndoUnit.RedoList.Count > 0)
                {
                    //get first element to redo
                    UndoUnit unitToRedo = this.containerUndoUnit.RedoList.First();
                    //remove it
                    this.containerUndoUnit.RedoList.RemoveAt(0);
                    //redo it
                    unitToRedo.Redo();
                    //add it to the end of undo list
                    this.containerUndoUnit.DoList.Add(unitToRedo);
                    succeeded = true;
                }
                return succeeded;
            }
        }
    }
}
