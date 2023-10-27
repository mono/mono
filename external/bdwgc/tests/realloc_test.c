
#include <stdio.h>
#include <stdlib.h>
#include "gc.h"

#define COUNT 10000000

int main(void) {
  int i;
  unsigned long last_heap_size = 0;

  GC_INIT();

  for (i = 0; i < COUNT; i++) {
    int **p = GC_NEW(int *);
    int *q = (int*)GC_MALLOC_ATOMIC(sizeof(int));

    if (p == 0 || *p != 0) {
      fprintf(stderr, "GC_malloc returned garbage (or NULL)\n");
      exit(1);
    }

    *p = (int*)GC_REALLOC(q, 2 * sizeof(int));

    if (i % 10 == 0) {
      unsigned long heap_size = (unsigned long)GC_get_heap_size();
      if (heap_size != last_heap_size) {
        printf("Heap size: %lu\n", heap_size);
        last_heap_size = heap_size;
      }
    }
  }
  return 0;
}
