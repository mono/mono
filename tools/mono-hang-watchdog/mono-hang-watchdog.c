/* Given a external process' id as argument, the program waits for a set timeout then attempts to abort that process */
/* Used by the Mono runtime's crash reporting. */

#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
#include <errno.h>
#include <unistd.h>
#include "config.h"
#include <signal.h>

#define TIMEOUT 30

static char* program_name;
void program_exit (int exit_code, const char* message);

int main (int argc, char* argv[])
{
    program_name = argv [0];
    if (argc != 2)
        program_exit (1, "Please provide one argument (pid)");
    errno = 0;
    pid_t pid = (pid_t)strtoul (argv [1], NULL, 10);
    if (errno)
        program_exit (2, "Invalid pid");

    sleep (TIMEOUT);

    /* if we survived the timeout, we consider the Mono process as hung */

#ifndef HAVE_KILL
    /* just inform the user */
    printf ("Mono process with pid %llu appears to be hung", (uint64_t)pid);
    return 0;
#else
    printf ("Mono process hang detected, sending kill signal to pid %llu\n", (uint64_t)pid);
    return kill (pid, SIGKILL);
#endif
}

void program_exit (int exit_code, const char* message)
{
    if (message)
        printf ("%s\n", message);
    printf ("Usage: '%s [pid]'\t\t[pid]: The id for the Mono process\n", program_name);
    exit (exit_code);
}
