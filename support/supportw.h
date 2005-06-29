#ifndef __SUPPORTW_H
#define __SUPPORTW_H

G_BEGIN_DECLS

gboolean supportw_register_delegate (const char *function_name, void *fnptr);
void supportw_test_all (void);

G_END_DECLS

#endif /* __SUPPORTW_H */

