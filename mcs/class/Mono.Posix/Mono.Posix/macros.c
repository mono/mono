#include <sys/types.h>
#include <sys/wait.h>

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
