/* 
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
namespace Lucene.Net.Demo.Html
{
	
    public class HTMLParserTokenManager : HTMLParserConstants
    {
        private void  InitBlock()
        {
            System.IO.StreamWriter temp_writer;
            temp_writer = new System.IO.StreamWriter(System.Console.OpenStandardOutput(), System.Console.Out.Encoding);
            temp_writer.AutoFlush = true;
            debugStream = temp_writer;
        }
        public System.IO.StreamWriter debugStream;
        public virtual void  SetDebugStream(System.IO.StreamWriter ds)
        {
            debugStream = ds;
        }
        private int jjStopStringLiteralDfa_0(int pos, long active0)
        {
            switch (pos)
            {
				
                case 0: 
                    if ((active0 & 0x32L) != 0L)
                        return 17;
                    return - 1;
				
                case 1: 
                    if ((active0 & 0x30L) != 0L)
                        return 22;
                    if ((active0 & 0x2L) != 0L)
                    {
                        if (jjmatchedPos != 1)
                        {
                            jjmatchedKind = 2;
                            jjmatchedPos = 1;
                        }
                        return 19;
                    }
                    return - 1;
				
                case 2: 
                    if ((active0 & 0x2L) != 0L)
                    {
                        jjmatchedKind = 2;
                        jjmatchedPos = 2;
                        return 20;
                    }
                    return - 1;
				
                case 3: 
                    if ((active0 & 0x2L) != 0L)
                    {
                        jjmatchedKind = 2;
                        jjmatchedPos = 3;
                        return 20;
                    }
                    return - 1;
				
                case 4: 
                    if ((active0 & 0x2L) != 0L)
                    {
                        jjmatchedKind = 2;
                        jjmatchedPos = 4;
                        return 20;
                    }
                    return - 1;
				
                case 5: 
                    if ((active0 & 0x2L) != 0L)
                    {
                        jjmatchedKind = 2;
                        jjmatchedPos = 5;
                        return 20;
                    }
                    return - 1;
				
                default: 
                    return - 1;
				
            }
        }
        private int jjStartNfa_0(int pos, long active0)
        {
            return jjMoveNfa_0(jjStopStringLiteralDfa_0(pos, active0), pos + 1);
        }
        private int jjStopAtPos(int pos, int kind)
        {
            jjmatchedKind = kind;
            jjmatchedPos = pos;
            return pos + 1;
        }
        private int jjStartNfaWithStates_0(int pos, int kind, int state)
        {
            jjmatchedKind = kind;
            jjmatchedPos = pos;
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return jjMoveNfa_0(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_0()
        {
            switch (curChar)
            {
				
                case (char) (60): 
                    return jjMoveStringLiteralDfa1_0(0x32L);
				
                default: 
                    return jjMoveNfa_0(11, 0);
				
            }
        }
        private int jjMoveStringLiteralDfa1_0(long active0)
        {
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                jjStopStringLiteralDfa_0(0, active0);
                return 1;
            }
            switch (curChar)
            {
				
                case (char) (33): 
                    if ((active0 & 0x20L) != 0L)
                    {
                        jjmatchedKind = 5;
                        jjmatchedPos = 1;
                    }
                    return jjMoveStringLiteralDfa2_0(active0, 0x10L);
				
                case (char) (115): 
                    return jjMoveStringLiteralDfa2_0(active0, 0x2L);
				
                default: 
                    break;
				
            }
            return jjStartNfa_0(0, active0);
        }
        private int jjMoveStringLiteralDfa2_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_0(0, old0);
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                jjStopStringLiteralDfa_0(1, active0);
                return 2;
            }
            switch (curChar)
            {
				
                case (char) (45): 
                    return jjMoveStringLiteralDfa3_0(active0, 0x10L);
				
                case (char) (99): 
                    return jjMoveStringLiteralDfa3_0(active0, 0x2L);
				
                default: 
                    break;
				
            }
            return jjStartNfa_0(1, active0);
        }
        private int jjMoveStringLiteralDfa3_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_0(1, old0);
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                jjStopStringLiteralDfa_0(2, active0);
                return 3;
            }
            switch (curChar)
            {
				
                case (char) (45): 
                    if ((active0 & 0x10L) != 0L)
                        return jjStopAtPos(3, 4);
                    break;
				
                case (char) (114): 
                    return jjMoveStringLiteralDfa4_0(active0, 0x2L);
				
                default: 
                    break;
				
            }
            return jjStartNfa_0(2, active0);
        }
        private int jjMoveStringLiteralDfa4_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_0(2, old0);
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                jjStopStringLiteralDfa_0(3, active0);
                return 4;
            }
            switch (curChar)
            {
				
                case (char) (105): 
                    return jjMoveStringLiteralDfa5_0(active0, 0x2L);
				
                default: 
                    break;
				
            }
            return jjStartNfa_0(3, active0);
        }
        private int jjMoveStringLiteralDfa5_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_0(3, old0);
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                jjStopStringLiteralDfa_0(4, active0);
                return 5;
            }
            switch (curChar)
            {
				
                case (char) (112): 
                    return jjMoveStringLiteralDfa6_0(active0, 0x2L);
				
                default: 
                    break;
				
            }
            return jjStartNfa_0(4, active0);
        }
        private int jjMoveStringLiteralDfa6_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_0(4, old0);
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                jjStopStringLiteralDfa_0(5, active0);
                return 6;
            }
            switch (curChar)
            {
				
                case (char) (116): 
                    if ((active0 & 0x2L) != 0L)
                        return jjStartNfaWithStates_0(6, 1, 20);
                    break;
				
                default: 
                    break;
				
            }
            return jjStartNfa_0(5, active0);
        }
        private void  jjCheckNAdd(int state)
        {
            if (jjrounds[state] != jjround)
            {
                jjstateSet[jjnewStateCnt++] = state;
                jjrounds[state] = jjround;
            }
        }
        private void  jjAddStates(int start, int end)
        {
            do 
            {
                jjstateSet[jjnewStateCnt++] = jjnextStates[start];
            }
            while (start++ != end);
        }
        private void  jjCheckNAddTwoStates(int state1, int state2)
        {
            jjCheckNAdd(state1);
            jjCheckNAdd(state2);
        }
        private void  jjCheckNAddStates(int start, int end)
        {
            do 
            {
                jjCheckNAdd(jjnextStates[start]);
            }
            while (start++ != end);
        }
        private void  jjCheckNAddStates(int start)
        {
            jjCheckNAdd(jjnextStates[start]);
            jjCheckNAdd(jjnextStates[start + 1]);
        }
        internal static readonly ulong[] jjbitVec0 = new ulong[]{0x0L, 0x0L, 0xffffffffffffffffL, 0xffffffffffffffffL};
        private int jjMoveNfa_0(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            jjnewStateCnt = 25;
            int i = 1;
            jjstateSet[0] = startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++jjround == 0x7fffffff)
                    ReInitRounds();
                if (curChar < 64)
                {
                    ulong l = ((ulong) 1L) << curChar;
MatchLoop: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 11: 
                                if ((0x3ff000000000000L & l) != 0L)
                                    jjCheckNAddTwoStates(7, 2);
                                else if ((0x100002600L & l) != 0L)
                                {
                                    if (kind > 10)
                                        kind = 10;
                                    jjCheckNAdd(10);
                                }
                                else if (curChar == 60)
                                    jjCheckNAddStates(0, 2);
                                else if (curChar == 38)
                                    jjAddStates(3, 4);
                                else if (curChar == 36)
                                    jjstateSet[jjnewStateCnt++] = 1;
                                if ((0x3ff000000000000L & l) != 0L)
                                {
                                    if (kind > 6)
                                        kind = 6;
                                    jjCheckNAddStates(5, 9);
                                }
                                break;
							
                            case 17: 
                                if (curChar == 33)
                                    jjstateSet[jjnewStateCnt++] = 22;
                                else if (curChar == 47)
                                    jjCheckNAdd(18);
                                break;
							
                            case 0: 
                                if (curChar == 36)
                                    jjstateSet[jjnewStateCnt++] = 1;
                                break;
							
                            case 1: 
                                if ((0x3ff000000000000L & l) != 0L)
                                    jjCheckNAdd(2);
                                break;
							
                            case 2: 
                                if ((0x500000000000L & l) != 0L)
                                    jjstateSet[jjnewStateCnt++] = 3;
                                break;
							
                            case 3: 
                            case 9: 
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 6)
                                    kind = 6;
                                jjCheckNAddStates(10, 12);
                                break;
							
                            case 4: 
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 6)
                                    kind = 6;
                                jjCheckNAddStates(5, 9);
                                break;
							
                            case 5: 
                                if ((0x880000000000L & l) == 0L)
                                    break;
                                if (kind > 6)
                                    kind = 6;
                                jjCheckNAddStates(13, 16);
                                break;
							
                            case 6: 
                                if ((0x3ff000000000000L & l) != 0L)
                                    jjCheckNAddTwoStates(7, 2);
                                break;
							
                            case 7: 
                                if (curChar != 34)
                                    break;
                                if (kind > 6)
                                    kind = 6;
                                jjCheckNAddStates(10, 12);
                                break;
							
                            case 8: 
                                if ((0x208000000000L & l) != 0L)
                                    jjstateSet[jjnewStateCnt++] = 9;
                                break;
							
                            case 10: 
                                if ((0x100002600L & l) == 0L)
                                    break;
                                kind = 10;
                                jjCheckNAdd(10);
                                break;
							
                            case 13: 
                                if (curChar == 59 && kind > 9)
                                    kind = 9;
                                break;
							
                            case 14: 
                                if (curChar == 35)
                                    jjCheckNAdd(15);
                                break;
							
                            case 15: 
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 9)
                                    kind = 9;
                                jjCheckNAddTwoStates(15, 13);
                                break;
							
                            case 16: 
                                if (curChar == 60)
                                    jjCheckNAddStates(0, 2);
                                break;
							
                            case 19: 
                                if ((0x9fffff7affffd9ffL & l) == 0L)
                                    break;
                                if (kind > 2)
                                    kind = 2;
                                jjCheckNAdd(20);
                                break;
							
                            case 20: 
                                if ((0x9ffffffeffffd9ffL & l) == 0L)
                                    break;
                                if (kind > 2)
                                    kind = 2;
                                jjCheckNAdd(20);
                                break;
							
                            case 21: 
                                if (curChar == 33)
                                    jjstateSet[jjnewStateCnt++] = 22;
                                break;
							
                            case 23: 
                                if ((0x9fffff7affffd9ffL & l) == 0L)
                                    break;
                                if (kind > 3)
                                    kind = 3;
                                jjCheckNAdd(24);
                                break;
							
                            case 24: 
                                if ((0x9ffffffeffffd9ffL & l) == 0L)
                                    break;
                                if (kind > 3)
                                    kind = 3;
                                jjCheckNAdd(24);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else if (curChar < 128)
                {
                    ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 11: 
                            case 4: 
                                if ((0x7fffffe07fffffeL & l) == 0L)
                                    break;
                                if (kind > 6)
                                    kind = 6;
                                jjCheckNAddStates(5, 9);
                                break;
							
                            case 17: 
                            case 18: 
                                if ((0x7fffffe07fffffeL & l) == 0L)
                                    break;
                                if (kind > 2)
                                    kind = 2;
                                jjstateSet[jjnewStateCnt++] = 19;
                                break;
							
                            case 9: 
                                if ((0x7fffffe07fffffeL & l) == 0L)
                                    break;
                                if (kind > 6)
                                    kind = 6;
                                jjCheckNAddStates(10, 12);
                                break;
							
                            case 12: 
                                if ((0x7fffffe07fffffeL & l) == 0L)
                                    break;
                                if (kind > 9)
                                    kind = 9;
                                jjAddStates(17, 18);
                                break;
							
                            case 19: 
                            case 20: 
                                if (kind > 2)
                                    kind = 2;
                                jjCheckNAdd(20);
                                break;
							
                            case 22: 
                                if ((0x7fffffe07fffffeL & l) == 0L)
                                    break;
                                if (kind > 3)
                                    kind = 3;
                                jjstateSet[jjnewStateCnt++] = 23;
                                break;
							
                            case 23: 
                            case 24: 
                                if (kind > 3)
                                    kind = 3;
                                jjCheckNAdd(24);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int i2 = (curChar & 0xff) >> 6;
                    ulong l2 = ((ulong) 1L) << (curChar & 63);
                MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 19: 
                            case 20: 
                                if ((jjbitVec0[i2] & l2) == 0L)
                                    break;
                                if (kind > 2)
                                    kind = 2;
                                jjCheckNAdd(20);
                                break;
							
                            case 23: 
                            case 24: 
                                if ((jjbitVec0[i2] & l2) == 0L)
                                    break;
                                if (kind > 3)
                                    kind = 3;
                                jjCheckNAdd(24);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    jjmatchedKind = kind;
                    jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = jjnewStateCnt) == (startsAt = 25 - (jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    curChar = input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjMoveStringLiteralDfa0_5()
        {
            return jjMoveNfa_5(1, 0);
        }
        private int jjMoveNfa_5(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            jjnewStateCnt = 2;
            int i = 1;
            jjstateSet[0] = startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++jjround == 0x7fffffff)
                    ReInitRounds();
                if (curChar < 64)
                {
                    ulong l = ((ulong) 1L) << curChar;
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                                if ((0xfffffffbffffffffL & l) != 0L)
                                {
                                    if (kind > 24)
                                        kind = 24;
                                    jjCheckNAdd(0);
                                }
                                else if (curChar == 34)
                                {
                                    if (kind > 25)
                                        kind = 25;
                                }
                                break;
							
                            case 0: 
                                if ((0xfffffffbffffffffL & l) == 0L)
                                    break;
                                kind = 24;
                                jjCheckNAdd(0);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else if (curChar < 128)
                {
                    ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                            case 0: 
                                kind = 24;
                                jjCheckNAdd(0);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int i2 = (curChar & 0xff) >> 6;
                    ulong l2 = ((ulong) 1L) << (curChar & 63);
                MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                            case 0: 
                                if ((jjbitVec0[i2] & l2) == 0L)
                                    break;
                                if (kind > 24)
                                    kind = 24;
                                jjCheckNAdd(0);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    jjmatchedKind = kind;
                    jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = jjnewStateCnt) == (startsAt = 2 - (jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    curChar = input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_7(int pos, long active0)
        {
            switch (pos)
            {
				
                default: 
                    return - 1;
				
            }
        }
        private int jjStartNfa_7(int pos, long active0)
        {
            return jjMoveNfa_7(jjStopStringLiteralDfa_7(pos, active0), pos + 1);
        }
        private int jjStartNfaWithStates_7(int pos, int kind, int state)
        {
            jjmatchedKind = kind;
            jjmatchedPos = pos;
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return jjMoveNfa_7(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_7()
        {
            switch (curChar)
            {
				
                case (char) (62): 
                    return jjStopAtPos(0, 29);
				
                default: 
                    return jjMoveNfa_7(0, 0);
				
            }
        }
        private int jjMoveNfa_7(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            jjnewStateCnt = 1;
            int i = 1;
            jjstateSet[0] = startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++jjround == 0x7fffffff)
                    ReInitRounds();
                if (curChar < 64)
                {
                    ulong l = ((ulong) 1L) << curChar;
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 0: 
                                if ((0xbfffffffffffffffL & l) == 0L)
                                    break;
                                kind = 28;
                                jjstateSet[jjnewStateCnt++] = 0;
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else if (curChar < 128)
                {
                    ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 0: 
                                kind = 28;
                                jjstateSet[jjnewStateCnt++] = 0;
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int i2 = (curChar & 0xff) >> 6;
                    ulong l2 = ((ulong) 1L) << (curChar & 63);
                MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 0: 
                                if ((jjbitVec0[i2] & l2) == 0L)
                                    break;
                                if (kind > 28)
                                    kind = 28;
                                jjstateSet[jjnewStateCnt++] = 0;
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    jjmatchedKind = kind;
                    jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = jjnewStateCnt) == (startsAt = 1 - (jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    curChar = input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjMoveStringLiteralDfa0_4()
        {
            return jjMoveNfa_4(1, 0);
        }
        private int jjMoveNfa_4(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            jjnewStateCnt = 2;
            int i = 1;
            jjstateSet[0] = startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++jjround == 0x7fffffff)
                    ReInitRounds();
                if (curChar < 64)
                {
                    ulong l = ((ulong) 1L) << curChar;
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                                if ((0xffffff7fffffffffL & l) != 0L)
                                {
                                    if (kind > 22)
                                        kind = 22;
                                    jjCheckNAdd(0);
                                }
                                else if (curChar == 39)
                                {
                                    if (kind > 23)
                                        kind = 23;
                                }
                                break;
							
                            case 0: 
                                if ((0xffffff7fffffffffL & l) == 0L)
                                    break;
                                kind = 22;
                                jjCheckNAdd(0);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else if (curChar < 128)
                {
                    ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                            case 0: 
                                kind = 22;
                                jjCheckNAdd(0);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int i2 = (curChar & 0xff) >> 6;
                    ulong l2 = ((ulong) 1L) << (curChar & 63);
                MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                            case 0: 
                                if ((jjbitVec0[i2] & l2) == 0L)
                                    break;
                                if (kind > 22)
                                    kind = 22;
                                jjCheckNAdd(0);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    jjmatchedKind = kind;
                    jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = jjnewStateCnt) == (startsAt = 2 - (jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    curChar = input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_3(int pos, long active0)
        {
            switch (pos)
            {
				
                default: 
                    return - 1;
				
            }
        }
        private int jjStartNfa_3(int pos, long active0)
        {
            return jjMoveNfa_3(jjStopStringLiteralDfa_3(pos, active0), pos + 1);
        }
        private int jjStartNfaWithStates_3(int pos, int kind, int state)
        {
            jjmatchedKind = kind;
            jjmatchedPos = pos;
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return jjMoveNfa_3(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_3()
        {
            switch (curChar)
            {
				
                case (char) (34): 
                    return jjStopAtPos(0, 20);
				
                case (char) (39): 
                    return jjStopAtPos(0, 19);
				
                default: 
                    return jjMoveNfa_3(0, 0);
				
            }
        }
        private int jjMoveNfa_3(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            jjnewStateCnt = 3;
            int i = 1;
            jjstateSet[0] = startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++jjround == 0x7fffffff)
                    ReInitRounds();
                if (curChar < 64)
                {
                    ulong l = ((ulong) 1L) << (int) curChar;
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 0: 
                                if ((0x9fffff7affffd9ffL & l) != 0L)
                                {
                                    if (kind > 18)
                                        kind = 18;
                                    jjCheckNAdd(1);
                                }
                                else if ((0x100002600L & l) != 0L)
                                {
                                    if (kind > 21)
                                        kind = 21;
                                    jjCheckNAdd(2);
                                }
                                break;
							
                            case 1: 
                                if ((0xbffffffeffffd9ffL & l) == 0L)
                                    break;
                                if (kind > 18)
                                    kind = 18;
                                jjCheckNAdd(1);
                                break;
							
                            case 2: 
                                if ((0x100002600L & l) == 0L)
                                    break;
                                kind = 21;
                                jjCheckNAdd(2);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else if (curChar < 128)
                {
                    ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 0: 
                            case 1: 
                                if (kind > 18)
                                    kind = 18;
                                jjCheckNAdd(1);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int i2 = (curChar & 0xff) >> 6;
                    ulong l2 = ((ulong) 1L) << (curChar & 63);
                MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 0: 
                            case 1: 
                                if ((jjbitVec0[i2] & l2) == 0L)
                                    break;
                                if (kind > 18)
                                    kind = 18;
                                jjCheckNAdd(1);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    jjmatchedKind = kind;
                    jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = jjnewStateCnt) == (startsAt = 3 - (jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    curChar = input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_6(int pos, long active0)
        {
            switch (pos)
            {
				
                case 0: 
                    if ((active0 & 0x8000000L) != 0L)
                    {
                        jjmatchedKind = 26;
                        return - 1;
                    }
                    return - 1;
				
                case 1: 
                    if ((active0 & 0x8000000L) != 0L)
                    {
                        if (jjmatchedPos == 0)
                        {
                            jjmatchedKind = 26;
                            jjmatchedPos = 0;
                        }
                        return - 1;
                    }
                    return - 1;
				
                default: 
                    return - 1;
				
            }
        }
        private int jjStartNfa_6(int pos, long active0)
        {
            return jjMoveNfa_6(jjStopStringLiteralDfa_6(pos, active0), pos + 1);
        }
        private int jjStartNfaWithStates_6(int pos, int kind, int state)
        {
            jjmatchedKind = kind;
            jjmatchedPos = pos;
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return jjMoveNfa_6(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_6()
        {
            switch (curChar)
            {
				
                case (char) (45): 
                    return jjMoveStringLiteralDfa1_6(0x8000000L);
				
                default: 
                    return jjMoveNfa_6(1, 0);
				
            }
        }
        private int jjMoveStringLiteralDfa1_6(long active0)
        {
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                jjStopStringLiteralDfa_6(0, active0);
                return 1;
            }
            switch (curChar)
            {
				
                case (char) (45): 
                    return jjMoveStringLiteralDfa2_6(active0, 0x8000000L);
				
                default: 
                    break;
				
            }
            return jjStartNfa_6(0, active0);
        }
        private int jjMoveStringLiteralDfa2_6(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_6(0, old0);
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                jjStopStringLiteralDfa_6(1, active0);
                return 2;
            }
            switch (curChar)
            {
				
                case (char) (62): 
                    if ((active0 & 0x8000000L) != 0L)
                        return jjStopAtPos(2, 27);
                    break;
				
                default: 
                    break;
				
            }
            return jjStartNfa_6(1, active0);
        }
        private int jjMoveNfa_6(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            jjnewStateCnt = 2;
            int i = 1;
            jjstateSet[0] = startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++jjround == 0x7fffffff)
                    ReInitRounds();
                if (curChar < 64)
                {
                    ulong l = ((ulong) 1L) << curChar;
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                                if ((0xffffdfffffffffffL & l) != 0L)
                                {
                                    if (kind > 26)
                                        kind = 26;
                                    jjCheckNAdd(0);
                                }
                                else if (curChar == 45)
                                {
                                    if (kind > 26)
                                        kind = 26;
                                }
                                break;
							
                            case 0: 
                                if ((0xffffdfffffffffffL & l) == 0L)
                                    break;
                                kind = 26;
                                jjCheckNAdd(0);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else if (curChar < 128)
                {
                    ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                            case 0: 
                                kind = 26;
                                jjCheckNAdd(0);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int i2 = (curChar & 0xff) >> 6;
                    ulong l2 = ((ulong) 1L) << (curChar & 63);
                MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                            case 0: 
                                if ((jjbitVec0[i2] & l2) == 0L)
                                    break;
                                if (kind > 26)
                                    kind = 26;
                                jjCheckNAdd(0);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    jjmatchedKind = kind;
                    jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = jjnewStateCnt) == (startsAt = 2 - (jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    curChar = input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjMoveStringLiteralDfa0_1()
        {
            return jjMoveNfa_1(1, 0);
        }
        private int jjMoveNfa_1(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            jjnewStateCnt = 12;
            int i = 1;
            jjstateSet[0] = startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++jjround == 0x7fffffff)
                    ReInitRounds();
                if (curChar < 64)
                {
                    ulong l = ((ulong) 1L) << curChar;
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                                if ((0xafffffffffffffffL & l) != 0L)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                    jjCheckNAdd(0);
                                }
                                else if ((0x5000000000000000L & l) != 0L)
                                {
                                    if (kind > 13)
                                        kind = 13;
                                }
                                if (curChar == 60)
                                    jjstateSet[jjnewStateCnt++] = 10;
                                break;
							
                            case 0: 
                                if ((0xafffffffffffffffL & l) == 0L)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                jjCheckNAdd(0);
                                break;
							
                            case 3: 
                                if ((0xafffffffffffffffL & l) != 0L)
                                    jjAddStates(19, 20);
                                break;
							
                            case 4: 
                                if (curChar == 62 && kind > 14)
                                    kind = 14;
                                break;
							
                            case 10: 
                                if (curChar == 47)
                                    jjstateSet[jjnewStateCnt++] = 9;
                                break;
							
                            case 11: 
                                if (curChar == 60)
                                    jjstateSet[jjnewStateCnt++] = 10;
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else if (curChar < 128)
                {
                    ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                            case 0: 
                                if (kind > 13)
                                    kind = 13;
                                jjCheckNAdd(0);
                                break;
							
                            case 2: 
                                if (curChar == 116)
                                    jjCheckNAddTwoStates(3, 4);
                                break;
							
                            case 3: 
                                jjCheckNAddTwoStates(3, 4);
                                break;
							
                            case 5: 
                                if (curChar == 112)
                                    jjstateSet[jjnewStateCnt++] = 2;
                                break;
							
                            case 6: 
                                if (curChar == 105)
                                    jjstateSet[jjnewStateCnt++] = 5;
                                break;
							
                            case 7: 
                                if (curChar == 114)
                                    jjstateSet[jjnewStateCnt++] = 6;
                                break;
							
                            case 8: 
                                if (curChar == 99)
                                    jjstateSet[jjnewStateCnt++] = 7;
                                break;
							
                            case 9: 
                                if (curChar == 115)
                                    jjstateSet[jjnewStateCnt++] = 8;
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int i2 = (curChar & 0xff) >> 6;
                    ulong l2 = ((ulong) 1L) << (curChar & 63);
                MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 1: 
                            case 0: 
                                if ((jjbitVec0[i2] & l2) == 0L)
                                    break;
                                if (kind > 13)
                                    kind = 13;
                                jjCheckNAdd(0);
                                break;
							
                            case 3: 
                                if ((jjbitVec0[i2] & l2) != 0L)
                                    jjAddStates(19, 20);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    jjmatchedKind = kind;
                    jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = jjnewStateCnt) == (startsAt = 12 - (jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    curChar = input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        private int jjStopStringLiteralDfa_2(int pos, long active0)
        {
            switch (pos)
            {
				
                default: 
                    return - 1;
				
            }
        }
        private int jjStartNfa_2(int pos, long active0)
        {
            return jjMoveNfa_2(jjStopStringLiteralDfa_2(pos, active0), pos + 1);
        }
        private int jjStartNfaWithStates_2(int pos, int kind, int state)
        {
            jjmatchedKind = kind;
            jjmatchedPos = pos;
            try
            {
                curChar = input_stream.ReadChar();
            }
            catch (System.IO.IOException e)
            {
                return pos + 1;
            }
            return jjMoveNfa_2(state, pos + 1);
        }
        private int jjMoveStringLiteralDfa0_2()
        {
            switch (curChar)
            {
				
                case (char) (34): 
                    return jjStopAtPos(0, 20);
				
                case (char) (39): 
                    return jjStopAtPos(0, 19);
				
                case (char) (61): 
                    return jjStartNfaWithStates_2(0, 16, 3);
				
                default: 
                    return jjMoveNfa_2(0, 0);
				
            }
        }
        private int jjMoveNfa_2(int startState, int curPos)
        {
            int[] nextStates;
            int startsAt = 0;
            jjnewStateCnt = 6;
            int i = 1;
            jjstateSet[0] = startState;
            int j, kind = 0x7fffffff;
            for (; ; )
            {
                if (++jjround == 0x7fffffff)
                    ReInitRounds();
                if (curChar < 64)
                {
                    ulong l = ((ulong) 1L) << curChar;
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 0: 
                                if ((0x9fffff7affffd9ffL & l) != 0L)
                                {
                                    if (kind > 15)
                                        kind = 15;
                                    jjCheckNAdd(1);
                                }
                                else if ((0x100002600L & l) != 0L)
                                {
                                    if (kind > 21)
                                        kind = 21;
                                    jjCheckNAdd(5);
                                }
                                else if (curChar == 61)
                                    jjstateSet[jjnewStateCnt++] = 3;
                                else if (curChar == 62)
                                {
                                    if (kind > 17)
                                        kind = 17;
                                }
                                break;
							
                            case 1: 
                                if ((0x9ffffffeffffd9ffL & l) == 0L)
                                    break;
                                if (kind > 15)
                                    kind = 15;
                                jjCheckNAdd(1);
                                break;
							
                            case 2: 
                            case 3: 
                                if (curChar == 62 && kind > 17)
                                    kind = 17;
                                break;
							
                            case 4: 
                                if (curChar == 61)
                                    jjstateSet[jjnewStateCnt++] = 3;
                                break;
							
                            case 5: 
                                if ((0x100002600L & l) == 0L)
                                    break;
                                kind = 21;
                                jjCheckNAdd(5);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else if (curChar < 128)
                {
                    ulong l = ((ulong) 1L) << (curChar & 63);
MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 0: 
                            case 1: 
                                if (kind > 15)
                                    kind = 15;
                                jjCheckNAdd(1);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                else
                {
                    int i2 = (curChar & 0xff) >> 6;
                    ulong l2 = ((ulong) 1L) << (curChar & 63);
                MatchLoop1: 
                    do 
                    {
                        switch (jjstateSet[--i])
                        {
							
                            case 0: 
                            case 1: 
                                if ((jjbitVec0[i2] & l2) == 0L)
                                    break;
                                if (kind > 15)
                                    kind = 15;
                                jjCheckNAdd(1);
                                break;
							
                            default:  break;
							
                        }
                    }
                    while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    jjmatchedKind = kind;
                    jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = jjnewStateCnt) == (startsAt = 6 - (jjnewStateCnt = startsAt)))
                    return curPos;
                try
                {
                    curChar = input_stream.ReadChar();
                }
                catch (System.IO.IOException e)
                {
                    return curPos;
                }
            }
        }
        internal static readonly int[] jjnextStates = new int[]{17, 18, 21, 12, 14, 5, 8, 0, 4, 6, 0, 4, 6, 5, 0, 4, 6, 12, 13, 3, 4};
        public static readonly System.String[] jjstrLiteralImages = new System.String[]{"", "\x003C\x0073\x0063\x0072\x0069\x0070\x0074", null, null, "\x003C\x0021\x002D\x002D", "\x003C\x0021", null, null, null, null, null, null, null, null, null, null, "\x003D", null, null, "\x0027", "\x0022", null, null, null, null, null, null, "\x002D\x002D\x003E", null, "\x003E"};
        public static readonly System.String[] lexStateNames = new System.String[]{"DEFAULT", "WithinScript", "WithinTag", "AfterEquals", "WithinQuote1", "WithinQuote2", "WithinComment1", "WithinComment2"};
        public static readonly int[] jjnewLexState = new int[]{-1, 1, 2, 2, 6, 7, -1, -1, - 1, -1, -1, -1, -1, -1, 0, -1, 3, 0, 2, 4, 5, -1, -1, 2, -1, 2, -1, 0, -1, 0};
        internal static readonly long[] jjtoToken = new long[]{0x3fdff67fL};
        internal static readonly long[] jjtoSkip = new long[]{0x200000L};
        protected internal SimpleCharStream input_stream;
        private uint[] jjrounds = new uint[25];
        private int[] jjstateSet = new int[50];
        protected internal char curChar;
        public HTMLParserTokenManager(SimpleCharStream stream)
        {
            InitBlock();
            if (SimpleCharStream.staticFlag)
                throw new System.ApplicationException("ERROR: Cannot use a static CharStream class with a non-static lexical analyzer.");
            input_stream = stream;
        }
        public HTMLParserTokenManager(SimpleCharStream stream, int lexState):this(stream)
        {
            SwitchTo(lexState);
        }
        public virtual void  ReInit(SimpleCharStream stream)
        {
            jjmatchedPos = jjnewStateCnt = 0;
            curLexState = defaultLexState;
            input_stream = stream;
            ReInitRounds();
        }
        private void  ReInitRounds()
        {
            int i;
            jjround = 0x80000001;
            for (i = 25; i-- > 0; )
                jjrounds[i] = 0x80000000;
        }
        public virtual void  ReInit(SimpleCharStream stream, int lexState)
        {
            ReInit(stream);
            SwitchTo(lexState);
        }
        public virtual void  SwitchTo(int lexState)
        {
            if (lexState >= 8 || lexState < 0)
                throw new TokenMgrError("Error: Ignoring invalid lexical state : " + lexState + ". State unchanged.", TokenMgrError.INVALID_LEXICAL_STATE);
            else
                curLexState = lexState;
        }
		
        protected internal virtual Token jjFillToken()
        {
            Token t = Token.newToken(jjmatchedKind);
            t.kind = jjmatchedKind;
            System.String im = jjstrLiteralImages[jjmatchedKind];
            t.image = (im == null)?input_stream.GetImage():im;
            t.beginLine = input_stream.GetBeginLine();
            t.beginColumn = input_stream.GetBeginColumn();
            t.endLine = input_stream.GetEndLine();
            t.endColumn = input_stream.GetEndColumn();
            return t;
        }
		
        internal int curLexState = 0;
        internal int defaultLexState = 0;
        internal int jjnewStateCnt;
        internal uint jjround;
        internal int jjmatchedPos;
        internal int jjmatchedKind;
		
        public virtual Token GetNextToken()
        {
            int kind;
            Token specialToken = null;
            Token matchedToken;
            int curPos = 0;
			
            for (; ; )
            {
                try
                {
                    curChar = input_stream.BeginToken();
                }
                catch (System.IO.IOException e)
                {
                    jjmatchedKind = 0;
                    matchedToken = jjFillToken();
                    return matchedToken;
                }
				
                switch (curLexState)
                {
					
                    case 0: 
                        jjmatchedKind = 0x7fffffff;
                        jjmatchedPos = 0;
                        curPos = jjMoveStringLiteralDfa0_0();
                        if (jjmatchedPos == 0 && jjmatchedKind > 12)
                        {
                            jjmatchedKind = 12;
                        }
                        break;
					
                    case 1: 
                        jjmatchedKind = 0x7fffffff;
                        jjmatchedPos = 0;
                        curPos = jjMoveStringLiteralDfa0_1();
                        break;
					
                    case 2: 
                        jjmatchedKind = 0x7fffffff;
                        jjmatchedPos = 0;
                        curPos = jjMoveStringLiteralDfa0_2();
                        break;
					
                    case 3: 
                        jjmatchedKind = 0x7fffffff;
                        jjmatchedPos = 0;
                        curPos = jjMoveStringLiteralDfa0_3();
                        break;
					
                    case 4: 
                        jjmatchedKind = 0x7fffffff;
                        jjmatchedPos = 0;
                        curPos = jjMoveStringLiteralDfa0_4();
                        break;
					
                    case 5: 
                        jjmatchedKind = 0x7fffffff;
                        jjmatchedPos = 0;
                        curPos = jjMoveStringLiteralDfa0_5();
                        break;
					
                    case 6: 
                        jjmatchedKind = 0x7fffffff;
                        jjmatchedPos = 0;
                        curPos = jjMoveStringLiteralDfa0_6();
                        break;
					
                    case 7: 
                        jjmatchedKind = 0x7fffffff;
                        jjmatchedPos = 0;
                        curPos = jjMoveStringLiteralDfa0_7();
                        break;
                }
                if (jjmatchedKind != 0x7fffffff)
                {
                    if (jjmatchedPos + 1 < curPos)
                        input_stream.Backup(curPos - jjmatchedPos - 1);
                    if ((jjtoToken[jjmatchedKind >> 6] & (1L << (jjmatchedKind & 63))) != 0L)
                    {
                        matchedToken = jjFillToken();
                        if (jjnewLexState[jjmatchedKind] != - 1)
                            curLexState = jjnewLexState[jjmatchedKind];
                        return matchedToken;
                    }
                    else
                    {
                        if (jjnewLexState[jjmatchedKind] != - 1)
                            curLexState = jjnewLexState[jjmatchedKind];
                        goto EOFLoop;
                    }
                }
                int error_line = input_stream.GetEndLine();
                int error_column = input_stream.GetEndColumn();
                System.String error_after = null;
                bool EOFSeen = false;
                try
                {
                    input_stream.ReadChar(); input_stream.Backup(1);
                }
                catch (System.IO.IOException e1)
                {
                    EOFSeen = true;
                    error_after = curPos <= 1?"":input_stream.GetImage();
                    if (curChar == '\n' || curChar == '\r')
                    {
                        error_line++;
                        error_column = 0;
                    }
                    else
                        error_column++;
                }
                if (!EOFSeen)
                {
                    input_stream.Backup(1);
                    error_after = curPos <= 1?"":input_stream.GetImage();
                }
                throw new TokenMgrError(EOFSeen, curLexState, error_line, error_column, error_after, curChar, TokenMgrError.LEXICAL_ERROR);

            EOFLoop: ;
            }
        }
    }
}