' Positive Test
' Test labels in functions
''''''''''''''''''''''''''''''''''
' vbc output
''''''''''''''''''''''''''''''''''
' Should be
'Error BC30451: Name 'y' is not declared
'Error BC30451: Name 'z' is not declared
''''''''''''''''''''''''''''''''''''''''
' mbas output
''''''''''''''''''''''''''''''''''''''''''
' syntax error, got token `IDENTIFIER', expecting EOL COLON
'flabel.vb(21,0) error BC29999: Line:     21 Col: 0
'VirtLine: 21 Token: 471
'Parsing error in flabel.vb
'Mono.MonoBASIC.yyParser.yyException: irrecoverable syntax error
'in <0x0081e> Mono.MonoBASIC.Parser:yyparse (Mono.MonoBASIC.yyParser.yyInput)
'in <0x002b0> Mono.MonoBASIC.Parser:parse ()
''''''''''''''''''''''''''''''''''''''''''''''
Imports System

Module labelD



    Function Abs1() As Integer

        Dim x As Integer
        x = 1
        If x >= 0 Then
            GoTo x
        End If

        x = -x

x:
y:
z:
        Return x

    End Function

    Sub Main()
        Dim x As Integer, y As Integer

        x = Abs1()
        y = Abs1()

        If x <> 1 Then
            Throw New Exception("#Lbl3")
        End If
        If y <> 1 Then
            Throw New Exception("#Lbl4")
        End If
    End Sub


End Module