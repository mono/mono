#include <stdio.h>
#include <semaphore.h>
#include <string.h>
#include <ctype.h>
#include <pthread.h>
#include <windows.h>

int pthread_kill (pthread_t thread, int signo)
{
  printf ("pthread_kill\n");
  return 0;
}

/*
 First implementation of funcitons usign ideas from wine/scheduler/pthread.c
*/

typedef struct {
  HANDLE 	semaphore;
  int		value;
} *wine_semaphore;

int sem_init (sem_t *sem, int pshared, unsigned int value)
{
  printf ("sem_init\n");
  ((wine_semaphore)sem)->semaphore = NULL;
  ((wine_semaphore)sem)->value = value;
  return 0;
}

static void sem_real_init(sem_t *sem)
{
  ((wine_semaphore)sem)->semaphore = CreateSemaphore(0,((wine_semaphore)sem)->value,1024,0);
  printf ("sem_real_init thread %d sem %p\n", (int)pthread_self(),((wine_semaphore)sem)->semaphore);
}

int sem_post (sem_t * sem)
{
  printf (">sem_post thread %d sem %p\n", (int)pthread_self(),((wine_semaphore)sem)->semaphore);
  if (!((wine_semaphore)sem)->semaphore)
    sem_real_init( sem );
  ReleaseSemaphore(((wine_semaphore)sem)->semaphore,1,0);
  printf ("<sem_post thread %d sem %p\n", (int)pthread_self(),((wine_semaphore)sem)->semaphore);
  return 0;
}

int sem_wait (sem_t * sem)
{
  printf (">sem_wait thread %d sem %p\n", (int)pthread_self(),((wine_semaphore)sem)->semaphore);
  if (!((wine_semaphore)sem)->semaphore)
    sem_real_init( sem );
  WaitForSingleObject(((wine_semaphore)sem)->semaphore, INFINITE);
  printf ("<sem_wait thread %d sem %p\n", (int)pthread_self(),((wine_semaphore)sem)->semaphore);
  return 0;
}

int sem_destroy(sem_t * sem)
{
  printf ("sem_destroy\n");
  return 0;
}

typedef struct {
  HANDLE 	event;
} *wine_cond_t;

int pthread_cond_init(pthread_cond_t *cond, const pthread_condattr_t *cond_attr)
{
  printf("SWF:pthread_cond_init %p\n",((wine_cond_t)cond)->event);
  ((wine_cond_t)cond)->event = NULL;
  return 0;
}

static void pthread_cond_real_init(pthread_cond_t *cond)
{
  ((wine_cond_t)cond)->event = CreateEvent(0,1,0,0);
  printf ("pthread_cond_real_init thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->event);
}

int pthread_cond_destroy(pthread_cond_t *cond)
{
  printf("SWF:pthread_cond_destroy\n");
  return 0;
}

int pthread_cond_signal(pthread_cond_t *cond)
{
  printf(">SWF:pthread_cond_signal thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->event);
  if (!((wine_cond_t)cond)->event)
    pthread_cond_real_init( cond );
  PulseEvent(((wine_cond_t)cond)->event);
  printf("<SWF:pthread_cond_signal thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->event);
  return 0;
}

int pthread_cond_broadcast(pthread_cond_t *cond)
{
  printf(">SWF:pthread_cond_broadcast thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->event);
  if (!((wine_cond_t)cond)->event)
    pthread_cond_real_init( cond );
  SetEvent(((wine_cond_t)cond)->event);
  // FIXME: we have to wait until all threads waiting for this condition will be resumed
  ResetEvent(((wine_cond_t)cond)->event);
  printf("<SWF:pthread_cond_broadcast thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->event);
  return 0;
}

// FIXME: may be more CRITICAL_SECTIONs are needed
//     one for every pthread_cond_t (wine_cond_t) or
//     one for pthread_cond_wait and one for pthread_cond_timedwait
//
static CRITICAL_SECTION cond_wait_section;
static int cond_wait_section_initialized = 0;

static void initialize_cond_wait_section()
{
  if( !cond_wait_section_initialized) {
    InitializeCriticalSection( &cond_wait_section );
    cond_wait_section_initialized = 1;
  }
}

int pthread_cond_wait(pthread_cond_t *cond, pthread_mutex_t *mutex)
{
  initialize_cond_wait_section();
  EnterCriticalSection( &cond_wait_section );
  printf(">SWF:pthread_cond_wait thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->event);
  if (!((wine_cond_t)cond)->event)
    pthread_cond_real_init( cond );
  pthread_mutex_unlock(mutex);
  WaitForSingleObject(((wine_cond_t)cond)->event, INFINITE);
  pthread_mutex_lock(mutex);
  printf("<SWF:pthread_cond_wait thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->event);
  LeaveCriticalSection( &cond_wait_section );
  return 0;
}

int pthread_cond_timedwait(pthread_cond_t *cond, pthread_mutex_t *mutex, const struct timespec *abstime)
{
  initialize_cond_wait_section();
  EnterCriticalSection( &cond_wait_section );
  printf(">SWF:pthread_cond_timedwait thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->event);

  if (!((wine_cond_t)cond)->event)
    pthread_cond_real_init( cond );

  pthread_mutex_unlock(mutex);
  // FIXME: wait for specified time here
  WaitForSingleObject(((wine_cond_t)cond)->event, INFINITE);
  pthread_mutex_lock(mutex);
  printf("<SWF:pthread_cond_timedwait thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->event);
  LeaveCriticalSection( &cond_wait_section );
  return 0;
}

// FIXME: this function is needed for Debugger
int pthread_sigmask (int __how, __const __sigset_t *__restrict __newmask, __sigset_t *__restrict __oldmask)
{
  printf("SWF:pthread_sigmask %d\n", (int)pthread_self());
}
