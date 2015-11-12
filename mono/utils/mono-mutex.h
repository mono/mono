/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 * mono-mutex.h: Portability wrappers around POSIX Mutexes
 *
 * Authors: Jeffrey Stedfast <fejj@ximian.com>
 *
 * Copyright 2002 Ximian, Inc. (www.ximian.com)
 */

#ifndef __MONO_MUTEX_H__
#define __MONO_MUTEX_H__

#include <config.h>
#include <glib.h>

#if !defined(HOST_WIN32)

#include <pthread.h>

typedef pthread_mutex_t mono_mutex_t;
typedef pthread_cond_t mono_cond_t;

#else

#include <winsock2.h>
#include <windows.h>

/* Vanilla MinGW is missing some defs, load them from MinGW-w64. */
#if defined __MINGW32__ && !defined __MINGW64_VERSION_MAJOR && (_WIN32_WINNT >= 0x0600)

/* Fixme: Opaque structs */
typedef PVOID RTL_CONDITION_VARIABLE;
typedef PVOID RTL_SRWLOCK;

#ifndef _RTL_RUN_ONCE_DEF
#define _RTL_RUN_ONCE_DEF 1
typedef PVOID RTL_RUN_ONCE, *PRTL_RUN_ONCE;
typedef DWORD (WINAPI *PRTL_RUN_ONCE_INIT_FN)(PRTL_RUN_ONCE, PVOID, PVOID *);
#define RTL_RUN_ONCE_INIT 0
#define RTL_RUN_ONCE_CHECK_ONLY 1UL
#define RTL_RUN_ONCE_ASYNC 2UL
#define RTL_RUN_ONCE_INIT_FAILED 4UL
#define RTL_RUN_ONCE_CTX_RESERVED_BITS 2
#endif /* _RTL_RUN_ONCE_DEF */
#define RTL_SRWLOCK_INIT 0
#define RTL_CONDITION_VARIABLE_INIT 0
#define RTL_CONDITION_VARIABLE_LOCKMODE_SHARED 1

#define CONDITION_VARIABLE_INIT RTL_CONDITION_VARIABLE_INIT
#define CONDITION_VARIABLE_LOCKMODE_SHARED RTL_CONDITION_VARIABLE_LOCKMODE_SHARED
#define SRWLOCK_INIT RTL_SRWLOCK_INIT

/*Condition Variables http://msdn.microsoft.com/en-us/library/ms682052%28VS.85%29.aspx*/
typedef RTL_CONDITION_VARIABLE CONDITION_VARIABLE, *PCONDITION_VARIABLE;
typedef RTL_SRWLOCK SRWLOCK, *PSRWLOCK;

WINBASEAPI VOID WINAPI InitializeConditionVariable(PCONDITION_VARIABLE ConditionVariable);
WINBASEAPI WINBOOL WINAPI SleepConditionVariableCS(PCONDITION_VARIABLE ConditionVariable, PCRITICAL_SECTION CriticalSection, DWORD dwMilliseconds);
WINBASEAPI WINBOOL WINAPI SleepConditionVariableSRW(PCONDITION_VARIABLE ConditionVariable, PSRWLOCK SRWLock, DWORD dwMilliseconds, ULONG Flags);
WINBASEAPI VOID WINAPI WakeAllConditionVariable(PCONDITION_VARIABLE ConditionVariable);
WINBASEAPI VOID WINAPI WakeConditionVariable(PCONDITION_VARIABLE ConditionVariable);

/*Slim Reader/Writer (SRW) Locks http://msdn.microsoft.com/en-us/library/aa904937%28VS.85%29.aspx*/
WINBASEAPI VOID WINAPI AcquireSRWLockExclusive(PSRWLOCK SRWLock);
WINBASEAPI VOID WINAPI AcquireSRWLockShared(PSRWLOCK SRWLock);
WINBASEAPI VOID WINAPI InitializeSRWLock(PSRWLOCK SRWLock);
WINBASEAPI VOID WINAPI ReleaseSRWLockExclusive(PSRWLOCK SRWLock);
WINBASEAPI VOID WINAPI ReleaseSRWLockShared(PSRWLOCK SRWLock);

WINBASEAPI BOOLEAN TryAcquireSRWLockExclusive(PSRWLOCK SRWLock);
WINBASEAPI BOOLEAN TryAcquireSRWLockShared(PSRWLOCK SRWLock);

/*One-Time Initialization http://msdn.microsoft.com/en-us/library/aa363808(VS.85).aspx*/
#define INIT_ONCE_ASYNC 0x00000002UL
#define INIT_ONCE_INIT_FAILED 0x00000004UL

typedef PRTL_RUN_ONCE PINIT_ONCE;
typedef PRTL_RUN_ONCE LPINIT_ONCE;
typedef WINBOOL CALLBACK (*PINIT_ONCE_FN) (PINIT_ONCE InitOnce, PVOID Parameter, PVOID *Context);

WINBASEAPI WINBOOL WINAPI InitOnceBeginInitialize(LPINIT_ONCE lpInitOnce, DWORD dwFlags, PBOOL fPending, LPVOID *lpContext);
WINBASEAPI WINBOOL WINAPI InitOnceComplete(LPINIT_ONCE lpInitOnce, DWORD dwFlags, LPVOID lpContext);
WINBASEAPI WINBOOL WINAPI InitOnceExecuteOnce(PINIT_ONCE InitOnce, PINIT_ONCE_FN InitFn, PVOID Parameter, LPVOID *Context);

#endif /* defined __MINGW32__ && !defined __MINGW64_VERSION_MAJOR && (_WIN32_WINNT >= 0x0600) */

typedef CRITICAL_SECTION mono_mutex_t;
typedef CONDITION_VARIABLE mono_cond_t;

#endif

G_BEGIN_DECLS

int
mono_mutex_init (mono_mutex_t *mutex);

int
mono_mutex_init_recursive (mono_mutex_t *mutex);

int
mono_mutex_destroy (mono_mutex_t *mutex);

int
mono_mutex_lock (mono_mutex_t *mutex);

int
mono_mutex_trylock (mono_mutex_t *mutex);

int
mono_mutex_unlock (mono_mutex_t *mutex);

int
mono_cond_init (mono_cond_t *cond);

int
mono_cond_destroy (mono_cond_t *cond);

int
mono_cond_wait (mono_cond_t *cond, mono_mutex_t *mutex);

int
mono_cond_timedwait (mono_cond_t *cond, mono_mutex_t *mutex, struct timespec *timeout);

int
mono_cond_timedwait_ms (mono_cond_t *cond, mono_mutex_t *mutex, int timeout_ms);

int
mono_cond_signal (mono_cond_t *cond);

int
mono_cond_broadcast (mono_cond_t *cond);

G_END_DECLS

#endif /* __MONO_MUTEX_H__ */
