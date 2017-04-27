#include <glib.h>
#include "Thread-c-api.h"

void
g_usleep(gulong microseconds)
{
    UnityPalSleep(microseconds/1000);
}
