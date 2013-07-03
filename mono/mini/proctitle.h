/* Sets up the arguments to support renaming the process title through rewriting argv 
   Must be called with the argv pointer on process entry. Will potentially return 
   a copy of argv.
*/
char** mono_proctitle_start (int argc, char **argv);

/* Sets the process title. Can be called at anytime */
int mono_proctitle_set(const char* title);

/* Called to cleanup after proc title */
void mono_proctitle_shutdown(void);

/* private */

#ifdef PLATFORM_MACOSX
int mono_proctitle_set_macosx(const char* title);
#endif

