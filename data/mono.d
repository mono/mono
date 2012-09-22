/* 
 * mono.d: DTrace provider for Mono
 * 
 * Authors:
 *   Andreas Faerber <andreas.faerber@web.de>
 * 
 */

provider mono {
	/* Virtual Execution System (VES) */
	probe ves__init__begin ();
	probe ves__init__end ();

	/* Just-in-time compiler (JIT) */
	probe method__compile__begin (char* class_name, char* method_name, char* signature);
	probe method__compile__end (char* class_name, char* method_name, char* signature, int success);

	/* Garbage Collector (GC) */	
	probe gc__begin (int generation);
	probe gc__end (int generation);

	probe gc__heap__alloc (void *addr, uintptr_t len);
	probe gc__heap__free (void *addr, uintptr_t len);

	probe gc__locked ();
	probe gc__unlocked ();

	probe gc__nursery__tlab__alloc (void *addr, uintptr_t len);
	probe gc__nursery__obj__alloc (void *addr, uintptr_t size, char *class_name);

	probe gc__major__obj__alloc__degraded (void *addr, uintptr_t size, char *class_name);
	probe gc__major__obj__alloc__mature (void *addr, uintptr_t size, char *class_name);

	probe gc__nursery__sweeped (void *addr, uintptr_t len);
	probe gc__major__sweeped (void *addr, uintptr_t len);
};

#pragma D attributes Evolving/Evolving/Common provider mono provider
#pragma D attributes Private/Private/Unknown provider mono module
#pragma D attributes Private/Private/Unknown provider mono function
#pragma D attributes Evolving/Evolving/Common provider mono name
#pragma D attributes Evolving/Evolving/Common provider mono args

