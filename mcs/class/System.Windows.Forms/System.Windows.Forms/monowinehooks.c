#include <semaphore.h>
#include <pthread.h>
#include <dlfcn.h>

static void * dmHandle;

static int printf_stub(const char *format,...)
{
	return 0;
}

#define TRACE printf_stub

static void InitLib ()
{
	if(dmHandle == 0) {
		dmHandle = dlopen("monostub.exe.so", 1);
		TRACE("Module monostub %p, %s\n", dmHandle, dlerror());
	}
}

int pthread_kill (pthread_t thread, int signo)
{
	int (*pthread_kill_) (pthread_t thread, int signo);
	InitLib ();
	pthread_kill_ = dlsym(dmHandle,"pthread_kill");
	return pthread_kill_(thread, signo);
}

int sem_init (sem_t *sem, int pshared, unsigned int value)
{
	int (*sem_init_) (sem_t *sem, int pshared, unsigned int value);
	InitLib ();
	sem_init_ = dlsym(dmHandle,"sem_init");
	return sem_init_(sem, pshared, value);
}

int sem_post (sem_t * sem)
{
	int (*sem_post_)(sem_t * sem);
	InitLib ();
	sem_post_ = dlsym(dmHandle,"sem_post");
	return sem_post_(sem);
}

int sem_wait (sem_t * sem)
{
	int (*sem_wait_)(sem_t * sem);
	InitLib ();
	sem_wait_ = dlsym(dmHandle,"sem_wait");
	return sem_wait_(sem);
}

int sem_destroy(sem_t * sem)
{
	int (*sem_destroy_)(sem_t * sem);
	InitLib ();
	sem_destroy_ = dlsym(dmHandle,"sem_destroy");
	return sem_destroy_(sem);
}

int pthread_cond_init(pthread_cond_t *cond, const pthread_condattr_t *cond_attr)
{
	int (*pthread_cond_init_)(pthread_cond_t *cond, const pthread_condattr_t *cond_attr);
	InitLib ();
	pthread_cond_init_ = dlsym(dmHandle,"pthread_cond_init");
	return pthread_cond_init_(cond, cond_attr);
}

int pthread_cond_destroy(pthread_cond_t *cond)
{
	int (*pthread_cond_destroy_)(pthread_cond_t *cond);
	InitLib ();
	pthread_cond_destroy_ = dlsym(dmHandle,"pthread_cond_destroy");
	return pthread_cond_destroy_(cond);
}

int pthread_cond_signal(pthread_cond_t *cond)
{
	int (*pthread_cond_signal_)(pthread_cond_t *cond);
	InitLib ();
	pthread_cond_signal_ = dlsym(dmHandle,"pthread_cond_signal");
	return pthread_cond_signal_(cond);
}

int pthread_cond_broadcast(pthread_cond_t *cond)
{
	int (*pthread_cond_broadcast_)(pthread_cond_t *cond);
	InitLib ();
	pthread_cond_broadcast_ = dlsym(dmHandle,"pthread_cond_broadcast");
	return pthread_cond_broadcast_(cond);
}

int pthread_cond_wait(pthread_cond_t *cond, pthread_mutex_t *mutex)
{
	int (*pthread_cond_wait_)(pthread_cond_t *cond, pthread_mutex_t *mutex);
	InitLib ();
	pthread_cond_wait_ = dlsym(dmHandle,"pthread_cond_wait");
	return pthread_cond_wait_(cond, mutex);
}

int pthread_cond_timedwait(pthread_cond_t *cond, pthread_mutex_t *mutex, const struct timespec *abstime)
{
	int (*pthread_cond_timedwait_)(pthread_cond_t *cond, pthread_mutex_t *mutex, const struct timespec *abstime);
	InitLib ();
	pthread_cond_timedwait_ = dlsym(dmHandle,"pthread_cond_timedwait");
	return pthread_cond_timedwait_(cond, mutex, abstime);
}

int pthread_sigmask (int __how, __const __sigset_t *__restrict __newmask, __sigset_t *__restrict __oldmask)
{
	int (*pthread_sigmask_)(int __how, __const __sigset_t *__restrict __newmask, __sigset_t *__restrict __oldmask);
	InitLib ();
	pthread_sigmask_ = dlsym(dmHandle,"pthread_sigmask");
	return pthread_sigmask_(__how, __newmask, __oldmask);
}
