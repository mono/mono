REM LineNo: 10
REM ExpectedError: BC30300
REM ErrorMessage: 'Public Function f(ByRef i As Integer) As Object' and 'Public Function f(i1 As Integer, [i2 As Integer = 5]) As Object' cannot overload each other because they differ only by optional parameters.

REM LineNo: 10
REM ExpectedError: BC30345
REM ErrorMessage: 'Public Function f(ByRef i As Integer) As Object' and 'Public Function f(i1 As Integer, [i2 As Integer = 5]) As Object' cannot overload each other because they differ only by parameters declared 'ByRef' or 'ByVal'.

Module OverloadingC2
    Function f(ByRef i As Integer)
    End Function

    Function f(ByVal i1 As Integer, Optional ByVal i2 As Integer = 5)
    End Function

    Sub Main()
    End Sub
End Module
