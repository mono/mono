#ifndef _TESTCASE_
#include <mono/jit/jit.h>
#endif

#include <mono/metadata/object.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-config.h>
#include <string.h>
#include <stdlib.h>

#ifndef FALSE
#define FALSE 0
#endif

/*
 * Simple mono embedding example.
 * We show how to create objects and invoke methods and set fields in them.
 * Compile with: 
 * 	gcc -Wall -o test-invoke test-invoke.c `pkg-config --cflags --libs mono-2` -lm
 * 	mcs -out:test-embed-invoke-cs.exe invoke.cs
 * Run with:
 * 	./test-invoke
 */

static void
access_valuetype_field (MonoObject *obj)
{
	MonoClass *klass;
	MonoClassField *field;
	int val;

	klass = mono_object_get_class (obj);

	/* Now we'll change the value of the 'val' field (see invoke.cs) */
	field = mono_class_get_field_from_name (klass, "val");

	/* This time we also add a bit of error checking... */
	if (!field) {
		fprintf (stderr, "Can't find field val in MyType\n");
		exit (1);
	}
	/* Check that val is an int (if you're paranoid or if you need to 
	 * show how this API is used) 
	 */
	if (mono_type_get_type (mono_field_get_type (field)) != MONO_TYPE_I4) {
		fprintf (stderr, "Field val is not a 32 bit integer\n");
		exit (1);
	}
	
	/* Note we pass a pointer to the value */
	mono_field_get_value (obj, field, &val);
	printf ("Value of field is: %d\n", val);
	val = 10;

	/* Note we pass a pointer to the value here as well */
	mono_field_set_value (obj, field, &val);

}

static void
access_reference_field (MonoObject *obj)
{
	MonoClass *klass;
	MonoDomain *domain;
	MonoClassField *str;
	MonoString *strval;
	char *p;

	klass = mono_object_get_class (obj);
	domain = mono_object_get_domain (obj);

	/* Now we'll see that a reference type is handled slightly differently.
	 * First, get the MonoClassField representing it.
	 */
	str = mono_class_get_field_from_name (klass, "str");
	
	/* No change here, we always pass a pointer */
	mono_field_get_value (obj, str, &strval);
	
	/* get the string in UTF-8 encoding to print it */
	p = mono_string_to_utf8 (strval);
	printf ("Value of str is: %s\n", p);
	/* we need to free the result from mono_string_to_utf8 () */
	mono_free (p);

	/* string are immutable, so we need to create a different string */
	strval = mono_string_new (domain, "hello from the embedding API");

	/* Here is the slight difference: for reference types we pass 
	 * the pointer directly, instead of a pointer to the value.
	 */
	mono_field_set_value (obj, str, strval);

}

