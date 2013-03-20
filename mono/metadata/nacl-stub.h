#ifndef _NACL_STUBS_H_
#define _NACL_STUBS_H_

#if defined(__native_client__)

#ifdef HAVE_CONFIG_H
#include <config.h>
#endif
#include <eglib/src/glib.h>
#include <errno.h>
#include <sys/stat.h>
#include <sys/types.h>

struct group *getgrnam(const char *name);
struct group *getgrgid(gid_t gid);
int fsync(int fd) ;
dev_t makedev(int maj, int min);

#ifdef USE_NEWLIB
int utime(const char *filename, const void *times);
int kill(pid_t pid, int sig);
int getrusage(int who, void *usage);
int lstat(const char *path, struct stat *buf);
int getdtablesize(void);
#define _POSIX_PATH_MAX 1024
# ifndef PATH_MAX
#  define PATH_MAX _POSIX_PATH_MAX
# endif
#endif

#endif

#endif
