//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Activities.Presentation.Model;

    class UpdatableGenericArgumentsFeature : Feature
    {
        public override void Initialize(EditingContext context, Type modelType)
        {
            GenericArgumentUpdater genericArgumentUpdater = context.Services.GetService<GenericArgumentUpdater>();
            if (genericArgumentUpdater == null)
            {
                genericArgumentUpdater = new GenericArgumentUpdater(context);
                context.Services.Publish<GenericArgumentUpdater>(genericArgumentUpdater);
            }
            genericArgumentUpdater.AddSupportForUpdatingTypeArgument(modelType);
        }
    }
}
