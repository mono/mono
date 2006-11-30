int mono_main (int argc, char* argv[]);

#include <stdlib.h>

int main (int argc, char* argv[])
{
	char **newargs = (char **) malloc (sizeof (char *) * (argc + 2));
	int i;

	newargs [0] = argv [0];
	newargs [1] = image_name;
	for (i = 1; i < argc; i++)
		newargs [i+1] = argv [i];
	newargs [++i] = NULL;
	
	if (config_dir != NULL && getenv ("MONO_CFG_DIR") == NULL)
		mono_set_dirs (getenv ("MONO_PATH"), config_dir);
	
	mono_mkbundle_init();

	return mono_main (argc+1, newargs);
}
