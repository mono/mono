/* Copyright Joyent, Inc. and other Node contributors. All rights reserved.
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */
 
#include "mini.h"
#include "proctitle.h"

#if defined(PLATFORM_MACOSX) || defined(PLATFORM_LINUX)

static void* args_mem;

static struct {
  char* str;
  size_t len;
} mono_process_title;

char** mono_proctitle_start (int argc, char** argv) {
  char** new_argv;
  size_t size;
  char* s;
  int i;

  if (argc <= 0)
    return argv;

  /* Calculate how much memory we need for the argv strings. */
  size = 0;
  for (i = 0; i < argc; i++)
    size += strlen(argv[i]) + 1;

  mono_process_title.str = argv[0];
  mono_process_title.len = argv[argc - 1] + strlen(argv[argc - 1]) - argv[0];
  assert(mono_process_title.len + 1 == size);  /* argv memory should be adjacent. */

  /* Add space for the argv pointers. */
  size += (argc + 1) * sizeof(char*);

  new_argv = malloc(size);
  if (new_argv == NULL)
    return argv;
  args_mem = new_argv;

  /* Copy over the strings and set up the pointer table. */
  s = (char*) &new_argv[argc + 1];
  for (i = 0; i < argc; i++) {
    size = strlen(argv[i]) + 1;
    memcpy(s, argv[i], size);
    new_argv[i] = s;
    s += size;
  }
  new_argv[i] = NULL;

  return new_argv;
}

int mono_proctitle_set(const char* title) 
{
  if (mono_process_title.len == 0)
    return 0;

  /* No need to terminate, byte after is always '\0'. */
  strncpy(mono_process_title.str, title, mono_process_title.len);
  
#ifdef PLATFORM_MACOSX  
  mono_proctitle_set_macosx(title);
#endif

  return 0;
}

void mono_proctitle_shutdown()
{
  free(args_mem);
  args_mem = NULL;
}

#else

char** mono_proctitle_start (int argc, char **argv)
{
    return argv;
}

int mono_proctitle_set(const char* title)
{
    return 0;
}

void mono_proctitle_shutdown()
{
}

#endif
