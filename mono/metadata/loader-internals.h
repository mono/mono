/**
* \file
*/

#ifndef _MONO_METADATA_LOADER_INTERNALS_H_
#define _MONO_METADATA_LOADER_INTERNALS_H_

#ifdef __cplusplus
extern "C++" // in case of surrounding extern "C"
{

template <typename T>
inline void
mono_add_internal_call (const char *name, T method)
{
	return mono_add_internal_call (name, (const void*)method);
}

} // extern "C++"

#endif // __cplusplus

#endif
