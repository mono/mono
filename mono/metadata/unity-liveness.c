#include <config.h>
#include <glib.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/domain-internals.h>
#include <mono/metadata/metadata.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/object.h>
#include <mono/metadata/tabledefs.h>
#include <mono/utils/mono-error.h>

typedef struct _LivenessState LivenessState;

#define k_block_size (8 * 1024)
#define k_array_elements_per_block ((k_block_size - 3 * sizeof (void*)) / sizeof (gpointer))

typedef struct _custom_array_block custom_array_block;

typedef struct _custom_array_block {
	gpointer *next_item;
	custom_array_block *prev_block;
	custom_array_block *next_block;
	gpointer p_data[k_array_elements_per_block];
} custom_array_block;

typedef struct _custom_block_array_iterator custom_block_array_iterator;

typedef struct _custom_growable_block_array {
	custom_array_block *first_block;
	custom_array_block *current_block;
	custom_block_array_iterator *iterator;
} custom_growable_block_array;

typedef struct _custom_block_array_iterator {
	custom_growable_block_array *array;
	custom_array_block *current_block;
	gpointer *current_position;
} custom_block_array_iterator;


typedef void(*register_object_callback) (gpointer *arr, int size, void *callback_userdata);
typedef void(*WorldStateChanged) ();
typedef void *(*ReallocateArray) (void *ptr, int size, void *callback_userdata);

struct _LivenessState {
	custom_growable_block_array *all_objects;

	MonoClass *filter;

	custom_growable_block_array *process_array;
	guint initial_alloc_count;

	void *callback_userdata;

	register_object_callback filter_callback;
	ReallocateArray reallocateArray;
	guint traverse_depth; // track recursion. Prevent stack overflow by limiting recursion
};

custom_growable_block_array * block_array_create(LivenessState *state)
{
	custom_growable_block_array *array = g_new0(custom_growable_block_array, 1);
	array->current_block = state->reallocateArray(NULL, k_block_size, state->callback_userdata);
	array->current_block->prev_block = NULL;
	array->current_block->next_block = NULL;
	array->current_block->next_item = array->current_block->p_data;
	array->first_block = array->current_block;

	array->iterator = g_new0(custom_block_array_iterator, 1);
	array->iterator->array = array;
	array->iterator->current_block = array->first_block;
	array->iterator->current_position = array->first_block->p_data;
	return array;
}

gboolean block_array_is_empty(custom_growable_block_array *block_array)
{
	return block_array->first_block->next_item == block_array->first_block->p_data;
}

void block_array_push_back(custom_growable_block_array *block_array, gpointer value, LivenessState *state)
{
	if (block_array->current_block->next_item == block_array->current_block->p_data + k_array_elements_per_block) {
		custom_array_block* new_block = block_array->current_block->next_block;
		if (block_array->current_block->next_block == NULL)
		{
			new_block = state->reallocateArray(NULL, k_block_size, state->callback_userdata);
			new_block->next_block = NULL;
			new_block->prev_block = block_array->current_block;
			new_block->next_item = new_block->p_data;
			block_array->current_block->next_block = new_block;
		}
		block_array->current_block = new_block;
	}
	*block_array->current_block->next_item++ = value;
}

gpointer block_array_pop_back(custom_growable_block_array *block_array)
{
	if (block_array->current_block->next_item == block_array->current_block->p_data) {
		if (block_array->current_block->prev_block == NULL)
			return NULL;
		block_array->current_block = block_array->current_block->prev_block;
		block_array->current_block->next_item = block_array->current_block->p_data + k_array_elements_per_block;
	}
	return *--block_array->current_block->next_item;
}

void block_array_reset_iterator(custom_growable_block_array *array)
{
	array->iterator->current_block = array->first_block;
	array->iterator->current_position = array->first_block->p_data;
}

