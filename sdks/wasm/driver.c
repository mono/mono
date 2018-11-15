#include <emscripten.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>

#include <mono/metadata/assembly.h>
#include <mono/jit/jit.h>
#include <mono/utils/mono-logger.h>

//JS funcs
extern MonoObject* mono_wasm_invoke_js_with_args (int js_handle, MonoString *method, MonoArray *args, int *is_exception);
extern MonoObject* mono_wasm_get_object_property (int js_handle, MonoString *method, int *is_exception);
extern MonoObject* mono_wasm_set_object_property (int js_handle, MonoString *method, MonoObject *value, int createIfNotExist, int hasOwnProperty, int *is_exception);
extern MonoObject* mono_wasm_get_global_object (MonoString *globalName, int *is_exception);

// Blazor specific custom routines - see dotnet_support.js for backing code
extern void* mono_wasm_invoke_js_marshalled (MonoString **exceptionMessage, void *asyncHandleLongPtr, MonoString *funcName, MonoString *argsJson);
extern void* mono_wasm_invoke_js_unmarshalled (MonoString **exceptionMessage, MonoString *funcName, void* arg0, void* arg1, void* arg2);

void mono_wasm_enable_debugging (void);

void mono_ee_interp_init (const char *opts);
void mono_marshal_ilgen_init (void);
void mono_method_builder_ilgen_init (void);
void mono_sgen_mono_ilgen_init (void);
void mono_icall_table_init (void);
void mono_aot_register_module (void **aot_info);
char *monoeg_g_getenv(const char *variable);
int monoeg_g_setenv(const char *variable, const char *value, int overwrite);
void mono_free (void*);

int mono_regression_test_step (int verbose_level, char *image, char *method_name);
void mono_trace_init (void);

static char*
m_strdup (const char *str)
{
	if (!str)
		return NULL;

	int len = strlen (str) + 1;
	char *res = malloc (len);
	memcpy (res, str, len);
	return res;
}

static MonoDomain *root_domain;

static MonoString*
mono_wasm_invoke_js (MonoString *str, int *is_exception)
{
	if (str == NULL)
		return NULL;

	char *native_val = mono_string_to_utf8 (str);
	mono_unichar2 *native_res = (mono_unichar2*)EM_ASM_INT ({
		var str = UTF8ToString ($0);
		try {
			var res = eval (str);
			if (res === null || res == undefined)
				return 0;
			res = res.toString ();
			setValue ($1, 0, "i32");
		} catch (e) {
			res = e.toString ();
			setValue ($1, 1, "i32");
			if (res === null || res === undefined)
				res = "unknown exception";
		}
		var buff = Module._malloc((res.length + 1) * 2);
		stringToUTF16 (res, buff, (res.length + 1) * 2);
		return buff;
	}, (int)native_val, is_exception);

	mono_free (native_val);

	if (native_res == NULL)
		return NULL;

	MonoString *res = mono_string_from_utf16 (native_res);
	free (native_res);
	return res;
}

static void
wasm_logger (const char *log_domain, const char *log_level, const char *message, mono_bool fatal, void *user_data)
{
	if (fatal) {
		EM_ASM(
			   var err = new Error();
			   console.log ("Stacktrace: \n");
			   console.log (err.stack);
			   );

		fprintf (stderr, "%s", message);

		abort ();
	} else {
		fprintf (stdout, "%s\n", message);
	}
}

#ifdef ENABLE_AOT
#include "driver-gen.c"
#endif

typedef struct WasmAssembly_ WasmAssembly;

struct WasmAssembly_ {
	MonoBundledAssembly assembly;
	WasmAssembly *next;
};

static WasmAssembly *assemblies;
static int assembly_count;

EMSCRIPTEN_KEEPALIVE void
mono_wasm_add_assembly (const char *name, const unsigned char *data, unsigned int size)
{
	int len = strlen (name);
	if (!strcasecmp (".pdb", &name [len - 4])) {
		char *new_name = m_strdup (name);
		//FIXME handle debugging assemblies with .exe extension
		strcpy (&new_name [len - 3], "dll");
		mono_register_symfile_for_assembly (new_name, data, size);
		return;
	}
	WasmAssembly *entry = (WasmAssembly *)malloc(sizeof (MonoBundledAssembly));
	entry->assembly.name = m_strdup (name);
	entry->assembly.data = data;
	entry->assembly.size = size;
	entry->next = assemblies;
	assemblies = entry;
	++assembly_count;
}

EMSCRIPTEN_KEEPALIVE void
mono_wasm_setenv (const char *name, const char *value)
{
	monoeg_g_setenv (strdup (name), strdup (value), 1);
}

