//------------------------------------------------------------------------------
// <copyright file="XmlAggregates.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Xml;
using System.Diagnostics;
using System.ComponentModel;

namespace System.Xml.Xsl.Runtime {

    /// <summary>
    /// Computes aggregates over a sequence of Int32 values.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct Int32Aggregator {
        private int result;
        private int cnt;

        public void Create() {
            this.cnt = 0;
        }

        public void Sum(int value) {
            if (this.cnt == 0) {
                this.result = value;
                this.cnt = 1;
            }
            else {
                this.result += value;
            }
        }

        public void Average(int value) {
            if (this.cnt == 0)
                this.result = value;
            else
                this.result += value;

            this.cnt++;
        }

        public void Minimum(int value) {
            if (this.cnt == 0 || value < this.result)
                this.result = value;

            this.cnt = 1;
        }

        public void Maximum(int value) {
            if (this.cnt == 0 || value > this.result)
                this.result = value;

            this.cnt = 1;
        }

        public int SumResult { get { return this.result; } }
        public int AverageResult { get { return this.result / this.cnt; } }
        public int MinimumResult { get { return this.result; } }
        public int MaximumResult { get { return this.result; } }

        public bool IsEmpty { get { return this.cnt == 0; } }
    }


    /// <summary>
    /// Computes aggregates over a sequence of Int64 values.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct Int64Aggregator {
        private long result;
        private int cnt;

        public void Create() {
            this.cnt = 0;
        }

        public void Sum(long value) {
            if (this.cnt == 0) {
                this.result = value;
                this.cnt = 1;
            }
            else {
                this.result += value;
            }
        }

        public void Average(long value) {
            if (this.cnt == 0)
                this.result = value;
            else
                this.result += value;

            this.cnt++;
        }

        public void Minimum(long value) {
            if (this.cnt == 0 || value < this.result)
                this.result = value;

            this.cnt = 1;
        }

        public void Maximum(long value) {
            if (this.cnt == 0 || value > this.result)
                this.result = value;

            this.cnt = 1;
        }

        public long SumResult { get { return this.result; } }
        public long AverageResult { get { return this.result / (long) this.cnt; } }
        public long MinimumResult { get { return this.result; } }
        public long MaximumResult { get { return this.result; } }

        public bool IsEmpty { get { return this.cnt == 0; } }
    }


    /// <summary>
    /// Computes aggregates over a sequence of Decimal values.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct DecimalAggregator {
        private decimal result;
        private int cnt;

        public void Create() {
            this.cnt = 0;
        }

        public void Sum(decimal value) {
            if (this.cnt == 0) {
                this.result = value;
                this.cnt = 1;
            }
            else {
                this.result += value;
            }
        }

        public void Average(decimal value) {
            if (this.cnt == 0)
                this.result = value;
            else
                this.result += value;

            this.cnt++;
        }

        public void Minimum(decimal value) {
            if (this.cnt == 0 || value < this.result)
                this.result = value;

            this.cnt = 1;
        }

        public void Maximum(decimal value) {
            if (this.cnt == 0 || value > this.result)
                this.result = value;

            this.cnt = 1;
        }

        public decimal SumResult { get { return this.result; } }
        public decimal AverageResult { get { return this.result / (decimal) this.cnt; } }
        public decimal MinimumResult { get { return this.result; } }
        public decimal MaximumResult { get { return this.result; } }

        public bool IsEmpty { get { return this.cnt == 0; } }
    }


    /// <summary>
    /// Computes aggregates over a sequence of Double values.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct DoubleAggregator {
        private double result;
        private int cnt;

        public void Create() {
            this.cnt = 0;
        }

        public void Sum(double value) {
            if (this.cnt == 0) {
                this.result = value;
                this.cnt = 1;
            }
            else {
                this.result += value;
            }
        }

        public void Average(double value) {
            if (this.cnt == 0)
                this.result = value;
            else
                this.result += value;

            this.cnt++;
        }

        public void Minimum(double value) {
            if (this.cnt == 0 || value < this.result || double.IsNaN(value))
                this.result = value;

            this.cnt = 1;
        }

        public void Maximum(double value) {
            if (this.cnt == 0 || value > this.result || double.IsNaN(value))
                this.result = value;

            this.cnt = 1;
        }

        public double SumResult { get { return this.result; } }
        public double AverageResult { get { return this.result / (double) this.cnt; } }
        public double MinimumResult { get { return this.result; } }
        public double MaximumResult { get { return this.result; } }

        public bool IsEmpty { get { return this.cnt == 0; } }
    }
}
