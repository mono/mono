// If you add functions to this file you also need to expose them in MonoBundle.exp
// Otherwise they wont be exported in the web plugin!
DO_API(void,mono_thread_suspend_all_other_threads, ())
DO_API(void,mono_thread_pool_cleanup, ())
DO_API(void,mono_threads_set_shutting_down, ())
DO_API(void,mono_runtime_set_shutting_down, ())
DO_API(gboolean,mono_domain_finalize, (MonoDomain *domain, int timeout))
DO_API(void,mono_runtime_cleanup, (MonoDomain *domain))
//DO_API(void,mono_print_thread_stats, ())
DO_API(MonoMethodDesc*,mono_method_desc_new, (const char *name, gboolean include_namespace))
DO_API(MonoMethod*,mono_method_desc_search_in_image, (MonoMethodDesc *desc, MonoImage *image))
DO_API(void,mono_verifier_set_mode,(MiniVerifierMode) )
DO_API(void,mono_security_set_mode,(MonoSecurityMode) )
DO_API(void,mono_add_internal_call,(const char *name, gconstpointer method))
DO_API(void,mono_jit_cleanup,(MonoDomain *domain))
DO_API(MonoDomain*,mono_jit_init,(const char *file))
DO_API(MonoDomain*,mono_jit_init_version,(const char *file, const char* runtime_version))
DO_API(int, mono_jit_exec, (MonoDomain *domain, MonoAssembly *assembly, int argc, char *argv[]))
DO_API(MonoClass *,mono_class_from_name,(MonoImage *image, const char* name_space, const char *name))
DO_API(MonoAssembly *,mono_domain_assembly_open,(MonoDomain *domain, const char *name))
DO_API(MonoDomain *, mono_domain_create_appdomain,(const char *domainname, const char* configfile))
DO_API(void, mono_domain_unload, (MonoDomain* domain))
DO_API(MonoObject*,mono_object_new,(MonoDomain *domain, MonoClass *klass))
DO_API(void,mono_runtime_object_init,(MonoObject *this_obj))
DO_API(MonoObject*,mono_runtime_invoke,(MonoMethod *method, void *obj, void **params, MonoObject **exc))
DO_API(void,mono_field_set_value,(MonoObject *obj, MonoClassField *field, void *value))
DO_API(void,mono_field_get_value,(MonoObject *obj, MonoClassField *field, void *value))
DO_API(int,mono_field_get_offset,(MonoClassField *field))
DO_API(MonoClassField*,mono_class_get_fields,(MonoClass* klass, gpointer *iter))
DO_API(MonoMethod*,mono_class_get_methods,(MonoClass* klass, gpointer *iter))
DO_API(MonoDomain*,mono_domain_get,())
DO_API(MonoDomain*,mono_get_root_domain,())
DO_API(gint32,mono_domain_get_id,(MonoDomain *domain))
DO_API(void,mono_assembly_foreach,(GFunc func, gpointer user_data))
DO_API(void,mono_image_close,(MonoImage *image))
DO_API(void,mono_unity_socket_security_enabled_set,(gboolean))
//DO_API(void,mono_set_unhandled_exception_handler,(void* function))
DO_API(const char*, mono_image_get_name, (MonoImage *image))
DO_API(MonoClass*, mono_get_object_class, ())
#if UNITY_WIN || UNITY_OSX || UNITY_ANDROID
DO_API(void,mono_set_signal_chaining, (bool))
#endif
DO_API(void, mono_set_commandline_arguments, (int, const char* argv[], const char*))
#if !UNITY_WIN /////// @TODO: ENABLE THIS ON ALL BUILDS OF MONO!
//DO_API(void*, mono_aot_get_method,(MonoDomain *domain, MonoMethod *method))
#endif
//DO_API(MonoMethod*, mono_marshal_get_xappdomain_invoke, (MonoMethod*))

