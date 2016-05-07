//------------------------------------------------------------------------------
// <copyright file="RegexInterpreter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// This RegexInterpreter class is internal to the RegularExpression package.
// It executes a block of regular expression codes while consuming
// input.


namespace System.Text.RegularExpressions
{

    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
        
    internal sealed class RegexInterpreter : RegexRunner {
        internal int runoperator;
        internal int [] runcodes;
        internal int runcodepos;
        internal String [] runstrings;
        internal RegexCode runcode;
        internal RegexPrefix runfcPrefix;
        internal RegexBoyerMoore runbmPrefix;
        internal int runanchors;
        internal bool runrtl;
        internal bool runci;
        internal CultureInfo runculture;

        internal RegexInterpreter(RegexCode code, CultureInfo culture) {
            runcode       = code;
            runcodes      = code._codes;
            runstrings    = code._strings;
            runfcPrefix   = code._fcPrefix;
            runbmPrefix   = code._bmPrefix;
            runanchors    = code._anchors;
            runculture    = culture;
        }

        protected override void InitTrackCount() {
            runtrackcount = runcode._trackcount;
        }

        private void Advance() {
            Advance(0);
        }

        private void Advance(int i) {
            runcodepos += (i + 1);
            SetOperator(runcodes[runcodepos]);
        }

        private void Goto(int newpos) {
            // when branching backward, ensure storage
            if (newpos < runcodepos)
                EnsureStorage();

            SetOperator(runcodes[newpos]);
            runcodepos = newpos;
        }

        private void Textto(int newpos) {
            runtextpos = newpos;
        }

        private void Trackto(int newpos) {
            runtrackpos = runtrack.Length - newpos;
        }

        private int Textstart() {
            return runtextstart;
        }

        private int Textpos() {
            return runtextpos;
        }

        // push onto the backtracking stack
        private int Trackpos() {
            return runtrack.Length - runtrackpos;
        }

        private void TrackPush() {
            runtrack[--runtrackpos] = runcodepos;
        }

        private void TrackPush(int I1) {
            runtrack[--runtrackpos] = I1;
            runtrack[--runtrackpos] = runcodepos;
        }

        private void TrackPush(int I1, int I2) {
            runtrack[--runtrackpos] = I1;
            runtrack[--runtrackpos] = I2;
            runtrack[--runtrackpos] = runcodepos;
        }

        private void TrackPush(int I1, int I2, int I3) {
            runtrack[--runtrackpos] = I1;
            runtrack[--runtrackpos] = I2;
            runtrack[--runtrackpos] = I3;
            runtrack[--runtrackpos] = runcodepos;
        }

        private void TrackPush2(int I1) {
            runtrack[--runtrackpos] = I1;
            runtrack[--runtrackpos] = -runcodepos;
        }

        private void TrackPush2(int I1, int I2) {
            runtrack[--runtrackpos] = I1;
            runtrack[--runtrackpos] = I2;
            runtrack[--runtrackpos] = -runcodepos;
        }

        private void Backtrack() {
            int newpos = runtrack[runtrackpos++];
#if DBG
            if (runmatch.Debug) {
                if (newpos < 0)
                    Debug.WriteLine("       Backtracking (back2) to code position " + (-newpos));
                else
                    Debug.WriteLine("       Backtracking to code position " + newpos);
            }
#endif

            if (newpos < 0) {
                newpos = -newpos;
                SetOperator(runcodes[newpos] | RegexCode.Back2);
            }
            else {
                SetOperator(runcodes[newpos] | RegexCode.Back);
            }

            // When branching backward, ensure storage
            if (newpos < runcodepos)
                EnsureStorage();

            runcodepos = newpos;
        }

        private void SetOperator(int op) {
            runci         = (0 != (op & RegexCode.Ci));
            runrtl        = (0 != (op & RegexCode.Rtl));
            runoperator   = op & ~(RegexCode.Rtl | RegexCode.Ci);
        }

        private void TrackPop() {
            runtrackpos++;
        }

        // pop framesize items from the backtracking stack
        private void TrackPop(int framesize) {
            runtrackpos += framesize;
        }

