#include <mono/metadata/object.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/metadata.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/domain-internals.h>

typedef struct _LivenessState LivenessState;

typedef struct _GPtrArray custom_growable_array;
#define array_at_index(array,index) (array)->pdata[(index)]

custom_growable_array* array_create_and_initialize (guint capacity)
{
	custom_growable_array* array = g_ptr_array_sized_new(capacity);
	array->len = 0;
	return array;
}

void array_destroy (custom_growable_array* array)
{
	g_ptr_array_free(array, TRUE);
}

void array_push_back(custom_growable_array* array, gpointer value)
{
	g_assert(!array_is_full(array));
	array->pdata[array->len] = value;
	array->len++;
}

gpointer array_pop_back(custom_growable_array* array)
{
	array->len--;
	return array->pdata[array->len];
}

gboolean array_is_full(custom_growable_array* array)
{
	return g_ptr_array_reserved_size(array) == array->len;
}

void array_clear(custom_growable_array* array)
{
	array->len = 0;
}

void array_grow(custom_growable_array* array)
{
	int oldlen = array->len;
	g_ptr_array_set_size(array, g_ptr_array_reserved_size(array)*2);
	array->len = oldlen;
}

typedef void (*register_object_callback)(gpointer* arr, int size, void* callback_userdata);
struct _LivenessState
{
	gint                first_index_in_all_objects;
	custom_growable_array* all_objects;

	MonoClass*          filter;

	custom_growable_array* process_array;
	guint               initial_alloc_count;

	void*               callback_userdata;

	register_object_callback filter_callback;
};

/* Liveness calculation */
LivenessState* mono_unity_liveness_calculation_begin (MonoClass* filter, guint max_count, register_object_callback callback, void* callback_userdata);
void           mono_unity_liveness_calculation_end (LivenessState* state);
void           mono_unity_liveness_calculation_from_root (MonoObject* root, LivenessState* state);
void           mono_unity_liveness_calculation_from_statics (LivenessState* state);

#define MARK_OBJ(obj) \
	do { \
		(obj)->vtable = ((gsize)(obj)->vtable) | 1; \
	} while (0)

#define CLEAR_OBJ(obj) \
	do { \
		(obj)->vtable = ((gsize)(obj)->vtable) & ~1; \
	} while (0)

#define IS_MARKED(obj) \
	(((gsize)(obj)->vtable) & 1)

#define GET_VTABLE(obj) \
	((MonoVTable*)(((gsize)(obj)->vtable) & ~1))


void mono_filter_objects(LivenessState* state);

void mono_reset_state(LivenessState* state)
{
	state->first_index_in_all_objects = state->all_objects->len;
	array_clear(state->process_array);
}

void array_safe_grow(LivenessState* state, custom_growable_array* array)
{
	// if all_objects run out of space, run through list
	// clear bit in vtable, start the world, reallocate, stop the world and continue
	int i;
	for (i = 0; i < state->all_objects->len; i++)
	{
		MonoObject* object = array_at_index(state->all_objects,i);
		CLEAR_OBJ(object);
	}
	GC_start_world ();
	array_grow(array);
	GC_stop_world ();
	for (i = 0; i < state->all_objects->len; i++)
	{
		MonoObject* object = array_at_index(state->all_objects,i);
		MARK_OBJ(object);
	}
}

static gboolean should_process_value (MonoObject* val, MonoClass* filter)
{
	MonoClass* val_class = GET_VTABLE(val)->klass;
	if (filter && 
		!mono_class_is_assignable_from (filter, val_class))
		return FALSE;

	return TRUE;
}

static void mono_traverse_array (MonoArray* array, LivenessState* state);
static void mono_traverse_object (MonoObject* object, LivenessState* state);
static void mono_traverse_gc_desc (MonoObject* object, LivenessState* state);
static void mono_traverse_objects (LivenessState* state);

static void mono_traverse_generic_object( MonoObject* object, LivenessState* state ) 
{
	int gc_desc;
	gc_desc = (int)GET_VTABLE(object)->gc_descr;

	if (gc_desc & 1)
		mono_traverse_gc_desc (object, state);
	else if (GET_VTABLE(object)->klass->rank)
		mono_traverse_array (object, state);
	else
		mono_traverse_object (object, state);
}


