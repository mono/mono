namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Resources;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using Microsoft.CSharp;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Diagnostics;

    #region Class ActivityCodeDomReferenceService
    /// <summary>
    /// This class implements the IReferenceService interface and overrides the GetName
    /// method of the wrapped IReferenceService.
    /// </summary>
    /// <remarks>
    /// The CodeDomSerializer in System.Design uses the IReferenceService to generate an
    /// id for all code variables it create during serialization.  By default, the GetName
    /// method ends up returning the QualifiedName property of an Activity, and since that
    /// property always contains a '.', it is an invalid variable id.  This class overrides
    /// the GetName method to return a valid name in the case of an Activity class.
    /// </remarks>
    internal sealed class ActivityCodeDomReferenceService : IReferenceService
    {
        private IReferenceService refService;

        public ActivityCodeDomReferenceService(IReferenceService referenceService)
        {
            this.refService = referenceService;
        }

        public object[] GetReferences(System.Type baseType)
        {
            if (refService != null)
                return refService.GetReferences(baseType);

            return null;
        }

        public object[] GetReferences()
        {
            if (refService != null)
                return refService.GetReferences();

            return null;
        }

        public System.ComponentModel.IComponent GetComponent(object reference)
        {
            if (refService != null)
                return refService.GetComponent(reference);

            return null;
        }

        public object GetReference(string name)
        {
            if (refService != null)
                return refService.GetReference(name);

            return null;
        }

        public string GetName(object reference)
        {
            // If the object is an activity, generate a name of <Scope Name>_<Activity Name>, since
            // the default result of GetName is the activity's QualifiedName, which includes a '.'
            Activity a = reference as Activity;
            if (a != null)
                return a.QualifiedName.Replace('.', '_');

            if (refService != null)
                return refService.GetName(reference);

            return null;
        }
    }
    #endregion

}
