REM LineNo: 11
REM ExpectedError: BC30149
REM ErrorMessage: 'B' must implement 'Function F2(i As Integer) As Object' for interface 'I'.

Interface I
    Function F1(ByVal i As Integer)
    Function F2(ByVal i As Integer)
End Interface

Class B
    Implements I

    Overridable Function CF1(ByVal i As Integer) Implements I.F1
    End Function
End Class

Module InterfaceC3
    Sub Main()
    End Sub
End Module