        // Technically we are actually peeking at items already popped.  So if you want to 
        // get and pop the top item from the stack, you do 
        // TrackPop();
        // TrackPeek();
        private int TrackPeek() {
            return runtrack[runtrackpos - 1];
        }

        // get the ith element down on the backtracking stack
        private int TrackPeek(int i) {
            return runtrack[runtrackpos - i - 1];
        }

        // Push onto the grouping stack
        private void StackPush(int I1) {
            runstack[--runstackpos] = I1;
        }

        private void StackPush(int I1, int I2) {
            runstack[--runstackpos] = I1;
            runstack[--runstackpos] = I2;
        }

        private void StackPop() {
            runstackpos++;
        }

        // pop framesize items from the grouping stack
        private void StackPop(int framesize) {
            runstackpos += framesize;
        }

        // Technically we are actually peeking at items already popped.  So if you want to 
        // get and pop the top item from the stack, you do 
        // StackPop();
        // StackPeek();
        private int StackPeek() {
            return runstack[runstackpos - 1];
        }

        // get the ith element down on the grouping stack
        private int StackPeek(int i) {
            return runstack[runstackpos - i - 1];
        }

        private int Operator() {
            return runoperator;
        }

        private int Operand(int i) {
            return runcodes[runcodepos + i + 1];
        }

        private int Leftchars() {
            return runtextpos - runtextbeg;
        }

        private int Rightchars() {
            return runtextend - runtextpos;
        }

        private int Bump() {
            return runrtl ? -1 : 1;
        }

        private int Forwardchars() {
            return runrtl ? runtextpos - runtextbeg : runtextend - runtextpos;
        }

        private char Forwardcharnext() {
            char ch = (runrtl ? runtext[--runtextpos] : runtext[runtextpos++]);

            return(runci ? Char.ToLower(ch, runculture) : ch);
        }

        private bool Stringmatch(String str) {
            int c;
            int pos;

            if (!runrtl) {
                if (runtextend - runtextpos < (c = str.Length))
                    return false;

                pos = runtextpos + c;
            }
            else {
                if (runtextpos - runtextbeg < (c = str.Length))
                    return false;

                pos = runtextpos;
            }

            if (!runci) {
                while (c != 0)
                    if (str[--c] != runtext[--pos])
                        return false;
            }
            else {
                while (c != 0)
                    if (str[--c] != Char.ToLower(runtext[--pos], runculture))
                        return false;
            }

            if (!runrtl) {
                pos += str.Length;
            }

            runtextpos = pos;

            return true;
        }

        private bool Refmatch(int index, int len) {
            int c;
            int pos;
            int cmpos;

            if (!runrtl) {
                if (runtextend - runtextpos < len)
                    return false;

                pos = runtextpos + len;
            }
            else {
                if (runtextpos - runtextbeg < len)
                    return false;

                pos = runtextpos;
            }
            cmpos = index + len;

            c = len;

            if (!runci) {
                while (c-- != 0)
                    if (runtext[--cmpos] != runtext[--pos])
                        return false;
            }
            else {
                while (c-- != 0)
                    if (Char.ToLower(runtext[--cmpos], runculture) != Char.ToLower(runtext[--pos], runculture))
                        return false;
            }

            if (!runrtl) {
                pos += len;
            }

            runtextpos = pos;

            return true;
        }

        private void Backwardnext() {
            runtextpos += runrtl ? 1 : -1;
        }

