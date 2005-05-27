'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.

' Testing whether all kinds of primitive types work well as 'Select' expression
' Testing the case clauses like '<relational op> X'

Imports System

Module ConditionalStatementsC

    Sub Main()
	Dim errMsg As String = ""
	Dim numMatches As Integer = 0

        Dim a As Byte = 12
        Select Case a
	Case is < 11 
		errMsg = errMsg & "#A1 Case statement not working with Select Expression of type byte " & vbCrLf
	Case >= 12
		numMatches += 1
		Console.WriteLine ("Byte")
	Case 5 To 16 
		errMsg = errMsg & "#A1 Case statement not working with Select Expression of type byte" & vbCrLf
        End Select

        Dim b As Short = 234
        Select Case b
	Case <= 23 
		errMsg = errMsg & "#A2 Case statement not working with Select Expression of type Short" & vbCrLf
	Case is = 200 
		errMsg = errMsg & "#A2 Case statement not working with Select Expression of type Short" & vbCrLf
	Case <> 24 
		numMatches += 1
		Console.WriteLine ("Short")
	End Select
		
        Dim c As Integer = 45
        Select Case c
	Case < 23
		errMsg = errMsg & "#A3 Case statement not working with Select Expression of type Integer" & vbCrLf
	Case <= 44
		errMsg = errMsg & "#A3 Case statement not working with Select Expression of type Integer" & vbCrLf
	End Select

        Dim d As Long = 465
        Select Case d
	Case is >= 480
		errMsg = errMsg & "#A4 Case statement not working with Select Expression of type Long" & vbCrLf
	Case Else
		numMatches += 1
		Console.WriteLine ("Long")
	End Select

        Dim e As Decimal = 234232
        Select Case e
	Case 12 To 34 
		errMsg = errMsg & "#A5 Case statement not working with Select Expression of type Decimal" & vbCrLf
	Case >= 200
		numMatches += 1
		Console.WriteLine ("Decimal")
	End Select

        Dim f As Single = 23.5
        Select Case f
	Case <= 23.6 
		numMatches += 1
		Console.WriteLine ("Single")
	Case = 24 
		errMsg = errMsg & "#A6 Case statement not working with Select Expression of type Single" & vbCrLf
	End Select

        Dim g As Double = 1.90
        Select Case g
	Case is > 34
		errMsg = errMsg & "#A7 Case statement not working with Select Expression of type double" & vbCrLf
	Case < 20
		numMatches += 1
		Console.WriteLine ("Double")
	End Select

        Dim h As String = "Sudha"
        Select Case h
	Case <> "Satya"
		numMatches += 1
		Console.WriteLine ("String")
	Case Else
		errMsg = errMsg & "#A8 Case statement not working with Select Expression of type String" & vbCrLf
	End Select

        Dim i As Char = "4"
        Select Case i
	Case < "g"
		Console.WriteLine ("Char")
		numMatches += 1
	Case Else
		errMsg = errMsg & "#A9 Case statement not working with Select Expression of type Char" & vbCrLf
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
	Case is = #1/1/1998#
		errMsg = errMsg & "#A11 Case statement not working with Select Expression of type DateTime" & vbCrLf
	Case >= #01/01/2002# 
		numMatches += 1
		Console.WriteLine ("DateTime")
	Case <= #02/11/2006#
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