/* Demostrate how to call methods */
static void
call_methods (MonoObject *obj)
{
	MonoClass *klass;
	MonoDomain *domain;
	MonoMethod *method = NULL, *m = NULL, *ctor = NULL, *fail = NULL, *mvalues;
	MonoProperty *prop;
	MonoObject *result, *exception;
	MonoString *str;
	char *p;
	void* iter;
	void* args [2];
	int val;

	klass = mono_object_get_class (obj);
	domain = mono_object_get_domain (obj);

	/* retrieve all the methods we need */
	iter = NULL;
	while ((m = mono_class_get_methods (klass, &iter))) {
		if (strcmp (mono_method_get_name (m), "method") == 0) {
			method = m;
		} else if (strcmp (mono_method_get_name (m), "Fail") == 0) {
			fail = m;
		} else if (strcmp (mono_method_get_name (m), "Values") == 0) {
			mvalues = m;
		} else if (strcmp (mono_method_get_name (m), ".ctor") == 0) {
			/* Check it's the ctor that takes two args:
			 * as you see a contrsuctor is a method like any other.
			 */
			MonoMethodSignature * sig = mono_method_signature (m);
			if (mono_signature_get_param_count (sig) == 2) {
				ctor = m;
			}
		}
	}
	/* Now we'll call method () on obj: since it takes no arguments 
	 * we can pass NULL as the third argument to mono_runtime_invoke ().
	 * The method will print the updated value.
	 */
	mono_runtime_invoke (method, obj, NULL, NULL);

	/* mono_object_new () doesn't call any constructor: this means that
	 * we'll have to invoke the constructor if needed ourselves. Note:
	 * invoking a constructor is no different than calling any other method,
	 * so we'll still call mono_runtime_invoke (). This also means that we 
	 * can invoke a constructor at any time, like now.
	 * First, setup the array of arguments and their values.
	 */

	/* As usual, we use the address of the data for valuetype arguments */
	val = 7;
	args [0] = &val;
	/* and the pointer for reference types: mono_array_new () returns a MonoArray* */
	args [1] = mono_array_new (domain, mono_get_byte_class (), 256);
	mono_runtime_invoke (ctor, obj, args, NULL);

	/* A property exists only as a metadata entity, so getting or setting the value
	 * is nothing more than calling mono_runtime_invoke () on the getter or setter method.
	 */
	prop = mono_class_get_property_from_name (klass, "Value");
	method = mono_property_get_get_method (prop);
	result = mono_runtime_invoke (method, obj, NULL, NULL);
	/* mono_runtime_invoke () always boxes the return value if it's a valuetype */
	val = *(int*)mono_object_unbox (result);
	
	printf ("Value of val from property is: %d\n", val);
	
	/* we also have an helper method: note that reference types are returned as is */
	prop = mono_class_get_property_from_name (klass, "Message");
	str = (MonoString*)mono_property_get_value (prop, obj, NULL, NULL);
	/* get the string in UTF-8 encoding to print it */
	p = mono_string_to_utf8 (str);
	printf ("Value of str from property is: %s\n", p);
	/* we need to free the result from mono_string_to_utf8 () */
	mono_free (p);

	/* Now we'll show two things:
	 * 1) static methods are invoked with mono_runtime_invoke () as well,
	 * we just pass NULL as the second argument.
	 * 2) we can catch exceptions thrown by the called method.
	 * Note: fail is declared as static void Fail () in invoke.cs.
	 * We first set result to NULL: if after the invocation it will have
	 * a different value, it will be the exception that was thrown from 
	 * the Fail () method. Note that if an exception was thrown, the return 
	 * value (if any) is undefined and can't be used in any way (yes, the above 
	 * invocations don't have this type of error checking to make things simpler).
	 */
	exception = NULL;
	mono_runtime_invoke (fail, NULL, NULL, &exception);
	if (exception) {
		printf ("An exception was thrown in Fail ()\n");
	}

	/* Now let's see how to handle methods that take by ref arguments:
	 * Valuetypes continue to be passed as pointers to the data.
	 * Reference arguments passed by ref (ref or out is the same)
	 * are handled the same way: a pointer to the pointer is used
	 * (so that the result can be read back).
	 * Small note: in this case (a System.Int32 valuetype) we can just
	 * use &val where val is a C 32 bit integer. In the general case 
	 * unmanaged code doesn't know the size of a valuetype, since the 
	 * runtime may decide to lay it out in what it thinks is a better way 
	 * (unless ExplicitLayout is set). To avoid issues, the best thing is to
	 * create an object of the valuetype's class and retrieve the pointer 
	 * to the data with the mono_object_unbox () function.
	 */
	val = 100;
	str = mono_string_new (domain, "another string");
	args [0] = &val;
	args [1] = &str;
	mono_runtime_invoke (mvalues, obj, args, NULL);
	/* get the string in UTF-8 encoding to print it */
	p = mono_string_to_utf8 (str);
	printf ("Values of str/val from Values () are: %s/%d\n", p, val);
	/* we need to free the result from mono_string_to_utf8 () */
	mono_free (p);
}

