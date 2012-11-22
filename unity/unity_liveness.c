#include <mono/metadata/object.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/metadata.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/domain-internals.h>

typedef struct _LivenessState LivenessState;
struct _LivenessState
{
	int first_index_in_all_objects;
	GPtrArray *all_objects;
	
	MonoClass* filter;
	GPtrArray *filtered_objects;
	
	GPtrArray *process_array;
};

/* Liveness calculation */
LivenessState* mono_unity_liveness_calculation_begin (MonoClass* filter);
void           mono_unity_liveness_calculation_end (LivenessState* state);
GPtrArray*     mono_unity_liveness_calculation_from_root (MonoObject* root, LivenessState* state);
GPtrArray*     mono_unity_liveness_calculation_from_statics (LivenessState* state);

void mono_reset_state(LivenessState* state)
{
	state->first_index_in_all_objects = state->all_objects->len;
	state->filtered_objects->len = 0;
	state->process_array->len = 0;
}

/* TODO: Endian safe */
#define MARK_OBJ(obj) \
	do { \
		(obj)->vtable = ((gsize)(obj)->vtable) | 1; \
	} while (0)

#define CLEAR_OBJ(obj) \
	do { \
		(obj)->vtable = ((gsize)(obj)->vtable) ^ 1; \
	} while (0)

#define IS_MARKED(obj) \
	(((gsize)(obj)->vtable) & 1)

#define GET_VTABLE(obj) \
	((MonoVTable*)(((gsize)(obj)->vtable) & ~1))

static gboolean should_process_value (MonoObject* val, MonoClass* filter)
{
	MonoClass* val_class = GET_VTABLE(val)->klass;
	if (filter && 
		!mono_class_is_assignable_from (filter, val_class))
		return FALSE;

	return TRUE;
}

static void mono_add_process_object (MonoObject* object, LivenessState* state)
{
	if (object && !IS_MARKED(object))
	{
		g_ptr_array_add (state->all_objects, object);
		MARK_OBJ(object);

		// Check if we should add val to process_array
		if (GET_VTABLE(object)->klass->has_references)
			g_ptr_array_add(state->process_array, object);
	}
}

static void mono_traverse_array (MonoArray* array, LivenessState* state);
static void mono_traverse_object (MonoObject* object, LivenessState* state);

static void mono_traverse_array (MonoArray* array, LivenessState* state)
{
	int i = 0;
	MonoObject* object = (MonoObject*)array;
	MonoClass* element_class;
	g_assert (object);

	element_class = GET_VTABLE(object)->klass->element_class;

	//TODO: This might contain object references. handle correctly
	if (mono_class_is_valuetype (element_class))
		return;

	for (i = 0; i < mono_array_length (array); i++)
	{
		MonoObject* val =  mono_array_get(array, MonoObject*, i);
		mono_add_process_object(val, state);
	}

}

static void mono_traverse_object (MonoObject* object, LivenessState* state)
{
	MonoClassField *field;
	MonoClass* klass = NULL;
	MonoClass *p;

	g_assert (object);

	klass = GET_VTABLE(object)->klass;
	
	for (p = klass; p != NULL; p = p->parent) {
		gpointer iter = NULL;
		while (field = mono_class_get_fields (p, &iter)) 
		{
			if (field->type->attrs & FIELD_ATTRIBUTE_STATIC)
				continue;
			if (!MONO_TYPE_IS_REFERENCE(field->type))
				continue;

			if (field->offset == -1) {
				g_assert_not_reached ();
			} else {
				MonoObject* val = NULL;
				MonoVTable *vtable = NULL;
				if (field->type == MONO_TYPE_STRING)
					continue;

				mono_field_get_value (object, field, &val);
				mono_add_process_object (val, state);
			}
		}
	}
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
		int gc_desc = 0;
		object = g_ptr_array_remove_index(state->process_array,state->process_array->len-1);
	
		gc_desc = (int)GET_VTABLE(object)->gc_descr;

		if (gc_desc & 1)
			mono_traverse_gc_desc (object, state);
		else if (GET_VTABLE(object)->klass->rank)
			mono_traverse_array (object, state);
		else
			mono_traverse_object (object, state);
	}
}

