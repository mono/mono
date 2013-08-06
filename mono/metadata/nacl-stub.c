#if defined(__native_client__)

#include "nacl-stub.h"

#define STUB(target) __attribute__ ((weak, alias ("stub_"#target)));

struct group *stub_getgrnam(const char *name)
{
	return NULL;
}
struct group *getgrnam(const char *name) STUB(getgrnam)

struct group *stub_getgrgid(gid_t gid)
{
	errno = EIO;
	return NULL;
}
struct group *getgrgid(gid_t gid) STUB(getgrgid)

int stub_fsync(int fd)
{
	errno = EINVAL;
	return -1;
}
int fsync(int fd) STUB(fsync)


#ifdef USE_NEWLIB
dev_t stub_makedev(int maj, int min)
{
	return (maj)*256+(min);
}
dev_t makedev(int maj, int min) STUB(makedev)

int stub_utime(const char *filename, const void *times)
{
	errno = EACCES;
	return -1;
}
int utime(const char *filename, const void *times) STUB(utime)

int stub_kill(pid_t pid, int sig)
{
	errno = EACCES;
	return -1;
}
int kill(pid_t pid, int sig) STUB(kill)

int stub_getrusage(int who, void *usage)
{
	errno = EACCES;
	return -1;
}
int getrusage(int who, void *usage) STUB(getrusage)

int stub_lstat(const char *path, struct stat *buf)
{
	return stat (path, buf);
}
int lstat(const char *path, struct stat *buf) STUB(lstat)


int stub_getdtablesize(void)
{
#ifdef OPEN_MAX
	return OPEN_MAX;
#else
	return 256;
#endif
}

int getdtablesize(void) STUB(getdtablesize)

int stub_getpagesize(void)
{
#ifdef PAGE_SIZE
	return PAGE_SIZE;
#else
	return 4096;
#endif
}
int getpagesize(void) STUB(getpagesize)

int stub_sem_trywait(sem_t *sem) {
	g_assert_not_reached ();
	return -1;
}
int sem_trywait(sem_t *sem) STUB(sem_trywait)

int stub_sem_timedwait(sem_t *sem, const struct timespec *abs_timeout) {
	g_assert_not_reached ();
	return -1;
}
int sem_timedwait(sem_t *sem, const struct timespec *abs_timeout) STUB(sem_timedwait)

#endif

#endif
