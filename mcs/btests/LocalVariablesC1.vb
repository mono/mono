REM LineNo: 12
REM ExpectedError: BC30616
REM ErrorMessage: Variable 'a' hides a variable in an enclosing block.

Imports System

Module LocalVariablesC1

    Function f1(ByVal a As Integer) As Integer

        Dim b As Integer = 10
	    For a As Integer = 0 to 10
            Console.WriteLine(a)
        Next
        Return a + b

    End Function

    Sub Main()
        f1(0)
    End Sub

End Module