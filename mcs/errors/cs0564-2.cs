// CS0564: Overloaded shift operator must have the type of the first operand be the containing type, and the type of the second operand must be int
// Line: 5

class SampleClass {
        public static int operator << (object value, int count) {
                return 0;
        }
}
