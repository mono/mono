REM LineNo: 26
REM ExpectedError: BC30455
REM ErrorMessage: Argument not specified for parameter 'i1' of 'Public Shared Function F(i1 As Integer, i2 As Integer) As Object'.

REM LineNo: 26
REM ExpectedError: BC30455
REM ErrorMessage: Argument not specified for parameter 'i2' of 'Public Shared Function F(i1 As Integer, i2 As Integer) As Object'.

REM LineNo: 27
REM ExpectedError: BC30455
REM ErrorMessage: Argument not specified for parameter 'i2' of 'Public Shared Function F(i1 As Integer, i2 As Integer) As Object'.

Class C1
    Shared Function F()
    End Function

    Shared Function F(ByVal i As Integer)
    End Function

    Class C2
        Shared Function F(ByVal i1 As Integer, ByVal i2 As Integer)
        End Function

        Function fnc()
            F(10, 20)
            F()
            F(10)
        End Function

    End Class
End Class



Module ScopeC1
    Sub Main()
    End Sub
End Module
