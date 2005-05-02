Imports System


Module WithStatementB
    Class C1
        Public a1 As Integer = 10
        Friend a2 As String = "Hello"
        Sub f1()
     		dim flag as boolean = True
		if a1=20 and a2="Hello World"
			flag = false
		end if
		if a1=10 and a2="Hello"
			flag = false
		end if
		if flag<>False
			throw new System.Exception("#A WithStatement not working")			
		end if

        End Sub
    End Class

    Sub main()
        Dim a As New C1()
        With a
            a.a1 = 20
            .a2 = "Hello World"
            Dim x As New C1()
            a = x  ' Tried reassiging the object inside With statement
            If .a1 = a.a1 Or .a2 = a.a2 Then
                Throw New Exception("#WS1 - With Statement failed")
            End If
            a.f1()
            .f1()
        End With

    End Sub

End Module