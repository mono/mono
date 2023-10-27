
#ifdef HAVE_CONFIG_H
# include "config.h"
#endif

#ifndef GC_THREADS
# define GC_THREADS
#endif

#define GC_NO_THREAD_REDIRECTS 1

#include "gc.h"

#include <stdio.h>
#include <stdlib.h>

#if (!defined(GC_PTHREADS) || defined(GC_SOLARIS_THREADS) \
     || defined(__native_client__)) && !defined(SKIP_THREADKEY_TEST)
  /* FIXME: Skip this test on Solaris for now.  The test may fail on    */
  /* other targets as well.  Currently, tested only on Linux, Cygwin    */
  /* and Darwin.                                                        */
# define SKIP_THREADKEY_TEST
#endif

#ifdef SKIP_THREADKEY_TEST

int main (void)
{
  printf("threadkey_test skipped\n");
  return 0;
}

#else

#include <pthread.h>

pthread_key_t key;

#ifdef GC_SOLARIS_THREADS
  /* pthread_once_t key_once = { PTHREAD_ONCE_INIT }; */
#else
  pthread_once_t key_once = PTHREAD_ONCE_INIT;
#endif

void * entry (void *arg)
{
  pthread_setspecific(key,
                      (void *)GC_HIDE_POINTER(GC_STRDUP("hello, world")));
  return arg;
}

void * GC_CALLBACK on_thread_exit_inner (struct GC_stack_base * sb, void * arg)
{
  int res = GC_register_my_thread (sb);
  pthread_t t;
  int creation_res;     /* Used to suppress a warning about     */
                        /* unchecked pthread_create() result.   */
  pthread_attr_t attr;

  if (pthread_attr_init(&attr) != 0
      || pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED) != 0) {
    fprintf(stderr, "Thread attribute init or setdetachstate failed\n");
    exit(2);
  }
  creation_res = GC_pthread_create(&t, &attr, entry, NULL);
  (void)pthread_attr_destroy(&attr);
  if (res == GC_SUCCESS)
    GC_unregister_my_thread ();

  return arg ? (void*)(GC_word)creation_res : 0;
}

void on_thread_exit (void *v)
{
  GC_call_with_stack_base (on_thread_exit_inner, v);
}

void make_key (void)
{
  pthread_key_create (&key, on_thread_exit);
}

#ifndef NTHREADS
# define NTHREADS 30 /* number of initial threads */
#endif

int main (void)
{
  int i;
  GC_INIT ();

# ifdef GC_SOLARIS_THREADS
    pthread_key_create (&key, on_thread_exit);
# else
    pthread_once (&key_once, make_key);
# endif
  for (i = 0; i < NTHREADS; i++) {
    pthread_t t;

    if (GC_pthread_create(&t, NULL, entry, NULL) == 0) {
      void *res;
      int code = (i & 1) != 0 ? GC_pthread_join(t, &res)
                                : GC_pthread_detach(t);

      if (code != 0) {
        fprintf(stderr, "Thread %s failed %d\n",
                (i & 1) != 0 ? "join" : "detach", code);
        exit(2);
      }
    }
  }
  return 0;
}

#endif /* !SKIP_THREADKEY_TEST */
