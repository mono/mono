REM LineNo: 6
REM ExpectedError: BC30345
REM ErrorMessage: 'Public Function f(ByRef i As Integer) As Object' and 'Public Function f(i1 As Integer) As Object' cannot overload each other because they differ only by parameters declared 'ByRef' or 'ByVal'.

Module OverloadingC2
    Function f(ByRef i As Integer)
    End Function

    Function f(ByVal i1 As Integer)
    End Function

    Sub Main()
    End Sub
End Module