DO_API(const char*,mono_field_get_name,(MonoClassField *field))
DO_API(MonoType*,mono_field_get_type,(MonoClassField *field))
DO_API(int,mono_type_get_type,(MonoType *type))
DO_API(const char*,mono_method_get_name,(MonoMethod *method))
DO_API(MonoImage*,mono_assembly_get_image,(MonoAssembly *assembly))
DO_API(MonoClass* ,mono_method_get_class,(MonoMethod *method))
DO_API(MonoClass*,mono_object_get_class,(MonoObject *obj))
DO_API(gboolean,mono_class_is_valuetype,(MonoClass *klass))
DO_API(guint32,mono_signature_get_param_count,(MonoMethodSignature *sig))
DO_API(char*,mono_string_to_utf8,(MonoString *string_obj))
DO_API(MonoString*,mono_string_new_wrapper,(const char* text))
DO_API(MonoClass*,mono_class_get_parent,(MonoClass *klass))
DO_API(const char*,mono_class_get_namespace,(MonoClass *klass))
DO_API(gboolean,mono_class_is_subclass_of,(MonoClass *klass, MonoClass *klassc,  gboolean check_interfaces))
DO_API(const char*,mono_class_get_name,(MonoClass *klass))
DO_API(char*,mono_type_get_name,(MonoType *type))
DO_API(MonoClass*,mono_type_get_class,(MonoType *type))
DO_API(MonoException *,mono_exception_from_name_msg,(MonoImage *image, const char *name_space, const char *name, const char *msg))
DO_API(void,mono_raise_exception,(MonoException *ex))
DO_API(MonoClass*,mono_get_exception_class,())
DO_API(MonoClass*,mono_get_array_class,())
DO_API(MonoClass*,mono_get_string_class,())
DO_API(MonoClass*,mono_get_int32_class,())
DO_API(MonoArray*,mono_array_new,(MonoDomain *domain, MonoClass *eclass, guint32 n))
//DO_API(MonoArray *, mono_array_new_specific, (MonoVTable *vtable, guint32 n))
DO_API(MonoArray*, mono_array_new_full, (MonoDomain *domain, MonoClass *array_class, guint32 *lengths, guint32 *lower_bounds))

DO_API(MonoClass *, mono_array_class_get, (MonoClass *eclass, guint32 rank))

DO_API(gint32,mono_class_array_element_size,(MonoClass *ac))
DO_API(MonoObject*, mono_type_get_object, (MonoDomain *domain, MonoType *type))

DO_API(MonoThread *,mono_thread_attach,(MonoDomain *domain))

DO_API(void, mono_thread_detach,(MonoThread *thread))
DO_API(MonoThread *,mono_thread_exit,())

DO_API(MonoThread *,mono_thread_current,(void))
DO_API(void,mono_thread_set_main,(MonoThread* thread))
DO_API(void,mono_set_find_plugin_callback,(gconstpointer method))
DO_API(void,mono_security_enable_core_clr, ())

typedef bool (*MonoCoreClrPlatformCB) (const char *image_name);
DO_API(bool,mono_security_set_core_clr_platform_callback, (MonoCoreClrPlatformCB))

DO_API(MonoRuntimeUnhandledExceptionPolicy, mono_runtime_unhandled_exception_policy_get, (void))
DO_API(void, mono_runtime_unhandled_exception_policy_set, (MonoRuntimeUnhandledExceptionPolicy policy))

#if UNITY_OSX	
//DO_API(void,SetNativeSigsegvHandler,(gconstpointer ptr))
#endif
#if UNITY_WIN
//DO_API(void,SetNativeSigsegvHandlerWin,(gconstpointer ptr))
// @TODO: move this out of windows specific
//DO_API(void,unity_mono_redirect_output,(const char* fout, const char* ferr))
//DO_API(void,unity_mono_close_output,())
#endif

DO_API(MonoClass*,mono_class_get_nesting_type,(MonoClass *klass))
DO_API(MonoVTable* ,mono_class_vtable,(MonoDomain *domain, MonoClass *klass))
DO_API(MonoReflectionMethod* ,mono_method_get_object,(MonoDomain *domain, MonoMethod *method, MonoClass *refclass))

