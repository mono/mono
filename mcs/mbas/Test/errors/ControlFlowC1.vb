REM LineNo: 14
REM ExpectedError: BC30240
REM ErrorMessage: 'Exit' must be followed by 'Sub', 'Function', 'Property', 
REM               'Do', 'For', 'While', 'Select', or 'Try'.

Imports System

Module ControlFlowC1

    Sub main()
        Dim i As Integer = 0
        If i = 0 Then
            i = 2
            Exit
            i = 4
        End If

    End Sub

End Module