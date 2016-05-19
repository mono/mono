//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection 
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation;

    // <summary>
    // Helper static class that attempts to resolve a given SelectionPath into the corresponding
    // visual in the CategoryList control.
    // </summary>
    internal static class SelectionPathResolver 
    {
        private static Dictionary<string, ISelectionPathInterpreter> _interpreters;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static SelectionPathResolver() 
        {

            // Register known SelectionPath interpreters
            //
            SelectionPathResolver.RegisterInterpreter(CategoryContainerSelectionPathInterpreter.Instance);
            SelectionPathResolver.RegisterInterpreter(CategoryEditorSelectionPathInterpreter.Instance);
            SelectionPathResolver.RegisterInterpreter(PropertySelectionPathInterpreter.Instance);
        }

        private static void RegisterInterpreter(ISelectionPathInterpreter interpreter) 
        {
            if (interpreter == null) 
            {
                throw FxTrace.Exception.ArgumentNull("interpreter");
            }

            if (_interpreters == null)
            {
                _interpreters = new Dictionary<string, ISelectionPathInterpreter>();
            }

            if (_interpreters.ContainsKey(interpreter.PathTypeId))
            {
                Debug.Fail(string.Format(System.Globalization.CultureInfo.CurrentCulture, "ISelectionPathInterpreter for path type of ID '{0}' already registered.  Ignoring.", interpreter.PathTypeId));
            }
            else 
            {
                _interpreters[interpreter.PathTypeId] = interpreter;
            }
        }

        // <summary>
        // Attempt to resolve the given SelectionPath into the corresponding visual in the
        // specified CategoryList control.
        // </summary>
        // <param name="root">CategoryList control instance to look in</param>
        // <param name="path">SelectionPath to resolve</param>
        // <returns>Corresponding visual, if found, null otherwise</returns>
        public static DependencyObject ResolveSelectionPath(CategoryList root, SelectionPath path, out bool pendingGeneration) 
        {
            pendingGeneration = false;
            if (root == null || path == null) 
            {
                return null;
            }

            ISelectionPathInterpreter interpreter;
            if (!_interpreters.TryGetValue(path.PathTypeId, out interpreter)) 
            {
                return null;
            }
            return interpreter.ResolveSelectionPath(root, path, out pendingGeneration);
        }
    }
}