DO_API(MonoMethodSignature* ,mono_method_signature,(MonoMethod *method))
DO_API(MonoType*,mono_signature_get_params,(MonoMethodSignature *sig, gpointer *iter))
DO_API(MonoType*,mono_signature_get_return_type,(MonoMethodSignature *sig))
DO_API(MonoType*,mono_class_get_type,(MonoClass *klass))
DO_API(void,mono_set_ignore_version_and_key_when_finding_assemblies_already_loaded,(gboolean value))
DO_API (void,mono_debug_init ,(int format))

#if !WEBPLUG
DO_API (void,mono_debug_open_image_from_memory,(MonoImage *image, const char *raw_contents, int size))
#endif
DO_API(guint32,mono_field_get_flags,(MonoClassField *field))
DO_API(MonoImage*,mono_image_open_from_data_full,(const void *data, guint32 data_len, gboolean need_copy, int *status, gboolean ref_only))
DO_API(MonoImage*,mono_image_open_from_data_with_name,(char *data, guint32 data_len, gboolean need_copy, int *status, gboolean refonly, const char *name))
DO_API(MonoAssembly *,mono_assembly_load_from,(MonoImage *image, const char*fname,  int *status))
DO_API(MonoObject *,mono_value_box,(MonoDomain *domain, MonoClass *klass, gpointer val))
DO_API(MonoImage*,mono_class_get_image,(MonoClass *klass))
DO_API(char,mono_signature_is_instance,(MonoMethodSignature *signature))
DO_API(MonoMethod*,mono_method_get_last_managed,())
DO_API(MonoClass*,mono_get_enum_class,())
DO_API(MonoType*,mono_class_get_byref_type,(MonoClass *klass))

DO_API (void,mono_field_static_get_value,(MonoVTable *vt, MonoClassField *field, void *value))
DO_API(void,mono_unity_set_embeddinghostname,(const char* name))
DO_API(void,mono_set_assemblies_path,(const char* name))

DO_API (guint32, mono_gchandle_new, (MonoObject *obj, gboolean pinned))
DO_API (MonoObject*, mono_gchandle_get_target, (guint32 gchandle))

DO_API (guint32, mono_gchandle_new_weakref, (MonoObject *obj, gboolean track_resurrection))

#if UNITY_EDITOR
DO_API (gboolean, mono_gchandle_is_in_domain, (guint32 gchandle, MonoDomain *domain))
#endif
DO_API (MonoObject*, mono_assembly_get_object, (MonoDomain *domain, MonoAssembly *assembly))

DO_API (void,mono_gchandle_free, (guint32 gchandle))

#if UNITY_EDITOR
DO_API (MonoObject*, mono_runtime_delegate_invoke, (MonoObject *delegate, void **params, MonoException **exc))
#endif

//DO_API (void,GC_free,(void* p))
//DO_API(void*,GC_malloc_uncollectable,(int size))
DO_API(MonoProperty*,mono_class_get_properties,(MonoClass* klass, gpointer *iter))
DO_API(MonoMethod*,mono_property_get_get_method,(MonoProperty *prop))
DO_API(MonoObject *,mono_object_new_alloc_specific,(MonoVTable *vtable))
DO_API(MonoObject *,mono_object_new_specific,(MonoVTable *vtable))

DO_API (void,mono_gc_collect,(int generation))
DO_API(int,mono_gc_max_generation,())

#if !UNITY_WIN /////// @TODO: ENABLE THIS ON ALL BUILDS OF MONO!
DO_API(gint64,mono_gc_get_used_size,())
DO_API(gint64,mono_gc_get_heap_size,())
#endif

DO_API(MonoAssembly*,mono_image_get_assembly,(MonoImage *image))
DO_API(MonoAssembly*,mono_assembly_open,(const char *filename, int *status))

