#ifndef __MONO_METHOD_STATE_H__
#define __MONO_METHOD_STATE_H__

/*
This struct tracks the state required by the front-end when translating a particular method body.
When there's inlining, another PMS will be computed and used.

BIG WARNING:
DON'T USE fields tagged with internal, use the accessors down this file, pretty please.

Notes:

Right now it's all static inline methods in this header to ensure O2 can optimize the indirection through those functions.
Maybe a better name could be PerMethodFrontEndData
*/
typedef struct {
	//Skip visibility checks
	gboolean skip_visibility : 1; /* internal */
	//Skip JIT internal verification
	gboolean dont_verify : 1; /* internal */
	//Skip verification of stlic
	gboolean dont_verify_stloc : 1; /* internal */
} PerMethodState;


static void
pms_init (PerMethodState *pms, MonoDomain *domain, MonoMethod *method)
{
	pms->skip_visibility = method->skip_visibility;
	/* serialization and xdomain stuff may need access to private fields and methods */
	pms->dont_verify = m_class_get_image (method->klass)->assembly->corlib_internal? TRUE: FALSE;
	pms->dont_verify |= method->wrapper_type == MONO_WRAPPER_XDOMAIN_INVOKE;
	pms->dont_verify |= method->wrapper_type == MONO_WRAPPER_XDOMAIN_DISPATCH;
 	pms->dont_verify |= method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE; /* bug #77896 */
	pms->dont_verify |= method->wrapper_type == MONO_WRAPPER_COMINTEROP;
	pms->dont_verify |= method->wrapper_type == MONO_WRAPPER_COMINTEROP_INVOKE;

	/* still some type unsafety issues in marshal wrappers... (unknown is PtrToStructure) */
	pms->dont_verify_stloc = method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE;
	pms->dont_verify_stloc |= method->wrapper_type == MONO_WRAPPER_UNKNOWN;
	pms->dont_verify_stloc |= method->wrapper_type == MONO_WRAPPER_NATIVE_TO_MANAGED;
	pms->dont_verify_stloc |= method->wrapper_type == MONO_WRAPPER_STELEMREF;

	/* SkipVerification is not allowed if core-clr is enabled */
	if (!pms->dont_verify && mini_assembly_can_skip_verification (domain, method)) {
		pms->dont_verify = TRUE;
		pms->dont_verify_stloc = TRUE;
	}
}

static gboolean
pms_should_skip_dead_blocks (PerMethodState *pms)
{
	return !pms->dont_verify;
}

static gboolean
pms_verify_stloc (PerMethodState *pms)
{
	return !pms->dont_verify_stloc;
}

static gboolean
pms_should_check_visiblity (PerMethodState *pms)
{
	return !pms->dont_verify && !pms->skip_visibility;
}

#endif