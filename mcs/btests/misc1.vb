Imports System
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
        If  (a > b) Then
            a  = b		   'Single extra whitespace 	 
        End If
	Dim e     As Integer       'Whitespaces with a sequence of spaces
	Dim c  		As Integer 'Whitespaces with a combination of tabs and spaces
	Dim d		As Integer 'Whitespaces with sequence of tabs
    End Function

    Function CaseSensitivity(ByVal A As Integer, ByVal b As Integer)
        If (A > b) Then
            A = b
        End If
    End Function

    Function Comment(ByVal ap As Integer,byVal bp As Integer)  'Comment function start
        Dim a As Integer REM Declaration
        '' Comment with two single quotes
        REM REM Comment With two REM's
                                                                                                                             
        Console.WriteLine(a)'Output
                                                                                                                             
        Dim b  As String="Hello'"            'The single quote within the string should not be treated as comment
        Dim c As String="Hello REM"          'The REM within the string should not be treated as comment
    End Function


    Sub main()
	try
        LineContinuation(10, 20)
        WhiteSpace(10, 20)
        CaseSensitivity(10, 20)
	Comment(10, 20)
	catch e as System.Exception
		 System.Console.WriteLine(e.message)
	end try
    End Sub

End Module