DO_API(gboolean,mono_class_is_enum,(MonoClass *klass))
DO_API(gint32,mono_class_instance_size,(MonoClass *klass))
DO_API(guint32,mono_object_get_size,(MonoObject *obj))
DO_API(const char*,mono_image_get_filename,(MonoImage *image))
DO_API(MonoAssembly*,mono_assembly_load_from_full,(MonoImage *image, const char *fname,int *status,gboolean refonly))
#if USE_ANCIENT_MONO
DO_API(gboolean,mono_assembly_preload_references,(MonoImage *image))
#endif
DO_API(MonoClass*,mono_class_get_interfaces,(MonoClass* klass, gpointer *iter))
DO_API (void,mono_assembly_close,(MonoAssembly *assembly))
DO_API(MonoProperty*,mono_class_get_property_from_name,(MonoClass *klass, const char *name))
DO_API(MonoMethod*,mono_class_get_method_from_name,(MonoClass *klass, const char *name, int param_count))
DO_API(MonoClass*,mono_class_from_mono_type,(MonoType *image))

DO_API (gboolean,mono_domain_set,(MonoDomain *domain, gboolean force))
DO_API (void,mono_thread_push_appdomain_ref,(MonoDomain *domain))
DO_API (void,mono_thread_pop_appdomain_ref,())

DO_API (int, mono_runtime_exec_main, (MonoMethod *method, MonoArray *args, MonoObject **exc))

DO_API (MonoImage*,mono_get_corlib,())
DO_API (MonoClassField*, mono_class_get_field_from_name, (MonoClass *klass, const char *name))
DO_API (guint32, mono_class_get_flags, (MonoClass *klass))

DO_API(int, mono_parse_default_optimizations, (const char* p))
DO_API (void, mono_set_defaults, (int verbose_level, guint32 opts))

DO_API(void, mono_set_dirs, (const char *assembly_dir, const char *config_dir))
//DO_API(void,ves_icall_System_AppDomain_InternalUnload,(int domain_id))
//DO_API(MonoObject*,ves_icall_System_AppDomain_createDomain,(MonoString *friendly_name, MonoObject *setup))
DO_API(void,mono_jit_parse_options,(int argc, char * argv[]))
DO_API(gpointer, mono_object_unbox, (MonoObject* o))

DO_API(MonoObject*, mono_custom_attrs_get_attr,  (MonoCustomAttrInfo *ainfo, MonoClass *attr_klass))

DO_API(gboolean, mono_custom_attrs_has_attr, (MonoCustomAttrInfo *ainfo, MonoClass *attr_klass))
DO_API(MonoCustomAttrInfo*, mono_custom_attrs_from_field, (MonoClass *klass, MonoClassField *field))
DO_API(MonoCustomAttrInfo*, mono_custom_attrs_from_method, (MonoMethod *method))
DO_API(MonoCustomAttrInfo*, mono_custom_attrs_from_class, (MonoClass *klass))
DO_API(void, mono_custom_attrs_free, (MonoCustomAttrInfo* attr))

///@TODO add this as an optimization when upgrading mono, used by MonoStringNewLength:
/// mono_string_new_len      (MonoDomain *domain, const char *text, guint length);

// Profiler
#if ENABLE_MEMORY_PROFILER
typedef void (*MonoProfileFunc) (void *prof);
typedef void (*MonoProfileMethodFunc)   (void *prof, MonoMethod   *method);
typedef void (*MonoProfileExceptionFunc) (void *prof, MonoObject *object);
typedef void (*MonoProfileExceptionClauseFunc) (void *prof, MonoMethod *method, int clause_type, int clause_num);
typedef void (*MonoProfileGCFunc)         (void *prof, int event, int generation);
typedef void (*MonoProfileGCResizeFunc)   (void *prof, SInt64 new_size);
typedef void (*MonoProfileAllocFunc)      (void *prof, MonoObject *obj, MonoClass *klass);
typedef void (*MonoProfileStatCallChainFunc) (void *prof, int call_chain_depth, guchar **ip, void *context);
typedef void (*MonoProfileStatFunc)       (void *prof, guchar *ip, void *context);


