//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    static class ActivityTemplateFactoryExtension
    {
        public static bool IsActivityTemplateFactory(this Type type)
        {
            return type.GetInterface(typeof(IActivityTemplateFactory).FullName) != null || 
                   type.GetInterface(typeof(IActivityTemplateFactory<>).FullName) != null;
        }

        public static bool TryGetActivityTemplateFactory(this Type type, out Type argumentType)
        {
            if (type.GetInterface(typeof(IActivityTemplateFactory).FullName) != null)
            {
                // Hard coding here, because we don't want to create instance before dropped. Suggestion is to use IActivityTemplateFactory<> instead.
                argumentType = typeof(Activity);
                return true;
            }

            Type activityFactoryInterfaceType = type.GetInterface(typeof(IActivityTemplateFactory<>).FullName);
            if (activityFactoryInterfaceType != null)
            {
                argumentType = activityFactoryInterfaceType.GetGenericArguments()[0];
                return true;
            }

            argumentType = null;
            return false;
        }
    }
}