gpointer block_array_next(custom_growable_block_array *block_array)
{
	custom_block_array_iterator *iterator = block_array->iterator;
	if (iterator->current_position != iterator->current_block->next_item)
		return *iterator->current_position++;
	if (iterator->current_block->next_block == NULL)
		return NULL;
	iterator->current_block = iterator->current_block->next_block;
	iterator->current_position = iterator->current_block->p_data;
	if (iterator->current_position == iterator->current_block->next_item)
		return NULL;
	return *iterator->current_position++;
}

void block_array_clear(custom_growable_block_array *block_array)
{
	custom_array_block *block = block_array->first_block;
	while (block != NULL) {
		block->next_item = block->p_data;
		block = block->next_block;
	}
}

void block_array_destroy(custom_growable_block_array *block_array, LivenessState *state)
{
	custom_array_block *block = block_array->first_block;
	while (block != NULL) {
		void *data_block = block;
		block = block->next_block;
		state->reallocateArray(data_block, 0, state->callback_userdata);
	}
	g_free(block_array->iterator);
	g_free(block_array);
}

/* number of sub elements of an array to process before recursing
 * we take a depth first approach to use stack space rather than re-allocating
 * processing array which requires restarting world to ensure allocator lock is not held
*/
const int kArrayElementsPerChunk = 256;

/* how far we recurse processing array elements before we stop. Prevents stack overflow */
const int kMaxTraverseRecursionDepth = 128;

/* Liveness calculation */
MONO_API LivenessState * mono_unity_liveness_allocate_struct(MonoClass *filter, guint max_count, register_object_callback callback, void *callback_userdata, ReallocateArray reallocateArray);
MONO_API void mono_unity_liveness_stop_gc_world();
MONO_API void mono_unity_liveness_finalize(LivenessState *state);
MONO_API void mono_unity_liveness_start_gc_world();
MONO_API void mono_unity_liveness_free_struct(LivenessState *state);

MONO_API void mono_unity_liveness_calculation_from_root(MonoObject *root, LivenessState *state);
MONO_API void mono_unity_liveness_calculation_from_statics(LivenessState *state);
MONO_API void mono_unity_heap_validation_from_statics(LivenessState* state);

#define MARK_OBJ(obj)                                                       \
	do {                                                                    \
		(obj)->vtable = (MonoVTable *)(((gsize) (obj)->vtable) | (gsize)1); \
	} while (0)

#define CLEAR_OBJ(obj)                                                       \
	do {                                                                     \
		(obj)->vtable = (MonoVTable *)(((gsize) (obj)->vtable) & ~(gsize)1); \
	} while (0)

#define IS_MARKED(obj) \
	(((gsize) (obj)->vtable) & (gsize)1)

#define GET_VTABLE(obj) \
	((MonoVTable *)(((gsize) (obj)->vtable) & ~(gsize)1))

void mono_filter_objects(LivenessState *state);

void mono_reset_state(LivenessState *state)
{
	block_array_clear(state->process_array);
}

static gboolean should_process_value(MonoObject *val, MonoClass *filter)
{
	MonoClass *val_class = GET_VTABLE(val)->klass;
	if (filter &&
		!mono_class_has_parent(val_class, filter))
		return FALSE;

	return TRUE;
}

static void mono_traverse_array(MonoArray *array, LivenessState *state);
static void mono_traverse_object(MonoObject *object, LivenessState *state);
static void mono_validate_array(MonoArray *array, LivenessState *state);
static void mono_validate_object(MonoObject *object, LivenessState *state);
static void mono_traverse_gc_desc(MonoObject *object, LivenessState *state);
static void mono_traverse_objects(LivenessState *state);

static void mono_traverse_generic_object(MonoObject *object, LivenessState *state)
{
#ifdef HAVE_SGEN_GC
	gsize gc_desc = 0;
#else
	gsize gc_desc = (gsize)(GET_VTABLE(object)->gc_descr);
#endif

	if (gc_desc & (gsize)1)
		mono_traverse_gc_desc(object, state);
	else if (GET_VTABLE(object)->klass->rank)
		mono_traverse_array((MonoArray *)object, state);
	else
		mono_traverse_object(object, state);
}

