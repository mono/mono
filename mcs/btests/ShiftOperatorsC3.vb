REM LineNo: 14
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected.

' BC30201: Expression expected

Imports System


Module ShiftOperatorsC2

    Sub Main()
        Dim a1 As Integer = 20
        a1 =  << 30
        Console.WriteLine(a1)
    End Sub

End Module