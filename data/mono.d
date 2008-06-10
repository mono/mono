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
};

#pragma D attributes Evolving/Evolving/Common provider mono provider
#pragma D attributes Private/Private/Unknown provider mono module
#pragma D attributes Private/Private/Unknown provider mono function
#pragma D attributes Evolving/Evolving/Common provider mono name
#pragma D attributes Evolving/Evolving/Common provider mono args

