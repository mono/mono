'System.DivideByZeroException: Attempt to divide by Zero is not valid'
                                                                                
Imports System
Imports Nunit.Framework
                                                                                
<TestFixture> _
Public Class ModByZero
                                                                                
                <Test, ExpectedException (GetType (DivideByZeroException))> _
                Public Sub TestForMod ()
                        Dim A As Integer
                        A = 12.345
                        Dim Zero As Integer = 0
                        A = A Mod 0
                                                                                
                End Sub

End Class









