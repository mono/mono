// CS0618: `ObsoleteDispose' is obsolete: `Class is obsolete'
// Line: 9
// Compiler options: -reference:CS0618-2-lib.dll -warnaserror

class AA
{
        public AA ()
        {
                using (ObsoleteDispose od = ObsoleteDispose.Factory) {
                }
        }
}