static void mono_add_process_object (MonoObject* object, LivenessState* state)
{
	if (object && !IS_MARKED(object))
	{
		// Check if klass has further references - if not skip adding
		if (GET_VTABLE(object)->klass->has_references)
		{
			if (array_is_full(state->all_objects))
				array_safe_grow(state, state->all_objects);
			array_push_back(state->all_objects, object);
			MARK_OBJ(object);

			if(array_is_full(state->process_array))
				array_safe_grow(state, state->process_array);
			array_push_back(state->process_array, object);
		}
	}
}

static gboolean mono_field_can_contain_references(MonoClassField* field)
{
	if (MONO_TYPE_ISSTRUCT(field->type))
		return TRUE;
	if (field->type->attrs & FIELD_ATTRIBUTE_LITERAL)
		return FALSE;
	if (field->type->type == MONO_TYPE_STRING)
		return FALSE;
	return MONO_TYPE_IS_REFERENCE(field->type);
}

static void mono_traverse_array (MonoArray* array, LivenessState* state)
{
	int i = 0;
	gboolean has_references;
	gpointer iter = NULL;
	MonoObject* object = (MonoObject*)array;
	MonoClass* element_class;
	MonoClassField *field;
	g_assert (object);

	element_class = GET_VTABLE(object)->klass->element_class;
	has_references = !mono_class_is_valuetype(element_class);
	while (field = mono_class_get_fields (element_class, &iter)) 
	{
		has_references |= mono_field_can_contain_references(field);
	}

	if (!has_references)
		return;

	for (i = 0; i < mono_array_length (array); i++)
	{
		MonoObject* val =  mono_array_get(array, MonoObject*, i);
		mono_add_process_object(val, state);
		// Add 64 objects at a time and then traverse
		if( ((i+1) & 63) == 0)
			mono_traverse_objects(state);
	}

}

static void mono_traverse_object_internal (MonoObject* object, gboolean isStruct, MonoClass* klass, LivenessState* state)
{
	MonoClassField *field;
	MonoClass *p;

	g_assert (object);
	
	// subtract the added offset for the vtable. This is added to the offset even though it is a struct
	if(isStruct)
		object--;

	for (p = klass; p != NULL; p = p->parent)
	{
		gpointer iter = NULL;
		while (field = mono_class_get_fields (p, &iter)) 
		{
			if (field->type->attrs & FIELD_ATTRIBUTE_STATIC)
				continue;

			if(!mono_field_can_contain_references(field))
				continue;

			if (MONO_TYPE_ISSTRUCT(field->type))
			{
				char* offseted = (char*)object;
				offseted += field->offset;
				if (field->type->type == MONO_TYPE_GENERICINST)
				{
					g_assert(field->type->data.generic_class->cached_class);
					mono_traverse_object_internal((MonoObject*)offseted, TRUE, field->type->data.generic_class->cached_class, state);
				}
				else
					mono_traverse_object_internal((MonoObject*)offseted, TRUE, field->type->data.klass, state);
				continue;
			}

			if (field->offset == -1) {
				g_assert_not_reached ();
			} else {
				MonoObject* val = NULL;
				MonoVTable *vtable = NULL;
				mono_field_get_value (object, field, &val);
				mono_add_process_object (val, state);
			}
		}
	}
}

static void mono_traverse_object (MonoObject* object, LivenessState* state)
{
	mono_traverse_object_internal (object, FALSE, GET_VTABLE(object)->klass, state);
}

static void mono_traverse_gc_desc (MonoObject* object, LivenessState* state)
{
	int i = 0;
	int mask = 0;
	mask = (int)GET_VTABLE(object)->gc_descr;

	g_assert (mask & 1);

	for (i = 0; i < 30; i++)
	{
		int offset = (1 << (31-i));
		if (mask & offset)
		{
			MonoObject* val = *(MonoObject**)(((char*)object) + i * sizeof(void*));
			mono_add_process_object(val, state);
		}
	}
}

