//
// System.Data.ObjectSpaces.ASTBaseParser.cs - Implements a base Abstract Syntax Tree parser
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

using System.Data.ObjectSpaces.Query;

namespace System.Data.ObjectSpaces
{
        public class ASTBaseParser
        {
                public int lexerPos;                //The current position in the lexical analyser
                public int parCount;                //The parse number
                public Expression parseTree;        //The parse tree
                
                [MonoTODO]
                public ASTBaseParser () {}
                


                [MonoTODO]
                protected Expression BuildBinaryNode (Expression left, Expression right, BinaryOperator op)
                {
                        return null;
                }

                [MonoTODO]
                protected Expression BuildConditionalNode (Expression condition, Expression trueBranch, Expression falseBranch)
                {
                        return null;
                }

                [MonoTODO]                
                protected Expression BuildUnaryNode (Expression node, UnaryOperator op)
                {
                        return null;
                }

                [MonoTODO]                
                protected Expression GetConstraint (Axis filter)
                {
                        return null;
                }

                [MonoTODO]
                public virtual void Parse (string opath) {}

                [MonoTODO]
                public virtual void ParseObjectQuery (Type type, string opath, bool baseTypeOnly) {}
                
        }
}

#endif
