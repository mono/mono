/* Conditionally execute the command argv[2] based if the file argv[1]  */
/* does not exist.  If the command is omitted (and the file does not    */
/* exist) then just exit with a non-zero code.                          */

# include "private/gc_priv.h"
# include <stdio.h>
# include <stdlib.h>
# include <unistd.h>
#ifdef __DJGPP__
#include <dirent.h>
#endif /* __DJGPP__ */

#ifdef __cplusplus
# define EXECV_ARGV_T char**
#else
# define EXECV_ARGV_T void* /* see the comment in if_mach.c */
#endif

int main(int argc, char **argv)
{
    FILE * f;
#ifdef __DJGPP__
    DIR * d;
#endif /* __DJGPP__ */
    char *fname;

    if (argc < 2 || argc > 3)
        goto Usage;

    fname = TRUSTED_STRING(argv[1]);
    f = fopen(fname, "rb");
    if (f != NULL) {
        fclose(f);
        return(0);
    }
    f = fopen(fname, "r");
    if (f != NULL) {
        fclose(f);
        return(0);
    }
#ifdef __DJGPP__
    if ((d = opendir(fname)) != 0) {
            closedir(d);
            return(0);
    }
#endif
    printf("^^^^Starting command^^^^\n");
    fflush(stdout);
    if (argc == 2)
        return(2); /* the file does not exist but no command is given */

    execvp(TRUSTED_STRING(argv[2]), (EXECV_ARGV_T)(argv + 2));
    exit(1);

Usage:
    fprintf(stderr, "Usage: %s file_name [command]\n", argv[0]);
    return(1);
}
