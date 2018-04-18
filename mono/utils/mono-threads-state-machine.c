/**
 * \file
 */

#include <config.h>

#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/mono-tls.h>
#include <mono/utils/mono-memory-model.h>
#include <mono/utils/atomic.h>
#include <mono/utils/checked-build.h>
#include <mono/utils/mono-threads-debug.h>

#include <errno.h>

/*thread state helpers*/
static inline int
get_thread_state (int thread_state)
{
	return thread_state & THREAD_STATE_MASK;
}

static inline int
get_thread_suspend_count (int thread_state)
{
	return (thread_state & THREAD_SUSPEND_COUNT_MASK) >> THREAD_SUSPEND_COUNT_SHIFT;
}

static inline int
build_thread_state (int thread_state, int suspend_count) 
{
	g_assert (suspend_count >= 0 && suspend_count <= THREAD_SUSPEND_COUNT_MAX);
	g_assert (thread_state >= 0 && thread_state <= STATE_MAX);

	return thread_state | (suspend_count << THREAD_SUSPEND_COUNT_SHIFT);
}

static const char*
state_name (int state)
{
	static const char *state_names [] = {
		"STARTING",
		"DETACHED",

		"RUNNING",
		"ASYNC_SUSPENDED",
		"SELF_SUSPENDED",
		"ASYNC_SUSPEND_REQUESTED",

		"STATE_BLOCKING",
		"STATE_BLOCKING_ASYNC_SUSPENDED",
		"STATE_BLOCKING_SELF_SUSPENDED",
		"STATE_BLOCKING_SUSPEND_REQUESTED",
	};
	return state_names [get_thread_state (state)];
}

#define UNWRAP_THREAD_STATE(RAW,CUR,COUNT,INFO) do {	\
	RAW = (INFO)->thread_state;	\
	CUR = get_thread_state (RAW);	\
	COUNT = get_thread_suspend_count (RAW);	\
} while (0)

static void
check_thread_state (MonoThreadInfo* info)
{
	int raw_state, cur_state, suspend_count;
	UNWRAP_THREAD_STATE (raw_state, cur_state, suspend_count, info);
	switch (cur_state) {
	case STATE_STARTING:
	case STATE_RUNNING:
	case STATE_DETACHED:
		g_assert (suspend_count == 0);
		break;
	case STATE_ASYNC_SUSPENDED:
	case STATE_SELF_SUSPENDED:
	case STATE_ASYNC_SUSPEND_REQUESTED:
	case STATE_BLOCKING_SELF_SUSPENDED:
	case STATE_BLOCKING_SUSPEND_REQUESTED:
	case STATE_BLOCKING_ASYNC_SUSPENDED:
		g_assert (suspend_count > 0);
		break;
	case STATE_BLOCKING:
		g_assert (suspend_count == 0);
		break;
	default:
		g_error ("Invalid state %d", cur_state);
	}
}

static inline void
trace_state_change_with_func (const char *transition, MonoThreadInfo *info, int cur_raw_state, int next_state, int suspend_count_delta, const char *func)
{
	check_thread_state (info);
	THREADS_STATE_MACHINE_DEBUG ("[%s][%p] %s -> %s (%d -> %d) %s\n",
		transition,
		mono_thread_info_get_tid (info),
		state_name (get_thread_state (cur_raw_state)),
		state_name (next_state),
		get_thread_suspend_count (cur_raw_state),
		get_thread_suspend_count (cur_raw_state) + suspend_count_delta,
		func);

	CHECKED_BUILD_THREAD_TRANSITION (transition, info, get_thread_state (cur_raw_state), get_thread_suspend_count (cur_raw_state), next_state, suspend_count_delta);
}

static inline void
trace_state_change_sigsafe (const char *transition, MonoThreadInfo *info, int cur_raw_state, int next_state, int suspend_count_delta, const char *func)
{
	check_thread_state (info);
	THREADS_STATE_MACHINE_DEBUG ("[%s][%p] %s -> %s (%d -> %d) %s\n",
		transition,
		mono_thread_info_get_tid (info),
		state_name (get_thread_state (cur_raw_state)),
		state_name (next_state),
		get_thread_suspend_count (cur_raw_state),
		get_thread_suspend_count (cur_raw_state) + suspend_count_delta,
		func);

	CHECKED_BUILD_THREAD_TRANSITION_NOBT (transition, info, get_thread_state (cur_raw_state), get_thread_suspend_count (cur_raw_state), next_state, suspend_count_delta);
}

