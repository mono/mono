using System;
public class Test<T>{
        private T[,] data;
        public Test(T[,] data){
                this.data = data;
        }
}
public class Program{
        public static void Main(string[] args){
                Test<double> test = new Test<double>(new double[2,2]);
        }
}     