EMSCRIPTEN_KEEPALIVE void
mono_wasm_load_runtime (const char *managed_path, int enable_debugging)
{
	monoeg_g_setenv ("MONO_LOG_LEVEL", "debug", 0);
	monoeg_g_setenv ("MONO_LOG_MASK", "gc", 0);

#ifdef ENABLE_AOT
	// Defined in driver-gen.c
	register_aot_modules ();
	mono_jit_set_aot_mode (MONO_AOT_MODE_LLVMONLY);
#else
	mono_jit_set_aot_mode (MONO_AOT_MODE_INTERP_LLVMONLY);
	if (enable_debugging)
		mono_wasm_enable_debugging ();
#endif

#ifndef ENABLE_AOT
	mono_icall_table_init ();
	mono_ee_interp_init ("");
	mono_marshal_ilgen_init ();
	mono_method_builder_ilgen_init ();
	mono_sgen_mono_ilgen_init ();
#endif

	if (assembly_count) {
		MonoBundledAssembly **bundle_array = (MonoBundledAssembly **)calloc (1, sizeof (MonoBundledAssembly*) * (assembly_count + 1));
		WasmAssembly *cur = assemblies;
		bundle_array [assembly_count] = NULL;
		int i = 0;
		while (cur) {
			bundle_array [i] = &cur->assembly;
			cur = cur->next;
			++i;
		}
		mono_register_bundled_assemblies ((const MonoBundledAssembly**)bundle_array);
	}

	mono_trace_init ();
	mono_trace_set_log_handler (wasm_logger, NULL);
	root_domain = mono_jit_init_version ("mono", "v4.0.30319");

	mono_add_internal_call ("WebAssembly.Runtime::InvokeJS", mono_wasm_invoke_js);
	mono_add_internal_call ("WebAssembly.Runtime::InvokeJSWithArgs", mono_wasm_invoke_js_with_args);
	mono_add_internal_call ("WebAssembly.Runtime::GetObjectProperty", mono_wasm_get_object_property);
	mono_add_internal_call ("WebAssembly.Runtime::SetObjectProperty", mono_wasm_set_object_property);
	mono_add_internal_call ("WebAssembly.Runtime::GetGlobalObject", mono_wasm_get_global_object);

	// Blazor specific custom routines - see dotnet_support.js for backing code		
	mono_add_internal_call ("WebAssembly.JSInterop.InternalCalls::InvokeJSMarshalled", mono_wasm_invoke_js_marshalled);
	mono_add_internal_call ("WebAssembly.JSInterop.InternalCalls::InvokeJSUnmarshalled", mono_wasm_invoke_js_unmarshalled);

}

EMSCRIPTEN_KEEPALIVE MonoAssembly*
mono_wasm_assembly_load (const char *name)
{
	MonoImageOpenStatus status;
	MonoAssemblyName* aname = mono_assembly_name_new (name);
	if (!name)
		return NULL;

	MonoAssembly *res = mono_assembly_load (aname, NULL, &status);
	mono_assembly_name_free (aname);

	return res;
}

EMSCRIPTEN_KEEPALIVE MonoClass*
mono_wasm_assembly_find_class (MonoAssembly *assembly, const char *namespace, const char *name)
{
	return mono_class_from_name (mono_assembly_get_image (assembly), namespace, name);
}

EMSCRIPTEN_KEEPALIVE MonoMethod*
mono_wasm_assembly_find_method (MonoClass *klass, const char *name, int arguments)
{
	return mono_class_get_method_from_name (klass, name, arguments);
}

EMSCRIPTEN_KEEPALIVE MonoObject*
mono_wasm_invoke_method (MonoMethod *method, MonoObject *this_arg, void *params[], int* got_exception)
{
	MonoObject *exc = NULL;
	MonoObject *res = mono_runtime_invoke (method, this_arg, params, &exc);
	*got_exception = 0;

	if (exc) {
		*got_exception = 1;

		MonoObject *exc2 = NULL;
		res = (MonoObject*)mono_object_to_string (exc, &exc2); 
		if (exc2)
			res = (MonoObject*) mono_string_new (root_domain, "Exception Double Fault");
		return res;
	}

	return res;
}

EMSCRIPTEN_KEEPALIVE MonoMethod*
mono_wasm_assembly_get_entry_point (MonoAssembly *assembly)
{
	MonoImage *image;
	MonoMethod *method;

	image = mono_assembly_get_image (assembly);
	uint32_t entry = mono_image_get_entry_point (image);
	if (!entry)
		return NULL;

	return mono_get_method (image, entry, NULL);
}

EMSCRIPTEN_KEEPALIVE char *
mono_wasm_string_get_utf8 (MonoString *str)
{
	return mono_string_to_utf8 (str); //XXX JS is responsible for freeing this
}

EMSCRIPTEN_KEEPALIVE MonoString *
mono_wasm_string_from_js (const char *str)
{
	return mono_string_new (root_domain, str);
}

