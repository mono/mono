' In this test all implemented
' member names are different from 
' the interface member names

Delegate Sub d()

Interface I
    Function F()
    Sub S (i as integer)
    Property P
    Event e (i as integer)
    Event e1 as d
End Interface

Class C
    Implements I

    Function CF() Implements I.F
    End Function

   Sub CS(i as integer) Implements I.S
   End Sub

   Sub S1(i as integer)
   End Sub


   Property CP Implements I.P
	Get
	End Get
	Set
	End Set
  End Property

  Event Ce(i as integer) Implements I.e

  Event Ce1 as d implements I.e1
   
End Class

Module InterfaceA
    Sub Main()
        Dim x As C = New C()
        x.CF()

        Dim y As I = New C()
        y.F()
    End Sub
End Module
