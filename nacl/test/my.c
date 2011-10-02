#include <stdio.h>
#include <stdlib.h>
#include <sys/stat.h>
#if defined(__GLIBC__)
#define __USE_GNU
#include <link.h>
#include <malloc.h>
#endif

#include <mono/metadata/appdomain.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-debug.h>

extern void* mono_aot_module_mscorlib_info;

extern void* mono_aot_module_hw_info;

extern void mono_set_corlib_data(void *data, size_t size);
extern void mono_aot_register_module(void *aot_info);
extern void mono_aot_init(void);
extern void mono_jit_set_aot_only(int aot_only);
extern MonoDomain * mini_init (const char *filename, const char *runtime_version);

#if !defined(TRUE)
#define TRUE 1
#endif
#if !defined(FALSE)
#define FALSE 0
#endif

void my_c_func(int arg, const char *str, double d) {
  /* str from c# is immutable */
  printf("*** my_c_func(%d, '%s', %1.4f) received\n", arg, str, (float)d);
}

void my_c_pass(int x) {
  char *msg = "undefined";
  switch(x) {
    case 0:  msg = "about to throw an exception...";  break;
    case 1:  msg = "thrown invalid cast exception was not caught!"; break;
    case 2:  msg = "thrown invalid cast exception was safely caught!"; break;
    case 3:  msg = "...leaving exeception test."; break;
    case 4:  msg = "generated invalid cast exception was not caught!"; break;
    case 5:  msg = "generated invalid cast exception was safely caught!"; break;
  }
  printf("*** my_c_pass(%d): %s\n", x, msg);
}

void try_mono() {
  MonoDomain *domain;
  MonoAssembly *ma;
  MonoImage *mi;
  MonoClass *mc;
  MonoMethodDesc *mmd;
  MonoMethod *mm;
  MonoObject *mo;
  FILE *mscorlib;
  char *corlib_data = NULL;
  void *args [2];
  static int x = 123000;
  args [0] = &x;
  args [1] = "hello world";

#if defined(__native_client__)
  mscorlib = fopen("mscorlib.dll", "r");
  if (NULL != mscorlib) {
    size_t size;
    struct stat st;
    if (0 == stat("mscorlib.dll", &st)) {
      size = st.st_size;
      printf("reading mscorlib.dll, size %ld\n", size);
      corlib_data = malloc(size);
      if (corlib_data != NULL) {
        while (fread(corlib_data, 1, size, mscorlib) != 0) ;
        if (!ferror(mscorlib)) {
          mono_set_corlib_data(corlib_data, size);
        } else {
          perror("error reading mscorlib.dll");
          free(corlib_data);
          corlib_data = NULL;
        }
      } else {
        perror("Could not allocate memory");
      }
    } else {
      perror("stat error");
    }
    fclose(mscorlib);
  }
#endif

#ifdef AOT_VERSION
  printf("address of mono_aot_module_mscorlib_info:  %p\n", mono_aot_module_mscorlib_info);
  printf("address of mono_aot_module_hw_info:  %p\n", mono_aot_module_hw_info);

  // mono_jit_set_aot_only(TRUE) should be enabled now.
  // if not enabled, I suspect we're still jitting...
  mono_jit_set_aot_only(TRUE);

  mono_aot_register_module(mono_aot_module_mscorlib_info);
  mono_aot_register_module(mono_aot_module_hw_info);
#endif

  mono_debug_init(MONO_DEBUG_FORMAT_MONO);
  domain = mono_jit_init("hw.exe", "v2.0.50727");

  printf("mono domain: %p\n", domain);

  ma = mono_domain_assembly_open(domain, "hw.exe");
  printf("mono assembly: %p\n", ma);

  mi = mono_assembly_get_image(ma);
  printf("mono image: %p\n", mi);

  mc = mono_class_from_name(mi, "Test", "HelloWorld");
  printf("mono class: %p\n", mc);

  mmd = mono_method_desc_new("Test.HelloWorld:Foobar(int,string)", TRUE);
  printf("mono desc method: %p\n", mmd);

  mm = mono_method_desc_search_in_image(mmd, mi);
  printf("mono method: %p\n", mm);

  // add c functions for mono test code to invoke
  mono_add_internal_call("Test.c_code::my_c_func", (void *) my_c_func);
  mono_add_internal_call("Test.c_code::my_c_pass", (void *) my_c_pass);

  mo = mono_runtime_invoke(mm, NULL, args, NULL);

  printf("mono object: %p\n", mo);
  if (NULL != corlib_data) free(corlib_data);
}

#if defined(__GLIBC__)
int dl_printer(struct dl_phdr_info *info, size_t size, void *data) {
    int j;
    printf("DL name=%s (%d segments)\n", info->dlpi_name,
        info->dlpi_phnum);
    for (j = 0; j < info->dlpi_phnum; j++) {
        if (info->dlpi_phdr[j].p_type == PT_LOAD)
         printf("\t\t header %2d: address=%10p\n", j,
             (void *) (info->dlpi_addr + info->dlpi_phdr[j].p_vaddr));
    }
    return 0;
}
#endif

int main() {
  int i;
  setvbuf(stdout, NULL, _IONBF, 0);
  printf("address of main(): %p\n", main);
  printf("address of stack : %p\n", &i);
#if defined(__GLIBC__)
  mallopt(M_TRIM_THRESHOLD, -1);
  dl_iterate_phdr(dl_printer, NULL);
#endif
  printf("\nProgram a.out output:\n");
  printf("==========================\n");
  try_mono();
  printf("==========================\n\n");
  return 0;
}
