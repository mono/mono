REM LineNo: 6
REM ExpectedError: BC30345
REM ErrorMessage: 'Public Function f(i As Integer) As Object' and 'Public Function f(ByRef i As Integer) As Object' cannot overload each other because they differ only by parameters declared 'ByRef' or 'ByVal'.

Module OverloadingC1
    Function f(ByVal i As Integer)
    End Function

    Function f(ByRef i As Integer)
    End Function

    Sub Main()
    End Sub
End Module
