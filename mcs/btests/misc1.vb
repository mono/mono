' This test checks for the follwoing features
' # comments
' # Line Continuation
' # white space
' # case sensitivity

REM check comments ;-)

Module misc1
    Function LineContinuation( _
        ByVal a As Integer, _
        ByVal b As Integer _
        )
        If a > b Then
            a = b
        Else
            b = a
        End If
    End Function

    Function WhiteSpace(ByVal a As Integer, ByVal b As Integer)
        If (a > b) Then
            a = b
        End If
    End Function

    Function CaseSensitivity(ByVal A As Integer, ByVal b As Integer)
        If (A > b) Then
            A = b
        End If
    End Function

    Sub Main()
	try
        LineContinuation(10, 20)
        WhiteSpace(10, 20)
        CaseSensitivity(10, 20)
	catch e as System.Exception
		 System.Console.WriteLine(e.message)
	end try
    End Sub

End Module
