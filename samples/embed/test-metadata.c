#include <mono/jit/jit.h>

/*
 * Very simple mono embedding example.
 * This sample shows how to access metadata elements from an image.
 * Compile with: 
 * 	gcc -o test-metadata test-metadata.c `pkg-config --cflags --libs mono` -lm
 * Run with:
 * 	./test-metadata namespace name
 */

void
output_desc (MonoClass* klass) {
	printf ("Class name: %s.%s\n", mono_class_get_namespace (klass), mono_class_get_name (klass));
	printf ("Rank: %d, flags 0x%0x\n", mono_class_get_rank (klass), mono_class_get_flags (klass));
}

void
output_ifaces (MonoClass* klass) {
	MonoClass *iface;
	gpointer iter = NULL;
	
	printf ("Implements: ");
	while ((iface = mono_class_get_interfaces (klass, &iter))) {
		printf ("%s ", mono_class_get_name (iface));
	}
	printf ("\n");
}

void
output_nested (MonoClass* klass) {
	MonoClass *nested;
	gpointer iter = NULL;
	
	printf ("Has nested types: ");
	while ((nested = mono_class_get_nested_types (klass, &iter))) {
		printf ("%s ", mono_class_get_name (nested));
	}
	printf ("\n");
}

void
output_fields (MonoClass* klass) {
	MonoClassField *field;
	gpointer iter = NULL;
	
	while ((field = mono_class_get_fields (klass, &iter))) {
		printf ("Field: %s, flags 0x%x\n", mono_field_get_name (field), 
				mono_field_get_flags (field));
	}
}

void
output_methods (MonoClass* klass) {
	MonoMethod *method;
	gpointer iter = NULL;
	
	while ((method = mono_class_get_methods (klass, &iter))) {
		guint32 flags, iflags;
		flags = mono_method_get_flags (method, &iflags);
		printf ("Method: %s, flags 0x%x, iflags 0x%x\n", 
				mono_method_get_name (method), flags, iflags);
	}
}

void
output_properties (MonoClass* klass) {
	MonoProperty *prop;
	gpointer iter = NULL;
	
	while ((prop = mono_class_get_properties (klass, &iter))) {
		printf ("Property: %s, flags 0x%x\n", mono_property_get_name (prop), 
				mono_property_get_flags (prop));
	}
}

void
output_events (MonoClass* klass) {
	MonoEvent *event;
	gpointer iter = NULL;
	
	while ((event = mono_class_get_events (klass, &iter))) {
		printf ("Event: %s, flags: 0x%x\n", mono_event_get_name (event), 
				mono_event_get_flags (event));
	}
}

int 
main (int argc, char* argv[]) {
	MonoDomain *domain;
	MonoClass *klass;
	MonoImage *image;
	const char *ns, *name;
	
	if (argc < 3){
		fprintf (stderr, "Please provide namespace and name of a type in mscorlib.\n");
		return 1;
	}
	ns = argv [1];
	name = argv [2];
	/*
	 * mono_jit_init() creates a domain: each assembly is
	 * loaded and run in a MonoDomain.
	 */
	domain = mono_jit_init (argv [0]);
	if (argc > 3) {
		MonoImageOpenStatus status;
		image = mono_image_open (argv [3], &status);
		if (!image) {
			fprintf (stderr, "Can't load assembly \"%s\".\n", argv [3]);
			return 1;
		}
	} else {
		/* we default to using mscorlib */
		image = mono_get_corlib ();
	}
	klass = mono_class_from_name (image, ns, name);
	if (!klass) {
		fprintf (stderr, "Class \"%s.%s\" not found.\n", ns, name);
		return 1;
	}
	output_desc (klass);
	output_ifaces (klass);
	output_nested (klass);
	output_fields (klass);
	output_methods (klass);
	output_properties (klass);
	output_events (klass);
	mono_jit_cleanup (domain);
	return 0;
}

