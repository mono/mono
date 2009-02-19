# [SecurityCritical] needed to execute code inside 'System.Windows.Browser, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e'.
# 56 methods needs to be decorated.

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Windows.Browser.EventHandlerDelegate::BeginInvoke(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Windows.Browser.GetPropertyDelegate::BeginInvoke(System.IntPtr,System.IntPtr,Mono.Value&,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Windows.Browser.InvokeDelegate::BeginInvoke(System.IntPtr,System.IntPtr,System.String,System.IntPtr[],System.Int32,Mono.Value&,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Windows.Browser.SetPropertyDelegate::BeginInvoke(System.IntPtr,System.IntPtr,Mono.Value&,System.AsyncCallback,System.Object)

# p/invoke declaration
+SC-M: System.IntPtr System.Windows.Browser.ScriptableObjectWrapper/ScriptableNativeMethods::moonlight_object_to_npobject(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr System.Windows.Browser.ScriptableObjectWrapper/ScriptableNativeMethods::npobject_to_moonlight_object(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr System.Windows.Browser.ScriptableObjectWrapper/ScriptableNativeMethods::wrapper_create(System.IntPtr,System.IntPtr,System.Windows.Browser.InvokeDelegate,System.Windows.Browser.SetPropertyDelegate,System.Windows.Browser.GetPropertyDelegate,System.Windows.Browser.EventHandlerDelegate,System.Windows.Browser.EventHandlerDelegate)

# p/invoke declaration
+SC-M: System.IntPtr System.Windows.Browser.ScriptableObjectWrapper/ScriptableNativeMethods::wrapper_create_root(System.IntPtr,System.IntPtr,System.Windows.Browser.InvokeDelegate,System.Windows.Browser.SetPropertyDelegate,System.Windows.Browser.GetPropertyDelegate,System.Windows.Browser.EventHandlerDelegate,System.Windows.Browser.EventHandlerDelegate)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.Browser.ScriptableObjectWrapper::get_MoonHandle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.Browser.ScriptObject::get_Handle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.WebApplication::get_PluginHandle()

# using 'System.IntPtr' as a parameter type
+SC-M: System.UInt32 System.Windows.Browser.Net.BrowserHttpWebRequest::OnAsyncDataAvailable(System.IntPtr,System.IntPtr,System.IntPtr,System.UInt32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.UInt32 System.Windows.Browser.Net.BrowserHttpWebRequest::OnAsyncDataAvailableSafe(System.IntPtr,System.IntPtr,System.IntPtr,System.UInt32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.UInt32 System.Windows.Browser.Net.BrowserHttpWebRequest::OnAsyncResponseFinished(System.IntPtr,System.IntPtr,System.Boolean,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.UInt32 System.Windows.Browser.Net.BrowserHttpWebRequest::OnAsyncResponseFinishedSafe(System.IntPtr,System.IntPtr,System.Boolean,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.UInt32 System.Windows.Browser.Net.BrowserHttpWebRequest::OnAsyncResponseStarted(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.UInt32 System.Windows.Browser.Net.BrowserHttpWebRequest::OnAsyncResponseStartedSafe(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.EventHandlerDelegate::Invoke(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.GetPropertyDelegate::Invoke(System.IntPtr,System.IntPtr,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.HtmlDocument::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.HtmlElement::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.HtmlObject/EventInfo::DomEventHandler(System.IntPtr,System.String,System.Int32,System.Int32,System.Int32,System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.HtmlObject::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.HtmlObject::SetPropertyInternal(System.IntPtr,System.String,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.HtmlWindow::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.InvokeDelegate::Invoke(System.IntPtr,System.IntPtr,System.String,System.IntPtr[],System.Int32,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.Net.BrowserHttpWebRequest::InitializeNativeRequest(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.Net.BrowserHttpWebRequest::InitializeNativeRequestSafe(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.Net.BrowserHttpWebResponse::.ctor(System.Windows.Browser.Net.BrowserHttpWebRequest,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.Net.BrowserHttpWebResponse::OnHttpHeader(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.Net.BrowserHttpWebResponse::Write(System.IntPtr,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper/EventDelegate::.ctor(System.Type,System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper/ScriptableNativeMethods::add_event(System.IntPtr,System.IntPtr,System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper/ScriptableNativeMethods::add_method(System.IntPtr,System.IntPtr,System.IntPtr,System.String,System.TypeCode,System.TypeCode[],System.Int32)

# p/invoke declaration
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper/ScriptableNativeMethods::add_property(System.IntPtr,System.IntPtr,System.IntPtr,System.String,System.TypeCode,System.Boolean,System.Boolean)

# p/invoke declaration
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper/ScriptableNativeMethods::emit_event(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper/ScriptableNativeMethods::register(System.IntPtr,System.String,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::.ctor(System.Object,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::AddEvent(System.Reflection.EventInfo,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::AddEventFromUnmanaged(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::AddEventFromUnmanagedSafe(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::GetPropertyFromUnmanaged(System.IntPtr,System.IntPtr,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::GetPropertyFromUnmanagedSafe(System.IntPtr,System.IntPtr,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::InvokeFromUnmanaged(System.IntPtr,System.IntPtr,System.String,System.IntPtr[],System.Int32,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::InvokeFromUnmanagedSafe(System.IntPtr,System.IntPtr,System.String,System.IntPtr[],System.Int32,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::RemoveEvent(System.Reflection.EventInfo,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::RemoveEventFromUnmanaged(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::RemoveEventFromUnmanagedSafe(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::SetPropertyFromUnmanaged(System.IntPtr,System.IntPtr,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptableObjectWrapper::SetPropertyFromUnmanagedSafe(System.IntPtr,System.IntPtr,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptObject::.ctor(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptObject::Initialize(System.IntPtr,System.IntPtr,System.Boolean,System.Boolean)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.ScriptObjectCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Browser.SetPropertyDelegate::Invoke(System.IntPtr,System.IntPtr,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: T System.Windows.Browser.HtmlObject::GetPropertyInternal(System.IntPtr,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: T System.Windows.Browser.HtmlObject::InvokeInternal(System.IntPtr,System.String,System.Object[])