static void mono_traverse_objects (LivenessState* state)
{
	int i = 0;
	MonoObject* object = NULL;

	while (state->process_array->len > 0)
	{
		object = array_pop_back(state->process_array);
		mono_traverse_generic_object(object, state);
	}
}

void mono_filter_objects(LivenessState* state)
{
	gpointer filtered_objects[64];
	gint num_objects = 0;

	int i = state->first_index_in_all_objects;
	for ( ; i < state->all_objects->len; i++)
	{
		MonoObject* object = state->all_objects->pdata[i];
		if (should_process_value (object, state->filter))
			filtered_objects[num_objects++] = object;
		if (num_objects == 64)
		{
			state->filter_callback(filtered_objects, 64, state->callback_userdata);
			num_objects = 0;
		}
	}

	if (num_objects != 0)
		state->filter_callback(filtered_objects, num_objects, state->callback_userdata);
}

/**
 * mono_unity_liveness_calculation_from_statics:
 *
 * Returns an array of MonoObject* that are reachable from the static roots
 * in the current domain and derive from @filter (if not NULL).
 */
void mono_unity_liveness_calculation_from_statics(LivenessState* liveness_state)
{
	int i = 0;
	MonoDomain* domain = mono_domain_get();
	int size = GPOINTER_TO_INT (domain->static_data_array [1]);

	mono_reset_state(liveness_state);

	for (i = 2; i < size; i++)
	{
		MonoClass* klass = domain->static_data_class_array[i];
		gpointer iter = NULL;
		MonoClassField *field;
		if (!klass)
			continue;
		if (klass->image == mono_defaults.corlib)
			continue;
		while (field = mono_class_get_fields (klass, &iter)) {
			if (!(field->type->attrs & FIELD_ATTRIBUTE_STATIC))
				continue;
			if(!mono_field_can_contain_references(field))
				continue;

			if (MONO_TYPE_ISSTRUCT(field->type))
			{
				char* offseted = (char*)mono_class_vtable (domain, klass)->data;
				offseted += field->offset;
				if (field->type->type == MONO_TYPE_GENERICINST)
				{
					g_assert(field->type->data.generic_class->cached_class);
					mono_traverse_object_internal((MonoObject*)offseted, TRUE, field->type->data.generic_class->cached_class, liveness_state);
				}
				else
				{
					mono_traverse_object_internal((MonoObject*)offseted, TRUE, field->type->data.klass, liveness_state);
				}
				continue;
			}

			if (field->offset == -1)
			{
				g_assert_not_reached ();
			}
			else
			{
				MonoObject* val = NULL;

				mono_field_static_get_value (mono_class_vtable (domain, klass), field, &val);

				if (val)
				{
					mono_add_process_object(val, liveness_state);
				}
			}
		}
	}
	
	mono_traverse_objects (liveness_state);

	//Filter objects and call callback to register found objects
	mono_filter_objects(liveness_state);
}

/**
 * mono_unity_liveness_calculation_from_statics_managed:
 *
 * Returns a gchandle to an array of MonoObject* that are reachable from the static roots
 * in the current domain and derive from type retrieved from @filter_handle (if not NULL).
 */
void mono_unity_liveness_add_object_callback(gpointer* objs, gint count, void* arr)
{
	int i;
	GPtrArray* objects = (GPtrArray*)arr;
	for (i = 0; i < count; i++)
	{
		if (g_ptr_array_reserved_size(objects) > objects->len)
			objects->pdata[objects->len++] = objs[i];
	}
}

gpointer mono_unity_liveness_calculation_from_statics_managed(gpointer filter_handle)
{
	int i = 0;
	MonoArray *res = NULL;
	MonoReflectionType* filter_type = mono_gchandle_get_target (GPOINTER_TO_UINT(filter_handle));
	MonoClass* filter = NULL;
	GPtrArray* objects = NULL;
	LivenessState* liveness_state = NULL;

	if (filter_type)
		filter = mono_class_from_mono_type (filter_type->type);

	objects = g_ptr_array_sized_new(1000);
	objects->len = 0;

	liveness_state = mono_unity_liveness_calculation_begin (filter, 1000, mono_unity_liveness_add_object_callback, (void*)objects);

	mono_unity_liveness_calculation_from_statics (liveness_state);

	mono_unity_liveness_calculation_end (liveness_state);

	res = mono_array_new (mono_domain_get (), filter ? filter: mono_defaults.object_class, objects->len);
	for (i = 0; i < objects->len; ++i) {
		MonoObject* o = g_ptr_array_index (objects, i);
		mono_array_setref (res, i, o);
	}
	g_ptr_array_free (objects, TRUE);

	
	return mono_gchandle_new (res, FALSE);

}

