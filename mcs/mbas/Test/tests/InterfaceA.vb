Interface I
    Function F()
    Sub S (i as integer)
    Property P
    Event e (i as integer)
End Interface

Class C
    Implements I

    Function F() Implements I.F
    End Function

    Sub S(i as integer) Implements I.S
    End Sub

   Property P Implements I.P
	Get
	End Get
	Set
	End Set
   End Property

   Event e(i as integer) Implements I.e
End Class

Module InterfaceA
    Sub Main()
        Dim x As C = New C()
        x.F()

        Dim y As I = New C()
        y.F()
    End Sub
End Module
