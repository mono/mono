#ifndef NL_H
#define NL_H
#include <glib.h>

G_BEGIN_DECLS
int CreateNLSocket (void);
int ReadEvents (gpointer sock, gpointer buffer, gint32 count, gint32 size);
int CloseNLSocket (gpointer sock);
G_END_DECLS

#endif

