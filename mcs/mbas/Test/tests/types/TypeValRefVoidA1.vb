REM LineNo: 8
REM ExpectedError: BC30649 
REM ErrorMessage: Void is an unsupported type

Imports System
Module Test 
Sub main () 
Dim A = GetType(System.Void)
Console.WriteLine(A)
End Sub
End Module
 