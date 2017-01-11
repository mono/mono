
#ifndef __MONO_UTILS_W32API_H__
#define __MONO_UTILS_W32API_H__

#include <glib.h>

G_BEGIN_DECLS

#ifndef HOST_WIN32

#define WAIT_FAILED        ((gint) 0xFFFFFFFF)
#define WAIT_OBJECT_0      ((gint) 0x00000000)
#define WAIT_ABANDONED_0   ((gint) 0x00000080)
#define WAIT_TIMEOUT       ((gint) 0x00000102)
#define WAIT_IO_COMPLETION ((gint) 0x000000C0)

#define WINAPI

typedef guint32 DWORD;
typedef gboolean BOOL;
typedef gint32 LONG;
typedef guint32 ULONG;
typedef guint UINT;

typedef gpointer HANDLE;
typedef gpointer HMODULE;

#endif /* HOST_WIN32 */

G_END_DECLS

#endif /* __MONO_UTILS_W32API_H__ */
