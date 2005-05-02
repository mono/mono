Imports System

Module LikeOperator
    Sub main()

        Dim a As Boolean

        a = "HELLO" Like "HELLO"
        If a <> True Then
            throw new System.Exception("#A1-LikeOperator:Failed")
        End If

        a = "HELLO" Like "HEllO"
        If a <> False Then
            throw new System.Exception("#A2-LikeOperator:Failed")
        End If

        a = "HELLO" Like "H*O"
        If a <> True Then
            throw new System.Exception("#A3-LikeOperator:Failed")
        End If

        a = "HELLO" Like "H[A-Z][!M-P][!A-K]O"
        If a <> True Then
            throw new System.Exception("#A4-LikeOperator:Failed")
        End If

        a = "HE12O" Like "H?##[L-P]"
        If a <> True Then
            throw new System.Exception("#A5-LikeOperator:Failed")
        End If

        a = "HELLO123WORLD" Like "H?*#*"
        If a <> True Then
            throw new System.Exception("#A6-LikeOperator:Failed")
        End If

        a = "HELLOworld" Like "B*O*d"
        If a <> False Then
            throw new System.Exception("#A7-LikeOperator:Failed")
        End If

        a = "" Like ""
        If a <> True Then
            throw new System.Exception("#A8-LikeOperator:Failed")
        End If

        a = "A" Like ""
        If a <> False Then
            throw new System.Exception("#A9-LikeOperator:Failed")
        End If

        a = "" Like "A"
        If a <> False Then
            throw new System.Exception("#A10-LikeOperator:Failed")
        End If

        a = "HELLO" Like "HELLO" & Nothing
        If a <> True Then
            throw new System.Exception("#A11-LikeOperator:Failed")
        End If


    End Sub

End Module
