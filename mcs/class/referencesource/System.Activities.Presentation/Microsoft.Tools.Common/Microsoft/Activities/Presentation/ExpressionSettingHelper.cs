//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.Activities.Presentation
{
    using System.Activities.Expressions;
using System.Activities.Presentation.Expressions;
using System.Activities.Presentation.Model;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.VisualBasic.Activities;

    internal static class ExpressionSettingHelper
    {
        internal static readonly string VBExpressionLanguageName = (new VisualBasicValue<string>() as ITextExpression).Language;

        [SuppressMessage("Reliability", "Reliability101", Justification = "We can't use Fx.Assert here since this is not a framework assembly.")]
        internal static string GetRootEditorSetting(ModelTreeManager modelTreeManager, FrameworkName targetFramework)
        {
            Debug.Assert(modelTreeManager != null, "modelTreeManager is null.");
            Debug.Assert(targetFramework != null, "targetFramework is null.");

            string globalEditorSetting = null;
            if (Is45OrHigher(targetFramework))
            {
                if (modelTreeManager != null)
                {
                    ModelItem rootItem = modelTreeManager.Root;
                    if (rootItem != null)
                    {
                        object root = rootItem.GetCurrentValue();
                        globalEditorSetting = ExpressionActivityEditor.GetExpressionActivityEditor(root);
                        if (string.IsNullOrEmpty(globalEditorSetting))
                        {
                            globalEditorSetting = VBExpressionLanguageName;
                        }
                    }
                }
            }
            else
            {
                // When the target framework is less than 4.5, the root setting is ignored and always return VB
                globalEditorSetting = VBExpressionLanguageName;
            }

            return globalEditorSetting;
        }

        private static bool Is45OrHigher(FrameworkName frameworkName)
        {
            return frameworkName.Version.Major > 4 || (frameworkName.Version.Major == 4 && frameworkName.Version.Minor >= 5);
        }
    }
}
