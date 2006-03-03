//
// ResourceManager representing an integer, used by other test cases
//
// Author:
//	Ankit Jain	<JAnkit@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using NUnit.Framework;

namespace MonoTests.System.Transactions
{
    public class IntResourceManager
    {
        public IntResourceManager (int value) 
        { 
            actual = value;
            guid = Guid.NewGuid ();
        }

        private int actual;
        private int tmpValue;
        private Transaction transaction = null;

        public int NumPrepare = 0;
        public int NumRollback = 0;
        public int NumCommit = 0;
        public int NumInDoubt = 0;
        public int NumSingle = 0;

        public bool FailPrepare = false;
        public bool FailWithException = false;
        public bool IgnorePrepare = false;

        public bool Volatile = true;
        public bool IgnoreSPC = false;
        public bool FailSPC = false;
        public bool FailCommit = false;
        public bool UseSingle = false;

        Guid guid;

        public int Actual {
            get { return actual; }
        }

        public int Value {
            get { return transaction == null ? actual : tmpValue; }
            set
            {
                if (Transaction.Current == null) {
                    /* Not in a transaction */
                    actual = value;
                    return;
                }
                /* FIXME: Do what in this case? */
                if (transaction != null)
                    Console.WriteLine ("WARNING: Setting value more than once");

                if (transaction != Transaction.Current) {
                    transaction = Transaction.Current;
                    
                    if (UseSingle) {
                        SinglePhaseNotification enlistment = new SinglePhaseNotification ( this );
                        if ( Volatile )
                            transaction.EnlistVolatile ( enlistment, EnlistmentOptions.None );
                        else
                            transaction.EnlistDurable ( guid, enlistment, EnlistmentOptions.None );
                    } else {
                        EnlistmentNotification enlistment = new EnlistmentNotification ( this );
                        if ( Volatile )
                            transaction.EnlistVolatile ( enlistment, EnlistmentOptions.None );
                        else
                            transaction.EnlistDurable ( guid, enlistment, EnlistmentOptions.None );
                    }
                }
                tmpValue = value;
            }
        }

        public void Commit ()
        {
            actual = tmpValue;
            transaction = null;
        }

        public void Rollback ()
        {
            transaction = null;
        }

        public  void CheckSPC ( string msg )
        {
            Check ( 1, 0, 0, 0, 0, msg );
        }

        public void Check2PC ( string msg)
        {
            Check ( 0, 1, 1, 0, 0, msg );
        }

        public void Check ( int s, int p, int c, int r, int d, string msg )
        {
            Assert.AreEqual ( s, NumSingle, msg + ": NumSingle" );
            Assert.AreEqual ( p, NumPrepare, msg + ": NumPrepare" );
            Assert.AreEqual ( c, NumCommit, msg + ": NumCommit" );
            Assert.AreEqual ( r, NumRollback, msg + ": NumRollback" );
            Assert.AreEqual ( d, NumInDoubt, msg + ": NumInDoubt" );
        }
       
        /* Used for volatile RMs */
        public void Check ( int p, int c, int r, int d, string msg )
        {
            Check ( 0, p, c, r, d, msg );
        }
    }

    public class EnlistmentNotification : IEnlistmentNotification {
        protected IntResourceManager resource;

        public EnlistmentNotification ( IntResourceManager resource )
        {
            this.resource = resource;
        }

        public void Prepare ( PreparingEnlistment preparingEnlistment )
        {
            resource.NumPrepare++;
            if ( resource.IgnorePrepare )
                return;

            if ( resource.FailPrepare ) {
                if (resource.FailWithException)
                    preparingEnlistment.ForceRollback ( new NotSupportedException () );
                else
                    preparingEnlistment.ForceRollback ();
            } else {
                preparingEnlistment.Prepared ();
            }
        }

        public void Commit ( Enlistment enlistment )
        {
            resource.NumCommit++;
            if ( resource.FailCommit )
                return;

            resource.Commit ();
            enlistment.Done ();
        }

        public void Rollback ( Enlistment enlistment )
        {
            resource.NumRollback++;
            resource.Rollback ();
        }

        public void InDoubt ( Enlistment enlistment )
        {
            resource.NumInDoubt++;
            throw new Exception ( "IntResourceManager.InDoubt is not implemented." );
        }

    }

    public class SinglePhaseNotification : EnlistmentNotification, ISinglePhaseNotification 
    {
        public SinglePhaseNotification ( IntResourceManager resource )
            : base ( resource )
        {
        }

        public void SinglePhaseCommit ( SinglePhaseEnlistment enlistment )
        {
            resource.NumSingle++;
            if ( resource.IgnoreSPC )
                return;

            if ( resource.FailSPC ) {
                if ( resource.FailWithException )
                    enlistment.Aborted ( new NotSupportedException () );
                else
                    enlistment.Aborted ();
            }
            else {
                resource.Commit ();
                enlistment.Committed ();
            }

        }

    }
}