static void mono_traverse_and_validate_generic_object(MonoObject *object, LivenessState *state)
{
#ifdef HAVE_SGEN_GC
	gsize gc_desc = 0;
#else
	gsize gc_desc = (gsize)(GET_VTABLE(object)->gc_descr);
#endif

	if (GET_VTABLE(object)->klass->rank)
		mono_validate_array((MonoArray*)object, state);
	else
		mono_validate_object(object, state);
}

static void validate_object_value(MonoObject *val, MonoType *storageType)
{
	if (val && storageType->type == MONO_TYPE_CLASS) {
		MonoClass *storageClass = storageType->data.klass;
		MonoClass *valClass = GET_VTABLE(val)->klass;
		if (mono_class_is_interface(storageClass)) {
			int found = 0;
			for (int i = 0; i < valClass->interface_offsets_count; ++i) {
				if (valClass->interfaces_packed[i] == storageClass) {
					found = TRUE;
					break;
				}
			}
			g_assert(found);
		}
		else {
			int res = mono_class_has_parent_fast(valClass, storageClass);
			g_assert(res);
		}
	}
}

static gboolean mono_add_process_object(MonoObject *object, LivenessState *state)
{
	if (object && !IS_MARKED(object)) {
		gboolean has_references = GET_VTABLE(object)->klass->has_references;
		if (has_references || should_process_value(object, state->filter)) {
			block_array_push_back(state->all_objects, object, state);
			MARK_OBJ(object);
		}
		// Check if klass has further references - if not skip adding
		if (has_references) {
			block_array_push_back(state->process_array, object, state);
			return TRUE;
		}
	}

	return FALSE;
}

MONO_API void mono_validate_object_pointer (MonoObject *object)
{
	if (object) {
		MonoVTable *vtable = NULL;
		MonoClass *klass = NULL;
		char *name = NULL;

		vtable = object->vtable;
		klass = vtable->klass;
		name = klass->name;

		g_assert(vtable);
		g_assert(klass);
		g_assert(name);
	}
}

MONO_API void mono_validate_string_pointer(MonoString *string)
{
	mono_validate_object_pointer(&string->object);
}

static gboolean mono_add_and_validate_object(MonoObject *object, LivenessState *state)
{
	if (object) {
		MonoVTable *vtable = NULL;
		MonoClass *klass = NULL;
		char *name = NULL;
		vtable = GET_VTABLE(object);
		klass = vtable->klass;
		name = klass->name;

		g_assert(vtable);
		g_assert(klass);
		g_assert(name);

		if (!IS_MARKED(object)) {
			gboolean has_references = vtable->klass->has_references;
			if (has_references || should_process_value(object, state->filter)) {
				block_array_push_back(state->all_objects, object, state);
				MARK_OBJ(object);
			}
			// Check if klass has further references - if not skip adding
			if (has_references) {
				block_array_push_back(state->process_array, object, state);
				return TRUE;
			}
		}
	}

	return FALSE;
}

static gboolean mono_field_can_contain_references(MonoClassField *field)
{
	if (MONO_TYPE_ISSTRUCT(field->type))
		return TRUE;
	if (field->type->attrs & FIELD_ATTRIBUTE_LITERAL)
		return FALSE;
	if (field->type->type == MONO_TYPE_STRING)
		return FALSE;
	return MONO_TYPE_IS_REFERENCE(field->type);
}

