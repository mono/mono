'Expected: System.DivideByZeroException, divide by Zero is not valid'
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class ArithmeticOperators2
                                                                                
                <Test, ExpectedException (GetType (DivideByZeroException))> _
                Public Sub TestForMod ()
                        Dim A1 As Integer
                        A1 = 12.345
                        Dim Zero As Integer = 0
                        A1 = A1 Mod 0
                                                                                
                End Sub

End Class









