//
// System.Windows.Forms
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002/3 Ximian, Inc
//
#include <windows.h>
#include <stdio.h>
#include <semaphore.h>
#include <string.h>
#include <ctype.h>
#include <pthread.h>
#include <errno.h>

static int printf_stub(const char *format,...)
{
  return 0;
}

#define TRACE printf_stub

int pthread_kill (pthread_t thread, int signo)
{
  TRACE ("pthread_kill\n");
  return 0;
}

/*
 More or less complete implementation of functions using ideas from wine/scheduler/pthread.c
 and some code from Pthreads-win32.
 This code does not pretend to be complete implementation of any of Pthreads specifications.
 It was written to let Mono execute System.Windows.Forms applications until Wine will have 
 complete threads implementation.
*/

typedef struct {
  HANDLE    semaphore;
  int       value;
} *wine_semaphore;

int sem_init (sem_t *sem, int pshared, unsigned int value)
{
  TRACE ("sem_init\n");
  ((wine_semaphore)sem)->semaphore = NULL;
  ((wine_semaphore)sem)->value = value;
  return 0;
}

static void sem_real_init(sem_t *sem)
{
  ((wine_semaphore)sem)->semaphore = CreateSemaphore(0,((wine_semaphore)sem)->value,10240,0);
  TRACE ("sem_real_init thread %d sem %p\n", (int)pthread_self(),((wine_semaphore)sem)->semaphore);
}

int sem_post (sem_t * sem)
{
  TRACE (">sem_post thread %d sem %p\n", (int)pthread_self(),((wine_semaphore)sem)->semaphore);
  if (!((wine_semaphore)sem)->semaphore)
    sem_real_init( sem );
  ReleaseSemaphore(((wine_semaphore)sem)->semaphore,1,0);
  TRACE ("<sem_post thread %d sem %p\n", (int)pthread_self(),((wine_semaphore)sem)->semaphore);
  return 0;
}

int sem_wait (sem_t * sem)
{
  TRACE (">sem_wait thread %d sem %p\n", (int)pthread_self(),((wine_semaphore)sem)->semaphore);
  if (!((wine_semaphore)sem)->semaphore)
    sem_real_init( sem );
  WaitForSingleObject(((wine_semaphore)sem)->semaphore, INFINITE);
  TRACE ("<sem_wait thread %d sem %p\n", (int)pthread_self(),((wine_semaphore)sem)->semaphore);
  return 0;
}

int sem_destroy(sem_t * sem)
{
  TRACE ("sem_destroy\n");
  return 0;
}

struct wine_cond_type{
  HANDLE  event;
  long    waiters;
  long    to_release;
};

typedef struct wine_cond_type *pwine_cond_type;

typedef struct {
  struct wine_cond_type *data;
} *wine_cond_t;

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

int pthread_cond_init(pthread_cond_t *cond, const pthread_condattr_t *cond_attr)
{
  TRACE("SWF:pthread_cond_init %p\n", ((wine_cond_t)cond)->data);
  ((wine_cond_t)cond)->data = NULL;
  return 0;
}

