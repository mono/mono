//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.ViewState
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Xaml;

    /// <summary>
    /// Class defining ViewStateManager and ViewStateId attached properties.
    /// </summary>
    public static class WorkflowViewState
    {
        /// <summary>
        /// Attachable property for ViewStateManager
        /// </summary>
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes, Justification = "XAML attached property declaration.")]
        public static readonly AttachableMemberIdentifier ViewStateManagerProperty = new AttachableMemberIdentifier(typeof(WorkflowViewState), "ViewStateManager");

        /// <summary>
        /// Attachable property for IdRef
        /// </summary>
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes, Justification = "XAML attached property declaration.")]
        public static readonly AttachableMemberIdentifier IdRefProperty = new AttachableMemberIdentifier(typeof(WorkflowViewState), "IdRef");

        /// <summary>
        /// Set ViewStateManager as an attached property on an object. This method is for XAML serialization purpose only and is not expected to be used by developers.
        /// </summary>
        /// <param name="instance">Instance object to attach ViewStateManager property on</param>
        /// <param name="viewStateManager">ViewStateManager object to attach</param>
        public static void SetViewStateManager(object instance, ViewStateManager viewStateManager)
        {
            AttachablePropertyServices.SetProperty(instance, ViewStateManagerProperty, viewStateManager);
        }

        /// <summary>
        /// Get ViewStateManager attached property value from an object if set
        /// </summary>
        /// <param name="instance">Instance object to retrieve ViewStateManager attached property from</param>
        /// <returns>ViewStateManager object if set; null otherise</returns>
        public static ViewStateManager GetViewStateManager(object instance)
        {
            ViewStateManager viewStateManager;
            if (AttachablePropertyServices.TryGetProperty(instance, ViewStateManagerProperty, out viewStateManager))
            {
                return viewStateManager;
            }

            return null;
        }

        /// <summary>
        /// Set IdRef as an attached property on an object. This method is for XAML serialization purpose only and is not expected to be used by developers.
        /// </summary>
        /// <param name="instance">Instance object to attach IdRef property on</param>
        /// <param name="idRef">refId value to attach</param>
        public static void SetIdRef(object instance, string idRef)
        {
            AttachablePropertyServices.SetProperty(instance, IdRefProperty, idRef);
        }

        /// <summary>
        /// Get RefId attached property value from an object if set
        /// </summary>
        /// <param name="instance">Instance object to retrieve RefId attached property from</param>
        /// <returns>RefId value if set; null otherise</returns>
        public static string GetIdRef(object instance)
        {
            string idRef;
            if (AttachablePropertyServices.TryGetProperty(instance, IdRefProperty, out idRef))
            {
                return idRef;
            }

            return null;
        }
    }
}
