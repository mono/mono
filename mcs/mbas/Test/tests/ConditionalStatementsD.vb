Imports System

Module ConditionalStatementsD

    Sub Main()

        Dim i As Integer
        Dim sarr() As String = {"cat", "awk", "zebra", "mouse", "snake", "tiger", "lion"}
	Dim str As String = "Lion"
	Dim arr(6) As Integer
	
        For i = 0 To 6

            Select sarr(i)
                Case "ant" To "cow"
                    arr(i) = 1
                Case < "dog", = "tiger", str
                    arr(i) = 2
		Case "lion"
		    arr(i) = 3
                Case >= "elepahant"
                    arr(i) = 4
                Case Else
                    arr(i) = 5
            End Select

        Next

        If arr(0) <> 1 Or arr(1) <> 1 Then
            Throw New Exception("#CSD1 - Switch Statement failed")
        ElseIf arr(5) <> 2 Then
            Throw New Exception("#CSD2 - Switch Statement failed")
        ElseIf arr(6) <> 3 Then
            Throw New Exception("#CSD3 - Switch Statement failed")
	ElseIf arr(2) <> 4 Or arr(3) <> 4 Or arr(4) <> 4 Then
            Throw New Exception("#CSD4 - Switch Statement failed")
        Else
            Console.WriteLine("OK")
        End If

    End Sub

End Module