static gboolean mono_traverse_object_internal(MonoObject *object, gboolean isStruct, MonoClass *klass, LivenessState *state)
{
	guint32 i;
	MonoClassField *field;
	MonoClass *p;
	gboolean added_objects = FALSE;

	if (!isStruct && mono_class_has_parent_fast(klass, mono_defaults.real_proxy_class)) return FALSE;

	g_assert(object);

	// subtract the added offset for the vtable. This is added to the offset even though it is a struct
	if (isStruct)
		object--;

	for (p = klass; p != NULL; p = p->parent) {
		if (p->size_inited == 0)
			continue;
		for (i = 0; i < mono_class_get_field_count(p); i++) {
			field = &p->fields[i];
			if (field->type->attrs & FIELD_ATTRIBUTE_STATIC)
				continue;

			if (!mono_field_can_contain_references(field))
				continue;

			if (MONO_TYPE_ISSTRUCT(field->type)) {
				char *offseted = (char *)object;
				offseted += field->offset;
				if (field->type->type == MONO_TYPE_GENERICINST) {
					g_assert(field->type->data.generic_class->cached_class);
					added_objects |= mono_traverse_object_internal((MonoObject *)offseted, TRUE, field->type->data.generic_class->cached_class, state);
				}
				else
					added_objects |= mono_traverse_object_internal((MonoObject *)offseted, TRUE, field->type->data.klass, state);
				continue;
			}

			if (field->offset == -1) {
				g_assert_not_reached();
			}
			else {
				MonoObject *val = NULL;
				MonoVTable *vtable = NULL;
				mono_field_get_value_internal(object, field, &val);
				added_objects |= mono_add_process_object(val, state);
			}
		}
	}

	return added_objects;
}

static gboolean mono_validate_object_internal(MonoObject *object, gboolean isStruct, MonoClass *klass, LivenessState *state)
{
	int i;
	MonoClassField* field;
	MonoClass* p;
	gboolean added_objects = FALSE;

	if (!isStruct && mono_class_has_parent_fast(klass, mono_defaults.real_proxy_class)) return FALSE;

	g_assert(object);

	// subtract the added offset for the vtable. This is added to the offset even though it is a struct
	if (isStruct)
		object--;

	for (p = klass; p != NULL; p = p->parent) {
		if (p->size_inited == 0)
			continue;
		for (i = 0; i < mono_class_get_field_count(p); i++) {
			field = &p->fields[i];
			if (field->type->attrs & FIELD_ATTRIBUTE_STATIC)
				continue;

			if (!mono_field_can_contain_references(field))
				continue;

			if (MONO_TYPE_ISSTRUCT(field->type)) {
				char* offseted = (char*)object;
				offseted += field->offset;
				if (field->type->type == MONO_TYPE_GENERICINST)
				{
					g_assert (field->type->data.generic_class->cached_class);
					added_objects |= mono_validate_object_internal((MonoObject*)offseted, TRUE, field->type->data.generic_class->cached_class, state);
				}
				else
					added_objects |= mono_validate_object_internal((MonoObject*)offseted, TRUE, field->type->data.klass, state);
				continue;
			}

			if (field->offset == -1) {
				g_assert_not_reached();
			}
			else {
				MonoObject* val = NULL;
				MonoVTable* vtable = NULL;
				mono_field_get_value_internal(object, field, &val);
				added_objects |= mono_add_and_validate_object(val, state);
				validate_object_value(val, field->type);
			}
		}
	}

	return added_objects;
}

static void mono_traverse_object(MonoObject *object, LivenessState *state)
{
	mono_traverse_object_internal(object, FALSE, GET_VTABLE(object)->klass, state);
}

static void mono_validate_object(MonoObject *object, LivenessState *state)
{
	mono_validate_object_internal(object, FALSE, GET_VTABLE(object)->klass, state);
}

static void mono_traverse_gc_desc(MonoObject *object, LivenessState *state)
{
#define WORDSIZE ((int)sizeof (gsize) * 8)
	int i = 0;
	gsize mask = (gsize)(GET_VTABLE(object)->gc_descr);

	g_assert(mask & (gsize)1);

	for (i = 0; i < WORDSIZE - 2; i++) {
		gsize offset = ((gsize)1 << (WORDSIZE - 1 - i));
		if (mask & offset) {
			MonoObject *val = *(MonoObject **)(((char *)object) + i * sizeof(void *));
			mono_add_process_object(val, state);
		}
	}
}

