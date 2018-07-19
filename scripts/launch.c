#define PROFILE_BASE_DIR "/mono/lib/mono/4.0"
#define MONO_BINARY "/mono/bin/mono"
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <malloc.h>

int
main (int argc, char *argv [])
{
	char **nargv = (char **) malloc (sizeof (char *) * (argc + 1));
	char *last = strrchr (argv [0], '/');
	char *command;
	int i, len;

	if (last == NULL){
		fprintf (stderr, "Do not know how to invoke the program given [%s]\n", argv [0]);
		free (nargv);
		return 1;
	}
	len = strlen (last) + strlen (PROFILE_BASE_DIR) + 1;
	command = malloc (len);
	if (command == NULL){
		fprintf (stderr, "Error allocating memory");
		free (nargv);
		return 1;
	}
	strcpy (command, PROFILE_BASE_DIR);
	strcat (command, last);
	nargv [0] = command;
	nargv [1] = command;
	
	for (i = 1; i < argc; i++)
		nargv [1+i] = argv [i];
	
	execvp (MONO_BINARY, nargv);
}