static void
more_methods (MonoDomain *domain)
{
	MonoClass *klass;
	MonoMethodDesc* mdesc;
	MonoMethod *method, *vtmethod;
	MonoString *str;
	MonoObject *obj;
	char *p;
	int val;

	/* Now let's call an instance method on a valuetype. There are two
	 * different case:
	 * 1) calling a virtual method defined in a base class, like ToString (): 
	 * we need to pass the value boxed in an object
	 * 2) calling a normal instance method: in this case
	 * we pass the address to the valuetype as the second argument 
	 * instead of an object.
	 * First some initialization.
	 */
	val = 25;
	klass = mono_get_int32_class ();
	obj = mono_value_box (domain, klass, &val);

	/* A different way to search for a method */
	mdesc = mono_method_desc_new (":ToString()", FALSE);
	vtmethod = mono_method_desc_search_in_class (mdesc, klass);

	str = (MonoString*)mono_runtime_invoke (vtmethod, &val, NULL, NULL);
	/* get the string in UTF-8 encoding to print it */
	p = mono_string_to_utf8 (str);
	printf ("25.ToString (): %s\n", p);
	/* we need to free the result from mono_string_to_utf8 () */
	mono_free (p);

	/* Now: see how the result is different if we search for the ToString ()
	 * method in System.Object: mono_runtime_invoke () doesn't do any sort of
	 * virtual method invocation: it calls the exact method that it was given 
	 * to execute. If a virtual call is needed, mono_object_get_virtual_method ()
	 * can be called.
	 */
	method = mono_method_desc_search_in_class (mdesc, mono_get_object_class ());
	str = (MonoString*)mono_runtime_invoke (method, obj, NULL, NULL);
	/* get the string in UTF-8 encoding to print it */
	p = mono_string_to_utf8 (str);
	printf ("25.ToString (), from System.Object: %s\n", p);
	/* we need to free the result from mono_string_to_utf8 () */
	mono_free (p);

	/* Now get the method that overrides ToString () in obj */
	vtmethod = mono_object_get_virtual_method (obj, method);
	if (mono_class_is_valuetype (mono_method_get_class (vtmethod))) {
		printf ("Need to unbox this for call to virtual ToString () for %s\n", mono_class_get_name (klass));
	}

	mono_method_desc_free (mdesc);
}

static void
create_object (MonoDomain *domain, MonoImage *image)
{
	MonoClass *klass;
	MonoObject *obj;

	klass = mono_class_from_name (image, "Embed", "MyType");
	if (!klass) {
		fprintf (stderr, "Can't find MyType in assembly %s\n", mono_image_get_filename (image));
		exit (1);
	}

	obj = mono_object_new (domain, klass);
	/* mono_object_new () only allocates the storage: 
	 * it doesn't run any constructor. Tell the runtime to run
	 * the default argumentless constructor.
	 */
	mono_runtime_object_init (obj);

	access_valuetype_field (obj);
	access_reference_field (obj);

	call_methods (obj);
	more_methods (domain);
}

static void main_function (MonoDomain *domain, const char *file, int argc, char **argv)
{
	MonoAssembly *assembly;

	/* Loading an assembly makes the runtime setup everything
	 * needed to execute it. If we're just interested in the metadata
	 * we'd use mono_image_load (), instead and we'd get a MonoImage*.
	 */
	assembly = mono_domain_assembly_open (domain, file);
	if (!assembly)
		exit (2);
	/*
	 * mono_jit_exec() will run the Main() method in the assembly.
	 * The return value needs to be looked up from
	 * System.Environment.ExitCode.
	 */
	mono_jit_exec (domain, assembly, argc, argv);

	create_object (domain, mono_assembly_get_image (assembly));
}

#ifdef _TESTCASE_
#ifdef __cplusplus
extern "C"
#endif
int
test_mono_embed_invoke_main (void);

int 
test_mono_embed_invoke_main (void)
{
#else
int
main (void)
{
#endif

	MonoDomain *domain;
	int argc = 2;
	char *argv[] = {
						(char*)"test-embed-invoke.exe",
						(char*)"test-embed-invoke-cs.exe",
						NULL
					};
	const char *file;
	int retval;
	file = argv [1];
	
    /*
	 * Load the default Mono configuration file, this is needed
	 * if you are planning on using the dllmaps defined on the
	 * system configuration
	 */
	mono_config_parse (NULL);
    /*
	 * mono_jit_init() creates a domain: each assembly is
	 * loaded and run in a MonoDomain.
	 */
	domain = mono_jit_init (file);

	main_function (domain, file, argc - 1, argv + 1);

	retval = mono_environment_exitcode_get ();
	
	mono_jit_cleanup (domain);
	return retval;
}

