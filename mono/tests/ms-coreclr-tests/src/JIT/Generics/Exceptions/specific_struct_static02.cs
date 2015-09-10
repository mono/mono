// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;

public struct ValX0 { }
public struct ValY0 { }
public struct ValX1<T> { }
public struct ValY1<T> { }
public struct ValX2<T, U> { }
public struct ValY2<T, U> { }
public struct ValX3<T, U, V> { }
public struct ValY3<T, U, V> { }
public class RefX0 { }
public class RefY0 { }
public class RefX1<T> { }
public class RefY1<T> { }
public class RefX2<T, U> { }
public class RefY2<T, U> { }
public class RefX3<T, U, V> { }
public class RefY3<T, U, V> { }



public class GenException<T> : Exception { }
public struct Gen<T>
{
    public static bool ExceptionTest(bool throwException)
    {
        if (throwException)
        {
            throw new GenException<T>();
        }
        else
        {
            return true;
        }
    }
}
public class Test
{
    public static int counter = 0;
    public static bool result = true;
    public static void Eval(bool exp)
    {
        if (!exp)
        {
            result = exp;
            Console.WriteLine("Test Failed at location: " + counter);
        }
        counter++;

    }

    public static int Main()
    {
        int cLabel = 0;

        while (cLabel < 50)
        {
            try
            {
                switch (cLabel)
                {
                    case 0: cLabel++; Gen<int>.ExceptionTest(true); break;
                    case 1: cLabel++; Gen<double>.ExceptionTest(true); break;
                    case 2: cLabel++; Gen<string>.ExceptionTest(true); break;
                    case 3: cLabel++; Gen<object>.ExceptionTest(true); break;
                    case 4: cLabel++; Gen<Guid>.ExceptionTest(true); break;

                    case 5: cLabel++; Gen<int[]>.ExceptionTest(true); break;
                    case 6: cLabel++; Gen<double[,]>.ExceptionTest(true); break;
                    case 7: cLabel++; Gen<string[][][]>.ExceptionTest(true); break;
                    case 8: cLabel++; Gen<object[, , ,]>.ExceptionTest(true); break;
                    case 9: cLabel++; Gen<Guid[][, , ,][]>.ExceptionTest(true); break;

                    case 10: cLabel++; Gen<RefX1<int>[]>.ExceptionTest(true); break;
                    case 11: cLabel++; Gen<RefX1<double>[,]>.ExceptionTest(true); break;
                    case 12: cLabel++; Gen<RefX1<string>[][][]>.ExceptionTest(true); break;
                    case 13: cLabel++; Gen<RefX1<object>[, , ,]>.ExceptionTest(true); break;
                    case 14: cLabel++; Gen<RefX1<Guid>[][, , ,][]>.ExceptionTest(true); break;
                    case 15: cLabel++; Gen<RefX2<int, int>[]>.ExceptionTest(true); break;
                    case 16: cLabel++; Gen<RefX2<double, double>[,]>.ExceptionTest(true); break;
                    case 17: cLabel++; Gen<RefX2<string, string>[][][]>.ExceptionTest(true); break;
                    case 18: cLabel++; Gen<RefX2<object, object>[, , ,]>.ExceptionTest(true); break;
                    case 19: cLabel++; Gen<RefX2<Guid, Guid>[][, , ,][]>.ExceptionTest(true); break;
                    case 20: cLabel++; Gen<ValX1<int>[]>.ExceptionTest(true); break;
                    case 21: cLabel++; Gen<ValX1<double>[,]>.ExceptionTest(true); break;
                    case 22: cLabel++; Gen<ValX1<string>[][][]>.ExceptionTest(true); break;
                    case 23: cLabel++; Gen<ValX1<object>[, , ,]>.ExceptionTest(true); break;
                    case 24: cLabel++; Gen<ValX1<Guid>[][, , ,][]>.ExceptionTest(true); break;

                    case 25: cLabel++; Gen<ValX2<int, int>[]>.ExceptionTest(true); break;
                    case 26: cLabel++; Gen<ValX2<double, double>[,]>.ExceptionTest(true); break;
                    case 27: cLabel++; Gen<ValX2<string, string>[][][]>.ExceptionTest(true); break;
                    case 28: cLabel++; Gen<ValX2<object, object>[, , ,]>.ExceptionTest(true); break;
                    case 29: cLabel++; Gen<ValX2<Guid, Guid>[][, , ,][]>.ExceptionTest(true); break;

                    case 30: cLabel++; Gen<RefX1<int>>.ExceptionTest(true); break;
                    case 31: cLabel++; Gen<RefX1<ValX1<int>>>.ExceptionTest(true); break;
                    case 32: cLabel++; Gen<RefX2<int, string>>.ExceptionTest(true); break;
                    case 33: cLabel++; Gen<RefX3<int, string, Guid>>.ExceptionTest(true); break;

                    case 34: cLabel++; Gen<RefX1<RefX1<int>>>.ExceptionTest(true); break;
                    case 35: cLabel++; Gen<RefX1<RefX1<RefX1<string>>>>.ExceptionTest(true); break;
                    case 36: cLabel++; Gen<RefX1<RefX1<RefX1<RefX1<Guid>>>>>.ExceptionTest(true); break;

                    case 37: cLabel++; Gen<RefX1<RefX2<int, string>>>.ExceptionTest(true); break;
                    case 38: cLabel++; Gen<RefX2<RefX2<RefX1<int>, RefX3<int, string, RefX1<RefX2<int, string>>>>, RefX2<RefX1<int>, RefX3<int, string, RefX1<RefX2<int, string>>>>>>.ExceptionTest(true); break;
                    case 39: cLabel++; Gen<RefX3<RefX1<int[][, , ,]>, RefX2<object[, , ,][][], Guid[][][]>, RefX3<double[, , , , , , , , , ,], Guid[][][][, , , ,][, , , ,][][][], string[][][][][][][][][][][]>>>.ExceptionTest(true); break;

                    case 40: cLabel++; Gen<ValX1<int>>.ExceptionTest(true); break;
                    case 41: cLabel++; Gen<ValX1<RefX1<int>>>.ExceptionTest(true); break;
                    case 42: cLabel++; Gen<ValX2<int, string>>.ExceptionTest(true); break;
                    case 43: cLabel++; Gen<ValX3<int, string, Guid>>.ExceptionTest(true); break;

                    case 44: cLabel++; Gen<ValX1<ValX1<int>>>.ExceptionTest(true); break;
                    case 45: cLabel++; Gen<ValX1<ValX1<ValX1<string>>>>.ExceptionTest(true); break;
                    case 46: cLabel++; Gen<ValX1<ValX1<ValX1<ValX1<Guid>>>>>.ExceptionTest(true); break;

                    case 47: cLabel++; Gen<ValX1<ValX2<int, string>>>.ExceptionTest(true); break;
                    case 48: cLabel++; Gen<ValX2<ValX2<ValX1<int>, ValX3<int, string, ValX1<ValX2<int, string>>>>, ValX2<ValX1<int>, ValX3<int, string, ValX1<ValX2<int, string>>>>>>.ExceptionTest(true); break;
                    case 49: cLabel++; Gen<ValX3<ValX1<int[][, , ,]>, ValX2<object[, , ,][][], Guid[][][]>, ValX3<double[, , , , , , , , , ,], Guid[][][][, , , ,][, , , ,][][][], string[][][][][][][][][][][]>>>.ExceptionTest(true); break;
                }
            }

            catch (GenException<int>) { Eval(cLabel == 1); }
            catch (GenException<double>) { Eval(cLabel == 2); }
            catch (GenException<string>) { Eval(cLabel == 3); }
            catch (GenException<object>) { Eval(cLabel == 4); }
            catch (GenException<Guid>) { Eval(cLabel == 5); }

            catch (GenException<int[]>) { Eval(cLabel == 6); }
            catch (GenException<double[,]>) { Eval(cLabel == 7); }
            catch (GenException<string[][][]>) { Eval(cLabel == 8); }
            catch (GenException<object[, , ,]>) { Eval(cLabel == 9); }
            catch (GenException<Guid[][, , ,][]>) { Eval(cLabel == 10); }

            catch (GenException<RefX1<int>[]>) { Eval(cLabel == 11); }
            catch (GenException<RefX1<double>[,]>) { Eval(cLabel == 12); }
            catch (GenException<RefX1<string>[][][]>) { Eval(cLabel == 13); }
            catch (GenException<RefX1<object>[, , ,]>) { Eval(cLabel == 14); }
            catch (GenException<RefX1<Guid>[][, , ,][]>) { Eval(cLabel == 15); }
            catch (GenException<RefX2<int, int>[]>) { Eval(cLabel == 16); }
            catch (GenException<RefX2<double, double>[,]>) { Eval(cLabel == 17); }
            catch (GenException<RefX2<string, string>[][][]>) { Eval(cLabel == 18); }
            catch (GenException<RefX2<object, object>[, , ,]>) { Eval(cLabel == 19); }
            catch (GenException<RefX2<Guid, Guid>[][, , ,][]>) { Eval(cLabel == 20); }
            catch (GenException<ValX1<int>[]>) { Eval(cLabel == 21); }
            catch (GenException<ValX1<double>[,]>) { Eval(cLabel == 22); }
            catch (GenException<ValX1<string>[][][]>) { Eval(cLabel == 23); }
            catch (GenException<ValX1<object>[, , ,]>) { Eval(cLabel == 24); }
            catch (GenException<ValX1<Guid>[][, , ,][]>) { Eval(cLabel == 25); }

            catch (GenException<ValX2<int, int>[]>) { Eval(cLabel == 26); }
            catch (GenException<ValX2<double, double>[,]>) { Eval(cLabel == 27); }
            catch (GenException<ValX2<string, string>[][][]>) { Eval(cLabel == 28); }
            catch (GenException<ValX2<object, object>[, , ,]>) { Eval(cLabel == 29); }
            catch (GenException<ValX2<Guid, Guid>[][, , ,][]>) { Eval(cLabel == 30); }

            catch (GenException<RefX1<int>>) { Eval(cLabel == 31); }
            catch (GenException<RefX1<ValX1<int>>>) { Eval(cLabel == 32); }
            catch (GenException<RefX2<int, string>>) { Eval(cLabel == 33); }
            catch (GenException<RefX3<int, string, Guid>>) { Eval(cLabel == 34); }

            catch (GenException<RefX1<RefX1<int>>>) { Eval(cLabel == 35); }
            catch (GenException<RefX1<RefX1<RefX1<string>>>>) { Eval(cLabel == 36); }
            catch (GenException<RefX1<RefX1<RefX1<RefX1<Guid>>>>>) { Eval(cLabel == 37); }

            catch (GenException<RefX1<RefX2<int, string>>>) { Eval(cLabel == 38); }
            catch (GenException<RefX2<RefX2<RefX1<int>, RefX3<int, string, RefX1<RefX2<int, string>>>>, RefX2<RefX1<int>, RefX3<int, string, RefX1<RefX2<int, string>>>>>>) { Eval(cLabel == 39); }
            catch (GenException<RefX3<RefX1<int[][, , ,]>, RefX2<object[, , ,][][], Guid[][][]>, RefX3<double[, , , , , , , , , ,], Guid[][][][, , , ,][, , , ,][][][], string[][][][][][][][][][][]>>>) { Eval(cLabel == 40); }

            catch (GenException<ValX1<int>>) { Eval(cLabel == 41); }
            catch (GenException<ValX1<RefX1<int>>>) { Eval(cLabel == 42); }
            catch (GenException<ValX2<int, string>>) { Eval(cLabel == 43); }
            catch (GenException<ValX3<int, string, Guid>>) { Eval(cLabel == 44); }

            catch (GenException<ValX1<ValX1<int>>>) { Eval(cLabel == 45); }
            catch (GenException<ValX1<ValX1<ValX1<string>>>>) { Eval(cLabel == 46); }
            catch (GenException<ValX1<ValX1<ValX1<ValX1<Guid>>>>>) { Eval(cLabel == 47); }

            catch (GenException<ValX1<ValX2<int, string>>>) { Eval(cLabel == 48); }
            catch (GenException<ValX2<ValX2<ValX1<int>, ValX3<int, string, ValX1<ValX2<int, string>>>>, ValX2<ValX1<int>, ValX3<int, string, ValX1<ValX2<int, string>>>>>>) { Eval(cLabel == 49); }
            catch (GenException<ValX3<ValX1<int[][, , ,]>, ValX2<object[, , ,][][], Guid[][][]>, ValX3<double[, , , , , , , , , ,], Guid[][][][, , , ,][, , , ,][][][], string[][][][][][][][][][][]>>>) { Eval(cLabel == 50); }
        }

        if (result)
        {
            Console.WriteLine("Test Passed");
            return 100;
        }
        else
        {
            Console.WriteLine("Test Failed");
            return 1;
        }
    }

}
