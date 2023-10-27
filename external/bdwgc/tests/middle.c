/*
 * Test at the boundary between small and large objects.
 * Inspired by a test case from Zoltan Varga.
 */
#include "gc.h"
#include <stdio.h>

int main (void)
{
  int i;

  GC_set_all_interior_pointers(0);
  GC_INIT();

  for (i = 0; i < 20000; ++i) {
    (void)GC_malloc_atomic(4096);
    (void)GC_malloc(4096);
  }
  for (i = 0; i < 20000; ++i) {
    (void)GC_malloc_atomic(2048);
    (void)GC_malloc(2048);
  }
  printf("Final heap size is %lu\n", (unsigned long)GC_get_heap_size());
  return 0;
}