static void pthread_cond_real_init(pthread_cond_t *cond)
{
  struct wine_cond_type  dummy;
  pwine_cond_type data = 0;
  initialize_cond_wait_section();
  data = (pwine_cond_type)malloc(sizeof(dummy));
  data->event = CreateEvent(0,1,0,0);
  data->waiters = 0;
  data->to_release = 0;
  ((wine_cond_t)cond)->data = data;
  TRACE ("pthread_cond_real_init thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->data->event);
}

int pthread_cond_destroy(pthread_cond_t *cond)
{
  TRACE("SWF:pthread_cond_destroy\n");
  return 0;
}

int pthread_cond_signal(pthread_cond_t *cond)
{
  if (!((wine_cond_t)cond)->data)
    pthread_cond_real_init( cond );
  TRACE(">SWF:pthread_cond_signal thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->data->event);
  EnterCriticalSection( &cond_wait_section );
  ((wine_cond_t)cond)->data->to_release = 1;
  LeaveCriticalSection( &cond_wait_section );
  PulseEvent(((wine_cond_t)cond)->data->event);
  TRACE("<SWF:pthread_cond_signal thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->data->event);
  return 0;
}

int pthread_cond_broadcast(pthread_cond_t *cond)
{
  wine_cond_t wcond = 0;
  if (!((wine_cond_t)cond)->data)
    pthread_cond_real_init( cond );
  TRACE(">SWF:pthread_cond_broadcast thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->data->event);
  wcond = (wine_cond_t)cond;
  EnterCriticalSection( &cond_wait_section );
  wcond->data->to_release = wcond->data->waiters;
  LeaveCriticalSection( &cond_wait_section );
  SetEvent(((wine_cond_t)cond)->data->event);
  while(wcond->data->waiters) {
    Sleep(0);
  }
  ResetEvent(((wine_cond_t)cond)->data->event);
  TRACE("<SWF:pthread_cond_broadcast thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->data->event);
  return 0;
}

int pthread_cond_wait(pthread_cond_t *cond, pthread_mutex_t *mutex)
{
  wine_cond_t wcond = 0;
  int  do_loop = 1;
  if (!((wine_cond_t)cond)->data)
    pthread_cond_real_init( cond );
  TRACE(">SWF:pthread_cond_wait thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->data->event);
  pthread_mutex_unlock(mutex);
  wcond = (wine_cond_t)cond;
  InterlockedIncrement(&(wcond->data->waiters));
  do_loop = 1;
  do {
    WaitForSingleObject(wcond->data->event, INFINITE);
    EnterCriticalSection( &cond_wait_section );
    if( wcond->data->to_release > 0) {
      --wcond->data->to_release;
      do_loop = 0;
    }
    LeaveCriticalSection( &cond_wait_section );
  } while( do_loop);
  InterlockedDecrement(&(wcond->data->waiters));
  pthread_mutex_lock(mutex);
  TRACE("<SWF:pthread_cond_wait thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->data->event);
  return 0;
}

/*
	The code to convert struct timespec time to Win32 time
	is from Pthreads-win32 project
*/

/*
 * time between jan 1, 1601 and jan 1, 1970 in units of 100 nanoseconds
 */
#define PTW32_TIMESPEC_TO_FILETIME_OFFSET \
	  ( ((LONGLONG) 27111902 << 32) + (LONGLONG) 3577643008 )

#define NANOSEC_PER_MILLISEC  1000000
#define MILLISEC_PER_SEC  1000

static void ptw32_filetime_to_timespec(const FILETIME *ft, struct timespec *ts)
     /*
      * -------------------------------------------------------------------
      * converts FILETIME (as set by GetSystemTimeAsFileTime), where the time is
      * expressed in 100 nanoseconds from Jan 1, 1601,
      * into struct timespec
      * where the time is expressed in seconds and nanoseconds from Jan 1, 1970.
      * -------------------------------------------------------------------
      */
{
  ts->tv_sec = (int)((*(LONGLONG *)ft - PTW32_TIMESPEC_TO_FILETIME_OFFSET) / 10000000);
  ts->tv_nsec = (int)((*(LONGLONG *)ft - PTW32_TIMESPEC_TO_FILETIME_OFFSET - ((LONGLONG)ts->tv_sec * (LONGLONG)10000000)) * 100);
}

int pthread_cond_timedwait(pthread_cond_t *cond, pthread_mutex_t *mutex, const struct timespec *abstime)
{
  int result = 0;
  unsigned long milliseconds = INFINITE;
  wine_cond_t wcond = 0;
  int  do_loop = 1;

  if (!((wine_cond_t)cond)->data)
    pthread_cond_real_init( cond );
  TRACE(">SWF:pthread_cond_timedwait thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->data->event);
  if( abstime != 0) {
    struct timespec  currSysTime;
    milliseconds = 0;

    if( abstime->tv_sec != 0 && abstime->tv_nsec != 0){
      FILETIME ft;
      SYSTEMTIME st;

      GetSystemTime(&st);
      SystemTimeToFileTime(&st, &ft);
      /*
       * GetSystemTimeAsFileTime(&ft); would be faster,
       * but it does not exist on WinCE
      */

      ptw32_filetime_to_timespec(&ft, &currSysTime);
      if (abstime->tv_sec >= currSysTime.tv_sec) {
        unsigned long tmpMilliseconds;
        unsigned long tmpCurrMilliseconds;

        tmpMilliseconds = (abstime->tv_sec - currSysTime.tv_sec) * MILLISEC_PER_SEC;
        tmpMilliseconds += ((abstime->tv_nsec + (NANOSEC_PER_MILLISEC/2))
                               / NANOSEC_PER_MILLISEC);
        tmpCurrMilliseconds = ((currSysTime.tv_nsec + (NANOSEC_PER_MILLISEC/2))
                               / NANOSEC_PER_MILLISEC);
        if (tmpMilliseconds > tmpCurrMilliseconds) {
          milliseconds = tmpMilliseconds - tmpCurrMilliseconds;
          if (milliseconds == INFINITE) {
            milliseconds--;
          }
        }
      }
    }
  }
  pthread_mutex_unlock(mutex);
  wcond = (wine_cond_t)cond;
  InterlockedIncrement(&(wcond->data->waiters));
  do_loop = 1;
  do {
    long waitres = WaitForSingleObject(wcond->data->event, milliseconds);
    if( waitres == WAIT_TIMEOUT) {
      do_loop = 0;
      result = ETIMEDOUT;
    }
    EnterCriticalSection( &cond_wait_section );
    if( wcond->data->to_release > 0) {
      --wcond->data->to_release;
      do_loop = 0;
    }
    LeaveCriticalSection( &cond_wait_section );
  } while( do_loop);
  InterlockedDecrement(&(wcond->data->waiters));
  pthread_mutex_lock(mutex);
  TRACE("<SWF:pthread_cond_timedwait thread %d evt %p\n", (int)pthread_self(),((wine_cond_t)cond)->data->event);
  return result;
}

// FIXME: this function is needed for Debugger
int pthread_sigmask (int __how, __const __sigset_t *__restrict __newmask, __sigset_t *__restrict __oldmask)
{
  printf("SWF:pthread_sigmask %d\n", (int)pthread_self());
}