static int
class_is_task (MonoClass *klass)
{
	if (!strcmp ("System.Threading.Tasks", mono_class_get_namespace (klass)) && 
		(!strcmp ("Task", mono_class_get_name (klass)) || !strcmp ("Task`1", mono_class_get_name (klass))))
		return 1;

	return 0;
}

#define MARSHAL_TYPE_INT 1
#define MARSHAL_TYPE_FP 2
#define MARSHAL_TYPE_STRING 3
#define MARSHAL_TYPE_VT 4
#define MARSHAL_TYPE_DELEGATE 5
#define MARSHAL_TYPE_TASK 6
#define MARSHAL_TYPE_OBJECT 7
#define MARSHAL_TYPE_BOOL 8
#define MARSHAL_TYPE_ENUM 9

// typed array marshalling
#define MARSHAL_ARRAY_BYTE 11
#define MARSHAL_ARRAY_UBYTE 12
#define MARSHAL_ARRAY_SHORT 13
#define MARSHAL_ARRAY_USHORT 14
#define MARSHAL_ARRAY_INT 15
#define MARSHAL_ARRAY_UINT 16
#define MARSHAL_ARRAY_FLOAT 17
#define MARSHAL_ARRAY_DOUBLE 18

EMSCRIPTEN_KEEPALIVE int
mono_wasm_get_obj_type (MonoObject *obj)
{
	if (!obj)
		return 0;
	MonoClass *klass = mono_object_get_class (obj);
	MonoType *type = mono_class_get_type (klass);

	switch (mono_type_get_type (type)) {
	// case MONO_TYPE_CHAR: prob should be done not as a number?
	case MONO_TYPE_BOOLEAN:
		return MARSHAL_TYPE_BOOL;
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		return MARSHAL_TYPE_INT;
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
		return MARSHAL_TYPE_FP;
	case MONO_TYPE_STRING:
		return MARSHAL_TYPE_STRING;
	case MONO_TYPE_SZARRAY:  { // simple zero based one-dim-array
		MonoClass *eklass = mono_class_get_element_class(klass);
		MonoType *etype = mono_class_get_type (eklass);

		switch (mono_type_get_type (etype)) {
			case MONO_TYPE_U1:
				return MARSHAL_ARRAY_UBYTE;
			case MONO_TYPE_I1:
				return MARSHAL_ARRAY_BYTE;
			case MONO_TYPE_U2:
				return MARSHAL_ARRAY_USHORT;			
			case MONO_TYPE_I2:
				return MARSHAL_ARRAY_SHORT;			
			case MONO_TYPE_U4:
				return MARSHAL_ARRAY_UINT;			
			case MONO_TYPE_I4:
				return MARSHAL_ARRAY_INT;			
			case MONO_TYPE_R4:
				return MARSHAL_ARRAY_FLOAT;
			case MONO_TYPE_R8:
				return MARSHAL_ARRAY_DOUBLE;
			default:
				return MARSHAL_TYPE_OBJECT;
		}		
	}
	default:
		if (mono_class_is_enum (klass))
			return MARSHAL_TYPE_ENUM;
		if (!mono_type_is_reference (type)) //vt
			return MARSHAL_TYPE_VT;
		if (mono_class_is_delegate (klass))
			return MARSHAL_TYPE_DELEGATE;
		if (class_is_task(klass))
			return MARSHAL_TYPE_TASK;

		return MARSHAL_TYPE_OBJECT;
	}
}


EMSCRIPTEN_KEEPALIVE int
mono_unbox_int (MonoObject *obj)
{
	if (!obj)
		return 0;
	MonoType *type = mono_class_get_type (mono_object_get_class(obj));

	void *ptr = mono_object_unbox (obj);
	switch (mono_type_get_type (type)) {
	case MONO_TYPE_I1:
	case MONO_TYPE_BOOLEAN:
		return *(signed char*)ptr;
	case MONO_TYPE_U1:
		return *(unsigned char*)ptr;
	case MONO_TYPE_I2:
		return *(short*)ptr;
	case MONO_TYPE_U2:
		return *(unsigned short*)ptr;
	case MONO_TYPE_I4:
		return *(int*)ptr;
	case MONO_TYPE_U4:
		return *(unsigned int*)ptr;
	// WASM doesn't support returning longs to JS
	// case MONO_TYPE_I8:
	// case MONO_TYPE_U8:
	default:
		printf ("Invalid type %d to mono_unbox_int\n", mono_type_get_type (type));
		return 0;
	}
}

EMSCRIPTEN_KEEPALIVE double
mono_wasm_unbox_float (MonoObject *obj)
{
	if (!obj)
		return 0;
	MonoType *type = mono_class_get_type (mono_object_get_class(obj));

	void *ptr = mono_object_unbox (obj);
	switch (mono_type_get_type (type)) {
	case MONO_TYPE_R4:
		return *(float*)ptr;
	case MONO_TYPE_R8:
		return *(double*)ptr;
	default:
		printf ("Invalid type %d to mono_wasm_unbox_float\n", mono_type_get_type (type));
		return 0;
	}
}

