Interface I
    Sub F1(ByVal i As Integer)
    Sub F2(ByVal i As Integer)
End Interface

Class C1
    Implements I

    Public Sub F1(ByVal i As Integer) Implements I.F1
    End Sub
    Public Sub F2(ByVal i As Integer) Implements I.F2
    End Sub
End Class

Module InterfaceD
    Sub Main()
        Dim myC As C1 = New C1()
	myC.F1(10)
	myC.F2(20)
    End Sub
End Module
