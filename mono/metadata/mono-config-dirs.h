/**
 * \file
 */

#ifndef __MONO_CONFIG_INTERNAL_H__
#define __MONO_CONFIG_INTERNAL_H__

#include <config.h>
#include <glib.h>

G_BEGIN_DECLS

const char*
mono_config_get_assemblies_dir (void);

const char*
mono_config_get_cfg_dir (void);

const char*
mono_config_get_bin_dir (void);

const char*
mono_config_get_reloc_lib_dir (void);

G_END_DECLS

#endif
