# [SecurityCritical] needed to execute code inside 'System.Windows, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e'.
# 830 methods needs to be decorated.

# using 'System.IntPtr' as a parameter type
+SC-M: Mono.INativeDependencyObjectWrapper Mono.NativeDependencyObjectHelper::FromIntPtr(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: Mono.INativeDependencyObjectWrapper Mono.NativeDependencyObjectHelper::Lookup(Mono.Kind,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: Mono.INativeDependencyObjectWrapper Mono.NativeDependencyObjectHelper::Lookup(System.IntPtr)

# p/invoke declaration
+SC-M: Mono.Kind Mono.NativeMethods::collection_get_element_type(System.IntPtr)

# p/invoke declaration
+SC-M: Mono.Kind Mono.NativeMethods::dependency_property_get_property_type(System.IntPtr)

# p/invoke declaration
+SC-M: Mono.Kind Mono.NativeMethods::event_object_get_object_type(System.IntPtr)

# p/invoke declaration
+SC-M: Mono.Kind Mono.NativeMethods::types_register_type(System.IntPtr,System.String,System.IntPtr,Mono.Kind)

# using 'System.IntPtr' as a parameter type
+SC-M: Mono.Xaml.ManagedXamlLoader Mono.ApplicationLauncher::CreateXamlLoader(System.IntPtr,System.IntPtr,System.IntPtr,System.String,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: Mono.Xaml.XamlLoader Mono.Xaml.XamlLoader::CreateManagedXamlLoader(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: Mono.Xaml.XamlLoader Mono.Xaml.XamlLoader::CreateManagedXamlLoader(System.Reflection.Assembly,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.ApplicationLauncher::InitializeDeployment(System.IntPtr,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.NativeMethods/GSourceFunc::Invoke(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::binding_get_is_sealed(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::binding_get_notify_on_validation_error(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::binding_get_validates_on_exceptions(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::collection_clear(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::collection_contains(System.IntPtr,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.NativeMethods::collection_insert(System.IntPtr,System.Int32,Mono.Value&)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::collection_insert_with_error_(System.IntPtr,System.Int32,Mono.Value&,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::collection_iterator_reset(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::collection_remove(System.IntPtr,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.NativeMethods::collection_remove_at(System.IntPtr,System.Int32)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::collection_remove_at_with_error_(System.IntPtr,System.Int32,Mono.MoonError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.NativeMethods::collection_set_value_at(System.IntPtr,System.Int32,Mono.Value&)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::collection_set_value_at_with_error_(System.IntPtr,System.Int32,Mono.Value&,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::control_apply_template(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.NativeMethods::dependency_object_set_marshalled_value(System.IntPtr,System.IntPtr,Mono.Value&)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::dependency_object_set_marshalled_value_with_error_(System.IntPtr,System.IntPtr,Mono.Value&,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::dependency_property_is_attached(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::dependency_property_is_nullable(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::dependency_property_is_read_only(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::downloader_request_get_response(System.IntPtr,Mono.NativeMethods/DownloaderResponseStartedDelegate,Mono.NativeMethods/DownloaderResponseAvailableDelegate,Mono.NativeMethods/DownloaderResponseFinishedDelegate,System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::downloader_request_is_aborted(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::managed_unzip_stream_to_stream(Mono.ManagedStreamCallbacks&,Mono.ManagedStreamCallbacks&,System.String)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::moon_window_get_transparent(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.NativeMethods::resource_dictionary_add(System.IntPtr,System.String,Mono.Value&)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::resource_dictionary_add_with_error_(System.IntPtr,System.String,Mono.Value&,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::resource_dictionary_clear(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::resource_dictionary_contains_key(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::resource_dictionary_remove(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::resource_dictionary_set(System.IntPtr,System.String,Mono.Value&)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::routed_event_args_get_handled(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.NativeMethods::storyboard_begin(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::storyboard_begin_with_error_(System.IntPtr,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::stroke_hit_test(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::surface_focus_element(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::surface_get_full_screen(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::surface_in_main_thread()

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::type_get_value_type(Mono.Kind)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::type_is_dependency_object(Mono.Kind)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::uielement_capture_mouse(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::uielement_update_layout(System.IntPtr)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::value_from_str(Mono.Kind,System.String,System.String,System.IntPtr&,System.Boolean)

# p/invoke declaration
+SC-M: System.Boolean Mono.NativeMethods::value_from_str_with_typename(System.String,System.String,System.String,System.IntPtr&,System.Boolean)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Stream_CanRead::Invoke(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Stream_CanSeek::Invoke(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.StreamWrapper::CanRead(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.StreamWrapper::CanSeek(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.LookupObjectCallback::Invoke(System.IntPtr,System.IntPtr,System.String,System.String,System.Boolean,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::cb_lookup_object(System.IntPtr,System.IntPtr,System.String,System.String,System.Boolean,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::cb_set_property(System.IntPtr,System.IntPtr,System.String,System.IntPtr,System.IntPtr,System.IntPtr,System.String,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::LookupComponentFromName(System.IntPtr,System.String,System.Boolean,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::LookupObject(System.IntPtr,System.String,System.String,System.Boolean,Mono.Value&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::SetProperty(System.IntPtr,System.IntPtr,System.String,System.IntPtr,System.IntPtr,System.IntPtr,System.String,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::SetPropertyFromValue(System.IntPtr,System.IntPtr,System.Object,System.IntPtr,System.IntPtr,System.Reflection.PropertyInfo,System.IntPtr,System.IntPtr,System.String&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::TryGetDefaultAssemblyName(System.IntPtr,System.String&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::TrySetAttachedProperty(System.IntPtr,System.String,System.IntPtr,System.String,System.String,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::TrySetEventReflection(System.IntPtr,System.String,System.Object,System.String,System.String,System.IntPtr,System.String&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::TrySetExpression(System.IntPtr,System.Object,System.IntPtr,System.String,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.ManagedXamlLoader::TrySetPropertyReflection(System.IntPtr,System.IntPtr,System.String,System.Object,System.IntPtr,System.IntPtr,System.String,System.String,System.IntPtr,System.IntPtr,System.String&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean Mono.Xaml.SetPropertyCallback::Invoke(System.IntPtr,System.IntPtr,System.String,System.IntPtr,System.IntPtr,System.IntPtr,System.String,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Boolean System.Windows.Deployment::InitializeDeployment(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Double Mono.NativeMethods::column_definition_get_actual_width(System.IntPtr)

# p/invoke declaration
+SC-M: System.Double Mono.NativeMethods::row_definition_get_actual_height(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.ApplyDefaultStyleCallback::BeginInvoke(System.IntPtr,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.ApplyStyleCallback::BeginInvoke(System.IntPtr,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.AsyncResponseAvailableHandler::BeginInvoke(System.IntPtr,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.NativeMethods/DomEventCallback::BeginInvoke(System.IntPtr,System.String,System.Int32,System.Int32,System.Int32,System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Int32,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.NativeMethods/DownloaderResponseAvailableDelegate::BeginInvoke(System.IntPtr,System.IntPtr,System.IntPtr,System.UInt32,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.NativeMethods/DownloaderResponseFinishedDelegate::BeginInvoke(System.IntPtr,System.IntPtr,System.Boolean,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.NativeMethods/DownloaderResponseStartedDelegate::BeginInvoke(System.IntPtr,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.NativeMethods/GSourceFunc::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.NativeMethods/HeaderVisitor::BeginInvoke(System.IntPtr,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.NativeMethods/TickCallHandler::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.NativeMethods/UpdateFunction::BeginInvoke(System.Int32,System.IntPtr,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.NativePropertyChangedHandler::BeginInvoke(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr,Mono.MoonError&,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.PlainEvent::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.SetCustomXamlAttributeCallback::BeginInvoke(System.IntPtr,System.String,System.String,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.SetValueCallback::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Stream_CanRead::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Stream_CanSeek::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Stream_Length::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Stream_Position::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Stream_Read::BeginInvoke(System.IntPtr,System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Stream_Seek::BeginInvoke(System.IntPtr,System.Int64,System.IO.SeekOrigin,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Stream_Write::BeginInvoke(System.IntPtr,System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.UnmanagedEventHandler::BeginInvoke(System.IntPtr,System.IntPtr,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Xaml.GetContentPropertyNameCallback::BeginInvoke(System.IntPtr,Mono.Kind,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Xaml.ImportXamlNamespaceCallback::BeginInvoke(System.IntPtr,System.String,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Xaml.LookupObjectCallback::BeginInvoke(System.IntPtr,System.IntPtr,System.String,System.String,System.Boolean,Mono.Value&,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.Xaml.SetPropertyCallback::BeginInvoke(System.IntPtr,System.IntPtr,System.String,System.IntPtr,System.IntPtr,System.IntPtr,System.String,System.IntPtr,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult Mono.XamlHookupEventCallback::BeginInvoke(System.IntPtr,System.String,System.String,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Windows.Media.MediaStreamSource/CloseDemuxerDelegate::BeginInvoke(System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Windows.Media.MediaStreamSource/GetDiagnosticAsyncDelegate::BeginInvoke(System.IntPtr,System.Windows.Media.MediaStreamSourceDiagnosticKind,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Windows.Media.MediaStreamSource/GetFrameAsyncDelegate::BeginInvoke(System.IntPtr,System.Windows.Media.MediaStreamType,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Windows.Media.MediaStreamSource/OpenDemuxerAsyncDelegate::BeginInvoke(System.IntPtr,System.IntPtr,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Windows.Media.MediaStreamSource/SeekAsyncDelegate::BeginInvoke(System.IntPtr,System.Int64,System.AsyncCallback,System.Object)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IAsyncResult System.Windows.Media.MediaStreamSource/SwitchMediaStreamAsyncDelegate::BeginInvoke(System.IntPtr,System.Windows.Media.MediaStreamDescription,System.AsyncCallback,System.Object)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::binding_get_binding_mode(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 Mono.NativeMethods::collection_add(System.IntPtr,Mono.Value&)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::collection_add_with_error_(System.IntPtr,Mono.Value&,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::collection_changed_event_args_get_index(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::collection_get_count(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::collection_index_of(System.IntPtr,Mono.Value&)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::collection_iterator_next(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::downloader_response_get_response_status(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::event_object_add_handler(System.IntPtr,System.String,Mono.UnmanagedEventHandler,System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::event_object_add_xaml_handler(System.IntPtr,System.String,Mono.UnmanagedEventHandler,System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::external_demuxer_add_stream(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::key_event_args_get_key(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::key_event_args_get_platform_key_code(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::keyboard_get_modifiers()

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::multi_scale_tile_source_get_tile_height(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::multi_scale_tile_source_get_tile_overlap(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::multi_scale_tile_source_get_tile_width(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::plugin_instance_get_actual_height(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::plugin_instance_get_actual_width(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::storyboard_get_current_state(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int32 Mono.NativeMethods::time_manager_get_maximum_refresh_rate(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 Mono.Stream_Read::Invoke(System.IntPtr,System.Byte[],System.Int32,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int32 Mono.StreamWrapper::Read(System.IntPtr,System.Byte[],System.Int32,System.Int32)

# p/invoke declaration
+SC-M: System.Int64 Mono.NativeMethods::multi_scale_tile_source_get_image_height(System.IntPtr)

# p/invoke declaration
+SC-M: System.Int64 Mono.NativeMethods::multi_scale_tile_source_get_image_width(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int64 Mono.Stream_Length::Invoke(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int64 Mono.Stream_Position::Invoke(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int64 Mono.StreamWrapper::Length(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Int64 Mono.StreamWrapper::Position(System.IntPtr)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.CreateCustomXamlElementCallback::EndInvoke(System.IAsyncResult)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.CreateCustomXamlElementCallback::Invoke(System.String,System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.GetResourceCallback::EndInvoke(System.Int32&,System.IAsyncResult)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.GetResourceCallback::Invoke(System.String,System.Int32&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Helper::AllocHGlobal(System.Int32)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Helper::GCHandleToIntPtr(System.Runtime.InteropServices.GCHandle)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Helper::StreamToIntPtr(System.IO.Stream)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Hosting::GetNativeObject(System.Windows.DependencyObject)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.INativeDependencyObjectWrapper::get_NativeHandle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.MoonError::get_GCHandlePtr()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::application_get_current()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::application_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::arc_segment_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::assembly_part_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::assembly_part_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::audio_stream_new(System.IntPtr,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::begin_storyboard_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::bezier_segment_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_expression_base_get_binding(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_expression_base_get_converter(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_expression_base_get_converter_culture_(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_expression_base_get_converter_parameter(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_expression_base_get_source(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_expression_base_get_target(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_expression_base_get_target_property(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_expression_base_get_value(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_expression_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_get_property_path_(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::binding_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::bitmap_image_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::border_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::brush_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::canvas_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::collection_changed_event_args_get_new_item(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::collection_changed_event_args_get_old_item(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::collection_changed_event_args_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::collection_get_iterator(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr Mono.NativeMethods::collection_get_value_at(System.IntPtr,System.Int32)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::collection_get_value_at_with_error_(System.IntPtr,System.Int32,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::collection_iterator_get_current(System.IntPtr,System.Int32&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::color_animation_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::color_animation_using_key_frames_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::color_key_frame_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::color_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::column_definition_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::column_definition_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::content_changed_event_args_get_new_content(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::content_changed_event_args_get_old_content(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::content_control_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::control_get_template_child(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::control_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::control_template_new()

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr Mono.NativeMethods::data_template_load_content(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::data_template_load_content_with_error_(System.IntPtr,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::data_template_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::deep_zoom_image_tile_source_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_object_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_object_find_name(System.IntPtr,System.String,Mono.Kind&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_object_get_name_(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr Mono.NativeMethods::dependency_object_get_value(System.IntPtr,Mono.Kind,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr Mono.NativeMethods::dependency_object_get_value_no_default(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_object_get_value_no_default_with_error_(System.IntPtr,System.IntPtr,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_object_get_value_with_error_(System.IntPtr,Mono.Kind,System.IntPtr,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_object_new()

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr Mono.NativeMethods::dependency_object_read_local_value(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_object_read_local_value_with_error_(System.IntPtr,System.IntPtr,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_property_get_default_value(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_property_get_dependency_property(Mono.Kind,System.String)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_property_get_dependency_property_full(Mono.Kind,System.String,System.Boolean)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_property_get_name_(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dependency_property_register_managed_property(System.String,Mono.Kind,Mono.Kind,Mono.Value&,System.Boolean,System.Boolean,Mono.NativePropertyChangedHandler)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::deployment_get_current()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::deployment_get_types(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::deployment_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::discrete_color_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::discrete_double_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::discrete_object_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::discrete_point_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::dispatcher_timer_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::double_animation_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::double_animation_using_key_frames_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::double_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::double_key_frame_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::double_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::downloader_create_webrequest(System.IntPtr,System.String,System.String)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::downloader_get_response_text(System.IntPtr,System.String,System.Int64&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::downloader_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::downloader_response_get_response_status_text_(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::drawing_attributes_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::ellipse_geometry_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::ellipse_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::event_object_get_type_name_(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::event_trigger_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::framework_element_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::framework_template_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::general_transform_get_matrix(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::general_transform_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::geometry_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::geometry_group_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::geometry_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::glyphs_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::gradient_brush_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::gradient_stop_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::gradient_stop_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::grid_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::hit_test_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::html_object_attach_event(System.IntPtr,System.IntPtr,System.String,Mono.NativeMethods/DomEventCallback,System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::html_object_detach_event(System.IntPtr,System.String,System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::image_brush_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::image_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::imedia_object_get_media_reffed(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::ink_presenter_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::inline_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::inline_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::input_method_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::item_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::key_event_args_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::key_frame_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::key_spline_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::line_break_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::line_geometry_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::line_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::line_segment_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::linear_color_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::linear_double_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::linear_gradient_brush_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::linear_point_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::matrix_get_matrix_values(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::matrix_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::matrix_transform_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::media_attribute_collection_get_item_by_name(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::media_attribute_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::media_attribute_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::media_base_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::media_element_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::media_element_set_demuxer_source(System.IntPtr,System.IntPtr,System.Windows.Media.MediaStreamSource/CloseDemuxerDelegate,System.Windows.Media.MediaStreamSource/GetDiagnosticAsyncDelegate,System.Windows.Media.MediaStreamSource/GetFrameAsyncDelegate,System.Windows.Media.MediaStreamSource/OpenDemuxerAsyncDelegate,System.Windows.Media.MediaStreamSource/SeekAsyncDelegate,System.Windows.Media.MediaStreamSource/SwitchMediaStreamAsyncDelegate)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::media_frame_new(System.IntPtr,System.IntPtr,System.UInt32,System.UInt64)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::moon_window_gtk_get_widget(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::moon_window_gtk_new(System.Boolean,System.Int32,System.Int32,System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::mouse_event_args_get_stylus_points(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::mouse_event_args_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::multi_scale_image_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::multi_scale_sub_image_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::multi_scale_sub_image_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::multi_scale_tile_source_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::object_animation_using_key_frames_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::object_key_frame_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::object_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::panel_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::parallel_timeline_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::password_box_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::path_figure_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::path_figure_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::path_geometry_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::path_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::path_segment_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::path_segment_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::plugin_instance_evaluate(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::plugin_instance_get_host(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::plugin_instance_get_id_(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::plugin_instance_get_init_params(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::plugin_instance_get_source(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::plugin_instance_get_source_location(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::plugin_instance_get_surface(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::point_animation_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::point_animation_using_key_frames_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::point_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::point_key_frame_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::point_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::poly_bezier_segment_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::poly_line_segment_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::poly_quadratic_bezier_segment_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::polygon_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::polyline_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::popup_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::quadratic_bezier_segment_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::radial_gradient_brush_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::rectangle_geometry_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::rectangle_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::resource_dictionary_get(System.IntPtr,System.String,System.Boolean&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::resource_dictionary_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::rotate_transform_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::routed_event_args_get_source(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::routed_event_args_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::row_definition_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::row_definition_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::run_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::scale_transform_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::setter_base_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::setter_base_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::setter_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::shape_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::size_changed_event_args_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::skew_transform_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::solid_color_brush_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::spline_color_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::spline_double_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::spline_point_key_frame_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::storyboard_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::stroke_collection_hit_test(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::stroke_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::stroke_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::style_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::stylus_info_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::stylus_point_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::stylus_point_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::surface_create_downloader(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::surface_get_focused_element(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::surface_get_time_manager(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::surface_new(System.IntPtr,System.Boolean)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::text_block_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::text_box_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::text_box_view_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::text_changed_event_args_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::tile_brush_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::timeline_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::timeline_group_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::timeline_marker_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::timeline_marker_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::timeline_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::transform_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::transform_group_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::transform_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::translate_transform_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::trigger_action_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::trigger_action_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::trigger_base_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::trigger_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::types_find(System.IntPtr,Mono.Kind)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::types_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::uielement_collection_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::uielement_get_subtree_object(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::uielement_get_transform_to_uielement(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::uielement_get_visual_parent(System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::unmanaged_matrix_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::user_control_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::video_brush_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::video_stream_new(System.IntPtr,System.Int32,System.UInt32,System.UInt32,System.UInt64)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::visual_brush_new()

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::xaml_get_template_parent(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr Mono.NativeMethods::xaml_loader_create_from_file(System.IntPtr,System.String,System.Boolean,Mono.Kind&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::xaml_loader_create_from_file_with_error_(System.IntPtr,System.String,System.Boolean,Mono.Kind&,Mono.MoonError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr Mono.NativeMethods::xaml_loader_create_from_string(System.IntPtr,System.String,System.Boolean,Mono.Kind&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::xaml_loader_create_from_string_with_error_(System.IntPtr,System.String,System.Boolean,Mono.Kind&,Mono.MoonError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.IntPtr Mono.NativeMethods::xaml_loader_hydrate_from_string(System.IntPtr,System.String,System.IntPtr,System.Boolean,Mono.Kind&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::xaml_loader_hydrate_from_string_with_error_(System.IntPtr,System.String,System.IntPtr,System.Boolean,Mono.Kind&,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::xaml_loader_new(System.String,System.String,System.IntPtr)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::xaml_lookup_named_item(System.IntPtr,System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.IntPtr Mono.NativeMethods::xap_unpack_(System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Surface::get_Native()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Types::get_Native()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Value::StringToIntPtr(System.String)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Xaml.XamlLoader::CreateFromFile(System.String,System.Boolean,Mono.Kind&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Xaml.XamlLoader::CreateFromString(System.String,System.Boolean,Mono.Kind&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Xaml.XamlLoader::get_NativeLoader()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Xaml.XamlLoader::get_PluginHandle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Xaml.XamlLoader::get_PluginInDomain()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Xaml.XamlLoader::get_Surface()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr Mono.Xaml.XamlLoader::get_SurfaceInDomain()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.Application::get_NativeHandle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.Application::get_resource_cb(System.String,System.Int32&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.Application::get_resource_cb_safe(System.String,System.Int32&)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.Application::Mono.INativeDependencyObjectWrapper.get_NativeHandle()

# p/invoke declaration
+SC-M: System.IntPtr System.Windows.Controls.OpenFileDialog::open_file_dialog_show(System.String,System.Boolean,System.String,System.Int32)

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.Data.Binding::get_Native()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.DependencyObject::get_native()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.DependencyObject::Mono.INativeDependencyObjectWrapper.get_NativeHandle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.DependencyProperty::get_Native()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.Interop.PluginHost::get_Handle()

# using 'System.IntPtr' as return type
+SC-M: System.IntPtr System.Windows.Media.MediaStreamDescription::get_NativeStream()

# using 'System.IntPtr' as a parameter type
+SC-M: System.Object Mono.NativeDependencyObjectHelper::CreateObject(Mono.Kind,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Object Mono.Value::ToObject(System.Type,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Object Mono.Xaml.ManagedXamlLoader::LookupObject(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Runtime.InteropServices.GCHandle Mono.Helper::GCHandleFromIntPtr(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String Mono.Helper::PtrToStringAuto(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String Mono.NativeMethods::binding_expression_base_get_converter_culture(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String Mono.NativeMethods::binding_get_property_path(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String Mono.NativeMethods::dependency_object_get_name(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String Mono.NativeMethods::dependency_property_get_name(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String Mono.NativeMethods::downloader_response_get_response_status_text(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String Mono.NativeMethods::event_object_get_type_name(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String Mono.NativeMethods::plugin_instance_get_id(System.IntPtr)

# p/invoke declaration
+SC-M: System.String Mono.NativeMethods::xaml_get_element_key(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.String Mono.NativeMethods::xaml_uri_for_prefix(System.IntPtr,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String Mono.Xaml.GetContentPropertyNameCallback::Invoke(System.IntPtr,Mono.Kind)

# using 'System.IntPtr' as a parameter type
+SC-M: System.String Mono.Xaml.ManagedXamlLoader::cb_get_content_property_name(System.IntPtr,Mono.Kind)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Type Mono.Xaml.ManagedXamlLoader::LookupType(System.IntPtr,System.String,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Type Mono.Xaml.ManagedXamlLoader::TypeFromString(System.IntPtr,System.IntPtr,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.UInt32 Mono.NativeMethods/DownloaderResponseAvailableDelegate::Invoke(System.IntPtr,System.IntPtr,System.IntPtr,System.UInt32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.UInt32 Mono.NativeMethods/DownloaderResponseFinishedDelegate::Invoke(System.IntPtr,System.IntPtr,System.Boolean,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.UInt32 Mono.NativeMethods/DownloaderResponseStartedDelegate::Invoke(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.UInt32 Mono.NativeMethods::time_manager_add_tick_call(System.IntPtr,Mono.NativeMethods/TickCallHandler,System.IntPtr)

# p/invoke declaration
+SC-M: System.UInt32 Mono.NativeMethods::time_manager_add_timeout(System.IntPtr,System.Int32,Mono.NativeMethods/GSourceFunc,System.IntPtr)

# p/invoke declaration
+SC-M: System.UInt32 Mono.NativeMethods::time_manager_remove_tick_call(System.IntPtr,Mono.NativeMethods/TickCallHandler)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void Microsoft.Internal.IManagedFrameworkInternalHelper::SetContextEx(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Microsoft.Internal.TextBoxView::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.ApplicationLauncher::DestroyApplication(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.ApplyDefaultStyleCallback::Invoke(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.ApplyStyleCallback::Invoke(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.AsyncResponseAvailableHandler::Invoke(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.DispatcherTimer::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.DispatcherTimer::UnmanagedTick(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events/<CreateSafeHandler>c__AnonStorey0::<>m__1(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::binding_validation_error_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::current_state_changed_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::current_state_changing_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::got_focus_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::InitSurface(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::key_down_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::key_up_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::layout_updated_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::loaded_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::lost_focus_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::lost_mouse_capture_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::mouse_button_down_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::mouse_button_up_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::mouse_enter_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::mouse_leave_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::mouse_motion_notify_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::size_changed_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::surface_full_screen_changed_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::surface_resized_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Events::template_applied_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Helper::FreeHGlobal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Hosting::SurfaceAttach(System.IntPtr,System.Windows.Controls.Canvas)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.INativeDependencyObjectWrapper::set_NativeHandle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeDependencyObjectHelper::AddNativeMapping(System.IntPtr,Mono.INativeDependencyObjectWrapper)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods/DomEventCallback::Invoke(System.IntPtr,System.String,System.Int32,System.Int32,System.Int32,System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods/HeaderVisitor::Invoke(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods/TickCallHandler::Invoke(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods/UpdateFunction::Invoke(System.Int32,System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::application_register_callbacks(System.IntPtr,Mono.ApplyDefaultStyleCallback,Mono.ApplyStyleCallback,Mono.GetResourceCallback)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::application_set_current(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_expression_base_register_managed_overrides(System.IntPtr,Mono.GetValueCallback,Mono.SetValueCallback)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_expression_base_set_binding(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_expression_base_set_converter(System.IntPtr,Mono.Value&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_expression_base_set_converter_culture(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_expression_base_set_converter_parameter(System.IntPtr,Mono.Value&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_expression_base_set_source(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_expression_base_set_target(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_expression_base_set_target_property(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_expression_base_update_source(System.IntPtr,Mono.Value&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_set_binding_mode(System.IntPtr,System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_set_is_sealed(System.IntPtr,System.Boolean)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_set_notify_on_validation_error(System.IntPtr,System.Boolean)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_set_property_path(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::binding_set_validates_on_exceptions(System.IntPtr,System.Boolean)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::bitmap_image_set_buffer(System.IntPtr,System.IntPtr,System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::collection_changed_event_args_set_index(System.IntPtr,System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::collection_changed_event_args_set_new_item(System.IntPtr,Mono.Value&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::collection_changed_event_args_set_old_item(System.IntPtr,Mono.Value&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::collection_iterator_destroy(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::deep_zoom_image_tile_source_set_downloaded_cb(System.IntPtr,Mono.NativeMethods/DownloadedHandler)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods::dependency_object_clear_value(System.IntPtr,System.IntPtr,System.Boolean)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::dependency_object_clear_value_(System.IntPtr,System.IntPtr,System.Boolean,Mono.MoonError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods::dependency_object_set_logical_parent(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::dependency_object_set_logical_parent_(System.IntPtr,System.IntPtr,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::dependency_object_set_name(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::dependency_property_set_is_nullable(System.IntPtr,System.Boolean)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::dependency_property_set_property_changed_callback(System.IntPtr,Mono.NativePropertyChangedHandler)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::deployment_set_current(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::deployment_set_current_application(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::dispatcher_timer_start(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::dispatcher_timer_stop(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::downloader_abort(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::downloader_request_abort(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::downloader_request_free(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::downloader_request_set_body(System.IntPtr,System.Byte[],System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::downloader_request_set_http_header(System.IntPtr,System.String,System.String)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::downloader_response_abort(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::downloader_response_free(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::downloader_response_set_header_visitor(System.IntPtr,Mono.NativeMethods/HeaderVisitor)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::downloader_send(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::downloader_want_events(System.IntPtr,Mono.NativeMethods/UpdateFunction,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::event_object_ref(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::event_object_remove_handler(System.IntPtr,System.String,Mono.UnmanagedEventHandler,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::event_object_set_object_type(System.IntPtr,Mono.Kind)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::event_object_unref(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::external_demuxer_set_can_seek(System.IntPtr,System.Boolean)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::framework_element_register_managed_overrides(System.IntPtr,Mono.MeasureOverrideCallback,Mono.ArrangeOverrideCallback)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::framework_template_add_xaml_binding(System.IntPtr,System.IntPtr,System.String,System.String)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::general_transform_transform_point(System.IntPtr,System.Windows.Point&,System.Windows.Point&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::html_object_get_property(System.IntPtr,System.IntPtr,System.String,Mono.Value&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::html_object_invoke(System.IntPtr,System.IntPtr,System.String,Mono.Value[],System.Int32,Mono.Value&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::html_object_set_property(System.IntPtr,System.IntPtr,System.String,Mono.Value&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::image_set_source(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::imedia_demuxer_report_get_frame_completed(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::imedia_demuxer_report_get_frame_progress(System.IntPtr,System.Double)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::imedia_demuxer_report_open_demuxer_completed(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::imedia_demuxer_report_seek_completed(System.IntPtr,System.UInt64)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::imedia_demuxer_report_switch_media_stream_completed(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::item_collection_set_parent(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::media_base_set_source(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::media_element_pause(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::media_element_play(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::media_element_report_error_occurred(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::media_element_set_stream_source(System.IntPtr,Mono.ManagedStreamCallbacks&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::media_element_stop(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::moon_window_set_transparent(System.IntPtr,System.Boolean)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::mouse_event_args_get_position(System.IntPtr,System.IntPtr,System.Double&,System.Double&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::multi_scale_image_zoom_about_logical_point(System.IntPtr,System.Double,System.Double,System.Double)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::multi_scale_tile_source_set_image_height(System.IntPtr,System.Int64)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::multi_scale_tile_source_set_image_uri_func(System.IntPtr,Mono.NativeMethods/ImageUriFunc)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::multi_scale_tile_source_set_image_width(System.IntPtr,System.Int64)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::multi_scale_tile_source_set_tile_height(System.IntPtr,System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::multi_scale_tile_source_set_tile_overlap(System.IntPtr,System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::multi_scale_tile_source_set_tile_width(System.IntPtr,System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::plugin_instance_report_exception(System.IntPtr,System.String,System.String,System.String[],System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods::popup_set_active_surface(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::popup_set_active_surface_(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::routed_event_args_set_handled(System.IntPtr,System.Boolean)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::routed_event_args_set_source(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::runtime_init(System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::runtime_init_browser()

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::runtime_init_desktop()

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::runtime_shutdown()

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::size_changed_event_args_get_new_size(System.IntPtr,System.Windows.Size&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::size_changed_event_args_get_prev_size(System.IntPtr,System.Windows.Size&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods::storyboard_pause(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::storyboard_pause_with_error_(System.IntPtr,Mono.MoonError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods::storyboard_resume(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::storyboard_resume_with_error_(System.IntPtr,Mono.MoonError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods::storyboard_seek(System.IntPtr,System.Int64)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::storyboard_seek_with_error_(System.IntPtr,System.Int64,Mono.MoonError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativeMethods::storyboard_stop(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::storyboard_stop_with_error_(System.IntPtr,Mono.MoonError&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::stroke_collection_get_bounds(System.IntPtr,System.Windows.Rect&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::stroke_get_bounds(System.IntPtr,System.Windows.Rect&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::style_seal(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::surface_attach(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::surface_paint(System.IntPtr,System.IntPtr,System.Int32,System.Int32,System.Int32,System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::surface_resize(System.IntPtr,System.Int32,System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::surface_set_full_screen(System.IntPtr,System.Boolean)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::text_box_select(System.IntPtr,System.Int32,System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::text_box_select_all(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::time_manager_remove_timeout(System.IntPtr,System.UInt32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::time_manager_set_maximum_refresh_rate(System.IntPtr,System.Int32)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::timeline_set_manual_target(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::types_free(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::uielement_arrange(System.IntPtr,System.Windows.Rect)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::uielement_element_added(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::uielement_element_removed(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::uielement_find_elements_in_host_coordinates_p(System.IntPtr,System.Windows.Point,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::uielement_find_elements_in_host_coordinates_r(System.IntPtr,System.Windows.Rect,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::uielement_invalidate_arrange(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::uielement_invalidate_measure(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::uielement_measure(System.IntPtr,System.Windows.Size)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::uielement_release_mouse_capture(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::uielement_set_subtree_object(System.IntPtr,System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::value_free_value(Mono.Value&)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::xaml_loader_add_missing(System.IntPtr,System.String)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::xaml_loader_free(System.IntPtr)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::xaml_loader_set_callbacks(System.IntPtr,Mono.Xaml.XamlLoaderCallbacks)

# p/invoke declaration
+SC-M: System.Void Mono.NativeMethods::xaml_set_property_from_str(System.IntPtr,System.IntPtr,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.NativePropertyChangedHandler::Invoke(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr,Mono.MoonError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.PlainEvent::Invoke(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.SetCustomXamlAttributeCallback::Invoke(System.IntPtr,System.String,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.SetValueCallback::Invoke(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Stream_Seek::Invoke(System.IntPtr,System.Int64,System.IO.SeekOrigin)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Stream_Write::Invoke(System.IntPtr,System.Byte[],System.Int32,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.StreamWrapper::Seek(System.IntPtr,System.Int64,System.IO.SeekOrigin)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.StreamWrapper::Write(System.IntPtr,System.Byte[],System.Int32,System.Int32)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Surface::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Types::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.UnmanagedEventHandler::Invoke(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Xaml.ImportXamlNamespaceCallback::Invoke(System.IntPtr,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Xaml.ManagedXamlLoader::.ctor(System.Reflection.Assembly,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Xaml.ManagedXamlLoader::cb_import_xaml_xmlns(System.IntPtr,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Xaml.ManagedXamlLoader::Setup(System.IntPtr,System.IntPtr,System.IntPtr,System.String,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Xaml.MarkupExpressionParser::.ctor(System.Windows.DependencyObject,System.String,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Xaml.XamlLoader::.ctor(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Xaml.XamlLoader::Hydrate(System.IntPtr,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Xaml.XamlLoader::set_PluginInDomain(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Xaml.XamlLoader::set_SurfaceInDomain(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.Xaml.XamlLoader::Setup(System.IntPtr,System.IntPtr,System.IntPtr,System.String,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void Mono.XamlHookupEventCallback::Invoke(System.IntPtr,System.String,System.String)

# using 'System.Byte*' as a parameter type
+SC-M: System.Void System.IO.SimpleUnmanagedMemoryStream::.ctor(System.Byte*,System.Int64)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Application::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Application::apply_default_style_cb(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Application::apply_default_style_cb_safe(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Application::apply_style_cb(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Application::apply_style_cb_safe(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Application::Mono.INativeDependencyObjectWrapper.set_NativeHandle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Application::set_NativeHandle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.AssemblyPart::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.AssemblyPartCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.Border::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.Canvas::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.ColumnDefinition::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.ColumnDefinitionCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.ContentControl/ContentChangedEventArgs::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.ContentControl::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.ContentControl::content_changed_callback(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.Control::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.ControlTemplate::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.Grid::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.HitTestCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.Image::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.Image::image_failed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.InkPresenter::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.ItemCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MediaElement::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MediaElement::buffering_progress_changed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MediaElement::current_state_changed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MediaElement::download_progress_changed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MediaElement::InvokeMarkerReached(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MediaElement::marker_reached_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MediaElement::media_ended_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MediaElement::media_failed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MediaElement::media_opened_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MultiScaleImage::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MultiScaleImage::image_failed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MultiScaleImage::image_open_failed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MultiScaleImage::image_open_succeeded_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MultiScaleImage::motion_finished_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MultiScaleImage::viewport_changed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MultiScaleSubImage::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.MultiScaleSubImageCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.Panel::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.PasswordBox::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.PasswordBox::password_changed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.Primitives.Popup::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.Primitives.Popup::<Popup>m__1E(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.Primitives.Popup::<Popup>m__1F(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.RowDefinition::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.RowDefinitionCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.TextBlock::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.TextBox::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.TextBox::selection_changed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.TextBox::text_changed_cb(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.TextChangedEventArgs::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.UIElementCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Controls.UserControl::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.CustomDependencyProperty::.ctor(System.IntPtr,System.String,Mono.ManagedType,Mono.ManagedType,System.Windows.PropertyMetadata)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Data.Binding::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Data.Binding::set_Native(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.DataTemplate::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.DependencyObject::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.DependencyObject::Mono.INativeDependencyObjectWrapper.set_NativeHandle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.DependencyObject::set_native(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.DependencyProperty::.ctor(System.IntPtr,System.Type,System.Type,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.DependencyProperty::CustomNativePropertyChangedCallbackSafe(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr,Mono.MoonError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.DependencyProperty::NativePropertyChangedCallback(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.DependencyProperty::NativePropertyChangedCallbackSafe(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr,Mono.MoonError&)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Deployment::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Deployment::InitializePluginHost(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Documents.Glyphs::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Documents.Inline::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Documents.InlineCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Documents.LineBreak::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Documents.Run::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.EventTrigger::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.FrameworkElement::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.FrameworkTemplate::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Ink.DrawingAttributes::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Ink.Stroke::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Ink.StrokeCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Input.InputMethod::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Input.KeyEventArgs::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Input.MouseButtonEventArgs::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Input.MouseEventArgs::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Input.StylusPoint::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Input.StylusPointCollection::.ctor(System.IntPtr)

# [VISIBLE] using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Interop.HostingRenderTargetBitmap::.ctor(System.Int32,System.Int32,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Interop.PluginHost::SetPluginHandle(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.BeginStoryboard::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.ColorAnimation::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.ColorAnimationUsingKeyFrames::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.ColorKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.ColorKeyFrameCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.DiscreteColorKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.DiscreteDoubleKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.DiscreteObjectKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.DiscretePointKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.DoubleAnimation::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.DoubleAnimationUsingKeyFrames::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.DoubleKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.DoubleKeyFrameCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.KeySpline::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.LinearColorKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.LinearDoubleKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.LinearPointKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.ObjectAnimationUsingKeyFrames::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.ObjectKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.ObjectKeyFrameCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.PointAnimation::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.PointAnimationUsingKeyFrames::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.PointKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.PointKeyFrameCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.SplineColorKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.SplineDoubleKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.SplinePointKeyFrame::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.Storyboard::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.Timeline::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.Timeline::UnmanagedCompleted(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Animation.TimelineCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.ArcSegment::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.BezierSegment::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Brush::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.CompositionTarget::UnmanagedRendering(System.IntPtr,System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.DeepZoomImageTileSource::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.DoubleCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.EllipseGeometry::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.GeneralTransform::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Geometry::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.GeometryCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.GeometryGroup::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.GradientBrush::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.GradientStop::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.GradientStopCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.ImageBrush::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.ImageSource::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Imaging.BitmapImage::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.LinearGradientBrush::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.LineGeometry::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.LineSegment::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Matrix::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MatrixTransform::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaAttribute::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaAttributeCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamDescription::set_NativeStream(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource/CloseDemuxerDelegate::Invoke(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource/GetDiagnosticAsyncDelegate::Invoke(System.IntPtr,System.Windows.Media.MediaStreamSourceDiagnosticKind)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource/GetFrameAsyncDelegate::Invoke(System.IntPtr,System.Windows.Media.MediaStreamType)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource/OpenDemuxerAsyncDelegate::Invoke(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource/SeekAsyncDelegate::Invoke(System.IntPtr,System.Int64)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource/SwitchMediaStreamAsyncDelegate::Invoke(System.IntPtr,System.Windows.Media.MediaStreamDescription)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource::CloseMediaInternal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource::GetDiagnosticAsyncInternal(System.IntPtr,System.Windows.Media.MediaStreamSourceDiagnosticKind)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource::GetSampleAsyncInternal(System.IntPtr,System.Windows.Media.MediaStreamType)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource::OpenMediaAsyncInternal(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource::OpenMediaAsyncInternal(System.IntPtr,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource::SeekAsyncInternal(System.IntPtr,System.Int64)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MediaStreamSource::SwitchMediaStreamAsyncInternal(System.IntPtr,System.Windows.Media.MediaStreamDescription)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.MultiScaleTileSource::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.PathFigure::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.PathFigureCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.PathGeometry::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.PathSegment::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.PathSegmentCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.PointCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.PolyBezierSegment::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.PolyLineSegment::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.PolyQuadraticBezierSegment::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.QuadraticBezierSegment::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.RadialGradientBrush::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.RectangleGeometry::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.RotateTransform::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.ScaleTransform::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.SkewTransform::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.SolidColorBrush::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.TileBrush::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.TimelineMarker::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.TimelineMarkerCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.Transform::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.TransformCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.TransformGroup::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.TranslateTransform::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.UnmanagedMatrix::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Media.VideoBrush::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.PresentationFrameworkCollection`1/CollectionIterator::.ctor(System.Type,System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.PresentationFrameworkCollection`1/GenericCollectionIterator::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.PresentationFrameworkCollection`1::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.ResourceDictionary::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.RoutedEventArgs::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Setter::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.SetterBase::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.SetterBaseCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Shapes.Ellipse::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Shapes.Line::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Shapes.Path::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Shapes.Polygon::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Shapes.Polyline::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Shapes.Rectangle::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Shapes.Shape::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.SizeChangedEventArgs::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Style::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.Threading.Dispatcher::dispatcher_callback(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.TriggerAction::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.TriggerActionCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.TriggerBase::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.TriggerCollection::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.UIElement::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Void System.Windows.VisualStateChangedEventArgs::.ctor(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Windows.Controls.Canvas System.Windows.Controls.Canvas::FromPtr(System.IntPtr)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Windows.DependencyProperty Mono.Xaml.ManagedXamlLoader::DependencyPropertyFromString(System.IntPtr,System.IntPtr,System.Object,System.IntPtr,System.String)

# using 'System.IntPtr' as a parameter type
+SC-M: System.Windows.DependencyProperty System.Windows.DependencyProperty::Lookup(System.IntPtr)

# p/invoke declaration
+SC-M: System.Windows.Point Mono.NativeMethods::multi_scale_image_element_to_logical_point(System.IntPtr,System.Windows.Point)

# p/invoke declaration
+SC-M: System.Windows.Rect Mono.NativeMethods::geometry_get_bounds(System.IntPtr)

# p/invoke declaration
+SC-M: System.Windows.Size Mono.NativeMethods::framework_element_arrange_override(System.IntPtr,System.Windows.Size)

# p/invoke declaration
+SC-M: System.Windows.Size Mono.NativeMethods::framework_element_measure_override(System.IntPtr,System.Windows.Size)

# p/invoke declaration
+SC-M: System.Windows.Size Mono.NativeMethods::uielement_get_desired_size(System.IntPtr)

# p/invoke declaration
+SC-M: System.Windows.Size Mono.NativeMethods::uielement_get_render_size(System.IntPtr)

