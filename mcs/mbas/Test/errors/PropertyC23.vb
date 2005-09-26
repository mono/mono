'Author:
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná

REM LineNo: 16
REM ExpectedError: BC30210
REM ErrorMessage: Option Strict On requires all function and property declarations to have an 'As' clause.

Option Strict
Imports System

Module Test

Mustinherit class teste
    Public Mustoverride Property Prop()
end class
    Public Sub Main()	
    End Sub
End Module
