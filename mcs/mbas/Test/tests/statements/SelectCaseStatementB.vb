'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.

' Testing whether all kinds of primitive types work well as 'Select' expression
' Testing the case clauses like 'x To Y'

Imports System

Module SelectCaseStatementB

    Sub Main()
	Dim errMsg As String = ""
	Dim numMatches As Integer = 0

        Dim a As Byte = 12
        Select Case a
	Case 1 To 11 
		errMsg = errMsg & "#A1 Case statement not working with Select Expression of type byte " & vbCrLf
	Case 12 To 23
		numMatches += 1
		Console.WriteLine ("Byte")
	Case 5 To 16 
		errMsg = errMsg & "#A1 Case statement not working with Select Expression of type byte" & vbCrLf
        End Select

        Dim b As Short = 234
        Select Case b
	Case 150 To 23 
		errMsg = errMsg & "#A2 Case statement not working with Select Expression of type Short" & vbCrLf
	Case 150 To 200 
		errMsg = errMsg & "#A2 Case statement not working with Select Expression of type Short" & vbCrLf
	Case 200 To 234 
		numMatches += 1
		Console.WriteLine ("Short")
	End Select
		
        Dim c As Integer = 45
        Select Case c
	Case 1 To 23
		errMsg = errMsg & "#A3 Case statement not working with Select Expression of type Integer" & vbCrLf
	Case 23 To 44
		errMsg = errMsg & "#A3 Case statement not working with Select Expression of type Integer" & vbCrLf
	End Select

        Dim d As Long = 465
        Select Case d
	Case 1 To 460
		errMsg = errMsg & "#A4 Case statement not working with Select Expression of type Long" & vbCrLf
	Case Else
		numMatches += 1
		Console.WriteLine ("Long")
	End Select

        Dim e As Decimal = 234232
        Select Case e
	Case 12 To 34 
		errMsg = errMsg & "#A5 Case statement not working with Select Expression of type Decimal" & vbCrLf
	Case Else
		numMatches += 1
		Console.WriteLine ("Decimal")
	End Select

        Dim f As Single = 23.5
        Select Case f
	Case 23 To 24 
		numMatches += 1
		Console.WriteLine ("Single")
	Case 23 To 5 
		errMsg = errMsg & "#A6 Case statement not working with Select Expression of type Single" & vbCrLf
	Case 12.34 
		errMsg = errMsg & "#A6 Case statement not working with Select Expression of type Single" & vbCrLf
	End Select

        Dim g As Double = 1.90
        Select Case g
	Case 34 To 327 
		errMsg = errMsg & "#A7 Case statement not working with Select Expression of type double" & vbCrLf
	Case 1 To 20
		numMatches += 1
		Console.WriteLine ("Double")
	End Select

        Dim h As String = "Sudha"
        Select Case h
	Case "K" To "Satya"
		errMsg = errMsg & "#A8 Case statement not working with Select Expression of type String" & vbCrLf
	Case "SAAA" To "Szzzz"
		numMatches += 1
		Console.WriteLine ("String")
	Case "None"
		errMsg = errMsg & "#A8 Case statement not working with Select Expression of type String" & vbCrLf
	End Select

        Dim i As Char = "4"
        Select Case i
	Case "g"
		errMsg = errMsg & "#A9 Case statement not working with Select Expression of type Char" & vbCrLf
	Case Else
		Console.WriteLine ("Char")
		numMatches += 1
	End Select

        Dim j As Object = 45.6
        Select Case j
	Case 23 To 90
		numMatches += 1
		Console.WriteLine ("Object")
	Case 45,23,234 
		errMsg = errMsg & "#A10 Case statement not working with Select Expression of type Object" & vbCrLf
	End Select

        Dim k As Date = #04/23/2005#
        Select Case k
	Case #1/1/1998#
		errMsg = errMsg & "#A11 Case statement not working with Select Expression of type DateTime" & vbCrLf
	Case #01/01/2002#  To #02/11/2006#
		numMatches += 1
		Console.WriteLine ("DateTime")
	Case Else
		errMsg = errMsg & "#A11 Case statement not working with Select Expression of type DateTime" & vbCrLf
	End Select

	if (errMsg <> "")
		Throw New Exception (errMsg)
	End If
	if  numMatches <> 10
		throw new Exception ("select-case statements not working properly")
	End If
    End Sub

End Module
