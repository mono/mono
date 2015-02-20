//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Collections.Generic;
    using System.Runtime;

    internal static class PropertyReferenceUtilities
    {
        public static string GetPropertyReference(object instance, string targetProperty)
        {
            Fx.Assert(instance != null, "instance should not be null.");
            Fx.Assert(!string.IsNullOrEmpty(targetProperty), "targetProperty should not be null or empty.");

            IList<ActivityPropertyReference> references = ActivityBuilder.GetPropertyReferences(instance);

            Fx.Assert(references != null, "references should not be null");

            foreach (ActivityPropertyReference reference in references)
            {
                if (StringComparer.Ordinal.Equals(reference.TargetProperty, targetProperty))
                {
                    return reference.SourceProperty;
                }
            }
            
            return null;
        }

        public static void SetPropertyReference(object instance, string targetProperty, string sourceProperty)
        {
            Fx.Assert(instance != null, "instance should not be null.");
            Fx.Assert(!string.IsNullOrEmpty(targetProperty), "targetProperty should not be null or empty.");

            ActivityPropertyReference entry = null;
            IList<ActivityPropertyReference> references = ActivityBuilder.GetPropertyReferences(instance);

            Fx.Assert(references != null, "references should not be null");

            foreach (ActivityPropertyReference reference in references)
            {
                if (StringComparer.Ordinal.Equals(reference.TargetProperty, targetProperty))
                {
                    entry = reference;
                    break;
                }
            }

            if (string.IsNullOrEmpty(sourceProperty))
            {
                if (entry != null)
                {
                    references.Remove(entry);
                }
            }
            else
            {
                if (entry != null)
                {
                    entry.SourceProperty = sourceProperty;
                }
                else
                {
                    entry = new ActivityPropertyReference();
                    entry.TargetProperty = targetProperty;
                    entry.SourceProperty = sourceProperty;
                    references.Add(entry);
                }
            }
        }
    }
}
