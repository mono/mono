// CS0122: `A.output' is inaccessible due to its protection level
// Line: 12

public class A {
        private string output;
}

public class B : A {
        public void Test() {
                switch ("a") {
                        case "1":
                                output.Replace("a", "b");
                                break;
                }
        }
}