static inline void
trace_state_change (const char *transition, MonoThreadInfo *info, int cur_raw_state, int next_state, int suspend_count_delta)
// FIXME migrate all uses
{
	trace_state_change_with_func (transition, info, cur_raw_state, next_state, suspend_count_delta, "");
}

/*
This is the transition that signals that a thread is functioning.
Its main goal is to catch threads been witnessed before been fully registered.
*/
void
mono_threads_transition_attach (MonoThreadInfo* info)
{
	int raw_state, cur_state, suspend_count;

retry_state_change:
	UNWRAP_THREAD_STATE (raw_state, cur_state, suspend_count, info);
	switch (cur_state) {
	case STATE_STARTING:
		if (!(suspend_count == 0))
			mono_fatal_with_history ("suspend_count = %d, but should be == 0", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, STATE_RUNNING, raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("ATTACH", info, raw_state, STATE_RUNNING, 0);
		break;
	default:
		mono_fatal_with_history ("Cannot transition current thread from %s with ATTACH", state_name (cur_state));
	}
}

/*
This is the transition that signals that a thread is no longer registered with the runtime.
Its main goal is to catch threads been witnessed after they detach.

This returns TRUE is the transition succeeded.
If it returns false it means that there's a pending suspend that should be acted upon.
*/
gboolean
mono_threads_transition_detach (MonoThreadInfo *info)
{
	int raw_state, cur_state, suspend_count;

retry_state_change:
	UNWRAP_THREAD_STATE (raw_state, cur_state, suspend_count, info);
	switch (cur_state) {
	case STATE_RUNNING:
	case STATE_BLOCKING: /* An OS thread on coop goes STARTING->BLOCKING->RUNNING->BLOCKING->DETACHED */
		if (!(suspend_count == 0))
			mono_fatal_with_history ("suspend_count = %d, but should be == 0", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, STATE_DETACHED, raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("DETACH", info, raw_state, STATE_DETACHED, 0);
		return TRUE;
	case STATE_ASYNC_SUSPEND_REQUESTED: //Can't detach until whoever asked us to suspend to be happy with us
	case STATE_BLOCKING_SUSPEND_REQUESTED:
		return FALSE;

/*
STATE_ASYNC_SUSPENDED: Code should not be running while suspended.
STATE_SELF_SUSPENDED: Code should not be running while suspended.
STATE_BLOCKING_SELF_SUSPENDED: This is a bug in coop x suspend that resulted the thread in an undetachable state.
STATE_BLOCKING_ASYNC_SUSPENDED: Same as BLOCKING_SELF_SUSPENDED
*/
	default:
		mono_fatal_with_history ("Cannot transition current thread %p from %s with DETACH", info, state_name (cur_state));
	}
}

/*
This transition initiates the suspension of another thread.

Returns one of the following values:

- ReqSuspendInitSuspendRunning: Thread suspend requested, caller must initiate suspend.
- ReqSuspendInitSuspendBlocking: Thread in blocking state, caller may initiate suspend.
- ReqSuspendAlreadySuspended: Thread was already suspended and not executing, nothing to do.
- ReqSuspendAlreadySuspendedBlocking: Thread was already in blocking and a suspend was requested
                                      and the thread is still executing (perhaps in a syscall),
                                      nothing to do.
*/
MonoRequestSuspendResult
mono_threads_transition_request_suspension (MonoThreadInfo *info)
{
	int raw_state, cur_state, suspend_count;
	g_assert (info != mono_thread_info_current ());

retry_state_change:
	UNWRAP_THREAD_STATE (raw_state, cur_state, suspend_count, info);

	switch (cur_state) {
	case STATE_RUNNING: //Post an async suspend request
		if (!(suspend_count == 0))
			mono_fatal_with_history ("suspend_count = %d, but should be == 0", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (STATE_ASYNC_SUSPEND_REQUESTED, 1), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("SUSPEND_INIT_REQUESTED", info, raw_state, STATE_ASYNC_SUSPEND_REQUESTED, 1);
		return ReqSuspendInitSuspendRunning; //This is the first async suspend request against the target

	case STATE_ASYNC_SUSPENDED:
	case STATE_SELF_SUSPENDED:
	case STATE_BLOCKING_SELF_SUSPENDED:
	case STATE_BLOCKING_ASYNC_SUSPENDED:
		if (!(suspend_count > 0 && suspend_count < THREAD_SUSPEND_COUNT_MAX))
			mono_fatal_with_history ("suspend_count = %d, but should be > 0 and < THREAD_SUSPEND_COUNT_MAX", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (cur_state, suspend_count + 1), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("SUSPEND_INIT_REQUESTED", info, raw_state, cur_state, 1);
		return ReqSuspendAlreadySuspended; //Thread is already suspended so we don't need to wait it to suspend

	case STATE_BLOCKING:
		if (!(suspend_count == 0))
			mono_fatal_with_history ("suspend_count = %d, but should be == 0", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (STATE_BLOCKING_SUSPEND_REQUESTED, 1), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("SUSPEND_INIT_REQUESTED", info, raw_state, STATE_BLOCKING_SUSPEND_REQUESTED, 1);
		return ReqSuspendInitSuspendBlocking; //A thread in the blocking state has its state saved so we can treat it as suspended.
	case STATE_BLOCKING_SUSPEND_REQUESTED:
		/* This should only be happening if we're doing a cooperative suspend of a blocking thread.
		 * In which case we could be in BLOCKING_SUSPEND_REQUESTED until we execute a done or abort blocking.
		 * In preemptive suspend of a blocking thread since there's a single suspend initiator active at a time,
		 * we would expect a finish_async_suspension or a done/abort blocking before the next suspension request
		 */
		if (!(suspend_count > 0 && suspend_count < THREAD_SUSPEND_COUNT_MAX))
			mono_fatal_with_history ("suspend_count = %d, but should be > 0 and < THREAD_SUSPEND_COUNT_MAX", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (cur_state, suspend_count + 1), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("SUSPEND_INIT_REQUESTED", info, raw_state, cur_state, 1);
		return ReqSuspendAlreadySuspendedBlocking;
		
/*

[1] It's questionable on what to do if we hit the beginning of a self suspend.
The expected behavior is that the target should poll its state very soon so the the suspend latency should be minimal.

STATE_ASYNC_SUSPEND_REQUESTED: Since there can only be one async suspend in progress and it must finish, it should not be possible to witness this.
*/
	default:
		mono_fatal_with_history ("Cannot transition thread %p from %s with SUSPEND_INIT_REQUESTED", mono_thread_info_get_tid (info), state_name (cur_state));
	}
	return (MonoRequestSuspendResult) FALSE;
}


/*
Check the current state of the thread and try to init a self suspend.
This must be called with self state saved.

Returns one of the following values:

- Resumed: Async resume happened and current thread should keep running
- Suspend: Caller should wait for a resume signal
- SelfSuspendNotifyAndWait: Notify the suspend initiator and wait for a resume signals
 suspend should start.

*/
MonoSelfSupendResult
mono_threads_transition_state_poll (MonoThreadInfo *info)
{
	int raw_state, cur_state, suspend_count;
	g_assert (mono_thread_info_is_current (info));

retry_state_change:
	UNWRAP_THREAD_STATE (raw_state, cur_state, suspend_count, info);
	switch (cur_state) {
	case STATE_RUNNING:
		if (!(suspend_count == 0))
			mono_fatal_with_history ("suspend_count = %d, but should be == 0", suspend_count);
		trace_state_change ("STATE_POLL", info, raw_state, cur_state, 0);
		return SelfSuspendResumed; //We're fine, don't suspend

	case STATE_ASYNC_SUSPEND_REQUESTED: //Async suspend requested, service it with a self suspend
		if (!(suspend_count > 0))
			mono_fatal_with_history ("suspend_count = %d, but should be > 0", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (STATE_SELF_SUSPENDED, suspend_count), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("STATE_POLL", info, raw_state, STATE_SELF_SUSPENDED, 0);
		return SelfSuspendNotifyAndWait; //Caller should notify suspend initiator and wait for resume

/*
STATE_ASYNC_SUSPENDED: Code should not be running while suspended.
STATE_SELF_SUSPENDED: Code should not be running while suspended.
STATE_BLOCKING:
STATE_BLOCKING_SUSPEND_REQUESTED:
STATE_BLOCKING_ASYNC_SUSPENDED:
STATE_BLOCKING_SELF_SUSPENDED: Poll is a local state transition. No VM activities are allowed while in blocking mode.
      (In all the blocking states - the local thread has no checkpoints, hence
      no polling, it can only do abort blocking or done blocking on itself).
*/
	default:
		mono_fatal_with_history ("Cannot transition thread %p from %s with STATE_POLL", mono_thread_info_get_tid (info), state_name (cur_state));
	}
}

/*
Try to resume a suspended thread.

Returns one of the following values:
- Sucess: The thread was resumed.
- Error: The thread was not suspended in the first place. [2]
- InitSelfResume: The thread is blocked on self suspend and should be resumed 
- InitAsyncResume: The thread is blocked on async suspend and should be resumed
- ResumeInitBlockingResume: The thread was suspended on the exit path of blocking state and should be resumed
      FIXME: ResumeInitBlockingResume is just InitSelfResume by a different name.

[2] This threading system uses an unsigned suspend count. Which means a resume cannot be
used as a suspend permit and cancel each other.

Suspend permits are really useful to implement managed synchronization structures that
don't consume native resources. The downside is that they further complicate the design of this
system as the RUNNING state now has a non zero suspend counter.

It can be implemented in the future if we find resume/suspend races that cannot be (efficiently) fixed by other means.

One major issue with suspend permits is runtime facilities (GC, debugger) that must have the target suspended when requested.
This would make permits really harder to add.
*/
MonoResumeResult
mono_threads_transition_request_resume (MonoThreadInfo* info)
{
	int raw_state, cur_state, suspend_count;
	g_assert (info != mono_thread_info_current ()); //One can't self resume [3]

retry_state_change:
	UNWRAP_THREAD_STATE (raw_state, cur_state, suspend_count, info);
	switch (cur_state) {
	case STATE_RUNNING: //Thread already running.
		if (!(suspend_count == 0))
			mono_fatal_with_history ("suspend_count = %d, but should be == 0", suspend_count);
		trace_state_change ("RESUME", info, raw_state, cur_state, 0);
		return ResumeError; //Resume failed because thread was not blocked

	case STATE_BLOCKING: //Blocking, might have a suspend count, we decrease if it's > 0
		if (!(suspend_count == 0))
			mono_fatal_with_history ("suspend_count = %d, but should be == 0", suspend_count);
		trace_state_change ("RESUME", info, raw_state, cur_state, 0);
		return ResumeError;
	case STATE_BLOCKING_SUSPEND_REQUESTED:
		if (!(suspend_count > 0))
			mono_fatal_with_history ("suspend_count = %d, but should be > 0", suspend_count);
		if (suspend_count > 1) {
			if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (cur_state, suspend_count - 1), raw_state) != raw_state)
				goto retry_state_change;
			trace_state_change ("RESUME", info, raw_state, cur_state, -1);
			return ResumeOk; //Resume worked and there's nothing for the caller to do.
		} else {
			if (mono_atomic_cas_i32 (&info->thread_state, STATE_BLOCKING, raw_state) != raw_state)
				goto retry_state_change;
			trace_state_change ("RESUME", info, raw_state, STATE_BLOCKING, -1);
			return ResumeOk; // Resume worked, back in blocking, nothing for the caller to do.
		}
	case STATE_BLOCKING_ASYNC_SUSPENDED:
		if (!(suspend_count > 0))
			mono_fatal_with_history ("suspend_count = %d, but should be > 0", suspend_count);
		if (suspend_count > 1) {
			if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (cur_state, suspend_count - 1), raw_state) != raw_state)
				goto retry_state_change;
			trace_state_change ("RESUME", info, raw_state, cur_state, -1);
			return ResumeOk; // Resume worked, there's nothing else for the caller to do.
		} else {
			if (mono_atomic_cas_i32 (&info->thread_state, STATE_BLOCKING, raw_state) != raw_state)
				goto retry_state_change;
			trace_state_change ("RESUME", info, raw_state, STATE_BLOCKING, -1);
			return ResumeInitAsyncResume; // Resume worked and caller must do async resume, thread resumes in BLOCKING
		}
	case STATE_ASYNC_SUSPENDED:
	case STATE_SELF_SUSPENDED:
	case STATE_BLOCKING_SELF_SUSPENDED: //Decrease the suspend_count and maybe resume
		if (!(suspend_count > 0))
			mono_fatal_with_history ("suspend_count = %d, but should be > 0", suspend_count);
		if (suspend_count > 1) {
			if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (cur_state, suspend_count - 1), raw_state) != raw_state)
					goto retry_state_change;
			trace_state_change ("RESUME", info, raw_state, cur_state, -1);

			return ResumeOk; //Resume worked and there's nothing for the caller to do.
		} else {
			if (mono_atomic_cas_i32 (&info->thread_state, STATE_RUNNING, raw_state) != raw_state)
				goto retry_state_change;
			trace_state_change ("RESUME", info, raw_state, STATE_RUNNING, -1);

			if (cur_state == STATE_ASYNC_SUSPENDED)
				return ResumeInitAsyncResume; //Resume worked and caller must do async resume
			else if (cur_state == STATE_SELF_SUSPENDED)
				return ResumeInitSelfResume; //Resume worked and caller must do self resume
			else
				return ResumeInitBlockingResume; //Resume worked and caller must do blocking resume
		}

/*

STATE_ASYNC_SUSPEND_REQUESTED: Only one async suspend/resume operation can be in flight, so a resume cannot witness an internal state of suspend

[3] A self-resume makes no sense given it requires the thread to be running, which means its suspend count must be zero. A self resume would make
sense as a suspend permit, but as explained in [2] we don't support it so this is a bug.

[4] It's questionable on whether a resume (an async operation) should be able to cancel a self suspend. The scenario where this would happen
is similar to the one described in [2] when this is used for as a synchronization primitive.

If this turns to be a problem we should either implement [2] or make this an invalid transition.

*/
	default:
		mono_fatal_with_history ("Cannot transition thread %p from %s with REQUEST_RESUME", mono_thread_info_get_tid (info), state_name (cur_state));
	}
}

/*
This performs the last step of preemptive suspend.

Returns TRUE if the caller should wait for resume.
*/
gboolean
mono_threads_transition_finish_async_suspend (MonoThreadInfo* info)
{
	int raw_state, cur_state, suspend_count;

retry_state_change:
	UNWRAP_THREAD_STATE (raw_state, cur_state, suspend_count, info);
	switch (cur_state) {

	case STATE_SELF_SUSPENDED: //async suspend raced with self suspend and lost
	case STATE_BLOCKING_SELF_SUSPENDED: //async suspend raced with blocking and lost
		trace_state_change_sigsafe ("FINISH_ASYNC_SUSPEND", info, raw_state, cur_state, 0, "");
		return FALSE; //let self suspend wait

	case STATE_ASYNC_SUSPEND_REQUESTED:
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (STATE_ASYNC_SUSPENDED, suspend_count), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change_sigsafe ("FINISH_ASYNC_SUSPEND", info, raw_state, STATE_ASYNC_SUSPENDED, 0, "");
		return TRUE; //Async suspend worked, now wait for resume
	case STATE_BLOCKING_SUSPEND_REQUESTED:
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (STATE_BLOCKING_ASYNC_SUSPENDED, suspend_count), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change_sigsafe ("FINISH_ASYNC_SUSPEND", info, raw_state, STATE_BLOCKING_ASYNC_SUSPENDED, 0, "");
		return TRUE; //Async suspend of blocking thread worked, now wait for resume

/*
STATE_RUNNING: A thread cannot escape suspension once requested.
STATE_ASYNC_SUSPENDED: There can be only one suspend initiator at a given time, meaning this state should have been visible on the first stage of suspend.
STATE_BLOCKING: If a thread is subject to preemptive suspend, there is no race as the resume initiator should have suspended the thread to STATE_BLOCKING_ASYNC_SUSPENDED or STATE_BLOCKING_SELF_SUSPENDED before resuming.
                With cooperative suspend, there are no finish_async_suspend transitions since there's no path back from asyns_suspend requested to running.
STATE_BLOCKING_ASYNC_SUSPENDED: There can only be one suspend initiator at a given time, meaning this state should have ben visible on the first stage of suspend.
*/
	default:
		mono_fatal_with_history ("Cannot transition thread %p from %s with FINISH_ASYNC_SUSPEND", mono_thread_info_get_tid (info), state_name (cur_state));
	}
}

/*
This transitions the thread into a cooperative state where it's assumed to be suspended but can continue.

Native runtime code might want to put itself into a state where the thread is considered suspended but can keep running.
That state only works as long as the only managed state touched is blitable and was pinned before the transition.

It returns the action the caller must perform:

- Continue: Entered blocking state sucessfully;
- PollAndRetry: Async suspend raced and won, try to suspend and then retry;

*/
MonoDoBlockingResult
mono_threads_transition_do_blocking (MonoThreadInfo* info, const char *func)
{
	int raw_state, cur_state, suspend_count;

retry_state_change:
	UNWRAP_THREAD_STATE (raw_state, cur_state, suspend_count, info);
	switch (cur_state) {

	case STATE_RUNNING: //transition to blocked
		if (!(suspend_count == 0))
			mono_fatal_with_history ("suspend_count = %d, but should be == 0", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (STATE_BLOCKING, suspend_count), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("DO_BLOCKING", info, raw_state, STATE_BLOCKING, 0);
		return DoBlockingContinue;

	case STATE_ASYNC_SUSPEND_REQUESTED:
		if (!(suspend_count > 0))
			mono_fatal_with_history ("suspend_count = %d, but should be > 0", suspend_count);
		trace_state_change ("DO_BLOCKING", info, raw_state, cur_state, 0);
		return DoBlockingPollAndRetry;
/*
STATE_ASYNC_SUSPENDED
STATE_SELF_SUSPENDED: Code should not be running while suspended.
STATE_BLOCKING:
STATE_BLOCKING_SUSPEND_REQUESTED:
STATE_BLOCKING_SELF_SUSPENDED: Blocking is not nestabled
STATE_BLOCKING_ASYNC_SUSPENDED: Blocking is not nestable _and_ code should not be running while suspended
*/
	default:
		mono_fatal_with_history ("%s Cannot transition thread %p from %s with DO_BLOCKING", func, mono_thread_info_get_tid (info), state_name (cur_state));
	}
}

/*
This is the exit transition from the blocking state. If this thread is logically async suspended it will have to wait
until its resumed before continuing.

It returns one of:
-Ok: Done with blocking, just move on;
-Wait: This thread was async suspended, wait for resume
-NotifyAndWait: This thread was suspended while in blocking, it must notify the initiator if it was suspended preemptively and wait for resume.
*/
MonoDoneBlockingResult
mono_threads_transition_done_blocking (MonoThreadInfo* info, const char *func)
{
	int raw_state, cur_state, suspend_count;

retry_state_change:
	UNWRAP_THREAD_STATE (raw_state, cur_state, suspend_count, info);
	switch (cur_state) {
	case STATE_BLOCKING:
		if (!(suspend_count == 0))
			mono_fatal_with_history ("%s suspend_count = %d, but should be == 0", func, suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (STATE_RUNNING, suspend_count), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change_with_func ("DONE_BLOCKING", info, raw_state, STATE_RUNNING, 0, func);
		return DoneBlockingOk;
	case STATE_BLOCKING_SUSPEND_REQUESTED:
		if (!(suspend_count > 0))
			mono_fatal_with_history ("suspend_count = %d, but should be > 0", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (STATE_BLOCKING_SELF_SUSPENDED, suspend_count), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("DONE_BLOCKING", info, raw_state, STATE_BLOCKING_SELF_SUSPENDED, 0);
		return DoneBlockingNotifyAndWait;
/*
STATE_RUNNING: //Blocking was aborted and not properly restored
STATE_ASYNC_SUSPEND_REQUESTED: //Blocking was aborted, not properly restored and now there's a pending suspend
STATE_ASYNC_SUSPENDED
STATE_SELF_SUSPENDED: Code should not be running while suspended.
STATE_BLOCKING_SELF_SUSPENDED: This an exit state of done blocking
STATE_BLOCKING_ASYNC_SUSPENDED: This is an exit state of done blocking
*/
	default:
		mono_fatal_with_history ("Cannot transition thread %p from %s with DONE_BLOCKING", mono_thread_info_get_tid (info), state_name (cur_state));
	}
}

/*
Transition a thread in what should be a blocking state back to running state.
This is different that done blocking because the goal is to get back to blocking once we're done.
This is required to be able to bail out of blocking in case we're back to inside the runtime.

It returns one of:
-Ignore: Thread was not in blocking, nothing to do;
-IgnoreAndPool: Thread was not blocking and there's a pending suspend that needs to be processed;
-Ok: Blocking state successfully aborted;
-Wait: Blocking state successfully aborted, there's a pending suspend to be processed though
-NotifyAndWait: Blocking state was successfully aborted but the thread was preemptively suspended while in blocking, it must notify the initiator and wait for resume.
*/
MonoAbortBlockingResult
mono_threads_transition_abort_blocking (THREAD_INFO_TYPE* info)
{
	int raw_state, cur_state, suspend_count;

retry_state_change:
	UNWRAP_THREAD_STATE (raw_state, cur_state, suspend_count, info);
	switch (cur_state) {
	case STATE_RUNNING: //thread already in runnable state
		trace_state_change ("ABORT_BLOCKING", info, raw_state, cur_state, 0);
		return AbortBlockingIgnore;

	case STATE_ASYNC_SUSPEND_REQUESTED: //thread is runnable and have a pending suspend
		trace_state_change ("ABORT_BLOCKING", info, raw_state, cur_state, 0);
		return AbortBlockingIgnoreAndPoll;

	case STATE_BLOCKING:
		if (!(suspend_count == 0))
			mono_fatal_with_history ("suspend_count = %d,  but should be == 0", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (STATE_RUNNING, suspend_count), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("ABORT_BLOCKING", info, raw_state, STATE_RUNNING, 0);
		return AbortBlockingOk;
	case STATE_BLOCKING_SUSPEND_REQUESTED:
		if (!(suspend_count > 0))
			mono_fatal_with_history ("suspend_count = %d, but should be > 0", suspend_count);
		if (mono_atomic_cas_i32 (&info->thread_state, build_thread_state (STATE_BLOCKING_SELF_SUSPENDED, suspend_count), raw_state) != raw_state)
			goto retry_state_change;
		trace_state_change ("ABORT_BLOCKING", info, raw_state, STATE_BLOCKING_SELF_SUSPENDED, 0);
		return AbortBlockingNotifyAndWait;
/*
STATE_ASYNC_SUSPENDED:
STATE_SELF_SUSPENDED: Code should not be running while suspended.
STATE_BLOCKING_SELF_SUSPENDED: This is an exit state of done blocking, can't happen here.
STATE_BLOCKING_ASYNC_SUSPENDED: This is an exit state of abort blocking, can't happen here.
*/
	default:
		mono_fatal_with_history ("Cannot transition thread %p from %s with DONE_BLOCKING", mono_thread_info_get_tid (info), state_name (cur_state));
	}
}

// State checking code
/**
 * Return TRUE is the thread is in a runnable state.
*/
gboolean
mono_thread_info_is_running (MonoThreadInfo *info)
{
	switch (get_thread_state (info->thread_state)) {
	case STATE_RUNNING:
	case STATE_ASYNC_SUSPEND_REQUESTED:
	case STATE_BLOCKING_SUSPEND_REQUESTED:
	case STATE_BLOCKING:
		return TRUE;
	}
	return FALSE;
}

/**
 * Return TRUE is the thread is in an usable (suspendable) state
 */
gboolean
mono_thread_info_is_live (MonoThreadInfo *info)
{
	switch (get_thread_state (info->thread_state)) {
	case STATE_STARTING:
	case STATE_DETACHED:
		return FALSE;
	}
	return TRUE;
}

int
mono_thread_info_suspend_count (MonoThreadInfo *info)
{
	return get_thread_suspend_count (info->thread_state);
}

int
mono_thread_info_current_state (MonoThreadInfo *info)
{
	return get_thread_state (info->thread_state);
}

const char*
mono_thread_state_name (int state)
{
	return state_name (state);
}

gboolean
mono_thread_is_gc_unsafe_mode (void)
{
	MonoThreadInfo *cur = mono_thread_info_current ();

	if (!cur)
		return FALSE;

	switch (mono_thread_info_current_state (cur)) {
	case STATE_RUNNING:
	case STATE_ASYNC_SUSPEND_REQUESTED:
		return TRUE;
	default:
		return FALSE;
	}
}
