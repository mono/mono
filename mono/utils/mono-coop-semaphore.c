/**
 * \file
 */

#include <config.h>
#include "mono/utils/mono-coop-semaphore.h"
#include "mono/utils/mono-threads.h"

static void
mono_coop_sem_interrupt (gpointer sem)
{
	mono_os_sem_post (&((MonoCoopSem*)sem)->s);
}

MonoSemTimedwaitRet
mono_coop_sem_timedwait (MonoCoopSem *sem, guint timeout_ms, MonoSemFlags flags)
{
	MonoSemTimedwaitRet res;
	gboolean const alertable = (flags & MONO_SEM_FLAGS_ALERTABLE) != 0;
	gboolean interrupted = FALSE;

	if (alertable) {
		mono_thread_info_install_interrupt (mono_coop_sem_interrupt, sem, &interrupted);
		if (interrupted)
			return MONO_SEM_TIMEDWAIT_RET_ALERTED;
	}

	MONO_ENTER_GC_SAFE;

	res = mono_os_sem_timedwait (&sem->s, timeout_ms, flags);

	MONO_EXIT_GC_SAFE;

	if (alertable)
		mono_thread_info_uninstall_interrupt (&interrupted);

	if (interrupted)
		return MONO_SEM_TIMEDWAIT_RET_ALERTED;

	return res;
}