        private char CharAt(int j) {
            return runtext[j];
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // !!!! This function must be kept synchronized with GenerateFindFirstChar !!!!
        // !!!! in RegexCompiler.cs                                                !!!!
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        protected override bool FindFirstChar() {
            int i;
            String set;

            if (0 != (runanchors & (RegexFCD.Beginning | RegexFCD.Start | RegexFCD.EndZ | RegexFCD.End))) {
                if (!runcode._rightToLeft) {
                    if ((0 != (runanchors & RegexFCD.Beginning) && runtextpos > runtextbeg) ||
                        (0 != (runanchors & RegexFCD.Start) && runtextpos > runtextstart)) {
                        runtextpos = runtextend;
                        return false;
                    }
                    if (0 != (runanchors & RegexFCD.EndZ) && runtextpos < runtextend - 1) {
                        runtextpos = runtextend - 1;
                    }
                    else if (0 != (runanchors & RegexFCD.End) && runtextpos < runtextend) {
                        runtextpos = runtextend;
                    }
                }
                else {
                    if ((0 != (runanchors & RegexFCD.End) && runtextpos < runtextend) ||
                        (0 != (runanchors & RegexFCD.EndZ) && (runtextpos < runtextend - 1 ||
                                                               (runtextpos == runtextend - 1 && CharAt(runtextpos) != '\n'))) ||
                        (0 != (runanchors & RegexFCD.Start) && runtextpos < runtextstart)) {
                        runtextpos = runtextbeg;
                        return false;
                    }
                    if (0 != (runanchors & RegexFCD.Beginning) && runtextpos > runtextbeg) {
                        runtextpos = runtextbeg;
                    }
                }

                if (runbmPrefix != null) {
                    return runbmPrefix.IsMatch(runtext, runtextpos, runtextbeg, runtextend);
                }

                return true; // found a valid start or end anchor
            }
            else if (runbmPrefix != null) {
                runtextpos = runbmPrefix.Scan(runtext, runtextpos, runtextbeg, runtextend);

                if (runtextpos == -1) {
                    runtextpos = (runcode._rightToLeft ? runtextbeg : runtextend);
                    return false;
                }

                return true;
            }
            else if (runfcPrefix == null) {
                return true;
            }

            runrtl = runcode._rightToLeft;
            runci = runfcPrefix.CaseInsensitive;
            set = runfcPrefix.Prefix;

            if (RegexCharClass.IsSingleton(set)) {
                char ch = RegexCharClass.SingletonChar(set);

                for (i = Forwardchars(); i > 0; i--) {
                    if (ch == Forwardcharnext()) {
                        Backwardnext();
                        return true;
                    }
                }
            }
            else {
                for (i = Forwardchars(); i > 0; i--) {
                    if (RegexCharClass.CharInClass(Forwardcharnext(), set)) {
                        Backwardnext();
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void Go() {
            Goto(0);

            for (;;) {
#if DBG
                if (runmatch.Debug) {
                    DumpState();
                }
#endif

                CheckTimeout();

                switch (Operator()) {
                    case RegexCode.Stop:
                        return;

                    case RegexCode.Nothing:
                        break;

                    case RegexCode.Goto:
                        Goto(Operand(0));
                        continue;

                    case RegexCode.Testref:
                        if (!IsMatched(Operand(0)))
                            break;
                        Advance(1);
                        continue;

                    case RegexCode.Lazybranch:
                        TrackPush(Textpos());
                        Advance(1);
                        continue;

                    case RegexCode.Lazybranch | RegexCode.Back:
                        TrackPop();
                        Textto(TrackPeek());
                        Goto(Operand(0));
                        continue;

                    case RegexCode.Setmark:
                        StackPush(Textpos());
                        TrackPush();
                        Advance();
                        continue;

                    case RegexCode.Nullmark:
                        StackPush(-1);
                        TrackPush();
                        Advance();
                        continue;

                    case RegexCode.Setmark | RegexCode.Back:
                    case RegexCode.Nullmark | RegexCode.Back:
                        StackPop();
                        break;

                    case RegexCode.Getmark:
                        StackPop();
                        TrackPush(StackPeek());
                        Textto(StackPeek());
                        Advance();
                        continue;

                    case RegexCode.Getmark | RegexCode.Back:
                        TrackPop();
                        StackPush(TrackPeek());
                        break;

                    case RegexCode.Capturemark:
                        if (Operand(1) != -1 && !IsMatched(Operand(1)))
                            break;
                        StackPop();
                        if (Operand(1) != -1)
                            TransferCapture(Operand(0), Operand(1), StackPeek(), Textpos());
                        else
                            Capture(Operand(0), StackPeek(), Textpos());
                        TrackPush(StackPeek());

                        Advance(2);

                        continue;

                    case RegexCode.Capturemark | RegexCode.Back:
                        TrackPop();
                        StackPush(TrackPeek());
                        Uncapture();
                        if (Operand(0) != -1 && Operand(1) != -1)
                            Uncapture();

                        break;

                    case RegexCode.Branchmark:
                        {
                            int matched;
                            StackPop();

                            matched = Textpos() - StackPeek();

                            if (matched != 0) {                     // Nonempty match -> loop now
                                TrackPush(StackPeek(), Textpos());  // Save old mark, textpos
                                StackPush(Textpos());               // Make new mark
                                Goto(Operand(0));                   // Loop
                            }
                            else {                                  // Empty match -> straight now
                                TrackPush2(StackPeek());            // Save old mark
                                Advance(1);                         // Straight
                            }
                            continue;
                        }

                    case RegexCode.Branchmark | RegexCode.Back:
                        TrackPop(2);
                        StackPop();
                        Textto(TrackPeek(1));                       // Recall position
                        TrackPush2(TrackPeek());                    // Save old mark
                        Advance(1);                                 // Straight
                        continue;

                    case RegexCode.Branchmark | RegexCode.Back2:
                        TrackPop();
                        StackPush(TrackPeek());                     // Recall old mark
                        break;                                      // Backtrack

                    case RegexCode.Lazybranchmark:
                        {
                            // We hit this the first time through a lazy loop and after each 
                            // successful match of the inner expression.  It simply continues
                            // on and doesn't loop. 
                            StackPop();

                            int oldMarkPos = StackPeek();

                            if (Textpos() != oldMarkPos) {              // Nonempty match -> try to loop again by going to 'back' state
                                if (oldMarkPos != -1)
                                    TrackPush(oldMarkPos, Textpos());   // Save old mark, textpos
                                else
                                    TrackPush(Textpos(), Textpos());   
                            }
                            else {
                                // The inner expression found an empty match, so we'll go directly to 'back2' if we
                                // backtrack.  In this case, we need to push something on the stack, since back2 pops.
                                // However, in the case of ()+? or similar, this empty match may be legitimate, so push the text 
                                // position associated with that empty match.
                                StackPush(oldMarkPos);

                                TrackPush2(StackPeek());                // Save old mark
                            }
                            Advance(1);
                            continue;
                        }

                    case RegexCode.Lazybranchmark | RegexCode.Back:
                        {
                            // After the first time, Lazybranchmark | RegexCode.Back occurs
                            // with each iteration of the loop, and therefore with every attempted
                            // match of the inner expression.  We'll try to match the inner expression, 
                            // then go back to Lazybranchmark if successful.  If the inner expression 
                            // failes, we go to Lazybranchmark | RegexCode.Back2
                            int pos;

                            TrackPop(2);
                            pos = TrackPeek(1);
                            TrackPush2(TrackPeek());                // Save old mark
                            StackPush(pos);                         // Make new mark
                            Textto(pos);                            // Recall position
                            Goto(Operand(0));                       // Loop
                            continue;
                        }

                    case RegexCode.Lazybranchmark | RegexCode.Back2:
                        // The lazy loop has failed.  We'll do a true backtrack and 
                        // start over before the lazy loop. 
                        StackPop();
                        TrackPop();
                        StackPush(TrackPeek());                      // Recall old mark
                        break;

                    case RegexCode.Setcount:
                        StackPush(Textpos(), Operand(0));
                        TrackPush();
                        Advance(1);
                        continue;

                    case RegexCode.Nullcount:
                        StackPush(-1, Operand(0));
                        TrackPush();
                        Advance(1);
                        continue;

                    case RegexCode.Setcount | RegexCode.Back:
                        StackPop(2);
                        break;

                    case RegexCode.Nullcount | RegexCode.Back:
                        StackPop(2);
                        break;

                    case RegexCode.Branchcount:
                        // StackPush:
                        //  0: Mark
                        //  1: Count
                        {
                            StackPop(2);
                            int mark = StackPeek();
                            int count = StackPeek(1);
                            int matched = Textpos() - mark;

                            if (count >= Operand(1) || (matched == 0 && count >= 0)) {                                   // Max loops or empty match -> straight now
                                TrackPush2(mark, count);            // Save old mark, count
                                Advance(2);                         // Straight
                            }
                            else {                                  // Nonempty match -> count+loop now
                                TrackPush(mark);                    // remember mark
                                StackPush(Textpos(), count + 1);    // Make new mark, incr count
                                Goto(Operand(0));                   // Loop
                            }
                            continue;
                        }

                    case RegexCode.Branchcount | RegexCode.Back:
                        // TrackPush:
                        //  0: Previous mark
                        // StackPush:
                        //  0: Mark (= current pos, discarded)
                        //  1: Count
                        TrackPop();
                        StackPop(2);
                        if (StackPeek(1) > 0) {                         // Positive -> can go straight
                            Textto(StackPeek());                        // Zap to mark
                            TrackPush2(TrackPeek(), StackPeek(1) - 1);  // Save old mark, old count
                            Advance(2);                                 // Straight
                            continue;
                        }
                        StackPush(TrackPeek(), StackPeek(1) - 1);       // recall old mark, old count
                        break;

                    case RegexCode.Branchcount | RegexCode.Back2:
                        // TrackPush:
                        //  0: Previous mark
                        //  1: Previous count
                        TrackPop(2);
                        StackPush(TrackPeek(), TrackPeek(1));           // Recall old mark, old count
                        break;                                          // Backtrack


                    case RegexCode.Lazybranchcount:
                        // StackPush:
                        //  0: Mark
                        //  1: Count
                        {
                            StackPop(2);
                            int mark = StackPeek();
                            int count = StackPeek(1);

                            if (count < 0) {                        // Negative count -> loop now
                                TrackPush2(mark);                   // Save old mark
                                StackPush(Textpos(), count + 1);    // Make new mark, incr count
                                Goto(Operand(0));                   // Loop
                            }
                            else {                                  // Nonneg count -> straight now
                                TrackPush(mark, count, Textpos());  // Save mark, count, position
                                Advance(2);                         // Straight
                            }
                            continue;
                        }

                    case RegexCode.Lazybranchcount | RegexCode.Back:
                        // TrackPush:
                        //  0: Mark
                        //  1: Count
                        //  2: Textpos
                        {
                            TrackPop(3);
                            int mark = TrackPeek();
                            int textpos = TrackPeek(2);

                            if (TrackPeek(1) < Operand(1) && textpos != mark) { // Under limit and not empty match -> loop
                                Textto(textpos);                            // Recall position
                                StackPush(textpos, TrackPeek(1) + 1);       // Make new mark, incr count
                                TrackPush2(mark);                           // Save old mark
                                Goto(Operand(0));                           // Loop
                                continue;
                            }
                            else {                                          // Max loops or empty match -> backtrack
                                StackPush(TrackPeek(), TrackPeek(1));       // Recall old mark, count
                                break;                                      // backtrack
                            }
                        }

                    case RegexCode.Lazybranchcount | RegexCode.Back2:
                        // TrackPush:
                        //  0: Previous mark
                        // StackPush:
                        //  0: Mark (== current pos, discarded)
                        //  1: Count
                        TrackPop();
                        StackPop(2);
                        StackPush(TrackPeek(), StackPeek(1) - 1);   // Recall old mark, count
                        break;                                      // Backtrack

                    case RegexCode.Setjump:
                        StackPush(Trackpos(), Crawlpos());
                        TrackPush();
                        Advance();
                        continue;

                    case RegexCode.Setjump | RegexCode.Back:
                        StackPop(2);
                        break;

                    case RegexCode.Backjump:
                        // StackPush:
                        //  0: Saved trackpos
                        //  1: Crawlpos
                        StackPop(2);
                        Trackto(StackPeek());

                        while (Crawlpos() != StackPeek(1))
                            Uncapture();

                        break;

                    case RegexCode.Forejump:
                        // StackPush:
                        //  0: Saved trackpos
                        //  1: Crawlpos
                        StackPop(2);
                        Trackto(StackPeek());
                        TrackPush(StackPeek(1));
                        Advance();
                        continue;

                    case RegexCode.Forejump | RegexCode.Back:
                        // TrackPush:
                        //  0: Crawlpos
                        TrackPop();

                        while (Crawlpos() != TrackPeek())
                            Uncapture();

                        break;

                    case RegexCode.Bol:
                        if (Leftchars() > 0 && CharAt(Textpos() - 1) != '\n')
                            break;
                        Advance();
                        continue;

                    case RegexCode.Eol:
                        if (Rightchars() > 0 && CharAt(Textpos()) != '\n')
                            break;
                        Advance();
                        continue;

                    case RegexCode.Boundary:
                        if (!IsBoundary(Textpos(), runtextbeg, runtextend))
                            break;
                        Advance();
                        continue;

                    case RegexCode.Nonboundary:
                        if (IsBoundary(Textpos(), runtextbeg, runtextend))
                            break;
                        Advance();
                        continue;

                    case RegexCode.ECMABoundary:
                        if (!IsECMABoundary(Textpos(), runtextbeg, runtextend))
                            break;
                        Advance();
                        continue;

                    case RegexCode.NonECMABoundary:
                        if (IsECMABoundary(Textpos(), runtextbeg, runtextend))
                            break;
                        Advance();
                        continue;

                    case RegexCode.Beginning:
                        if (Leftchars() > 0)
                            break;
                        Advance();
                        continue;

                    case RegexCode.Start:
                        if (Textpos() != Textstart())
                            break;
                        Advance();
                        continue;

                    case RegexCode.EndZ:
                        if (Rightchars() > 1 || Rightchars() == 1 && CharAt(Textpos()) != '\n')
                            break;
                        Advance();
                        continue;

                    case RegexCode.End:
                        if (Rightchars() > 0)
                            break;
                        Advance();
                        continue;

                    case RegexCode.One:
                        if (Forwardchars() < 1 || Forwardcharnext() != (char)Operand(0))
                            break;

                        Advance(1);
                        continue;

                    case RegexCode.Notone:
                        if (Forwardchars() < 1 || Forwardcharnext() == (char)Operand(0))
                            break;

                        Advance(1);
                        continue;

                    case RegexCode.Set:
                        if (Forwardchars() < 1 || !RegexCharClass.CharInClass(Forwardcharnext(), runstrings[Operand(0)]))
                            break;

                        Advance(1);
                        continue;

                    case RegexCode.Multi:
                        {
                            if (!Stringmatch(runstrings[Operand(0)]))
                                break;

                            Advance(1);
                            continue;
                        }

                    case RegexCode.Ref:
                        {
                            int capnum = Operand(0);

                            if (IsMatched(capnum)) {
                                if (!Refmatch(MatchIndex(capnum), MatchLength(capnum)))
                                    break;
                            } else {
                                if ((runregex.roptions & RegexOptions.ECMAScript) == 0)
                                    break;
                            }

                            Advance(1);
                            continue;
                        }

                    case RegexCode.Onerep:
                        {
                            int c = Operand(1);

                            if (Forwardchars() < c)
                                break;

                            char ch = (char)Operand(0);

                            while (c-- > 0)
                                if (Forwardcharnext() != ch)
                                    goto BreakBackward;

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Notonerep:
                        {
                            int c = Operand(1);

                            if (Forwardchars() < c)
                                break;

                            char ch = (char)Operand(0);

                            while (c-- > 0)
                                if (Forwardcharnext() == ch)
                                    goto BreakBackward;

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Setrep:
                        {
                            int c = Operand(1);

                            if (Forwardchars() < c)
                                break;

                            String set = runstrings[Operand(0)];

                            while (c-- > 0)
                                if (!RegexCharClass.CharInClass(Forwardcharnext(), set))
                                    goto BreakBackward;

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Oneloop:
                        {
                            int c = Operand(1);

                            if (c > Forwardchars())
                                c = Forwardchars();

                            char ch = (char)Operand(0);
                            int i;

                            for (i = c; i > 0; i--) {
                                if (Forwardcharnext() != ch) {
                                    Backwardnext();
                                    break;
                                }
                            }

                            if (c > i)
                                TrackPush(c - i - 1, Textpos() - Bump());

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Notoneloop:
                        {
                            int c = Operand(1);

                            if (c > Forwardchars())
                                c = Forwardchars();

                            char ch = (char)Operand(0);
                            int i;

                            for (i = c; i > 0; i--) {
                                if (Forwardcharnext() == ch) {
                                    Backwardnext();
                                    break;
                                }
                            }

                            if (c > i)
                                TrackPush(c - i - 1, Textpos() - Bump());

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Setloop:
                        {
                            int c = Operand(1);

                            if (c > Forwardchars())
                                c = Forwardchars();

                            String set = runstrings[Operand(0)];
                            int i;

                            for (i = c; i > 0; i--) {
                                if (!RegexCharClass.CharInClass(Forwardcharnext(), set)) {
                                    Backwardnext();
                                    break;
                                }
                            }

                            if (c > i)
                                TrackPush(c - i - 1, Textpos() - Bump());

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Oneloop | RegexCode.Back:
                    case RegexCode.Notoneloop | RegexCode.Back:
                        {
                            TrackPop(2);
                            int i   = TrackPeek();
                            int pos = TrackPeek(1);

                            Textto(pos);

                            if (i > 0)
                                TrackPush(i - 1, pos - Bump());

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Setloop | RegexCode.Back:
                        {
                            TrackPop(2);
                            int i   = TrackPeek();
                            int pos = TrackPeek(1);

                            Textto(pos);

                            if (i > 0)
                                TrackPush(i - 1, pos - Bump());

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Onelazy:
                    case RegexCode.Notonelazy:
                        {
                            int c = Operand(1);

                            if (c > Forwardchars())
                                c = Forwardchars();

                            if (c > 0)
                                TrackPush(c - 1, Textpos());

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Setlazy:
                        {
                            int c = Operand(1);

                            if (c > Forwardchars())
                                c = Forwardchars();

                            if (c > 0)
                                TrackPush(c - 1, Textpos());

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Onelazy | RegexCode.Back:
                        {
                            TrackPop(2);
                            int pos = TrackPeek(1);
                            Textto(pos);

                            if (Forwardcharnext() != (char)Operand(0))
                                break;

                            int i = TrackPeek();

                            if (i > 0)
                                TrackPush(i - 1, pos + Bump());

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Notonelazy | RegexCode.Back:
                        {
                            TrackPop(2);
                            int pos = TrackPeek(1);
                            Textto(pos);

                            if (Forwardcharnext() == (char)Operand(0))
                                break;

                            int i = TrackPeek();

                            if (i > 0)
                                TrackPush(i - 1, pos + Bump());

                            Advance(2);
                            continue;
                        }

                    case RegexCode.Setlazy | RegexCode.Back:
                        {
                            TrackPop(2);
                            int pos = TrackPeek(1);
                            Textto(pos);

                            if (!RegexCharClass.CharInClass(Forwardcharnext(), runstrings[Operand(0)]))
                                break;

                            int i = TrackPeek();

                            if (i > 0)
                                TrackPush(i - 1, pos + Bump());

                            Advance(2);
                            continue;
                        }

                    default:
                        throw new NotImplementedException(SR.GetString(SR.UnimplementedState));
                }

                BreakBackward: 
                ;

                // "break Backward" comes here:
                Backtrack();
            }

        }

#if DBG
        internal override void DumpState() {
            base.DumpState();
            Debug.WriteLine("       " + runcode.OpcodeDescription(runcodepos) +
                              ((runoperator & RegexCode.Back) != 0 ? " Back" : "") +
                              ((runoperator & RegexCode.Back2) != 0 ? " Back2" : ""));
            Debug.WriteLine("");
        }
#endif

    }



}
