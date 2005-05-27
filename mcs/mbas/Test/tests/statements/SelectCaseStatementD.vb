'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.

' Testing whether variables work well as 'case' expressions
' 'Select' expressions and 'case' expressions are of different types

Imports System

Module SelectCaseStatementD

    Sub Main()
	Dim errMsg As String = ""
	Dim numMatches As Integer = 0

        Dim a As Byte = 5
        Dim b As Integer = 5
        Dim c As Decimal = 10
        Dim d As String = "15"

        Select Case a
	Case is <= b
		numMatches += 1
	Case >= c
		errMsg = errMsg & "#A1 Case statement not working with Select Expression of type byte " & vbCrLf
	Case c To d 
		errMsg = errMsg & "#A1 Case statement not working with Select Expression of type byte" & vbCrLf
        End Select

        a = 12
        Select Case a
	Case <= d 
		numMatches += 1
	Case is = c 
		errMsg = errMsg & "#A2 Case statement not working with Select Expression of type Short" & vbCrLf
	Case <> b 
		errMsg = errMsg & "#A2 Case statement not working with Select Expression of type Short" & vbCrLf
	End Select
		
        a = 20
        Select Case a
	Case < c, is = b, d To 90
		numMatches += 1
	Case Else
		errMsg = errMsg & "#A3 Case statement not working with Select Expression of type Integer" & vbCrLf
	End Select
	if (errMsg <> "")
		throw new Exception (errMsg)
	End If
	if (numMatches <> 3)
		throw new Exception ("Select Case statement not working properly")
	End If
    End Sub

End Module
