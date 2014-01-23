#ifndef GC_PTHREAD_STOP_WORLD_H
#define GC_PTHREAD_STOP_WORLD_H

struct thread_stop_info {
    int	signal;
    word last_stop_count;	/* GC_last_stop_count value when thread	*/
    				/* last successfully handled a suspend	*/
    				/* signal.				*/
    ptr_t stack_ptr;  		/* Valid only when stopped.      	*/
#ifdef NACL
/* Grab NACL_GC_REG_STORAGE_SIZE pointers off the stack when going into */
/* a syscall.  20 is more than we need, but it's an overestimate in case*/
/* the instrumented function uses any callee saved registers, they may  */
/* be pushed to the stack much earlier.  Also, on amd64 'push' puts 8   */
/* bytes on the stack even though our pointers are 4 bytes.             */
#ifdef __arm__
/* For ARM we save r4-r8, r10-r12, r14 */
#define NACL_GC_REG_STORAGE_SIZE 9
#else
#define NACL_GC_REG_STORAGE_SIZE 20
#endif
    ptr_t reg_storage[NACL_GC_REG_STORAGE_SIZE];
#endif
};
    
#endif
