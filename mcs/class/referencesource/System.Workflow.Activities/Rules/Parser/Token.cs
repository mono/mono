#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace System.Workflow.Activities.Rules
{
    internal enum TokenID
    {
        Unknown,
        Identifier,
        Dot,
        Comma,
        LParen,
        RParen,
        Plus,               // +
        Minus,              // -
        Divide,             // /
        Multiply,           // *
        Modulus,            // MOD
        BitAnd,             // &
        BitOr,              // |
        And,                // AND, &&
        Or,                 // OR, ||
        Not,                // NOT, !
        Equal,              // ==
        NotEqual,           // !=, <>
        Less,               // <
        LessEqual,          // <=
        Greater,            // >
        GreaterEqual,       // >=
        StringLiteral,      // " ... "
        CharacterLiteral,   // ' ... '
        IntegerLiteral,
        DecimalLiteral,
        FloatLiteral,
        True,
        False,
        Null,
        This,
        In,
        Out,
        Ref,
        Assign,
        TypeName,
        Update,
        Halt,
        Semicolon,          // ;
        LBracket,           // [
        RBracket,           // ]
        LCurlyBrace,        // {
        RCurlyBrace,        // }
        New,
        Illegal,

        EndOfInput
    }

    internal class Token
    {
        internal TokenID TokenID;
        internal int StartPosition;
        internal object Value;

        internal Token(TokenID tokenID, int position, object value)
        {
            this.TokenID = tokenID;
            this.StartPosition = position;
            this.Value = value;
        }
    }
}
