imports System

Module IntegerLiteral2Test
    Sub main()
	    Dim i As Integer
            i = &H2B
            If (i <> 43) Then
                Throw New Exception("#A1 : Unexpected behaviour")
            End If

            i = &O35
            If (i <> 29) Then
                Throw New Exception("#A2 : Unexpected behaviour")
            End If
    End Sub
End Module