void mono_filter_objects(LivenessState* state)
{
	int i = state->first_index_in_all_objects;
	for ( ; i < state->all_objects->len; i++)
	{
		MonoObject* object = state->all_objects->pdata[i];
		if (should_process_value (object, state->filter))
			g_ptr_array_add (state->filtered_objects, object);
	}
}

/**
 * mono_unity_liveness_calculation_from_statics:
 *
 * Returns an array of MonoObject* that are reachable from the static roots
 * in the current domain and derive from @filter (if not NULL).
 */
GPtrArray* mono_unity_liveness_calculation_from_statics(LivenessState* liveness_state)
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

			//TODO: We should handle value types as static variables (eg. struct with reference types)
			if (!MONO_TYPE_IS_REFERENCE(field->type))
				continue;

			if (field->offset == -1) {
				g_assert_not_reached ();
			} else {
				MonoObject* val = NULL;
				if (field->type->attrs & FIELD_ATTRIBUTE_LITERAL)
					continue;

				mono_field_static_get_value (mono_class_vtable (domain, klass), field, &val);

				if (val)
				{
					g_ptr_array_add(liveness_state->process_array, val);
//					g_queue_push_tail (liveness_state->queue, val);
				}
			}
		}
	}
	
	mono_traverse_objects (liveness_state);

	mono_filter_objects(liveness_state);

	return liveness_state->filtered_objects;
}

/**
 * mono_unity_liveness_calculation_from_statics_managed:
 *
 * Returns a gchandle to an array of MonoObject* that are reachable from the static roots
 * in the current domain and derive from type retrieved from @filter_handle (if not NULL).
 */
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

	liveness_state = mono_unity_liveness_calculation_begin (filter);

	objects = mono_unity_liveness_calculation_from_statics (liveness_state);

	res = mono_array_new (mono_domain_get (), filter ? filter: mono_defaults.object_class, objects->len);
	for (i = 0; i < objects->len; ++i) {
		MonoObject* o = g_ptr_array_index (objects, i);
		mono_array_setref (res, i, o);
	}

	g_ptr_array_free (objects, TRUE);

	mono_unity_liveness_calculation_end (liveness_state);

	return mono_gchandle_new (res, FALSE);

}

/**
 * mono_unity_liveness_calculation_from_root:
 *
 * Returns an array of MonoObject* that are reachable from @root
 * in the current domain and derive from @filter (if not NULL).
 */
GPtrArray* mono_unity_liveness_calculation_from_root (MonoObject* root, LivenessState* liveness_state)
{
	mono_reset_state(liveness_state);

	g_ptr_array_add(liveness_state->process_array,root);

	mono_traverse_objects (liveness_state);
	mono_filter_objects(liveness_state);

	return liveness_state->filtered_objects;
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

	if (filter_type)
		filter = mono_class_from_mono_type (filter_type->type);
	
	liveness_state = mono_unity_liveness_calculation_begin (filter);
	objects = mono_unity_liveness_calculation_from_root (root, liveness_state);

	res = mono_array_new (mono_domain_get (), filter ? filter: mono_defaults.object_class, objects->len);
	for (i = 0; i < objects->len; ++i) {
		MonoObject* o = g_ptr_array_index (objects, i);
		mono_array_setref (res, i, o);
	}

	g_ptr_array_free (objects, TRUE);

	mono_unity_liveness_calculation_end (liveness_state);

	return mono_gchandle_new (res, FALSE);
}


LivenessState* mono_unity_liveness_calculation_begin (MonoClass* filter)
{
	LivenessState* state = NULL;
	GC_stop_world ();
	//construct liveness_state;
	
	state = g_new(LivenessState, 1);
	state->all_objects = g_ptr_array_new ();
	state->first_index_in_all_objects = 0; 
	state->filter = filter;
	state->filtered_objects = g_ptr_array_new ();
	state->process_array = g_ptr_array_new ();

	return state;
}

void mono_unity_liveness_calculation_end (LivenessState* state)
{
	int i;
	for (i = 0; i < state->all_objects->len; i++)
	{
		MonoObject* object = state->all_objects->pdata[i];
		CLEAR_OBJ(object);
	}
	//cleanup the liveness_state
	g_ptr_array_free(state->all_objects, TRUE);
	g_ptr_array_free(state->filtered_objects, TRUE);
	g_ptr_array_free(state->process_array, TRUE);
	g_free(state);

	GC_start_world ();
}
