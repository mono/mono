//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Collections.Generic;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Runtime;

    class FeatureManager
    {
        EditingContext context;

        HashSet<Type> initializedTypes;

        public FeatureManager(EditingContext context)
        {
            this.context = context;
            initializedTypes = new HashSet<Type>();
        }

        public void InitializeFeature(Type modelType)
        {
            Fx.Assert(modelType != null, "Why would anyone initialize a feature that is not associated with a type");

            if (!initializedTypes.Contains(modelType))
            {
                initializedTypes.Add(modelType);
                foreach (FeatureAttribute featureAttribute in ExtensibilityAccessor.GetAttributes<FeatureAttribute>(modelType))
                {
                    if (typeof(Feature).IsAssignableFrom(featureAttribute.Type))
                    {
                        Feature feature = (Feature)Activator.CreateInstance(featureAttribute.Type);
                        if (feature != null)
                        {
                            feature.Initialize(this.context, modelType);
                        }
                    }
                }
                if (modelType.IsGenericType)
                {
                    InitializeFeature(modelType.GetGenericTypeDefinition());
                }
            }
        }
    }
}
