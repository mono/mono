' Author:
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná


Imports System
Imports System.Collections

Module ForEachC

    Class C1
        Public ReadOnly index As string = ""

        Sub New()
            Dim arr As New ArrayList
	    arr.Add("a")
	    arr.Add("b")
	    arr.Add("c")

            For Each index In arr
            Next
        End Sub

    End Class

    Sub main()
        Dim c As New C1()
        If not c.index.Equals("c") Then
            Throw New Exception("#FEC1")
        End If
    End Sub

End Module
