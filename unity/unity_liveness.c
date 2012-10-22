#include <mono/metadata/object.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/metadata.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/domain-internals.h>

/* Liveness calculation */
void mono_unity_liveness_calculation_begin (void);
void mono_unity_liveness_calculation_end (void);
GPtrArray* mono_unity_liveness_calculation_from_root (MonoObject* root, MonoClass* filter);
GPtrArray* mono_unity_liveness_calculation_from_statics (MonoClass* filter);

static void mono_add_and_traverse_object (GQueue* queue, MonoClass* filter, GPtrArray* objects)
{
	int i = 0;
	MonoObject* object = NULL;

	while (object = g_queue_pop_head (queue))
	{
		MonoClassField *field;
		MonoClass* klass = NULL;
		MonoClass *p;

		g_assert (object);

		if (((gsize)object->vtable) & 1)
			continue;
		
		g_ptr_array_add (objects, object);

		klass = mono_object_class (object);
		
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
					MonoClass* field_class = mono_class_from_mono_type (field->type);
					if (filter && 
						!mono_class_is_assignable_from (filter, field_class) && 
						!mono_class_is_assignable_from (field_class, filter))
						continue;

					mono_field_get_value (object, field, &val);

					if (val)
					{
						MonoClass* val_class = NULL;
						if (((gsize)val->vtable) & 1)
							continue;

						val_class = mono_object_class (val);
						if (filter && 
							!mono_class_is_assignable_from (filter, val_class))
							continue;
						g_queue_push_tail (queue, val);
					}
				}
			}
		}

		object->vtable = ((gsize)object->vtable) | 1;
	}

	for (i = 0; i < objects->len; i++)
	{
		MonoObject* object = objects->pdata[i];
		object->vtable = ((gsize)object->vtable) ^ 1;
	}
}

/**
 * mono_unity_liveness_calculation_from_statics:
 *
 * Returns an array of MonoObject* that are reachable from the static roots
 * in the current domain and derive from @filter (if not NULL).
 */
GPtrArray* mono_unity_liveness_calculation_from_statics(MonoClass* filter)
{
	int i = 0;
	MonoDomain* domain = mono_domain_get();
	GPtrArray *objects = g_ptr_array_new ();
	GQueue* queue = g_queue_new ();

	int size = GPOINTER_TO_INT (domain->static_data_array [1]);

	objects = g_ptr_array_new ();
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
			if (!MONO_TYPE_IS_REFERENCE(field->type))
				continue;

			if (field->offset == -1) {
				g_assert_not_reached ();
			} else {
				MonoObject* val = NULL;
				MonoVTable *vtable = NULL;
				MonoClass* field_class = mono_class_from_mono_type (field->type);
				if (filter && 
					!mono_class_is_assignable_from (filter, field_class) && 
					!mono_class_is_assignable_from (field_class, filter))
					continue;

				vtable = mono_class_vtable (domain, klass);
				mono_field_static_get_value (vtable, field, &val);

				if (val)
				{
					MonoClass* val_class = mono_object_class (val);
					if (filter && 
						!mono_class_is_assignable_from (filter, val_class))
						continue;
					g_queue_push_tail (queue, val);
				}
			}
		}
	}
	
	mono_add_and_traverse_object (queue, filter, objects);

	g_queue_free (queue);
	
	return objects;
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

	if (filter_type)
		filter = mono_class_from_mono_type (filter_type->type);
	
	objects = mono_unity_liveness_calculation_from_statics (filter);

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
GPtrArray* mono_unity_liveness_calculation_from_root (MonoObject* root, MonoClass* filter)
{
	GPtrArray* objects = g_ptr_array_new ();
	GQueue* queue = g_queue_new ();

	g_queue_push_head (queue, root);
	/* GC_stop_world (); */
	mono_add_and_traverse_object (queue, filter, objects);
	/* GC_start_world (); */

	return objects;
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

	if (filter_type)
		filter = mono_class_from_mono_type (filter_type->type);
	
	objects = mono_unity_liveness_calculation_from_root (root, filter);

	res = mono_array_new (mono_domain_get (), filter ? filter: mono_defaults.object_class, objects->len);
	for (i = 0; i < objects->len; ++i) {
		MonoObject* o = g_ptr_array_index (objects, i);
		mono_array_setref (res, i, o);
	}

	g_ptr_array_free (objects, TRUE);

	return mono_gchandle_new (res, FALSE);
}