//
// Financial.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

//
// Copyright (c) 2002-2003 Mainsoft Corporation.
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic
{
	[StandardModule] 
	sealed public class Financial {
		// Declarations
		// Constructors
		private Financial() {} // prevent public default constructor
		// Properties
		// Methods
		public static double DDB (double Cost, double Salvage, double Life, double Period, 
					  [Optional, __DefaultArgumentValue(2)] double Factor)
		{ 
			// LAMESPEC: MSDN says Life and Factor only throws exception if < 0, but Implementation throws exception if <= 0
			if (Cost < 0
			    || Salvage < 0
			    || Life <= 0
			    || Period < 0
			    || Factor <= 0
			    || Period > Life)
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "Factor"));
			
			return (((Cost - Salvage) * Factor) / Life) * Period;
		}
		
		public static double FV (double Rate, double NPer, double Pmt, 
					 [Optional, __DefaultArgumentValue(0)] double PV, 
					 [Optional, __DefaultArgumentValue(0)] DueDate Due)
		{ 
			Pmt = -Pmt;
			PV = -PV;
			double currentRate = Math.Pow (Rate + 1, NPer);
			double sum = 0;
			
			if (Rate != 0)
				sum = Pmt * ((currentRate - 1) / Rate);
			else
				sum = Pmt * NPer;
	
			if (Due == DueDate.BegOfPeriod)
				sum *= (1 + Rate);
			
			return PV * currentRate + sum;
		}
		
		public static double IPmt (double Rate, double Per, double NPer, double PV, 
					   [Optional, __DefaultArgumentValue(0)] double FV, 
					   [Optional, __DefaultArgumentValue(0)] DueDate Due)
		{ 
			double totalFutureVal;
			double totalPaymentValue;
			double numberOfPeriods;
			
			if ((Per <= 0) || (Per >= (NPer + 1)))
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "Per"));
			if ((Due == DueDate.BegOfPeriod) && (Per == 1))
				return 0;
			
			totalPaymentValue = Pmt (Rate, NPer, PV, FV, Due);
			if (Due == DueDate.BegOfPeriod)
				PV = (PV + totalPaymentValue);
			
			numberOfPeriods = Per - ((int)Due) - 1;
			totalFutureVal =
				Financial.FV (Rate, numberOfPeriods, totalPaymentValue, PV, DueDate.EndOfPeriod);
			
			return (totalFutureVal * Rate);
		}
		
		public static double IRR (ref double[] ValueArray, [Optional, __DefaultArgumentValue(0.1)] double Guess) 
		{ 
			double origPV, updatedPV, updateGuess, tmp;
			double rateDiff = 0.0;
			double pvDiff = 0.0;
			int length;
			
			// MS.NET 2.0 docs say that Guess may not be <= -1
			if (Guess <= -1)        
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "Guess"));        
			try {
				length = ValueArray.GetLength(0);
			}
			catch {
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "ValueArray"));
			}
			if (length < 2)        
			    throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "ValueArray"));       
			
			origPV = NPV (Guess, ref ValueArray);
			updateGuess = (origPV > 0) ? Guess + 0.00001 : Guess - 0.00001;   
			
			if (updateGuess < -1)
			    throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "Rate"));
			
			rateDiff =  updateGuess - Guess;       
			updatedPV = NPV (updateGuess, ref ValueArray);
			pvDiff = updatedPV - origPV;
			for (int i = 0; i < 20; i++) {
				Guess = (updateGuess > Guess) ? (Guess - 0.00001) : (Guess + 0.00001);
				origPV = NPV (Guess, ref ValueArray);
				rateDiff =  updateGuess - Guess;
				pvDiff = updatedPV - origPV;   
				if (pvDiff == 0)
					throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue"));
				Guess = updateGuess - (rateDiff * updatedPV / pvDiff);
				if (Guess < -1)
				Guess = -1;
				origPV = NPV (Guess, ref ValueArray);
				if ((Math.Abs (origPV) < 0.0000001) && (Math.Abs (rateDiff) < 0.00001))
					return Guess;
				
				tmp = Guess;
				Guess = updateGuess;
				updateGuess = tmp;
				tmp = origPV;
				origPV = updatedPV;
				updatedPV = tmp;
			}
			double origPVAbs = Math.Abs (origPV);
			double updatedPVAbs = Math.Abs (updatedPV);
			if ((origPVAbs < 0.0000001) && (updatedPVAbs < 0.0000001))
			    return (origPVAbs < updatedPVAbs) ? Guess : updateGuess;
			else if (origPVAbs < 0.0000001)
			    return  Guess;
			else if (updatedPVAbs < 0.0000001)
			    return updateGuess;
			else                
			    throw new ArgumentException(Utils.GetResourceString ("Argument_InvalidValue"));
		}
		
		public static double MIRR (ref double[] ValueArray, double FinanceRate, double ReinvestRate)
		{ 
			double [] array = ValueArray;
			double loansVal = 0;
			double assetsVal = 0;
			double currentLoanRate = 1;
			double currentAssetsRate = 1;
			double totalInterestRate = 0;
			int arrayLength = 0;
			if (array.Rank != 1)
				throw new ArgumentException (Utils.GetResourceString ("Argument_RankEQOne1", "ValueArray"));
			else if (FinanceRate == -1)
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "FinanceRate"));
			else if (ReinvestRate == -1)
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "ReinvestRate"));
			
			arrayLength = array.Length;
			if (arrayLength < 2)
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "ValueArray"));
			
			for (int i = 0; i < arrayLength; i++) {
				currentLoanRate *= (1 + FinanceRate);
				currentAssetsRate *= (1 + ReinvestRate);
				if (array [i] < 0)
				loansVal += (array [i] / currentLoanRate);
				else if (array [i] > 0)
				assetsVal += (array [i] / currentAssetsRate);
			}
			
			if (loansVal == 0)
				throw new DivideByZeroException (Utils.GetResourceString ("Financial_CalcDivByZero"));
			
			totalInterestRate =
				((-assetsVal * Math.Pow (ReinvestRate + 1, arrayLength))
				/ (loansVal * (FinanceRate + 1)));
			if (totalInterestRate < 0)
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue"));
			
			return (Math.Pow (totalInterestRate, 1 / (double) (arrayLength - 1))) - 1;
		}
		
		public static double NPer (double Rate, double Pmt, double PV, 
					   [Optional, __DefaultArgumentValue(0)] double FV, 
					   [Optional, __DefaultArgumentValue(0)] DueDate Due)
		{ 
			double totalIncomeFromFlow, sumOfPvAndPayment, currentValueOfPvAndPayment;
			if (Rate == 0 && Pmt == 0)
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "Pmt"));
			else if (Rate == 0)
				return (- (PV + FV) / Pmt);
			// MainSoft had Rate < -1, but MS.NET 2.0 Doc says Rate should not be <= -1
			else if (Rate <= -1)
				throw new ArgumentException(Utils.GetResourceString ("Argument_InvalidValue1", "Rate"));
			totalIncomeFromFlow = (Pmt / Rate);
			if (Due == DueDate.BegOfPeriod)
			totalIncomeFromFlow *= (1 + Rate);
			
			sumOfPvAndPayment = (-FV + totalIncomeFromFlow);
			currentValueOfPvAndPayment = (PV + totalIncomeFromFlow);
			if ((sumOfPvAndPayment < 0) && (currentValueOfPvAndPayment < 0)) {
				sumOfPvAndPayment = -sumOfPvAndPayment;
				currentValueOfPvAndPayment = -currentValueOfPvAndPayment;
			}
			else if ((sumOfPvAndPayment <= 0) || (currentValueOfPvAndPayment < 0))
				throw new ArgumentException (Utils.GetResourceString ("Financial_CannotCalculateNPer"));
			
			double totalInterestRate = sumOfPvAndPayment / currentValueOfPvAndPayment;
			return Math.Log (totalInterestRate) / Math.Log (Rate + 1);
		}
		
		public static double NPV (double Rate, ref double[] ValueArray) 
		{ 
			if (ValueArray == null)
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidNullValue1", "ValueArray"));
			
			double [] arr = ValueArray;
			if (arr.Rank != 1)
				throw new ArgumentException (Utils.GetResourceString ("Argument_RankEQOne1", "ValueArray"));
			if (Rate == -1)
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "Rate"));
			int length = arr.Length;
			if (length < 0)
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "ValueArray"));
			
			double currentValue = 0;
			double currentRate = 1;
			double sum = 0;
			for (int index = 0; index < length; index++) {
				currentValue = arr [index];
				currentRate *= (1 + Rate);
				sum += (currentValue / currentRate);
			}
			return sum;
		}
		
		public static double Pmt (double Rate, double NPer, double PV, 
					  [Optional, __DefaultArgumentValue(0)] double FV, 
					  [Optional, __DefaultArgumentValue(0)] DueDate Due)
		{ 
			PV = -PV;
			FV = -FV;
			if (NPer == 0)
				throw new ArgumentException (Utils.GetResourceString ("Argument_InvalidValue1", "NPer"));
			
			double totalFutureVal = 0;
			double geometricSum = 0; 
			if (Rate == 0) {
				totalFutureVal = FV + PV;
				geometricSum = NPer;
			}
			else if (Due == DueDate.EndOfPeriod) {
				double totalRate = Math.Pow (Rate + 1, NPer);
				totalFutureVal = FV + PV * totalRate;
				geometricSum = (totalRate - 1) / Rate;
			}
			else if (Due == DueDate.BegOfPeriod) {
				double totalRate = Math.Pow (Rate + 1, NPer);
				totalFutureVal = FV + PV * totalRate;
				geometricSum = ((1 + Rate) * (totalRate - 1)) / Rate;        
			}
			return (totalFutureVal) / geometricSum; 
		}
		
		public static double PPmt (double Rate, double Per, double NPer, double PV, 
					   [Optional, __DefaultArgumentValue(0)] double FV, 
					   [Optional, __DefaultArgumentValue(0)] DueDate Due)
		{ 
			if ((Per <= 0) || (Per >= (NPer + 1)))
				throw new ArgumentException(Utils.GetResourceString ("PPMT_PerGT0AndLTNPer", "Per"));
			double interestPayment = IPmt (Rate, Per, NPer, PV, FV, Due);
			double totalPayment = Pmt (Rate, NPer, PV, FV, Due);
			return (totalPayment - interestPayment);
		}
		
		public static double PV (double Rate, double NPer, double Pmt, 
					 [Optional, __DefaultArgumentValue(0)] double FV, 
					 [Optional, __DefaultArgumentValue(0)] DueDate Due)
		{ 
			Pmt = -Pmt;
			FV = -FV;
			double currentRate = 1;
			double sum = 0;
			for (int index = 1; index <= NPer; index++) {
				currentRate *= (1 + Rate);
				sum += (Pmt / currentRate);
			}
			
			if (Due == DueDate.BegOfPeriod)
				sum *= (1 + Rate);
			return sum + FV / currentRate;    
		}
		
		public static double Rate (double NPer, double Pmt, double PV, 
					   [Optional, __DefaultArgumentValue(0)] double FV, 
					   [Optional, __DefaultArgumentValue(0)] DueDate Due, 
					   [Optional, __DefaultArgumentValue(0.1)] double Guess)
		{ 
			double updatedGuess, tmp, origFv, updatedFv;
			double rateDiff = 0.0;
			double fvDiff = 0.0;
			
			if (NPer < 0)
				throw new ArgumentException (Utils.GetResourceString ("Rate_NPerMustBeGTZero"));
			origFv = -Financial.FV (Guess, NPer, Pmt, PV, Due) + FV;
			updatedGuess = (origFv > 0) ? (Guess / 2) : (Guess * 2);
			rateDiff = updatedGuess - Guess;
			updatedFv = -Financial.FV (updatedGuess, NPer, Pmt, PV, Due) + FV;
			fvDiff = updatedFv - origFv;
			for (int i = 0; i < 20; i++) {
				Guess += (updatedGuess > Guess) ? -0.00001 : 0.00001;
				origFv = -Financial.FV (Guess, NPer, Pmt, PV, Due) + FV;
				rateDiff = updatedGuess - Guess;
				fvDiff = updatedFv - origFv;
				if (fvDiff == 0)
					throw new ArgumentException (Utils.GetResourceString ("Financial_CalcDivByZero"));
				Guess = updatedGuess - (rateDiff * updatedFv / fvDiff);
				origFv = -Financial.FV (Guess, NPer, Pmt, PV, Due) + FV;
				if (Math.Abs (origFv) < 0.0000001)
					return Guess;
				tmp = Guess;
				Guess = updatedGuess;
				updatedGuess = tmp;
				tmp = origFv;
				origFv = updatedFv;
				updatedFv = tmp;
			}
			double origFVAbs = Math.Abs (origFv);
			double updatedFVAbs = Math.Abs (updatedFv);
			if ((origFVAbs < 0.0000001) && (updatedFVAbs < 0.0000001))
				return (origFVAbs < updatedFVAbs) ? Guess : updatedGuess;
			else if (origFVAbs < 0.0000001)
				return Guess;
			else if (updatedFVAbs < 0.0000001)
				return updatedGuess;
			else
				throw new ArgumentException (Utils.GetResourceString ("Financial_CannotCalculateRate")); 
		}
		
		public static double SLN (double Cost, double Salvage, double Life)
		{ 
			if (Life == 0)
				throw new ArgumentException (Utils.GetResourceString ("Financial_LifeNEZero"));
			
			return (Cost - Salvage) / Life;
		}
		
		public static double SYD (double Cost, double Salvage, double Life, double Period) 
		{ 
			if (Period <= 0)
				throw new ArgumentException (Utils.GetResourceString("Financial_ArgGTZero1", "Period"));
			else if (Salvage < 0)
				throw new ArgumentException (Utils.GetResourceString("Financial_ArgGEZero1", "Salvage"));
			else if (Period > Life)
				throw new ArgumentException (Utils.GetResourceString("Financial_PeriodLELife"));
			
			double depreciation =  (Cost - Salvage);
			double sumOfDigits = (Life + 1) * Life / 2;
			double currentPeriodPart = Life + 1 - Period ; 
			
			return depreciation * currentPeriodPart / sumOfDigits;
		}
		// Events
	}
}