/**
 * mono_unity_liveness_calculation_from_root:
 *
 * Returns an array of MonoObject* that are reachable from @root
 * in the current domain and derive from @filter (if not NULL).
 */
void mono_unity_liveness_calculation_from_root (MonoObject* root, LivenessState* liveness_state)
{
	mono_reset_state (liveness_state);

	array_push_back (liveness_state->process_array,root);

	mono_traverse_objects (liveness_state);

	//Filter objects and call callback to register found objects
	mono_filter_objects (liveness_state);
}

/**
 * mono_unity_liveness_calculation_from_root_managed:
 *
 * Returns a gchandle to an array of MonoObject* that are reachable from the static roots
 * in the current domain and derive from type retrieved from @filter_handle (if not NULL).
 */
gpointer mono_unity_liveness_calculation_from_root_managed(gpointer root_handle, gpointer filter_handle)
{
	int i = 0;
	MonoArray *res = NULL;
	MonoReflectionType* filter_type = mono_gchandle_get_target (GPOINTER_TO_UINT(filter_handle));
	MonoObject* root = mono_gchandle_get_target (GPOINTER_TO_UINT(root_handle));
	MonoClass* filter = NULL;
	GPtrArray* objects = NULL;
	LivenessState* liveness_state = NULL;

	objects = g_ptr_array_sized_new(1000);
	objects->len = 0;

	if (filter_type)
		filter = mono_class_from_mono_type (filter_type->type);

	liveness_state = mono_unity_liveness_calculation_begin (filter, 1000, mono_unity_liveness_add_object_callback, (void*)objects);
	mono_unity_liveness_calculation_from_root (root, liveness_state);
	mono_unity_liveness_calculation_end (liveness_state);

	res = mono_array_new (mono_domain_get (), filter ? filter: mono_defaults.object_class, objects->len);
	for (i = 0; i < objects->len; ++i) {
		MonoObject* o = g_ptr_array_index (objects, i);
		mono_array_setref (res, i, o);
	}

	g_ptr_array_free (objects, TRUE);

	return mono_gchandle_new (res, FALSE);
}

LivenessState* mono_unity_liveness_calculation_begin (MonoClass* filter, guint max_count, register_object_callback callback, void* callback_userdata)
{
	LivenessState* state = NULL;
	
	// construct liveness_state;
	// allocate memory for the following structs
	// all_objects: contains a list of all referenced objects to be able to clean the vtable bits after the traversal
	// process_array. array that contains the objcets that should be processed. this should run depth first to reduce memory usage
	// if all_objects run out of space, run through list, add objects that match the filter, clear bit in vtable and then clear the array.
	
	state = g_new(LivenessState, 1);
	max_count = max_count < 1000 ? 1000 : max_count;
	state->all_objects = array_create_and_initialize(max_count*4);
	state->process_array = array_create_and_initialize (max_count);

	state->first_index_in_all_objects = 0; 
	state->filter = filter;

	state->callback_userdata = callback_userdata;
	state->filter_callback = callback;

	GC_stop_world ();
	// no allocations can happen beyond this point

	return state;
}

void mono_unity_liveness_calculation_end (LivenessState* state)
{
	int i;
	for (i = 0; i < state->all_objects->len; i++)
	{
		MonoObject* object = g_ptr_array_index(state->all_objects,i);
		CLEAR_OBJ(object);
	}
	GC_start_world ();

	//cleanup the liveness_state
	array_destroy(state->all_objects);
	array_destroy(state->process_array);
	g_free(state);
}
