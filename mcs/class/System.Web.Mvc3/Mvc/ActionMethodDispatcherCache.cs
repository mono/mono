namespace System.Web.Mvc {
    using System;
    using System.Reflection;

    internal sealed class ActionMethodDispatcherCache : ReaderWriterCache<MethodInfo,ActionMethodDispatcher> {

        public ActionMethodDispatcherCache() {
        }

        public ActionMethodDispatcher GetDispatcher(MethodInfo methodInfo) {
            return FetchOrCreateItem(methodInfo, () => new ActionMethodDispatcher(methodInfo));
        }

    }
}
