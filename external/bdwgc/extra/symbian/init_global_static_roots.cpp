// Symbian-specific file.

// INCLUDE FILES
#include <e32def.h>

#include "private/gcconfig.h"
#include "gc.h"

extern "C" {

void GC_init_global_static_roots()
{
    ptr_t dataStart = NULL;
    ptr_t dataEnd = NULL;
#   if defined (__WINS__)
        extern int winscw_data_start, winscw_data_end;
        dataStart = ((ptr_t)&winscw_data_start);
        dataEnd   = ((ptr_t)&winscw_data_end);
#   else
        extern int Image$$RW$$Limit[], Image$$RW$$Base[];
        dataStart = ((ptr_t)Image$$RW$$Base);
        dataEnd   = ((ptr_t)Image$$RW$$Limit);
#   endif

    GC_add_roots(dataStart, dataEnd);
}

} /* extern "C" */
