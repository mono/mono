//------------------------------------------------------------------------------
// <copyright file="ModelEditingScope.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Activities.Presentation;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    /// <summary>
    /// A ModelEditingScope represents a group of changes to the 
    /// editing store.  Change groups are transactional:  
    /// changes made under an editing scope can be committed 
    /// or aborted as a unit.  
    /// 
    /// When an editing scope is committed, the editing store 
    /// takes all changes that occurred within it and applies 
    /// them to the model.  If the editing scope’s Revert method 
    /// is called, or the editing scope is disposed before Complete 
    /// is called, the editing scope will instead reverse the 
    /// changes made to the underlying objects, reapplying state 
    /// from the editing store.  This provides a solid basis for 
    /// an undo mechanism.
    /// </summary>
    public abstract class ModelEditingScope : IDisposable
    {
        private string _description;
        private bool _completed;
        private bool _reverted;

        /// <summary>
        /// Creates a new ModelEditingScope object.
        /// </summary>
        protected ModelEditingScope() { }

        /// <summary>
        /// Describes the group.  You may change this property 
        /// anytime before the group is committed.
        /// </summary>
        public string Description
        {
            get { return (_description == null ? string.Empty : _description); }
            set { _description = value; }
        }

        /// <summary>
        /// Completes the editing scope.  If the editing scope has 
        /// already been reverted or completed, this will throw an 
        /// InvalidOperationException.  Calling Complete calls the 
        /// protected OnComplete method.
        /// </summary>
        /// <exception cref="InvalidOperationException">If ModelEditingScope 
        /// has already been complted or reverted.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "By design.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        public void Complete()
        {
            if (_reverted) throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.Presentation.Internal.Properties.Resources.Error_EditingScopeReverted));
            if (_completed) throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.Presentation.Internal.Properties.Resources.Error_EdtingScopeCompleted));

            if (CanComplete())
            {
                bool successful = false;
                _completed = true; // prevent recursive commits

                try
                {
                    OnComplete();
                    successful = true;
                }
                catch (Exception e)
                {
                    _completed = false;
                    Revert();
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (!OnException(e))
                    {
                        throw;
                    }
                }
                finally
                {
                    if (successful)
                    {
                        GC.SuppressFinalize(this);
                    }
                    else
                    {
                        _completed = false;
                    }
                }
            }
            else
                Revert(); // We are not allowed to do completion, revert the change.
        }

        /// <summary>
        /// Abandons the changes made during the editing scope.  If the 
        /// group has already been committed or aborted, this will 
        /// throw an InvalidOperationException.  Calling Revert calls 
        /// the protected OnRevert with “false?for the 
        /// finalizing parameter.
        /// </summary>
        /// <exception cref="InvalidOperationException">If ModelEditingScope 
        /// has already been committed.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "By design.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By design.")]
        public void Revert()
        {
            if (_completed) throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.Presentation.Internal.Properties.Resources.Error_EdtingScopeCompleted));
            if (_reverted) return;

            bool successful = false;
            _reverted = true;

            try
            {
                OnRevert(false);
                successful = true;
            }
            catch (Exception e)
            {
                _reverted = false;
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (!OnException(e))
                {
                    throw;
                }
            }
            finally
            {
                if (successful)
                {
                    GC.SuppressFinalize(this);
                }
                else
                {
                    _reverted = false;
                }
            }
        }

        /// <summary>
        /// Implements IDisposable.Dispose as follows:
        ///
        /// 1.  If the editing scope has already been completed or reverted, do nothing.
        /// 2.  Revert the editing scope.
        /// </summary>
        /// GC.SuppressFinalize(this) is called through Revert(), so suppress the FxCop violation.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1816:DisposeMethodsShouldCallSuppressFinalize")]
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes this object by aborting changes.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_completed && !_reverted)
            {
                if (disposing)
                {
                    Revert();
                }
                else
                {
                    OnRevert(true);
                }
            }
        }

        /// <summary>
        /// Performs the actual complete of the editing scope.
        /// </summary>
        protected abstract void OnComplete();

        /// <summary>
        /// Determines if OnComplete should be called, or if the change should instead be reverted.  Reasons
        /// for reverting may include file cannot be checked out of a source control system for modification.
        /// </summary>
        /// <returns>Returns true if completion can occur, false if the change should instead revert</returns>
        protected abstract bool CanComplete();

        /// <summary>
        /// Performs the actual revert of the editing scope.
        /// </summary>
        /// <param name="finalizing">
        /// True if the abort is ocurring because the object 
        /// is being finalized.  Some undo systems may attempt to 
        /// abort in this case, while others may abandon the 
        /// change and record it as a reactive undo.
        /// </param>
        protected abstract void OnRevert(bool finalizing);

        /// <summary>
        /// Handle the exception during Complete and Revert.
        /// </summary>
        /// <param name="exception">
        /// The exception to handle.
        /// </param>
        /// <returns>
        /// True if the exception is handled, false if otherwise
        /// </returns>
        protected abstract bool OnException(Exception exception);
    }
}

