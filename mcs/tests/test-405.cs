// Compiler options: -unsafe

using System;

namespace testapp{
        public unsafe class LibTestAPI{

                struct LibTestStruct{
                        void* pData;
                        void* pTest1;
                }

                LibTestStruct* the_struct;

                public void Create(){
                        IntPtr MyPtr = new IntPtr(0); // Usually created elsewhere
                        the_struct = (LibTestStruct *) 0;  // error CS1002
                }
        }

        class TestApp{
                public static void Main(string[] args){
                        LibTestAPI myapi = new LibTestAPI();
                        myapi.Create();
                }
        }
}

