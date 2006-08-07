#include <glib.h>
#include <mono/jit/jit.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/profiler.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/assembly.h>
#include <string.h>

#define FIELD_ATTRIBUTE_STATIC 0x10

int
memory_usage (MonoObject *this, GHashTable *visited)
{
	int total = 0;
	MonoClass *class;
	gpointer iter = (gpointer) 0;
	MonoClassField *field;
	
	if (g_hash_table_lookup (visited, this))
		return total;

	class = mono_object_get_class (this);
	
	g_hash_table_insert (visited, this, this);

	while ((field = mono_class_get_fields (class, &iter)) != NULL){
		MonoType *ftype = mono_field_get_type (field);
		void *value;

		if ((ftype->attrs & FIELD_ATTRIBUTE_STATIC) != 0)
			continue;

		switch (ftype->type){
		case MONO_TYPE_CLASS: 
		case MONO_TYPE_OBJECT:
			mono_field_get_value (this, field, &value);

			if (value != NULL)
				total += memory_usage ((MonoObject *) value, visited);
			break;

		case MONO_TYPE_SZARRAY:
			printf ("implement me\n");
			break;
			
		case MONO_TYPE_I4:
		case MONO_TYPE_I1:
		case MONO_TYPE_I2:
		case MONO_TYPE_U4:
		case MONO_TYPE_U2:
		case MONO_TYPE_U1:
		case MONO_TYPE_VOID:
		case MONO_TYPE_BOOLEAN:
		case MONO_TYPE_CHAR:
			/* ignore */
			break;
		default:
			printf ("unhandled type: 0x%x\n", ftype->type);
		}
	}
	
	total += mono_class_instance_size (class);

	return total;
}

/*
 * Only returns data for instances, not for static fields, those might
 * be larger, or hold larger structures
 */
static int
GetMemoryUsage (MonoObject *this)
{
	GHashTable *visited = g_hash_table_new (NULL, NULL);
	int n;
	
	n = memory_usage (this, visited);

	g_hash_table_destroy (visited);
	
	return n;
}

static int installed = 0;

void install_icall (MonoProfiler *prof, MonoMethod *method, MonoJitInfo* jinfo, int result)
{
	if (installed)
		return;

	mono_add_internal_call ("Mono.ObjectServices.ObjectInspector::GetMemoryUsage", GetMemoryUsage);
	installed = 1;
}

void
mono_profiler_startup (const char *desc)
{
	mono_profiler_install_jit_end (install_icall);
	mono_profiler_set_events (MONO_PROFILE_JIT_COMPILATION);
}
