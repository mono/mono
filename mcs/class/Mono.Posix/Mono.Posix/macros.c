#include <sys/types.h>
#include <sys/wait.h>
#include <sys/stat.h>
#include <unistd.h>
#include <pwd.h>
#include <grp.h>
#include <errno.h>
#include <dirent.h>

int wifexited (int status)
{
	return WIFEXITED (status);
}

int wexitstatus (int status)
{
	return WEXITSTATUS (status);
}

int wifsignaled (int status)
{
	return WIFSIGNALED (status);
}

int wtermsig (int status)
{
	return WTERMSIG (status);
}

int wifstopped (int status)
{
	return WIFSTOPPED (status);
}

int wstopsig (int status)
{
	return WSTOPSIG (status);
}

int helper_Mono_Posix_Stat(char *filename, int dereference, 
	int *device,
	int *inode,
	int *mode,
	int *nlinks,
	int *uid,
	int *gid,
	int *rdev,
	long *size,
	long *blksize,
	long *blocks,
	long *atime,
	long *mtime,
	long *ctime
	) {
	int ret;
	struct stat buf;
	
	if (!dereference)
		ret = stat(filename, &buf);
	else
		ret = lstat(filename, &buf);
	
	if (ret) return ret;
	
	*device = buf.st_dev;
	*inode = buf.st_ino;
	*mode = buf.st_mode;
	*nlinks = buf.st_nlink;
	*uid = buf.st_uid;
	*gid = buf.st_gid;
	*rdev = buf.st_rdev;
	*size = buf.st_size;
	*blksize = buf.st_blksize;
	*blocks = buf.st_blocks;
	*atime = buf.st_atime;
	*mtime = buf.st_mtime;
	*ctime = buf.st_ctime;
	return 0;
}

char *helper_Mono_Posix_GetUserName(int uid) {
	struct passwd *p = getpwuid(uid);
	if (p == NULL) return NULL;
	return p->pw_name;
}
char *helper_Mono_Posix_GetGroupName(int gid) {
	struct group *p = getgrgid(gid);
	if (p == NULL) return NULL;
	return p->gr_name;
}

char *helper_Mono_Posix_readdir(DIR *dir) {
	struct dirent* e = readdir(dir);
	if (e == NULL) return NULL;
	return e->d_name;
}
