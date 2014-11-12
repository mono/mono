// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// ResourcesEtwProvider.cs
//
// <OWNER>[....]</OWNER>
// <OWNER>[....]</OWNER>
//
// Managed event source for things that can version with MSCORLIB.  
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.Tracing {

    // To use the framework provider
    // 
    //     \\clrmain\tools\Perfmonitor /nokernel /noclr /provider:8E9F5090-2D75-4d03-8A81-E5AFBF85DAF1 start
    //     Run run your app
    //     \\clrmain\tools\Perfmonitor stop
    //     \\clrmain\tools\Perfmonitor print
    //
    // This will produce an XML file, where each event is pretty-printed with all its arguments nicely parsed.
    //
    [FriendAccessAllowed]
    [EventSource(Guid = "8E9F5090-2D75-4d03-8A81-E5AFBF85DAF1", Name = "System.Diagnostics.Eventing.FrameworkEventSource")]
    sealed internal class FrameworkEventSource : EventSource {
        // Defines the singleton instance for the Resources ETW provider
        public static readonly FrameworkEventSource Log = new FrameworkEventSource();

        // Keyword definitions.  These represent logical groups of events that can be turned on and off independently
        // Often each task has a keyword, but where tasks are determined by subsystem, keywords are determined by
        // usefulness to end users to filter.  Generally users don't mind extra events if they are not high volume
        // so grouping low volume events together in a single keywords is OK (users can post-filter by task if desired)
        public static class Keywords {
            public const EventKeywords Loader     = (EventKeywords)0x0001; // This is bit 0
            public const EventKeywords ThreadPool = (EventKeywords)0x0002; 
            public const EventKeywords NetClient  = (EventKeywords)0x0004;
            //
            // This is a private event we do not want to expose to customers.  It is to be used for profiling
            // uses of dynamic type loading by ProjectN applications running on the desktop CLR
            //
            public const EventKeywords DynamicTypeUsage = (EventKeywords)0x0008;
            public const EventKeywords ThreadTransfer   = (EventKeywords)0x0010;
        }

        /// <summary>ETW tasks that have start/stop events.</summary>
        [FriendAccessAllowed]
        public static class Tasks // this name is important for EventSource
        {
            /// <summary>Begin / End - GetResponse.</summary>
            public const EventTask GetResponse      = (EventTask)1;
            /// <summary>Begin / End - GetRequestStream</summary>
            public const EventTask GetRequestStream = (EventTask)2;
            /// <summary>Send / Receive - begin transfer/end transfer</summary>
            public const EventTask ThreadTransfer = (EventTask)3;
        }

        [FriendAccessAllowed]
        public static class Opcodes
        {
            public const EventOpcode ReceiveHandled = (EventOpcode)11;
        }

        // This predicate is used by consumers of this class to deteremine if the class has actually been initialized,
        // and therefore if the public statics are available for use. This is typically not a problem... if the static
        // class constructor fails, then attempts to access the statics (or even this property) will result in a 
        // TypeInitializationException. However, that is not the case while the class loader is actually trying to construct
        // the TypeInitializationException instance to represent that failure, and some consumers of this class are on
        // that code path, specifically the resource manager. 
        public static bool IsInitialized
        {
            get
            {
                return Log != null;
            }
        }

        // The FrameworkEventSource GUID is {8E9F5090-2D75-4d03-8A81-E5AFBF85DAF1}
        private FrameworkEventSource() : base(new Guid(0x8e9f5090, 0x2d75, 0x4d03, 0x8a, 0x81, 0xe5, 0xaf, 0xbf, 0x85, 0xda, 0xf1), "System.Diagnostics.Eventing.FrameworkEventSource") { }

        // WriteEvent overloads (to avoid the "params" EventSource.WriteEvent

        // optimized for common signatures (used by the ThreadTransferSend/Receive events)
        [NonEvent, System.Security.SecuritySafeCritical]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        private unsafe void WriteEvent(int eventId, long arg1, int arg2, string arg3, bool arg4)
        {
            if (IsEnabled())
            {
                if (arg3 == null) arg3 = "";
                fixed (char* string3Bytes = arg3)
                {
                    EventSource.EventData* descrs = stackalloc EventSource.EventData[4];
                    descrs[0].DataPointer = (IntPtr)(&arg1);
                    descrs[0].Size = 8;
                    descrs[1].DataPointer = (IntPtr)(&arg2);
                    descrs[1].Size = 4;
                    descrs[2].DataPointer = (IntPtr)string3Bytes;
                    descrs[2].Size = ((arg3.Length + 1) * 2);
                    descrs[3].DataPointer = (IntPtr)(&arg4);
                    descrs[3].Size = 4;
                    WriteEventCore(eventId, 4, descrs);
                }
            }
        }

        // optimized for common signatures (used by the ThreadTransferSend/Receive events)
        [NonEvent, System.Security.SecuritySafeCritical]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "This does not need to be correct when racing with other threads")]
        private unsafe void WriteEvent(int eventId, long arg1, int arg2, string arg3)
        {
            if (IsEnabled())
            {
                if (arg3 == null) arg3 = "";
                fixed (char* string3Bytes = arg3)
                {
                    EventSource.EventData* descrs = stackalloc EventSource.EventData[3];
                    descrs[0].DataPointer = (IntPtr)(&arg1);
                    descrs[0].Size = 8;
                    descrs[1].DataPointer = (IntPtr)(&arg2);
                    descrs[1].Size = 4;
                    descrs[2].DataPointer = (IntPtr)string3Bytes;
                    descrs[2].Size = ((arg3.Length + 1) * 2);
                    WriteEventCore(eventId, 3, descrs);
                }
            }
        }

        // ResourceManager Event Definitions 

        [Event(1, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerLookupStarted(String baseName, String mainAssemblyName, String cultureName) {
            WriteEvent(1, baseName, mainAssemblyName, cultureName);
        }

        [Event(2, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerLookingForResourceSet(String baseName, String mainAssemblyName, String cultureName) {
            if (IsEnabled())
                WriteEvent(2, baseName, mainAssemblyName, cultureName);
        }

        [Event(3, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerFoundResourceSetInCache(String baseName, String mainAssemblyName, String cultureName) {
            if (IsEnabled())
                WriteEvent(3, baseName, mainAssemblyName, cultureName);
        }

        // After loading a satellite assembly, we already have the ResourceSet for this culture in
        // the cache. This can happen if you have an assembly load callback that called into this
        // instance of the ResourceManager.
        [Event(4, Level = EventLevel.Warning, Keywords = Keywords.Loader)]
        public void ResourceManagerFoundResourceSetInCacheUnexpected(String baseName, String mainAssemblyName, String cultureName) {
            if (IsEnabled())
                WriteEvent(4, baseName, mainAssemblyName, cultureName);
        }

        // manifest resource stream lookup succeeded
        [Event(5, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerStreamFound(String baseName, String mainAssemblyName, String cultureName, String loadedAssemblyName, String resourceFileName) {
            if (IsEnabled())
                WriteEvent(5, baseName, mainAssemblyName, cultureName, loadedAssemblyName, resourceFileName);
        }

        // manifest resource stream lookup failed
        [Event(6, Level = EventLevel.Warning, Keywords = Keywords.Loader)]
        public void ResourceManagerStreamNotFound(String baseName, String mainAssemblyName, String cultureName, String loadedAssemblyName, String resourceFileName) {
            if (IsEnabled())
                WriteEvent(6, baseName, mainAssemblyName, cultureName, loadedAssemblyName, resourceFileName);
        }

        [Event(7, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerGetSatelliteAssemblySucceeded(String baseName, String mainAssemblyName, String cultureName, String assemblyName) {
            if (IsEnabled())
                WriteEvent(7, baseName, mainAssemblyName, cultureName, assemblyName);
        }

        [Event(8, Level = EventLevel.Warning, Keywords = Keywords.Loader)]
        public void ResourceManagerGetSatelliteAssemblyFailed(String baseName, String mainAssemblyName, String cultureName, String assemblyName) {
            if (IsEnabled())
                WriteEvent(8, baseName, mainAssemblyName, cultureName, assemblyName);
        }

        [Event(9, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerCaseInsensitiveResourceStreamLookupSucceeded(String baseName, String mainAssemblyName, String assemblyName, String resourceFileName) {
            if (IsEnabled())
                WriteEvent(9, baseName, mainAssemblyName, assemblyName, resourceFileName);
        }

        [Event(10, Level = EventLevel.Warning, Keywords = Keywords.Loader)]
        public void ResourceManagerCaseInsensitiveResourceStreamLookupFailed(String baseName, String mainAssemblyName, String assemblyName, String resourceFileName) {
            if (IsEnabled())
                WriteEvent(10, baseName, mainAssemblyName, assemblyName, resourceFileName);
        }

        // Could not access the manifest resource the assembly
        [Event(11, Level = EventLevel.Error, Keywords = Keywords.Loader)]
        public void ResourceManagerManifestResourceAccessDenied(String baseName, String mainAssemblyName, String assemblyName, String canonicalName) {
            if (IsEnabled())
                WriteEvent(11, baseName, mainAssemblyName, assemblyName, canonicalName);
        }

        // Neutral resources are sufficient for this culture. Skipping satellites
        [Event(12, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerNeutralResourcesSufficient(String baseName, String mainAssemblyName, String cultureName) {
            if (IsEnabled())
                WriteEvent(12, baseName, mainAssemblyName, cultureName);
        }

        [Event(13, Level = EventLevel.Warning, Keywords = Keywords.Loader)]
        public void ResourceManagerNeutralResourceAttributeMissing(String mainAssemblyName) {
            if (IsEnabled())
                WriteEvent(13, mainAssemblyName);
        }

        [Event(14, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerCreatingResourceSet(String baseName, String mainAssemblyName, String cultureName, String fileName) {
            if (IsEnabled())
                WriteEvent(14, baseName, mainAssemblyName, cultureName, fileName);
        }

        [Event(15, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerNotCreatingResourceSet(String baseName, String mainAssemblyName, String cultureName) {
            if (IsEnabled())
                WriteEvent(15, baseName, mainAssemblyName, cultureName);
        }

        [Event(16, Level = EventLevel.Warning, Keywords = Keywords.Loader)]
        public void ResourceManagerLookupFailed(String baseName, String mainAssemblyName, String cultureName) {
            if (IsEnabled())
                WriteEvent(16, baseName, mainAssemblyName, cultureName);
        }

        [Event(17, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerReleasingResources(String baseName, String mainAssemblyName) {
            if (IsEnabled())
                WriteEvent(17, baseName, mainAssemblyName);
        }

        [Event(18, Level = EventLevel.Warning, Keywords = Keywords.Loader)]
        public void ResourceManagerNeutralResourcesNotFound(String baseName, String mainAssemblyName, String resName) {
            if (IsEnabled())
                WriteEvent(18, baseName, mainAssemblyName, resName);
        }

        [Event(19, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerNeutralResourcesFound(String baseName, String mainAssemblyName, String resName) {
            if (IsEnabled())
                WriteEvent(19, baseName, mainAssemblyName, resName);
        }

        [Event(20, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerAddingCultureFromConfigFile(String baseName, String mainAssemblyName, String cultureName) {
            if (IsEnabled())
                WriteEvent(20, baseName, mainAssemblyName, cultureName);
        }

        [Event(21, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerCultureNotFoundInConfigFile(String baseName, String mainAssemblyName, String cultureName) {
            if (IsEnabled())
                WriteEvent(21, baseName, mainAssemblyName, cultureName);
        }

        [Event(22, Level = EventLevel.Informational, Keywords = Keywords.Loader)]
        public void ResourceManagerCultureFoundInConfigFile(String baseName, String mainAssemblyName, String cultureName) {
            if (IsEnabled())
                WriteEvent(22, baseName, mainAssemblyName, cultureName);
        }


        // ResourceManager Event Wrappers

        [NonEvent]
        public void ResourceManagerLookupStarted(String baseName, Assembly mainAssembly, String cultureName) {
            if (IsEnabled())
                ResourceManagerLookupStarted(baseName, GetName(mainAssembly), cultureName);
        }

        [NonEvent]
        public void ResourceManagerLookingForResourceSet(String baseName, Assembly mainAssembly, String cultureName) {
            if (IsEnabled())
                ResourceManagerLookingForResourceSet(baseName, GetName(mainAssembly), cultureName);
        }

        [NonEvent]
        public void ResourceManagerFoundResourceSetInCache(String baseName, Assembly mainAssembly, String cultureName) {
            if (IsEnabled())
                ResourceManagerFoundResourceSetInCache(baseName, GetName(mainAssembly), cultureName);
        }

        [NonEvent]
        public void ResourceManagerFoundResourceSetInCacheUnexpected(String baseName, Assembly mainAssembly, String cultureName) {
            if (IsEnabled())
                ResourceManagerFoundResourceSetInCacheUnexpected(baseName, GetName(mainAssembly), cultureName);
        }

        [NonEvent]
        public void ResourceManagerStreamFound(String baseName, Assembly mainAssembly, String cultureName, Assembly loadedAssembly, String resourceFileName) {
            if (IsEnabled())
                ResourceManagerStreamFound(baseName, GetName(mainAssembly), cultureName, GetName(loadedAssembly), resourceFileName);
        }

        [NonEvent]
        public void ResourceManagerStreamNotFound(String baseName, Assembly mainAssembly, String cultureName, Assembly loadedAssembly, String resourceFileName) {
            if (IsEnabled())
                ResourceManagerStreamNotFound(baseName, GetName(mainAssembly), cultureName, GetName(loadedAssembly), resourceFileName);
        }

        [NonEvent]
        public void ResourceManagerGetSatelliteAssemblySucceeded(String baseName, Assembly mainAssembly, String cultureName, String assemblyName) {
            if (IsEnabled())
                ResourceManagerGetSatelliteAssemblySucceeded(baseName, GetName(mainAssembly), cultureName, assemblyName);
        }

        [NonEvent]
        public void ResourceManagerGetSatelliteAssemblyFailed(String baseName, Assembly mainAssembly, String cultureName, String assemblyName) {
            if (IsEnabled())
                ResourceManagerGetSatelliteAssemblyFailed(baseName, GetName(mainAssembly), cultureName, assemblyName);
        }

        [NonEvent]
        public void ResourceManagerCaseInsensitiveResourceStreamLookupSucceeded(String baseName, Assembly mainAssembly, String assemblyName, String resourceFileName) {
            if (IsEnabled())
                ResourceManagerCaseInsensitiveResourceStreamLookupSucceeded(baseName, GetName(mainAssembly), assemblyName, resourceFileName);
        }

        [NonEvent]
        public void ResourceManagerCaseInsensitiveResourceStreamLookupFailed(String baseName, Assembly mainAssembly, String assemblyName, String resourceFileName) {
            if (IsEnabled())
                ResourceManagerCaseInsensitiveResourceStreamLookupFailed(baseName, GetName(mainAssembly), assemblyName, resourceFileName);
        }

        [NonEvent]
        public void ResourceManagerManifestResourceAccessDenied(String baseName, Assembly mainAssembly, String assemblyName, String canonicalName) {
            if (IsEnabled())
                ResourceManagerManifestResourceAccessDenied(baseName, GetName(mainAssembly), assemblyName, canonicalName);
        }

        [NonEvent]
        public void ResourceManagerNeutralResourcesSufficient(String baseName, Assembly mainAssembly, String cultureName) {
            if (IsEnabled()) 
                ResourceManagerNeutralResourcesSufficient(baseName, GetName(mainAssembly), cultureName);
        }

        [NonEvent]
        public void ResourceManagerNeutralResourceAttributeMissing(Assembly mainAssembly) {
            if (IsEnabled())
                ResourceManagerNeutralResourceAttributeMissing(GetName(mainAssembly));
        }

        [NonEvent]
        public void ResourceManagerCreatingResourceSet(String baseName, Assembly mainAssembly, String cultureName, String fileName) {
            if (IsEnabled())
                ResourceManagerCreatingResourceSet(baseName, GetName(mainAssembly), cultureName, fileName);
        }

        [NonEvent]
        public void ResourceManagerNotCreatingResourceSet(String baseName, Assembly mainAssembly, String cultureName) {
            if (IsEnabled())
                ResourceManagerNotCreatingResourceSet(baseName, GetName(mainAssembly), cultureName);
        }

        [NonEvent]
        public void ResourceManagerLookupFailed(String baseName, Assembly mainAssembly, String cultureName) {
            if (IsEnabled())
                ResourceManagerLookupFailed(baseName, GetName(mainAssembly), cultureName);
        }

        [NonEvent]
        public void ResourceManagerReleasingResources(String baseName, Assembly mainAssembly) {
            if (IsEnabled())
                ResourceManagerReleasingResources(baseName, GetName(mainAssembly));
        }

        [NonEvent]
        public void ResourceManagerNeutralResourcesNotFound(String baseName, Assembly mainAssembly, String resName) {
            if (IsEnabled())
                ResourceManagerNeutralResourcesNotFound(baseName, GetName(mainAssembly), resName);
        }

        [NonEvent]
        public void ResourceManagerNeutralResourcesFound(String baseName, Assembly mainAssembly, String resName) {
            if (IsEnabled())
                ResourceManagerNeutralResourcesFound(baseName, GetName(mainAssembly), resName);
        }

        [NonEvent]
        public void ResourceManagerAddingCultureFromConfigFile(String baseName, Assembly mainAssembly, String cultureName) {
            if (IsEnabled())
                ResourceManagerAddingCultureFromConfigFile(baseName, GetName(mainAssembly), cultureName);
        }

        [NonEvent]
        public void ResourceManagerCultureNotFoundInConfigFile(String baseName, Assembly mainAssembly, String cultureName) {
            if (IsEnabled())
                ResourceManagerCultureNotFoundInConfigFile(baseName, GetName(mainAssembly), cultureName);
        }

        [NonEvent]
        public void ResourceManagerCultureFoundInConfigFile(String baseName, Assembly mainAssembly, String cultureName) {
            if (IsEnabled())
                ResourceManagerCultureFoundInConfigFile(baseName, GetName(mainAssembly), cultureName);
        }

        private static string GetName(Assembly assembly) {
            if (assembly == null)
                return "<<NULL>>";
            else
                return assembly.FullName;
        }

        [Event(30, Level = EventLevel.Verbose, Keywords = Keywords.ThreadPool|Keywords.ThreadTransfer)]
        public void ThreadPoolEnqueueWork(long workID) {
            WriteEvent(30, workID);
        }
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void ThreadPoolEnqueueWorkObject(object workID) {
            // convert the Object Id to a long
            ThreadPoolEnqueueWork((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref workID)));
        }

        [Event(31, Level = EventLevel.Verbose, Keywords = Keywords.ThreadPool|Keywords.ThreadTransfer)]
        public void ThreadPoolDequeueWork(long workID) {
            WriteEvent(31, workID);
        }

        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void ThreadPoolDequeueWorkObject(object workID) {
            // convert the Object Id to a long
            ThreadPoolDequeueWork((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref workID)));
        }

#region Dynamic Type Usage Events - These should not be documented!
#if !FEATURE_CORECLR
        [Event(32, Keywords = Keywords.DynamicTypeUsage)]
        public void ActivatorCreateInstance(String typeName)
        {
            WriteEvent(32, typeName);
        }

        [Event(33, Keywords = Keywords.DynamicTypeUsage)]
        public void ActivatorCreateInstanceT(String typeName)
        {
            WriteEvent(33, typeName);
        }

        [Event(34, Keywords = Keywords.DynamicTypeUsage)]
        public void ArrayCreateInstance(String typeName)
        {
            WriteEvent(34, typeName);
        }
        
        [Event(35, Keywords = Keywords.DynamicTypeUsage)]
        public void TypeGetType(String typeName)
        {
            WriteEvent(35, typeName);
        }

        [Event(36, Keywords = Keywords.DynamicTypeUsage)]
        public void AssemblyGetType(String typeName)
        {
            WriteEvent(36, typeName);
        }

        [Event(37, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetMethodFromHandle()
        {
            WriteEvent(37);
        }

        [Event(38, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetMethodFromHandle(String typeName, String method)
        {
            WriteEvent(38, typeName, method);
        }

        [Event(39, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetFieldFromHandle()
        {
            WriteEvent(39);
        }

        [Event(40, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetFieldFromHandle(String typeName, String field)
        {
            WriteEvent(40, typeName, field);
        }

        [Event(41, Keywords = Keywords.DynamicTypeUsage)]
        public void EnumTryParseEnum(String typeName, String value)
        {
            WriteEvent(41, typeName, value);
        }

        [Event(42, Keywords = Keywords.DynamicTypeUsage)]
        public void EnumGetUnderlyingType(String typeName)
        {
            WriteEvent(42, typeName);
        }

        [Event(43, Keywords = Keywords.DynamicTypeUsage)]
        public void EnumGetValues(String typeName)
        {
            WriteEvent(43, typeName);
        }

        [Event(44, Keywords = Keywords.DynamicTypeUsage)]
        public void EnumGetName(String typeName)
        {
            WriteEvent(44, typeName);
        }

        [Event(45, Keywords = Keywords.DynamicTypeUsage)]
        public void EnumGetNames(String typeName)
        {
            WriteEvent(45, typeName);
        }

        [Event(46, Keywords = Keywords.DynamicTypeUsage)]
        public void EnumIsDefined(String typeName)
        {
            WriteEvent(46, typeName);
        }

        [Event(47, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginEnumFormat(String typeName)
        {
            WriteEvent(47, typeName);
        }

        [Event(48, Keywords = Keywords.DynamicTypeUsage)]
        public void EndEnumFormat(String typeName)
        {
            WriteEvent(48, typeName);
        }

        [Event(49, Keywords = Keywords.DynamicTypeUsage)]
        public void EnumToObject(String typeName)
        {
            WriteEvent(49, typeName);
        }

        [Event(50, Keywords = Keywords.DynamicTypeUsage)]
        public void TypeFullName(String typeName)
        {
            WriteEvent(50, typeName);
        }

        [Event(51, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginTypeAssemblyQualifiedName(String typeName)
        {
            WriteEvent(51, typeName);
        }

        [Event(52, Keywords = Keywords.DynamicTypeUsage)]
        public void EndTypeAssemblyQualifiedName(String typeName)
        {
            WriteEvent(52, typeName);
        }

        [Event(53, Keywords = Keywords.DynamicTypeUsage)]
        public void TypeNamespace(String typeName)
        {
            WriteEvent(53, typeName);
        }

        [Event(54, Keywords = Keywords.DynamicTypeUsage)]
        public void MethodName(String typeName, String methodName)
        {
            WriteEvent(54, typeName, methodName);
        }

        [Event(55, Keywords = Keywords.DynamicTypeUsage)]
        public void FieldName(String typeName, String fieldName)
        {
            WriteEvent(55, typeName, fieldName);
        }

        [Event(56, Keywords = Keywords.DynamicTypeUsage)]
        public void TypeName(String typeName)
        {
            WriteEvent(56, typeName);
        }
        
        [Event(57, Keywords = Keywords.DynamicTypeUsage)]
        public void IntrospectionExtensionsGetTypeInfo(String typeName)
        {
            WriteEvent(57, typeName);
        }

        [Event(58, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetRuntimeProperties(String typeName)
        {
            WriteEvent(58, typeName);
        }

        [Event(59, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetRuntimeProperties(String typeName)
        {
            WriteEvent(59, typeName);
        }

        [Event(60, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetRuntimeEvents(String typeName)
        {
            WriteEvent(60, typeName);
        }

        [Event(61, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetRuntimeEvents(String typeName)
        {
            WriteEvent(61, typeName);
        }

        [Event(62, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetRuntimeMethods(String typeName)
        {
            WriteEvent(62, typeName);
        }

        [Event(63, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetRuntimeMethods(String typeName)
        {
            WriteEvent(63, typeName);
        }

        [Event(64, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetRuntimeFields(String typeName)
        {
            WriteEvent(64, typeName);
        }

        [Event(65, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetRuntimeFields(String typeName)
        {
            WriteEvent(65, typeName);
        }
        
        [Event(66, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetRuntimeProperty(String typeName, String propertyName)
        {
            WriteEvent(66, typeName, propertyName);
        }

        [Event(67, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetRuntimeProperty(String typeName, String propertyName)
        {
            WriteEvent(67, typeName, propertyName);
        }

        [Event(68, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetRuntimeEvent(String typeName, String eventName)
        {
            WriteEvent(68, typeName, eventName);
        }

        [Event(69, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetRuntimeEvent(String typeName, String eventName)
        {
            WriteEvent(69, typeName, eventName);
        }

        [Event(70, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetRuntimeMethod(String typeName, String methodName)
        {
            WriteEvent(70, typeName, methodName);
        }

        [Event(71, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetRuntimeMethod(String typeName, String methodName)
        {
            WriteEvent(71, typeName, methodName);
        }

        [Event(72, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetRuntimeField(String typeName, String fieldName)
        {
            WriteEvent(72, typeName, fieldName);
        }

        [Event(73, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetRuntimeField(String typeName, String fieldName)
        {
            WriteEvent(73, typeName, fieldName);
        }

        [Event(79, Keywords = Keywords.DynamicTypeUsage)]
        public void MethodInfoInvoke(String typeName, String methodName)
        {
            WriteEvent(79, typeName, methodName);
        }

        [Event(80, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginMethodInfoCreateDelegate(String typeName, String methodName, String delegateTypeName)
        {
            WriteEvent(80, typeName, methodName, delegateTypeName);
        }

        [Event(81, Keywords = Keywords.DynamicTypeUsage)]
        public void EndMethodInfoCreateDelegate(String typeName, String methodName, String delegateTypeName)
        {
            WriteEvent(81, typeName, methodName, delegateTypeName);
        }

        [Event(82, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginCreateIReference()
        {
            WriteEvent(82);
        }

        [Event(83, Keywords = Keywords.DynamicTypeUsage)]
        public void EndCreateIReference(String typeName)
        {
            WriteEvent(83, typeName);
        }
        
        [Event(84, Keywords = Keywords.DynamicTypeUsage)]
        public void ConstructorInfoInvoke(String typeName, String methodName)
        {
            WriteEvent(84, typeName, methodName);
        }

        [Event(85, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalAsAnyConvertToNative(String typeName)
        {
            WriteEvent(85, typeName);
        }

        [Event(86, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalAsAnyConvertToManaged(String typeName)
        {
            WriteEvent(86, typeName);
        }

        [Event(87, Keywords = Keywords.DynamicTypeUsage)]
        public void ManagedActivationFactoryConstructor(String typeName)
        {
            WriteEvent(87, typeName);
        }

        [Event(88, Keywords = Keywords.DynamicTypeUsage)]
        public void WindowsRuntimeMarshalGetActivationFactory(String typeName)
        {
            WriteEvent(88, typeName);
        }

        [Event(89, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalCreateAggregatedObject(String typeName)
        {
            WriteEvent(89, typeName);
        }

        [Event(90, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalCreateWrapperOfType(String comObjectTypeName, String wrapperTypeName)
        {
            WriteEvent(90, comObjectTypeName, wrapperTypeName);
        }

        [Event(91, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalDestroyStructure(String typeName)
        {
            WriteEvent(91, typeName);
        }

        [Event(92, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetComInterfaceForObject(String objectTypeName, String typeName, String implementsAndMode)
        {
            WriteEvent(92, objectTypeName, typeName, implementsAndMode);
        }

        [Event(93, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetDelegateForFunctionPointer(String typeName)
        {
            WriteEvent(93, typeName);
        }

        [Event(94, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetExceptionCode()
        {
            WriteEvent(94);
        }

        [Event(95, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetExceptionForHR()
        {
            WriteEvent(95);
        }
        
        [Event(96, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetExceptionForHR2()
        {
            WriteEvent(96);
        }

        [Event(97, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetFunctionPointerForDelegate(String typeName, String methodName)
        {
            WriteEvent(97, typeName, methodName);
        }

        [Event(98, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetIUnknownForObject(String typeName)
        {
            WriteEvent(98, typeName);
        }

        [Event(99, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetNativeVariantForObject(String typeName)
        {
            WriteEvent(99, typeName);
        }

        [Event(100, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetObjectForIUnknown(String typeName)
        {
            WriteEvent(100, typeName);
        }

        [Event(101, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetObjectForNativeVariant(String typeName)
        {
            WriteEvent(101, typeName);
        }

        [Event(102, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetObjectsForNativeVariants(String typeNames)
        {
            WriteEvent(102, typeNames);
        }

        [Event(103, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetStartComSlot(String typeName)
        {
            WriteEvent(103, typeName);
        }

        [Event(104, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetTypeFromCLSID(String typeName, String guid)
        {
            WriteEvent(104, typeName, guid);
        }

        [Event(105, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetTypeInfoName(String typeName)
        {
            WriteEvent(105, typeName);
        }

        [Event(106, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalGetUniqueObjectForIUnknown(String typeName)
        {
            WriteEvent(106, typeName);
        }

        [Event(107, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginMarshalOffsetOf(String typeName, String fieldName)
        {
            WriteEvent(107, typeName, fieldName);
        }

        [Event(108, Keywords = Keywords.DynamicTypeUsage)]
        public void EndMarshalOffsetOf(String typeName, String fieldName)
        {
            WriteEvent(108, typeName, fieldName);
        }

        [Event(109, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginMarshalPtrToStructure(String typeName)
        {
            WriteEvent(109, typeName);
        }

        [Event(110, Keywords = Keywords.DynamicTypeUsage)]
        public void EndMarshalPtrToStructure(String typeName)
        {
            WriteEvent(110, typeName);
        }

        [Event(111, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalSizeOf(String typeName)
        {
            WriteEvent(111, typeName);
        }

        [Event(112, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalStructureToPtr(String typeName, String deleteOld)
        {
            WriteEvent(112, typeName, deleteOld);
        }

        [Event(113, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalThrowExceptionForHR()
        {
            WriteEvent(113);
        }

        [Event(114, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalUnsafeAddrOfPinnedArrayElement(String typeName)
        {
            WriteEvent(114, typeName);
        }

        [Event(115, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginICustomPropertyProviderCreateProperty(String typeName, String propertyName)
        {
            WriteEvent(115, typeName, propertyName);
        }

        [Event(116, Keywords = Keywords.DynamicTypeUsage)]
        public void EndICustomPropertyProviderCreateProperty(String typeName, String propertyName)
        {
            WriteEvent(116, typeName, propertyName);
        }

        [Event(117, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginICustomPropertyProviderCreateIndexedProperty(String typeName, String propertyName, String indexedParamTypeName)
        {
            WriteEvent(117, typeName, propertyName, indexedParamTypeName);
        }

        [Event(118, Keywords = Keywords.DynamicTypeUsage)]
        public void EndICustomPropertyProviderCreateIndexedProperty(String typeName, String propertyName, String indexedParamTypeName)
        {
            WriteEvent(118, typeName, propertyName, indexedParamTypeName);
        }

        [Event(119, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginCustomPropertyImplGetValue(String typeName, String propertyTypeName)
        {
            WriteEvent(119, typeName, propertyTypeName);
        }

        [Event(120, Keywords = Keywords.DynamicTypeUsage)]
        public void EndCustomPropertyImplGetValue(String typeName, String propertyTypeName)
        {
            WriteEvent(120, typeName, propertyTypeName);
        }

        [Event(121, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginCustomPropertyImplGetValueIndexValue(String typeName, String propertyTypeName)
        {
            WriteEvent(121, typeName, propertyTypeName);
        }

        [Event(122, Keywords = Keywords.DynamicTypeUsage)]
        public void EndCustomPropertyImplGetValueIndexValue(String typeName, String propertyTypeName)
        {
            WriteEvent(122, typeName, propertyTypeName);
        }

        [Event(123, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginCustomPropertyImplSetValue(String typeName, String valueTypeName)
        {
            WriteEvent(123, typeName, valueTypeName);
        }

        [Event(124, Keywords = Keywords.DynamicTypeUsage)]
        public void EndCustomPropertyImplSetValue(String typeName, String valueTypeName)
        {
            WriteEvent(124, typeName, valueTypeName);
        }

        [Event(125, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginCustomPropertyImplSetValueIndexValue(String typeName, String propertyTypeName, String indexValueTypeName)
        {
            WriteEvent(125, typeName, propertyTypeName, indexValueTypeName);
        }

        [Event(126, Keywords = Keywords.DynamicTypeUsage)]
        public void EndCustomPropertyImplSetValueIndexValue(String typeName, String propertyTypeName, String indexValueTypeName)
        {
            WriteEvent(126, typeName, propertyTypeName, indexValueTypeName);
        }
        
        [Event(127, Keywords = Keywords.DynamicTypeUsage)]
        public void MarshalThrowExceptionForHR2()
        {
            WriteEvent(127);
        }
        
        [Event(128, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetRuntimeConstructors(String typeName)
        {
            WriteEvent(128, typeName);
        }

        [Event(129, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetRuntimeConstructors(String typeName)
        {
            WriteEvent(129, typeName);
        }

        [Event(130, Keywords = Keywords.DynamicTypeUsage)]
        public void BeginGetRuntimeMembers(String typeName)
        {
            WriteEvent(130, typeName);
        }

        [Event(131, Keywords = Keywords.DynamicTypeUsage)]
        public void EndGetRuntimeMembers(String typeName)
        {
            WriteEvent(131, typeName);
        }

        [Event(132, Keywords = Keywords.DynamicTypeUsage)]
        public void EventName(String typeName, String eventName)
        {
            WriteEvent(132, typeName, eventName);
        }

        [Event(133, Keywords = Keywords.DynamicTypeUsage)]
        public void QueryAttributeIsDefined(String typeName)
        {
            WriteEvent(133, typeName);
        }
#endif
#endregion

    
        [Event(140, Level = EventLevel.Informational, Keywords = Keywords.NetClient, Task = Tasks.GetResponse, Opcode = EventOpcode.Start)]
        private void BeginGetResponse(long id, string uri) {
            if (IsEnabled())
                WriteEvent(140, id, uri);
        }
        
        [Event(141, Level = EventLevel.Informational, Keywords = Keywords.NetClient, Task = Tasks.GetResponse, Opcode = EventOpcode.Stop)]
        private void EndGetResponse(long id) {
            if (IsEnabled())
                WriteEvent(141, id);
        }
        
        [Event(142, Level = EventLevel.Informational, Keywords = Keywords.NetClient, Task = Tasks.GetRequestStream, Opcode = EventOpcode.Start)]
        private void BeginGetRequestStream(long id, string uri) {
            if (IsEnabled())
                WriteEvent(142, id, uri);
        }
        
        [Event(143, Level = EventLevel.Informational, Keywords = Keywords.NetClient, Task = Tasks.GetRequestStream, Opcode = EventOpcode.Stop)]
        private void EndGetRequestStream(long id) {
            if (IsEnabled())
                WriteEvent(143, id);
        }
        
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void BeginGetResponse(object id, string uri) {
            BeginGetResponse((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref id)), uri);
        }
            
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void EndGetResponse(object id) {
            EndGetResponse((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref id)));
        }
        
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void BeginGetRequestStream(object id, string uri) {
            BeginGetRequestStream((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref id)), uri);
        }
        
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void EndGetRequestStream(object id) {
            EndGetRequestStream((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref id)));
        }

        // id -   represents a correlation ID that allows correlation of two activities, one stamped by 
        //        ThreadTransferSend, the other by ThreadTransferReceive
        // kind - identifies the transfer: values below 64 are reserved for the runtime. Currently used values:
        //        1 - managed Timers ("roaming" ID)
        //        2 - managed async IO operations (FileStream, PipeStream, a.o.)
        //        3 - WinRT dispatch operations
        // info - any additional information user code might consider interesting
        [Event(150, Level = EventLevel.Informational, Keywords = Keywords.ThreadTransfer, Task = Tasks.ThreadTransfer, Opcode = EventOpcode.Send)]
        public void ThreadTransferSend(long id, int kind, string info, bool multiDequeues) {
            if (IsEnabled())
                WriteEvent(150, id, kind, info, multiDequeues);
        }
        // id - is a managed object. it gets translated to the object's address. ETW listeners must
        //      keep track of GC movements in order to correlate the value passed to XyzSend with the
        //      (possibly changed) value passed to XyzReceive
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void ThreadTransferSendObj(object id, int kind, string info, bool multiDequeues) {
            ThreadTransferSend((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref id)), kind, info, multiDequeues);
        }

        // id -   represents a correlation ID that allows correlation of two activities, one stamped by 
        //        ThreadTransferSend, the other by ThreadTransferReceive
        // kind - identifies the transfer: values below 64 are reserved for the runtime. Currently used values:
        //        1 - managed Timers ("roaming" ID)
        //        2 - managed async IO operations (FileStream, PipeStream, a.o.)
        //        3 - WinRT dispatch operations
        // info - any additional information user code might consider interesting
        [Event(151, Level = EventLevel.Informational, Keywords = Keywords.ThreadTransfer, Task = Tasks.ThreadTransfer, Opcode = EventOpcode.Receive)]
        public void ThreadTransferReceive(long id, int kind, string info) {
            if (IsEnabled())
                WriteEvent(151, id, kind, info);
        }
        // id - is a managed object. it gets translated to the object's address. ETW listeners must
        //      keep track of GC movements in order to correlate the value passed to XyzSend with the
        //      (possibly changed) value passed to XyzReceive
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void ThreadTransferReceiveObj(object id, int kind, string info) {
            ThreadTransferReceive((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref id)), kind, info);
        }

        // id -   represents a correlation ID that allows correlation of two activities, one stamped by 
        //        ThreadTransferSend, the other by ThreadTransferReceive
        // kind - identifies the transfer: values below 64 are reserved for the runtime. Currently used values:
        //        1 - managed Timers ("roaming" ID)
        //        2 - managed async IO operations (FileStream, PipeStream, a.o.)
        //        3 - WinRT dispatch operations
        // info - any additional information user code might consider interesting
        [Event(152, Level = EventLevel.Informational, Keywords = Keywords.ThreadTransfer, Task = Tasks.ThreadTransfer, Opcode = Opcodes.ReceiveHandled)]
        public void ThreadTransferReceiveHandled(long id, int kind, string info) {
            if (IsEnabled())
                WriteEvent(152, id, kind, info);
        }
        // id - is a managed object. it gets translated to the object's address. ETW listeners must
        //      keep track of GC movements in order to correlate the value passed to XyzSend with the
        //      (possibly changed) value passed to XyzReceive
        [NonEvent, System.Security.SecuritySafeCritical]
        public unsafe void ThreadTransferReceiveHandledObj(object id, int kind, string info) {
            ThreadTransferReceive((long) *((void**) JitHelpers.UnsafeCastToStackPointer(ref id)), kind, info);
        }

    }
}