DO_API(void, mono_profiler_install, (void *prof, MonoProfileFunc shutdown_callback))
DO_API(void, mono_profiler_set_events, (int events))
DO_API(void, mono_profiler_install_enter_leave, (MonoProfileMethodFunc enter, MonoProfileMethodFunc fleave))
DO_API(void, mono_profiler_install_gc, (MonoProfileGCFunc callback, MonoProfileGCResizeFunc heap_resize_callback))
DO_API(void, mono_profiler_install_allocation, (MonoProfileAllocFunc callback))
//DO_API(void, mono_gc_base_init, ())
//DO_API(void, mono_profiler_install_statistical, (MonoProfileStatFunc callback))
//DO_API(void, mono_profiler_install_statistical_call_chain, (MonoProfileStatCallChainFunc callback, int call_chain_depth))
//DO_API(void, mono_profiler_install_exception, (MonoProfileExceptionFunc throw_callback, MonoProfileMethodFunc exc_method_leave, MonoProfileExceptionClauseFunc clause_callback))
#endif

#if UNITY_IPHONE || UNITY_PEPPER // iPhone uses eglib which just calls free
DO_API(void, mono_aot_register_module, (void *aot_info))
#endif

// GLib functions
#if UNITY_IPHONE || UNITY_PEPPER // iPhone uses eglib which just calls free
#define g_free free
#else
DO_API(void,g_free,(void* p))
#endif
/*
#if UNITY_PS3 || UNITY_XENON || UNITY_ANDROID
static inline char   *g_strdup (const char *str) { if (str) {return strdup (str);} return NULL; }
#define g_mem_set_vtable(x)
#else
DO_API(char*,g_strdup,(const char *image))
DO_API(void,g_mem_set_vtable,(gpointer vtable))
#endif*/

#if UNITY_OSX
//DO_API(void,macosx_register_exception_handler,())

DO_API(void, mono_trace_set_level_string, (const char *value))
DO_API(void, mono_trace_set_mask_string, (const char *value))
#endif
 
#if UNITY_WII
DO_API(char*, g_strdup_d, (char const* str))

// memory and string functions
DO_API(void*, wii_g_malloc, ( size_t size ))
DO_API(void*, wii_g_calloc, ( size_t size, size_t elemSize ))
DO_API(void*, wii_g_realloc, (void *memblock, size_t size))
DO_API(void, wii_g_free, ( void* p ))

// specific allocations
DO_API(void*, wii_g_malloc_image, (size_t size))
DO_API(void, wii_g_free_image, (void* p))

DO_API(void*, wii_g_malloc0_handles, (size_t size))
DO_API(void, wii_g_free_handles, (void* p))

#define g_free                  wii_g_free
//#define g_malloc_d(x)	        wii_g_malloc(x)
//#define g_calloc_d(obj,size)	wii_g_calloc(obj,size)
#define g_free_d(x)				wii_g_free(x)
#define g_strdup_d(x)			g_strdup(x)

DO_API(void*, g_memdup, (void const* mem, unsigned int byte_size))
DO_API(char*, g_strdup, (const char *str))

#endif

#if UNITY_PEPPER
DO_API(void, mono_set_corlib_data, (void *data, size_t size))
DO_API(void, mono_runtime_set_no_exec, (gboolean val))
DO_API(void, mono_jit_set_aot_only, (gboolean val))
typedef MonoAssembly *(*MonoAssemblySearchFunc)         (char **aname, void* user_data);

DO_API(void, mono_install_assembly_postload_search_hook, (MonoAssemblySearchFunc func, gpointer user_data))
#endif

#if UNITY_PS3 || UNITY_XENON
DO_API(char*, g_strdup_d, (char const* str))
#define g_strdup_d(x)			g_strdup(x)
DO_API(void*, g_memdup, (void const* mem, unsigned int byte_size))
//DO_API(char*, g_strdup, (const char *str))

#endif

#undef DO_API
