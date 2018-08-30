#ifdef _MSC_VER
#define EXPORT __declspec(dllexport)
#else
#define EXPORT
#endif

EXPORT void TestMarshalling (void** ptr) {
    *ptr = (void*)2;
}