static void mono_traverse_objects(LivenessState *state)
{
	int i = 0;
	MonoObject *object = NULL;

	state->traverse_depth++;
	while (!block_array_is_empty(state->process_array)) {
		object = block_array_pop_back(state->process_array);
		mono_traverse_generic_object(object, state);
	}
	state->traverse_depth--;
}

static void mono_traverse_and_validate_objects(LivenessState *state)
{
	int i = 0;
	MonoObject* object = NULL;

	state->traverse_depth++;
	while (!block_array_is_empty(state->process_array)) {
		object = block_array_pop_back(state->process_array);
		mono_traverse_and_validate_generic_object(object, state);
	}
	state->traverse_depth--;
}

static gboolean should_traverse_objects(size_t index, gint32 recursion_depth)
{
	// Add kArrayElementsPerChunk objects at a time and then traverse
	return ((index + 1) & (kArrayElementsPerChunk - 1)) == 0 &&
		recursion_depth < kMaxTraverseRecursionDepth;
}

static void mono_traverse_array(MonoArray *array, LivenessState *state)
{
	size_t i = 0;
	gboolean has_references;
	MonoObject *object = (MonoObject *)array;
	MonoClass *element_class;
	int32_t elementClassSize;
	size_t array_length;

	g_assert(object);

	element_class = GET_VTABLE(object)->klass->element_class;
	has_references = !m_class_is_valuetype(element_class);
	g_assert(element_class->size_inited != 0);

	for (i = 0; i < mono_class_get_field_count(element_class); i++) {
		has_references |= mono_field_can_contain_references(&element_class->fields[i]);
	}

	if (!has_references)
		return;

	array_length = mono_array_length_internal(array);
	if (element_class->valuetype) {
		size_t items_processed = 0;
		elementClassSize = mono_class_array_element_size(element_class);
		for (i = 0; i < array_length; i++) {
			MonoObject *object = (MonoObject *)mono_array_addr_with_size_internal(array, elementClassSize, i);
			if (mono_traverse_object_internal(object, 1, element_class, state))
				items_processed++;

			if (should_traverse_objects(items_processed, state->traverse_depth))
				mono_traverse_objects(state);
		}
	}
	else {
		size_t items_processed = 0;
		for (i = 0; i < array_length; i++) {
			MonoObject *val = mono_array_get(array, MonoObject *, i);
			if (mono_add_process_object(val, state))
				items_processed++;

			if (should_traverse_objects(items_processed, state->traverse_depth))
				mono_traverse_objects(state);
		}
	}
}

static void mono_validate_array(MonoArray *array, LivenessState *state)
{
	size_t i = 0;
	gboolean has_references;
	MonoObject *object = (MonoObject*)array;
	MonoClass *element_class;
	size_t elementClassSize;
	size_t array_length;

	g_assert(object);



	element_class = GET_VTABLE(object)->klass->element_class;
	has_references = !m_class_is_valuetype(element_class);
	g_assert(element_class->size_inited != 0);

	for (i = 0; i < mono_class_get_field_count(element_class); i++)	{
		has_references |= mono_field_can_contain_references(&element_class->fields[i]);
	}

	if (!has_references)
		return;

	array_length = mono_array_length_internal(array);
	if (element_class->valuetype) {
		size_t items_processed = 0;
		elementClassSize = mono_class_array_element_size(element_class);
		for (i = 0; i < array_length; i++) {
			MonoObject *object = (MonoObject*)mono_array_addr_with_size_internal(array, elementClassSize, i);
			if (mono_validate_object_internal(object, 1, element_class, state))
				items_processed++;

			if (should_traverse_objects(items_processed, state->traverse_depth))
				mono_traverse_and_validate_objects(state);
		}
	}
	else {
		size_t items_processed = 0;
		for (i = 0; i < array_length; i++) {
			MonoObject *val = mono_array_get(array, MonoObject*, i);
			if (mono_add_and_validate_object(val, state))
				items_processed++;

			validate_object_value(val, &element_class->_byval_arg);

			if (should_traverse_objects (items_processed, state->traverse_depth))
				mono_traverse_and_validate_objects(state);
		}
	}
}

