int mono_main (int argc, char* argv[]);

#include <malloc.h>

int main (int argc, char* argv[])
{
	char **newargs = (char **) malloc (sizeof (char *) * argc + 2);
	int i;

	newargs [0] = argv [0];
	newargs [1] = image_name;
	for (i = 1; i < argc; i++)
		newargs [i+1] = argv [i];
	newargs [++i] = NULL;
	
	mono_register_bundled_assemblies(bundled);
	return mono_main (argc+1, newargs);
}
