Imports System

Module ForA

    Sub main()
        Dim i, j, k As Integer

        For i = 0 To 10
            If i = 1 Then
                Exit For
            End If
            For j = 0 To 10 Step 2
                If j = 2 Then
                    Exit For
                End If
                For k = 0 To 10 Step 3
                    If k = 3 Then
                        Exit For
                    End If
			  if i<>0 and j<>0 and k<>0
	                    throw new System.Exception("#A1 For not working")
			  end if
                Next k
            Next
        Next i

        If i <> 1 Or j <> 2 Or k <> 3 Then
            Throw New Exception("#ForA1 - For..Next Statement failed")
        End If

    End Sub

End Module