EMSCRIPTEN_KEEPALIVE int
mono_wasm_array_length (MonoArray *array)
{
	return mono_array_length (array);
}

EMSCRIPTEN_KEEPALIVE MonoObject*
mono_wasm_array_get (MonoArray *array, int idx)
{
	return mono_array_get (array, MonoObject*, idx);
}

EMSCRIPTEN_KEEPALIVE MonoArray*
mono_wasm_obj_array_new (int size)
{
	return mono_array_new (root_domain, mono_get_object_class (), size);
}

EMSCRIPTEN_KEEPALIVE void
mono_wasm_obj_array_set (MonoArray *array, int idx, MonoObject *obj)
{
	mono_array_setref (array, idx, obj);
}

EMSCRIPTEN_KEEPALIVE MonoArray*
mono_wasm_string_array_new (int size)
{
	return mono_array_new (root_domain, mono_get_string_class (), size);
}

// Int8Array 		| int8_t	| byte or SByte (signed byte)
// Uint8Array		| uint8_t	| byte or Byte (unsigned byte)
// Uint8ClampedArray| uint8_t	| byte or Byte (unsigned byte)
// Int16Array		| int16_t	| short (signed short)
// Uint16Array		| uint16_t	| ushort (unsigned short)
// Int32Array		| int32_t	| int (signed integer)
// Uint32Array		| uint32_t	| uint (unsigned integer)
// Float32Array		| float		| float
// Float64Array		| double	| double

EMSCRIPTEN_KEEPALIVE MonoArray*
mono_wasm_typed_array_new (char *arr, int length, int size, int type)
{
	MonoClass *typeClass = mono_get_byte_class(); // default is Byte
	switch (type) {
	case MARSHAL_ARRAY_BYTE:
		typeClass = mono_get_sbyte_class();
		break;
	case MARSHAL_ARRAY_SHORT:
		typeClass = mono_get_int16_class();
		break;
	case MARSHAL_ARRAY_USHORT:
		typeClass = mono_get_uint16_class();
		break;
	case MARSHAL_ARRAY_INT:
		typeClass = mono_get_int32_class();
		break;
	case MARSHAL_ARRAY_UINT:
		typeClass = mono_get_uint32_class();
		break;
	case MARSHAL_ARRAY_FLOAT:
		typeClass = mono_get_single_class();
		break;
	case MARSHAL_ARRAY_DOUBLE:
		typeClass = mono_get_double_class();
		break;
	}

	MonoArray *buffer;

	buffer = mono_array_new (root_domain, typeClass, length);
	memcpy(mono_array_addr_with_size(buffer, sizeof(char), 0), arr, length * size);

	return buffer;
}


EMSCRIPTEN_KEEPALIVE void
mono_wasm_array_to_heap (MonoArray *src, char *dest)
{
	int element_size;
	void *source_addr;
	int arr_length;

	element_size = mono_array_element_size ( mono_object_get_class((MonoObject*)src));
	//DBG("mono_wasm_to_heap element size %i  / length %i\n",element_size, mono_array_length(src));

	// get our src address
	source_addr = mono_array_addr_with_size (src, element_size, 0);
	// copy the array memory to heap via ptr dest
	memcpy (dest, source_addr, mono_array_length(src) * element_size);
}

EMSCRIPTEN_KEEPALIVE int
mono_wasm_exec_regression (int verbose_level, char *image)
{
	return mono_regression_test_step (verbose_level, image, NULL) ? 0 : 1;
}

EMSCRIPTEN_KEEPALIVE int
mono_wasm_unbox_enum (MonoObject *obj)
{
	if (!obj)
		return 0;
	
	MonoType *type = mono_class_get_type (mono_object_get_class(obj));

	void *ptr = mono_object_unbox (obj);
	switch (mono_type_get_type(mono_type_get_underlying_type (type))) {
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
		return *(unsigned char*)ptr;
	case MONO_TYPE_I2:
		return *(short*)ptr;
	case MONO_TYPE_U2:
		return *(unsigned short*)ptr;
	case MONO_TYPE_I4:
		return *(int*)ptr;
	case MONO_TYPE_U4:
		return *(unsigned int*)ptr;
	// WASM doesn't support returning longs to JS
	// case MONO_TYPE_I8:
	// case MONO_TYPE_U8:
	default:
		printf ("Invalid type %d to mono_unbox_enum\n", mono_type_get_type(mono_type_get_underlying_type (type)));
		return 0;
	}
}

EMSCRIPTEN_KEEPALIVE int
mono_wasm_exit (int exit_code)
{
	exit (exit_code);
}
