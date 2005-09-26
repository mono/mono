'Author:
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná

REM LineNo: 15
REM ExpectedError: BC30210
REM ErrorMessage: Option Strict On requires all function and property declarations to have an 'As' clause.

Option Strict
Imports System

Module Test
    Interface A
       Property Prop()
    End Interface

    Public Sub Main()
    End Sub
End Module
