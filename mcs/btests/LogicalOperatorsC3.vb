REM LineNo: 37
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Integer' to 'Boolean'.

REM LineNo: 38
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Integer' to 'Boolean'.

REM LineNo: 38
REM ExpectedError: BC30512
REM ErrorMessage: Option Strict On disallows implicit conversions from 'Integer' to 'Boolean'.

' BC30512: Option Strict On disallows implicit conversions from 'Integer' to 'Boolean'.

Option Strict On
Imports System

Module LogicalOperatorsC3

    Sub Main()
        Console.WriteLine(f())
    End Sub

    Function f1() As Integer
        Console.WriteLine("Function f1() is called")
        Return 1
    End Function

    Function f2() As Boolean
        Console.WriteLine("Function f2() is called")
        Return False
    End Function

    Function f() As Integer

        Dim a1, a2 As Integer
        a1 = f1() AndAlso f2()
        a2 = a1 OrElse f1()

        Return 0

    End Function

End Module