void mono_filter_objects(LivenessState *state)
{
	gpointer filtered_objects[64];
	gint num_objects = 0;

	gpointer value = block_array_next(state->all_objects);
	while (value != NULL) {
		MonoObject *object = value;
		if (should_process_value(object, state->filter))
			filtered_objects[num_objects++] = object;
		if (num_objects == 64) {
			state->filter_callback(filtered_objects, 64, state->callback_userdata);
			num_objects = 0;
		}
		value = block_array_next(state->all_objects);
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
void mono_unity_liveness_calculation_from_statics(LivenessState *liveness_state)
{
	guint i, j;
	MonoDomain *domain = mono_domain_get();
	MonoMemoryManager* memory_manager = mono_domain_memory_manager(domain);

	mono_reset_state(liveness_state);

	for (i = 0; i < memory_manager->class_vtable_array->len; ++i) {
		MonoVTable *vtable = (MonoVTable *)g_ptr_array_index(memory_manager->class_vtable_array, i);
		MonoClass *klass = vtable->klass;
		MonoClassField *field;
		if (!klass)
			continue;
		if (!klass->has_static_refs)
			continue;
		if (klass->image == mono_defaults.corlib)
			continue;
		if (klass->size_inited == 0)
			continue;
		for (j = 0; j < mono_class_get_field_count(klass); j++) {
			field = &klass->fields[j];
			if (!(field->type->attrs & FIELD_ATTRIBUTE_STATIC))
				continue;
			if (!mono_field_can_contain_references(field))
				continue;
			// shortcut check for special statics
			if (field->offset == -1)
				continue;

			char *offseted = (char *)mono_vtable_get_static_field_data(vtable);
				offseted += field->offset;

			if (MONO_TYPE_ISSTRUCT(field->type)) {
				if (field->type->type == MONO_TYPE_GENERICINST) {
					g_assert(field->type->data.generic_class->cached_class);
					mono_traverse_object_internal((MonoObject *)offseted, TRUE, field->type->data.generic_class->cached_class, liveness_state);
				}
				else
					mono_traverse_object_internal((MonoObject *)offseted, TRUE, field->type->data.klass, liveness_state);
			}
			else {
				MonoObject* val = *(MonoObject**)offseted;
				if (val)
					mono_add_process_object(val, liveness_state);
			}
		}
	}
	mono_traverse_objects(liveness_state);
	//Filter objects and call callback to register found objects
	mono_filter_objects(liveness_state);
}

void gchandle_process(void *data, void *user_data)
{
	MonoObject *target = data;
	LivenessState *liveness_state = user_data;

	mono_add_and_validate_object(target, liveness_state);
}

#if HAVE_BOEHM_GC
extern void
mono_gc_strong_handle_foreach(GFunc func, gpointer user_data);
#endif

static void
foreach_thread_static_field (gpointer key, gpointer value, gpointer user_data)
{
	MonoClassField *field = key;
	guint32 offset = GPOINTER_TO_UINT(value);
	LivenessState *liveness_state = user_data;

	if (!mono_field_can_contain_references(field))
		return;

	if (MONO_TYPE_ISSTRUCT(field->type))
		return;

	MonoInternalThread *thread;

	thread = mono_thread_internal_current();

	gpointer data = mono_get_special_static_data_for_thread(thread, offset);

	MonoObject *val = *(MonoObject**)data;

	if (val) {
		mono_add_and_validate_object(val, liveness_state);
		validate_object_value(val, field->type);
	}
}

void mono_unity_heap_validation_from_statics(LivenessState *liveness_state)
{
	int i, j;
	MonoDomain *domain = mono_domain_get();
	MonoMemoryManager* memory_manager = mono_domain_memory_manager(domain);

	mono_reset_state(liveness_state);

#if HAVE_BOEHM_GC
	mono_gc_strong_handle_foreach(gchandle_process, liveness_state);
#else
	g_assert_not_reached();
#endif

	g_hash_table_foreach(domain->special_static_fields, foreach_thread_static_field, liveness_state);

	for (i = 0; i < memory_manager->class_vtable_array->len; ++i) {
		MonoVTable *vtable = (MonoVTable*)g_ptr_array_index(memory_manager->class_vtable_array, i);
		MonoClass *klass = vtable->klass;
		MonoClassField *field;
		if (!klass)
			continue;
		if (!klass->has_static_refs)
			continue;
		if (klass->image == mono_defaults.corlib)
			continue;
		if (klass->size_inited == 0)
			continue;
		for (j = 0; j < mono_class_get_field_count(klass); j++)	{
			field = &klass->fields[j];
			if (!(field->type->attrs & FIELD_ATTRIBUTE_STATIC))
				continue;
			if (!mono_field_can_contain_references(field))
				continue;
			// shortcut check for special statics
			if (field->offset == -1)
				continue;
			if (field->type->attrs & FIELD_ATTRIBUTE_LITERAL)
				continue;

			if (MONO_TYPE_ISSTRUCT(field->type)) {
				char* offseted = (char*)mono_vtable_get_static_field_data(vtable);
				offseted += field->offset;
				if (field->type->type == MONO_TYPE_GENERICINST) {
					g_assert(field->type->data.generic_class->cached_class);
					mono_validate_object_internal((MonoObject*)offseted, TRUE, field->type->data.generic_class->cached_class, liveness_state);
				}
				else {
					mono_validate_object_internal((MonoObject*)offseted, TRUE, field->type->data.klass, liveness_state);
				}
			}
			else {
				MonoObject* val = NULL;

				char* offseted = (char*)mono_vtable_get_static_field_data(vtable);
				offseted += field->offset;
				val = *((MonoObject**)offseted);

				if (val)
					mono_add_and_validate_object(val, liveness_state);
			}
		}
	}
	mono_traverse_and_validate_objects(liveness_state);
	//Filter objects and call callback to register found objects
	//mono_filter_objects (liveness_state);
}

/**
 * mono_unity_liveness_calculation_from_root:
 *
 * Returns an array of MonoObject* that are reachable from @root
 * in the current domain and derive from @filter (if not NULL).
 */
void mono_unity_liveness_calculation_from_root(MonoObject *root, LivenessState *liveness_state)
{
	mono_reset_state(liveness_state);

	block_array_push_back(liveness_state->process_array, root, liveness_state);

	mono_traverse_objects(liveness_state);

	//Filter objects and call callback to register found objects
	mono_filter_objects(liveness_state);
}

LivenessState * mono_unity_liveness_allocate_struct(MonoClass *filter, guint max_count, register_object_callback callback, void *callback_userdata, ReallocateArray reallocateArray)
{
	LivenessState *state = NULL;

	// construct liveness_state;
	// allocate memory for the following structs
	// all_objects: contains a list of all referenced objects to be able to clean the vtable bits after the traversal
	// process_array. array that contains the objcets that should be processed. this should run depth first to reduce memory usage
	// if all_objects run out of space, run through list, add objects that match the filter, clear bit in vtable and then clear the array.

	state = g_new0(LivenessState, 1);

	state->filter = filter;
	state->traverse_depth = 0;

	state->callback_userdata = callback_userdata;
	state->filter_callback = callback;
	state->reallocateArray = reallocateArray;

	state->all_objects = block_array_create(state);
	state->process_array = block_array_create(state);

	return state;
}

void mono_unity_liveness_finalize(LivenessState *state)
{
	block_array_reset_iterator(state->all_objects);
	gpointer it = block_array_next(state->all_objects);
	while (it != NULL) {
		MonoObject *object = it;
		CLEAR_OBJ(object);
		it = block_array_next(state->all_objects);
	}
}

void mono_unity_liveness_free_struct(LivenessState *state)
{
	//cleanup the liveness_state
	block_array_destroy(state->all_objects, state);
	block_array_destroy(state->process_array, state);
	g_free(state);
}
