using System;
using System.Collections.Generic;
using System.Collections;
using System.Workflow.Activities.Rules;


namespace System.Workflow.Activities.Rules
{
    internal class IntellisenseParser
    {
        private List<Token> tokens = new List<Token>();
        private int tokenIndex;

        internal IntellisenseParser(string inputString)
        {
            Scanner scanner = new Scanner(inputString);
            // Tokenize the input, but insert a marker at the beginning.
            tokens.Add(new Token(TokenID.EndOfInput, 0, null));

            scanner.TokenizeForIntellisense(tokens);
        }

        private Token CurrentToken
        {
            get { return tokens[tokenIndex]; }
        }

        private Token PrevToken()
        {
            if (tokenIndex > 0)
                --tokenIndex;

            return CurrentToken;
        }

        internal ParserContext BackParse()
        {
            tokenIndex = tokens.Count - 1;
            if (tokenIndex < 0)
                return null;

            Token token = CurrentToken;
            bool valid = false;

            // Skip past the right-most EndOfIput.  For our backparsing, we've inserted an
            // EndOfInput at the start.
            if (token.TokenID == TokenID.EndOfInput)
                token = PrevToken();

            int endTokenIndex = tokenIndex;

            if (token.TokenID == TokenID.Identifier && ((string)token.Value).Length == 1 && PrevToken().TokenID != TokenID.Dot)
            {
                // Assume this is the start of a root identifier
                valid = true;
            }
            else if (token.TokenID == TokenID.Dot)
            {
                // Assume it's a member selection operator.
                valid = BackParsePostfix();
            }
            else if (token.TokenID == TokenID.LParen)
            {
                // Assume it's the start of a method call argument list.
                if (PrevToken().TokenID == TokenID.Identifier)
                {
                    if (PrevToken().TokenID == TokenID.Dot)
                    {
                        // The tail looked like ".identifier(", so now we continue as in
                        // the member selection operator reverse parse above.
                        valid = BackParsePostfix();
                    }
                    else
                    {
                        // The tail looked like "identifier(", with no preceeding ".", so
                        // we're actually done.
                        valid = true;
                    }

                    if (valid)
                    {
                        // Back up over the "new" if there is one.
                        if (CurrentToken.TokenID == TokenID.New)
                            PrevToken();
                    }
                }
            }

            if (!valid)
                return null;

            // We successfully backward-parsed a postfix expression.  Create a
            // ParserContext for the real parser, giving it the subrange of tokens
            // that comprise the postfix expression.
            List<Token> postfixTokens = tokens.GetRange(tokenIndex + 1, endTokenIndex - tokenIndex);
            postfixTokens.Add(new Token(TokenID.EndOfInput, 0, null));
            ParserContext parserContext = new ParserContext(postfixTokens);
            return parserContext;
        }

        private bool BackParsePostfix()
        {
            while (CurrentToken.TokenID == TokenID.Dot)
            {
                Token token = PrevToken();
                switch (token.TokenID)
                {
                    case TokenID.Identifier:
                    case TokenID.TypeName:
                        PrevToken(); // eat the token
                        break;

                    case TokenID.This:
                        PrevToken(); // eat the "this"
                        // This is the start of the expression.
                        return true;

                    case TokenID.RParen:
                        // This may be the argument list of a method call,
                        // or it may be a parenthesized expression.
                        if (!BackParseMatchingDelimiter(TokenID.LParen))
                            return false;

                        if (CurrentToken.TokenID == TokenID.Identifier)
                        {
                            // It was a method call.
                            PrevToken(); // eat the method identifier
                        }
                        else
                        {
                            // It was a parenthesized subexpression.  We are finished,
                            // we have found the start of the subexpression.
                            return true;
                        }
                        break;

                    case TokenID.RBracket:
                        // Loop backward over all [..][..]
                        do
                        {
                            if (!BackParseMatchingDelimiter(TokenID.LBracket))
                                return false;
                        } while (CurrentToken.TokenID == TokenID.RBracket);

                        // Preceeding the indexers might be an identifier, or a method call.
                        if (CurrentToken.TokenID == TokenID.Identifier)
                        {
                            // It was an identifier.  Eat it and continue.
                            PrevToken(); // eat the identifier.
                        }
                        else if (CurrentToken.TokenID == TokenID.RParen)
                        {
                            // This may be the argument list of a method call,
                            // or it may be a parenthesized expression.
                            if (!BackParseMatchingDelimiter(TokenID.LParen))
                                return false;

                            if (CurrentToken.TokenID == TokenID.Identifier)
                            {
                                // It was a method call.
                                PrevToken(); // eat the method identifier
                            }
                            else
                            {
                                // It was a parenthesized subexpression.  We are finished,
                                // we have found the start of the subexpression.
                                return true;
                            }
                        }
                        else
                        {
                            // It's not valid.
                            return false;
                        }

                        break;

                    case TokenID.Greater:
                        if (!BackParseMatchingDelimiter(TokenID.Less))
                            return false;

                        if (CurrentToken.TokenID == TokenID.Identifier)
                        {
                            // It was a generic type
                            PrevToken(); // Eat the type identifier
                        }
                        else
                        {
                            // This wasn't valid... it was a type argument list, but wasn't
                            // preceeded by an identifier.
                            return false;
                        }
                        break;

                    default:
                        // We saw a "." that wasn't preceeded by a useful token.
                        return false;
                }
            }

            // If an identifier or type name is preceeded by "new", keep that.
            if (CurrentToken.TokenID == TokenID.New)
                PrevToken();

            return true;
        }

        // Having parsed a closing delimiter, eat tokens until the matching open delimiter
        // is found.
        private bool BackParseMatchingDelimiter(TokenID openDelimiter)
        {
            TokenID closeDelimiter = CurrentToken.TokenID;
            int level = 1;

            Token token = PrevToken(); // Eat the close delimiter
            while (token.TokenID != TokenID.EndOfInput)
            {
                if (token.TokenID == closeDelimiter)
                {
                    ++level;
                }
                else if (token.TokenID == openDelimiter)
                {
                    --level;
                    if (level == 0)
                    {
                        PrevToken(); // eat the open delimiter
                        break;
                    }
                }

                token = PrevToken();
            }

            // Back parse was successful if we matched all delimiters.
            return level == 0;
        }
    }
}
