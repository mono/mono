'System.InvalidCastException: Cast from string "String" to type 'Boolean' is not valid.

Imports System

Module ConditionalStatements1

    Sub Main()

      	if "String" Then
		throw new exception("#CSC2")
	end If

    End Sub

End Module
