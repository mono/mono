REM LineNo: 6
REM ExpectedError: BC30345
REM ErrorMessage: 'Public Function f(i As Integer) As Object' and 'Private Function f(ByRef i As Integer) As Object' cannot overload each other because they differ only by parameters declared 'ByRef' or 'ByVal'.

Module OverloadingC3
    Public Function f(ByVal i As Integer)
    End Function

    Private Function f(ByRef i As Integer)
    End Function

    Sub Main()
    End Sub
End Module
