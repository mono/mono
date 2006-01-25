'To see if AddHandler supports non-AddressOf arguments

Imports System

Class C
    Public Event E As EventHandler

    Public Sub S()
        RaiseEvent E(Me, EventArgs.Empty)
    End Sub
End Class

Class C1
    Dim x As C = New C

    Sub setHandler()
        AddHandler x.E, New EventHandler(AddressOf xh)
    End Sub

    Sub unsetHandler()
        RemoveHandler x.E, New EventHandler(AddressOf xh)
    End Sub

    Sub call_S()
        x.S()
    End Sub

    Sub xh(ByVal sender As Object, ByVal e As EventArgs)
        Console.WriteLine("event called")
    End Sub
End Class

Module M
    Sub Main()
        Dim y As New C1
        y.setHandler()
        y.call_S()
        y.unsetHandler()
        y.call_S()
    End Sub

End Module
