using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jetstream;

// copyright notice from jetstream benchmark:
/*
 * Copyright (C) 2018 Apple Inc. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY APPLE INC. ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL APPLE INC. OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
 * OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
 */


public static class Statistics {
    public static readonly Dictionary<double, double[]> tDistributionByOneSidedProbability = 
        new Dictionary<double, double[]> {
        { 0.9, new [] {
            3.077684, 1.885618, 1.637744, 1.533206, 1.475884, 1.439756, 1.414924, 1.396815, 1.383029, 1.372184,
            1.363430, 1.356217, 1.350171, 1.345030, 1.340606, 1.336757, 1.333379, 1.330391, 1.327728, 1.325341,
            1.323188, 1.321237, 1.319460, 1.317836, 1.316345, 1.314972, 1.313703, 1.312527, 1.311434, 1.310415,
            1.309464, 1.308573, 1.307737, 1.306952, 1.306212, 1.305514, 1.304854, 1.304230, 1.303639, 1.303077,
            1.302543, 1.302035, 1.301552, 1.301090, 1.300649, 1.300228, 1.299825, 1.299439, 1.299069, 1.298714,

            1.298373, 1.298045, 1.297730, 1.297426, 1.297134, 1.296853, 1.296581, 1.296319, 1.296066, 1.295821,
            1.295585, 1.295356, 1.295134, 1.294920, 1.294712, 1.294511, 1.294315, 1.294126, 1.293942, 1.293763,
            1.293589, 1.293421, 1.293256, 1.293097, 1.292941, 1.292790, 1.292643, 1.292500, 1.292360, 1.292224,
            1.292091, 1.291961, 1.291835, 1.291711, 1.291591, 1.291473, 1.291358, 1.291246, 1.291136, 1.291029,
            1.290924, 1.290821, 1.290721, 1.290623, 1.290527, 1.290432, 1.290340, 1.290250, 1.290161, 1.290075 } },
        { 0.95, new [] {
            6.313752, 2.919986, 2.353363, 2.131847, 2.015048, 1.943180, 1.894579, 1.859548, 1.833113, 1.812461,
            1.795885, 1.782288, 1.770933, 1.761310, 1.753050, 1.745884, 1.739607, 1.734064, 1.729133, 1.724718,
            1.720743, 1.717144, 1.713872, 1.710882, 1.708141, 1.705618, 1.703288, 1.701131, 1.699127, 1.697261,
            1.695519, 1.693889, 1.692360, 1.690924, 1.689572, 1.688298, 1.687094, 1.685954, 1.684875, 1.683851,
            1.682878, 1.681952, 1.681071, 1.680230, 1.679427, 1.678660, 1.677927, 1.677224, 1.676551, 1.675905,

            1.675285, 1.674689, 1.674116, 1.673565, 1.673034, 1.672522, 1.672029, 1.671553, 1.671093, 1.670649,
            1.670219, 1.669804, 1.669402, 1.669013, 1.668636, 1.668271, 1.667916, 1.667572, 1.667239, 1.666914,
            1.666600, 1.666294, 1.665996, 1.665707, 1.665425, 1.665151, 1.664885, 1.664625, 1.664371, 1.664125,
            1.663884, 1.663649, 1.663420, 1.663197, 1.662978, 1.662765, 1.662557, 1.662354, 1.662155, 1.661961,
            1.661771, 1.661585, 1.661404, 1.661226, 1.661052, 1.660881, 1.660715, 1.660551, 1.660391, 1.660234 } },
        { 0.975, new [] {
            12.706205, 4.302653, 3.182446, 2.776445, 2.570582, 2.446912, 2.364624, 2.306004, 2.262157, 2.228139,
            2.200985, 2.178813, 2.160369, 2.144787, 2.131450, 2.119905, 2.109816, 2.100922, 2.093024, 2.085963,
            2.079614, 2.073873, 2.068658, 2.063899, 2.059539, 2.055529, 2.051831, 2.048407, 2.045230, 2.042272,
            2.039513, 2.036933, 2.034515, 2.032245, 2.030108, 2.028094, 2.026192, 2.024394, 2.022691, 2.021075,
            2.019541, 2.018082, 2.016692, 2.015368, 2.014103, 2.012896, 2.011741, 2.010635, 2.009575, 2.008559,

            2.007584, 2.006647, 2.005746, 2.004879, 2.004045, 2.003241, 2.002465, 2.001717, 2.000995, 2.000298,
            1.999624, 1.998972, 1.998341, 1.997730, 1.997138, 1.996564, 1.996008, 1.995469, 1.994945, 1.994437,
            1.993943, 1.993464, 1.992997, 1.992543, 1.992102, 1.991673, 1.991254, 1.990847, 1.990450, 1.990063,
            1.989686, 1.989319, 1.988960, 1.988610, 1.988268, 1.987934, 1.987608, 1.987290, 1.986979, 1.986675,
            1.986377, 1.986086, 1.985802, 1.985523, 1.985251, 1.984984, 1.984723, 1.984467, 1.984217, 1.983972 } },
        { 0.99, new [] {
            31.820516, 6.964557, 4.540703, 3.746947, 3.364930, 3.142668, 2.997952, 2.896459, 2.821438, 2.763769,
            2.718079, 2.680998, 2.650309, 2.624494, 2.602480, 2.583487, 2.566934, 2.552380, 2.539483, 2.527977,
            2.517648, 2.508325, 2.499867, 2.492159, 2.485107, 2.478630, 2.472660, 2.467140, 2.462021, 2.457262,
            2.452824, 2.448678, 2.444794, 2.441150, 2.437723, 2.434494, 2.431447, 2.428568, 2.425841, 2.423257,
            2.420803, 2.418470, 2.416250, 2.414134, 2.412116, 2.410188, 2.408345, 2.406581, 2.404892, 2.403272,

            2.401718, 2.400225, 2.398790, 2.397410, 2.396081, 2.394801, 2.393568, 2.392377, 2.391229, 2.390119,
            2.389047, 2.388011, 2.387008, 2.386037, 2.385097, 2.384186, 2.383302, 2.382446, 2.381615, 2.380807,
            2.380024, 2.379262, 2.378522, 2.377802, 2.377102, 2.376420, 2.375757, 2.375111, 2.374482, 2.373868,
            2.373270, 2.372687, 2.372119, 2.371564, 2.371022, 2.370493, 2.369977, 2.369472, 2.368979, 2.368497,
            2.368026, 2.367566, 2.367115, 2.366674, 2.366243, 2.365821, 2.365407, 2.365002, 2.364606, 2.364217 } },
    };

    public const bool debuggingSegmentation = false;
    public const bool debuggingTestingRangeNomination = false;

    public static double oneSidedToTwoSidedProbability (double probability) { return 2 * probability - 1; }
    public static double twoSidedToOneSidedProbability (double probability) { return (1 - (1 - probability) / 2); }

    public static double min (double[] values) {
        var result = values[0];
        for (int i = 1; i < values.Length; i++)
            result = Math.Min(result, values[i]);
        return result;
    }

    public static double max (double[] values) {
        var result = values[0];
        for (int i = 1; i < values.Length; i++)
            result = Math.Max(result, values[i]);
        return result;
    }

    public static double sum (double[] values) {
        double result = 0;
        foreach (var v in values)
            result += v;
        return result;
    }

    public static double mean (double[] values) {
        return sum(values) / values.Length;
    }

    public static double median (double[] values) {
        Array.Sort(values, (a, b) => a.CompareTo(b));
        return values[values.Length / 2];
    }

    public static double squareSum (double[] values) {
        double result = 0;
        foreach (var v in values)
            result += (v * v);
        return result;
    }

    // With sum and sum of squares, we can compute the sample standard deviation in O(1).
    // See https://rniwa.com/2012-11-10/sample-standard-deviation-in-terms-of-sum-and-square-sum-of-samples/
    public static double sampleStandardDeviation (double numberOfSamples, double sum, double squareSum) {
        if (numberOfSamples < 2)
            return 0;
        return Math.Sqrt(squareSum / (numberOfSamples - 1) - sum * sum / (numberOfSamples - 1) / numberOfSamples);
    }

    // FIXME
    public static double[] supportedConfidenceIntervalProbabilities () {
        var supportedProbabilities = new List<double>();
        foreach (var probability in tDistributionByOneSidedProbability.Keys) {
            var p = oneSidedToTwoSidedProbability(probability);
            supportedProbabilities.Add(Math.Round(p, 2));
        }
        return supportedProbabilities.ToArray();
    }

    // FIXME
    public static double[] supportedOneSideTTestProbabilities () {
        return tDistributionByOneSidedProbability.Keys.ToArray();
    }

    // Computes the delta d s.t. (mean - d, mean + d) is the confidence interval with the specified probability in O(1).
    public static double confidenceIntervalDelta (double probability, int numberOfSamples, double sum, double squareSum) {
        var oneSidedProbability = twoSidedToOneSidedProbability(probability);

        if (!tDistributionByOneSidedProbability.ContainsKey(oneSidedProbability))
            throw new Exception("Confidence interval not supported");
        if (numberOfSamples - 2 < 0)
            return double.NaN;
        var deltas = tDistributionByOneSidedProbability[oneSidedProbability];
        var degreesOfFreedom = numberOfSamples - 1;
        if (degreesOfFreedom > deltas.Length)
            throw new Exception("We only support up to " + deltas.Length + " degrees of freedom");

        // d = c * S/sqrt(numberOfSamples) where c ~ t-distribution(degreesOfFreedom) and S is the sample standard deviation.
        return deltas[degreesOfFreedom - 1] * sampleStandardDeviation(numberOfSamples, sum, squareSum) / Math.Sqrt(numberOfSamples);
    }

    public static double[] confidenceInterval (double[] values, double probability = 0.95) {
        var sum = Statistics.sum(values);
        var mean = sum / values.Length;
        var delta = confidenceIntervalDelta(probability, values.Length, sum, squareSum(values));
        return new double[] { mean - delta, mean + delta };
    }

    // Welch's t-test (http://en.wikipedia.org/wiki/Welch%27s_t_test)
    public static object testWelchsT (double[] values1, double[] values2, double probability) {
        return computeWelchsT(values1, 0, values1.Length, values2, 0, values2.Length, probability).significantlyDifferent;
    }

    public class ProbabilityRangeForWelchsT {
        public double t;
        public double degreesOfFreedom;
        public double?[] range;
    }

    public static ProbabilityRangeForWelchsT probabilityRangeForWelchsT (double[] values1, double[] values2) {
        var result = computeWelchsT(values1, 0, values1.Length, values2, 0, values2.Length);
        if (Double.IsNaN(result.t) || Double.IsNaN(result.degreesOfFreedom))
            return new ProbabilityRangeForWelchsT {t = Double.NaN, degreesOfFreedom = Double.NaN, range = new double?[] { null, null } };

        double? lowerBound = null;
        double? upperBound = null;
        foreach (var probability in tDistributionByOneSidedProbability.Keys) {
            var twoSidedProbability = oneSidedToTwoSidedProbability(probability);
            var idx = (int)Math.Round(result.degreesOfFreedom - 1);
            if (result.t > tDistributionByOneSidedProbability[probability][idx])
                lowerBound = twoSidedProbability;
            else if (lowerBound.HasValue) {
                upperBound = twoSidedProbability;
                break;
            }
        }
        return new ProbabilityRangeForWelchsT {t = result.t, degreesOfFreedom = result.degreesOfFreedom, range = new [] { lowerBound, upperBound } };
    }

    public class ComputedWelchsT {
        public double t;
        public double degreesOfFreedom;
        public bool significantlyDifferent;
    }

    public static ComputedWelchsT computeWelchsT (double[] values1, int startIndex1, int length1, double[] values2, int startIndex2, int length2, double probability = 0.8) {
        var stat1 = sampleMeanAndVarianceForValues(values1, startIndex1, length1);
        var stat2 = sampleMeanAndVarianceForValues(values2, startIndex2, length2);
        var sumOfSampleVarianceOverSampleSize = stat1.variance / stat1.size + stat2.variance / stat2.size;
        var t = Math.Abs((stat1.mean - stat2.mean) / Math.Sqrt(sumOfSampleVarianceOverSampleSize));

        // http://en.wikipedia.org/wiki/Welchâ€“Satterthwaite_equation
        var degreesOfFreedom = sumOfSampleVarianceOverSampleSize * sumOfSampleVarianceOverSampleSize
            / (stat1.variance * stat1.variance / stat1.size / stat1.size / stat1.degreesOfFreedom
                + stat2.variance * stat2.variance / stat2.size / stat2.size / stat2.degreesOfFreedom);
        var idx = (int)Math.Round(degreesOfFreedom - 1);
        var minT = tDistributionByOneSidedProbability[twoSidedToOneSidedProbability(probability)][idx];
        return new ComputedWelchsT {
            t = t,
            degreesOfFreedom = degreesOfFreedom,
            significantlyDifferent = t > minT,
        };
    }
    
    public class MeanAndVariance {
        public double mean, variance;
        public double size;
        public double degreesOfFreedom;
    }

    public static MeanAndVariance sampleMeanAndVarianceForValues (double[] values, int startIndex, int length) {
        double sum = 0;
        for (var i = 0; i < length; i++)
            sum += values[startIndex + i];
        double squareSum = 0;
        for (var i = 0; i < length; i++)
            squareSum += values[startIndex + i] * values[startIndex + i];
        var sampleMean = sum / length;
        // FIXME: Maybe we should be using the biased sample variance.
        var unbiasedSampleVariance = (squareSum - sum * sum / length) / (length - 1);
        return new MeanAndVariance {
            mean = sampleMean,
            variance = unbiasedSampleVariance,
            size = length,
            degreesOfFreedom = length - 1,
        };
    }

    public static double[] movingAverage (double[] values, int backwardWindowSize, int forwardWindowSize) {
        var averages = new double[values.Length];
        // We use naive O(n^2) algorithm for simplicy as well as to avoid accumulating round-off errors.
        for (var i = 0; i < values.Length; i++) {
            double sum = 0;
            double count = 0;
            for (var j = i - backwardWindowSize; j <= i + forwardWindowSize; j++) {
                if (j >= 0 && j < values.Length) {
                    sum += values[j];
                    count++;
                }
            }
            averages[i] = sum / count;
        }
        return averages;
    }

    public static double[] cumulativeMovingAverage (double[] values) {
        var averages = new double[values.Length];
        double sum = 0;
        for (var i = 0; i < values.Length; i++) {
            sum += values[i];
            averages[i] = sum / (double)(i + 1);
        }
        return averages;
    }

    public static object exponentialMovingAverage (double[] values, double smoothingFactor) {
        var averages = new double[values.Length];
        var movingAverage = values[0];
        averages[0] = movingAverage;
        for (var i = 1; i < values.Length; i++) {
            movingAverage = smoothingFactor * values[i] + (1 - smoothingFactor) * movingAverage;
            averages[i] = movingAverage;
        }
        return averages;
    }

    // The return value is the starting indices of each segment.
    public static int[] segmentTimeSeriesGreedyWithStudentsTTest (double[] values, int minLength) {
        if (values.Length < 2)
            return new int[] { 0 };
        var segments = new List<int>();
        recursivelySplitIntoTwoSegmentsAtMaxTIfSignificantlyDifferent(values, 0, values.Length, minLength, segments);
        segments.Add(values.Length);
        return segments.ToArray();
    }

    public static int[] segmentTimeSeriesByMaximizingSchwarzCriterion (double[] values, double segmentCountWeight, int gridLength = 500) {
        // Split the time series into grids since splitIntoSegmentsUntilGoodEnough is O(n^2).
        var totalSegmentation = new List<int> { 0 };
        for (int gridCount = 0, l = (int)Math.Ceiling(values.Length / (double)gridLength); gridCount < l; gridCount++) {

            var gridValues = values.slice(gridCount * gridLength, (gridCount + 1) * gridLength);
            var segmentation = splitIntoSegmentsUntilGoodEnough(gridValues, segmentCountWeight);

            if (Statistics.debuggingSegmentation)
                Console.WriteLine($"grid={gridCount}, {segmentation}");

            for (var i = 1; i < segmentation.Length - 1; i++)
                totalSegmentation.Add(gridCount * gridLength + segmentation[i]);
        }

        if (Statistics.debuggingSegmentation)
            Console.WriteLine($"Final segmentation {totalSegmentation}");

        totalSegmentation.Add(values.Length);

        return totalSegmentation.ToArray();
    }

    public static void recursivelySplitIntoTwoSegmentsAtMaxTIfSignificantlyDifferent (double[] values, int startIndex, int length, int minLength, List<int> segments) {
        double tMax = 0;
        int? argTMax = null;
        for (var i = 1; i < length - 1; i++) {
            var firstLength = i;
            var secondLength = length - i;
            if (firstLength < minLength || secondLength < minLength)
                continue;
            var result = Statistics.computeWelchsT(values, startIndex, firstLength, values, startIndex + i, secondLength, 0.9);
            if (result.significantlyDifferent && result.t > tMax) {
                tMax = result.t;
                argTMax = i;
            }
        }
        if (tMax <= 0) {
            segments.Add(startIndex);
            return;
        }
        recursivelySplitIntoTwoSegmentsAtMaxTIfSignificantlyDifferent(values, startIndex, argTMax.Value, minLength, segments);
        recursivelySplitIntoTwoSegmentsAtMaxTIfSignificantlyDifferent(values, startIndex + argTMax.Value, length - argTMax.Value, minLength, segments);
    }

    public static int[] splitIntoSegmentsUntilGoodEnough(double[] values, double BirgeAndMassartC = 2.5) {
        if (values.Length < 2)
            return new[] { 0, values.Length };

        var matrix = new SampleVarianceUpperTriangularMatrix(values);

        var SchwarzCriterionBeta = Compat.log1p(values.Length - 1) / values.Length;

        var BirgeAndMassartPenalization = (Func<int, double>)((segmentCount) =>
            segmentCount * (1 + BirgeAndMassartC * Compat.log1p(values.Length / (double)segmentCount - 1))
        );

        int[] segmentation = null;
        var minTotalCost = double.PositiveInfinity;
        var maxK = Math.Min(50, values.Length);

        for (var k = 1; k < maxK; k++) {
            // var start = Date.now();
            var result = findOptimalSegmentation(values, matrix, k);
            var cost = result.cost / (double)values.Length;
            var penalty = SchwarzCriterionBeta * BirgeAndMassartPenalization(k);
            if (cost + penalty < minTotalCost) {
                minTotalCost = cost + penalty;
                segmentation = result.segmentation;
            } else
                maxK = Math.Min(maxK, k + 3);
            /*
            if (Statistics.debuggingSegmentation)
                console.log('splitIntoSegmentsUntilGoodEnough', k, Date.now() - start, cost + penalty);
            */
        }

        return segmentation;
    }

    public static float[][] allocateCostUpperTriangularForSegmentation (double[] values, int segmentCount)
    {
        // Dynamic programming. cost[i][k] = The cost to segmenting values up to i into k segments.
        var cost = new float[values.Length][];
        for (var segmentEnd = 0; segmentEnd < values.Length; segmentEnd++)
            cost[segmentEnd] = new float[segmentCount + 1];
        return cost;
    }

    public static int[][] allocatePreviousNodeForSegmentation(double[] values, int segmentCount)
    {
        // previousNode[i][k] = The start of the last segment in an optimal segmentation that ends at i with k segments.
        var previousNode = new int[values.Length][];
        for (var i = 0; i < values.Length; i++)
            previousNode[i] = new int[segmentCount + 1];
        return previousNode;
    }

    public static bool IsInRange<T> (T[] arr, int index) {
        if (index < 0)
            return false;
        if (index >= arr.Length)
            return false;
        return true;
    }

    public static void findOptimalSegmentationInternal(float[][] cost, int[][] previousNode, double[] values, SampleVarianceUpperTriangularMatrix costMatrix, int segmentCount)
    {
        // The cost of segmenting single value is always 0.
        for (int i = 0; i < cost[0].Length; i++)
            cost[0][i] = 0;
        for (int i = 0; i < previousNode[0].Length; i++)
            previousNode[0][i] = -1;

        for (var segmentStart = 0; segmentStart < values.Length; segmentStart++) {
            var costOfOptimalSegmentationThatEndAtCurrentStart = cost[segmentStart];
            for (var k = 0; k < segmentCount; k++) {
                var noSegmentationOfLenghtKEndsAtCurrentStart = !IsInRange(previousNode[segmentStart], k);
                if (noSegmentationOfLenghtKEndsAtCurrentStart)
                    continue;
                for (var segmentEnd = segmentStart + 1; segmentEnd < values.Length; segmentEnd++) {
                    var costOfOptimalSegmentationOfLengthK = costOfOptimalSegmentationThatEndAtCurrentStart[k];
                    var costOfCurrentSegment = costMatrix.costBetween(segmentStart, segmentEnd);
                    var totalCost = costOfOptimalSegmentationOfLengthK + costOfCurrentSegment;
                    if (!IsInRange(previousNode[segmentEnd], k + 1) || totalCost < cost[segmentEnd][k + 1]) {
                        cost[segmentEnd][k + 1] = (float)totalCost;
                        previousNode[segmentEnd][k + 1] = segmentStart;
                    }
                }
            }
        }
    }

    public class OptimalSegmentation {
        public int[] segmentation;
        public float cost;
    }

    public static OptimalSegmentation findOptimalSegmentation (double[] values, SampleVarianceUpperTriangularMatrix costMatrix, int segmentCount) {
        var cost = allocateCostUpperTriangularForSegmentation(values, segmentCount);
        var previousNode = allocatePreviousNodeForSegmentation(values, segmentCount);

        findOptimalSegmentationInternal(cost, previousNode, values, costMatrix, segmentCount);

        if (Statistics.debuggingSegmentation) {
        /*
            console.log('findOptimalSegmentation with', segmentCount, 'segments');
            for (var end = 0; end < values.length; end++) {
                for (var k = 0; k <= segmentCount; k++) {
                    var start = previousNode[end][k];
                    if (start === undefined)
                        continue;
                    console.log(`C(segment=[${start}, ${end + 1}], segmentCount=${k})=${cost[end][k]}`);
                }
            }
        */
        }

        var segmentEnd = values.Length - 1;
        var segmentation = new int[segmentCount + 1];
        segmentation[segmentCount] = values.Length;

        for (var k = segmentCount; k > 0; k--) {
            if (segmentEnd < 0)
                break;

            segmentEnd = previousNode[segmentEnd][k];
            segmentation[k - 1] = segmentEnd;
        }

        var costOfOptimalSegmentation = cost[values.Length - 1][segmentCount];

        if (Statistics.debuggingSegmentation)
            Console.WriteLine($"Optimal segmentation: {segmentation} with cost = {costOfOptimalSegmentation}");

        return new OptimalSegmentation {segmentation = segmentation, cost = costOfOptimalSegmentation};
    }
}

public class SampleVarianceUpperTriangularMatrix {
    public float[][] costMatrix;

    public SampleVarianceUpperTriangularMatrix (double[] values) {
        // The cost of segment (i, j].
        costMatrix = new float[values.Length - 1][];
        for (var i = 0; i < values.Length - 1; i++) {
            var remainingValueCount = values.Length - i - 1;
            costMatrix[i] = new float[remainingValueCount];
            var sum = values[i];
            var squareSum = sum * sum;
            costMatrix[i][0] = 0;
            for (var j = i + 1; j < values.Length; j++) {
                var currentValue = values[j];
                sum += currentValue;
                squareSum += currentValue * currentValue;
                var sampleSize = j - i + 1;
                var stdev = Statistics.sampleStandardDeviation(sampleSize, sum, squareSum);
                costMatrix[i][j - i - 1] = (float)(stdev > 0 ? sampleSize * Math.Log(stdev * stdev) : 0);
            }
        }
    }

    public double costBetween (int from, int to) {
        if (from >= this.costMatrix.Length || from == to)
            return 0; // The cost of the segment that starts at the last data point is 0.
        return this.costMatrix[from][to - from - 1];
    }
}

public partial class Benchmark {
    public const int WarmingIterationCount = 2;
    public const int IterationCount = 7;
    public const int InnerIterationCount = 1;

    public double[] d1, d2;

    public Benchmark () {
        d1 = new double[] { 103.63428467218,100.65698512757,103.41681901544,101.73297541769,106.1637181408,98.412208909224,102.1910683922,100.6529954993,100.2060895554,102.79763572285,101.58124402819,104.83646850886,105.68205557779,101.37268411726,101.21856897174,103.29471813954,105.99085083645,104.00152807402,102.5515579466,100.95765471931,101.30414879687,101.28842115694,103.18365782867,104.32770094781,101.04841668133,103.48198436419,100.89825149314,101.96224301484,102.57514127219,101.88941623092,102.55684506087,103.32436792906,102.14059443954,101.34202300433,100.71509510859,102.84019181314,103.90795641251,103.79317123977,102.33281400385,101.05661999771,99.527245552223,104.04756755696,100.08258193089,102.32602929128,101.12649175316,98.59777764562,100.51219905198,100.3384460803,101.15283289168,104.45829696358,103.08640647121,101.15646661427,104.87981301026,102.96022354121,102.91057904096,102.94557493889,104.19286657388,101.49564839627,104.56628696589,102.64328394303,102.98955045659,100.30795615252,101.9596010783,102.7787577747,105.08450703012,105.6083617604,100.58871732306,102.29447246575,103.93156436242,100.15444968522,100.33027723674,99.709998115277,104.77614797428,101.80467295351,103.83722962812,103.8922179754,99.668513176783,103.54088096393,99.79641717842,99.217827453945,100.63604774585,99.397345569613,103.41566155646,102.86730291818,101.3466271856,101.0430581605,101.75352329686,101.79205972782,103.58235064873,102.70955387997,103.513441865,103.55358827182,101.27954344729,100.72000812989,100.50572692724,103.29725364285,99.878238665041,101.33136147166,107.82148653962,106.33087978795,105.76698605645,106.73722912118,105.09474645086,106.93812007831,104.58796212158,108.74454496591,108.77609339631,106.36234738479,107.08594417641,103.14990449109,103.08799682189,106.42944302492,104.95681265496,104.19185455295,103.37266620831,106.88393619713,103.77307460747,107.22959460967,102.92373151821,102.27169909087,105.35352579142,108.55485303073,106.86928831633,107.6604165255,106.08100829687,105.69860408885,104.15919883914,103.43072470928,107.61607415001,105.12730526857,108.26003924405,102.52392958652,103.49742913465,106.40355299133,106.6205839791,103.75599735706,104.02717048675,102.8616712232,107.31403440793,105.71715870348,108.62026687466,103.89002871878,101.02136648518,105.71922937008,107.65246680959,106.90056953891,103.116341753,107.4819154547,104.30217606275,106.2163429931,108.76888355744,106.25769430792,105.62856894786,106.5974802856,107.4572456313,103.76930483468,109.22117086979,103.95101290047,103.04369239044,105.8978020889,108.08880932927,107.09476068374,107.84718462015,104.3185702112,108.58358811573,103.08208736102,106.30281582081,105.5071989199,106.69382657222,105.5661979062,107.55317794727,106.81112895681,103.49181806572,108.11406008626,105.82558000937,110.7280951536,110.57391099468,96.195798803035,98.050055515401,98.34258752078,96.437461839433,107.86695915352,107.50220866371,96.408603678811,96.183884592145,93.874527361487,106.01388920009,106.68752585206,95.748639659824,97.620195660744,95.709384830238,94.844312788627,97.194219302695,95.228621910585,95.299983299732,95.878520884831,95.494218128339,95.721877181905,95.30324004452,93.761957719231,99.052683337698,97.414624016474,98.955647001503,96.944084189531,94.98363688231,97.084748591591,96.56489094851,96.602599613176,99.011103877236,108.56332396293,96.328721476217,112.59520822831,111.4457635202,109.57249840458,112.52658585356,111.79387339089,107.83633335508,110.46783549074,113.07325087266,114.03320142819,108.91999302834,113.28679171319,112.84647681497,112.30446173472,107.70596309315,113.90782983196,110.52702067922,110.77146399218,108.82316306309,112.31704998895,112.04572347677,102.01583120296,102.26933512167,101.04034529285,102.82388967602,103.5215835497,103.60628281381,100.24147995637,102.56920667296,111.65264599649,109.90970916739,110.91610377373,110.86460228468,110.66591901693,110.08877264895,110.07273691588,108.50573934243,111.74386044451,109.613011316,111.00131231439,112.36541199185,107.7696667543,108.22526773007,110.72718495033,110.60353399069,110.27959438874,109.58777079374,111.98279815171,110.36414556569,110.70297309292,111.30023230049,111.53753771801,111.98382518632,105.84905409588,111.46509413983,108.80378506166,108.87305949198,106.71554016504,108.90363691526,110.56509895199,110.36621011006,109.26421997574,110.84819795531,108.01463000771,110.16319399901,110.07964827933,109.98910490708,108.31446832729,108.38910105911,112.30023252646,111.50251919968,109.58690843228,106.49996527433,109.79602509693,96.52144390049,108.95662597735,111.8968272024,110.28041012397,111.55946971612,111.13212900067,109.06043835093,111.0511769347,108.32635458606,107.65798353661,107.34909902585,110.25041961026,109.1718567223,108.03672114421,110.24074486601,107.97075096529,111.3628301031,108.6301339157,103.7289403799,106.60945920642,106.53605315427,103.20148515981,105.11749841121,104.2666389565,103.42858705344,106.1975050171,104.72818386307,106.37807528636,105.02872802439,105.85690151022,105.89551276982,104.00980895851,102.15931948242,101.93822430512,103.88026933265,104.02911726802,105.45277026572,107.28621619873,104.88538888472,106.46227513472,102.03669070644,100.28974689639,101.00164916533,100.40024603645,103.46761953127,103.19122472699,102.70368522158,99.207331708706,103.32735779387,99.393345858695,101.67323904678,101.83843448048,103.8096755809,100.07754554042,102.90036917446,101.36972227148,101.1028917943,103.98028142042,99.343445851924,102.68303364677,100.13864808551,102.06324764043,107.79036765716,109.30827245843,107.77681259301,109.2272039817,109.94103153686,110.11045439969,109.26858382323,109.86474746433,108.03563439025,107.44965212968,108.68801251856,107.555863417,108.50665543054,108.59584149294,110.05310410531,107.7822189141,101.73452326798,103.67155882495,103.7884560479,103.10314424014,105.75036872995,105.14408070176,103.55531858799,104.81391175968,104.48725990567,102.15770239253,103.78475517921,105.20376204681,105.99604990865,103.34101862299,103.84304837547,104.09055338979,105.19294212307,104.6868086816,104.37872417938,105.21675245573,105.84290954812,102.2381163623,103.88699033483,106.72744040302,103.60808643711,102.26066200938,104.86679255385,106.95671273406,102.89587242376,106.32794069648,102.17207761491,104.1190378646,105.42365608734,104.18343900089,104.45647493017,101.85144235447,104.78625046169,103.63679991852,102.93311258241,102.08262070132,104.45507807304,104.26569754298,103.6855209839,104.3855058604,102.84694267055,103.80277119698,105.13043932289,107.19340286282,107.27418098772,105.44646036129,106.58535072266,106.70674247716,109.74653826265,107.55263860475,107.64382572874,108.88617006897,110.16057605603,107.23522789707,106.96858598479,107.2924364301,106.95505703,108.64548154613,106.38627916877,107.9697059537,109.11078836259,105.7383515187,111.62849949276,108.18834326491,106.61907697166,102.57936780489,109.12848475298,106.97407076747,106.75001101721,110.01297811186,106.88701879128,105.23816862112,103.30932848825,104.23496997415,107.27442211619,107.67674217628,107.2484938097,104.90362090647,103.5824676923,103.73311304665,105.24653305056,103.07288539963,104.53340912047,101.40634915111,108.91468223586,106.88660019638,106.43223907891,103.68024464352,103.83193873675,104.07848468419,105.74990713462,105.60409525502,107.63448837756,102.32464833149,106.68111586381,107.49684134993,107.56105061565,109.80212039499,105.86575000176,107.0999687276,106.09127016986,108.61823355593,110.96870454617,108.05128102642,103.82303578025,106.47370904183,106.05172709383,108.54015076925,104.84000717884,104.42027007867,111.58253292689,107.06443662014,108.32915993876,106.48326461583,109.52722766039,110.23657862797,90.014683825532,107.14354437057,105.02011555235,107.29011147082,107.32564928096,110.03050165289,109.68038400797,106.13627357048,106.91836913395,107.6568462389,109.49552099466,103.48146288275,106.60690157731,106.25082059252,106.12633439608,103.71583791631,106.63162721942,109.4167257436,108.35830711959,109.3780233265,106.16836695023,109.48330209042,110.00318676132,107.75226581519,108.18079748954,104.86522515621,109.07002653939,103.48242565378,108.36991859734,104.91387974451,110.0534234197,104.17566840849,103.8526228076,107.11070726591,104.37587635149,107.59707975791,107.82315072957,107.42340335804,104.39129527838,106.76407211048,109.48436385904,107.8069751983,106.36636568612,107.90231428382,106.41636661618,104.50956259774,105.73976923357,105.95973792515,105.49909631409,107.15010520625,104.2993219361,106.5113152442,109.03852855002,104.43222213763,103.35262836563,110.37545514446,106.61296509329,105.20601648301,110.07003582386,107.35723191948,107.06565382911,106.32649479355,106.91774203594,105.98846619346,108.38531769785,106.67228979729,106.38319568219,109.41682548398,106.85330652959,110.04844977099,103.29189986193,107.45191936645,108.56299916427,107.19467686356,108.74559787133,109.49750524068,108.85376100144,107.61867662715,107.61972209229,85.17761029361,110.29793613688,108.22229412473,107.72637808547,108.08083925849,109.05162171957,105.00910489327,107.89747600605,105.50501540347,110.56673374564,107.41704851625,109.05720695352,77.315568649673,108.30931235606,110.0336422004,108.40634742598,105.44148311453,106.99870231831,107.48865028281,111.85322653316,111.52490937153,106.87608186925,109.84499551232,106.82192167306,109.10039919905,108.08205114381,105.52406656028,108.53781900617,109.2455681953,108.23476050139,107.9895744853,107.06132113391,107.68123077942,110.15854065285,112.38931264552,113.23557172115,107.50033697873,111.97739284935,110.33332447678,105.39111786426,109.77143960139,113.2434126604,112.01142158172,108.31742391815,106.00536616667,111.63044502651,109.54666226437,105.84620920981,105.70694021217,107.71520837049,106.68160150684,109.36668848004,107.24008728807,111.89618422489,109.40417244003,111.10352933926,112.64062602992,112.73043977289,110.19908857706,112.22605781462,111.83700690785,110.35959751239,112.56822376648,112.87788867633,111.32916452602,111.40728752384,111.06984985934,109.72978220736,107.21281726739,112.62995835147,112.9385404111,113.19063693355,111.84059909991,109.71591686573,108.43332424382,109.42020557766,109.87267407998,109.54346420431,111.23632867679,112.26816465835,109.72777418456,106.25970618647,112.04978554754,110.99671047766,111.04736666386,107.45836716078,106.01654154393,108.72985158659,111.57863844289,112.0445911048,113.52742690426,111.22059046007,110.99322652133,103.63257611613,106.73724781462,107.20253868534,112.49503189876,110.56459100093,109.20806540627,106.79539124497,109.80188075066,108.09224928247,109.40954215642,107.04867245532,109.36017867242,110.73653903336,112.39002225749,110.93090695318,112.25338463549,113.39947117,99.061657491608,106.25790414308,111.68964098739,108.95819542835,102.47230535848,111.11430763374,110.26313646026,111.66912804187,109.0056855922,109.27074717988,98.082073692922,111.21194970364,92.409183908285,90.461167671458,104.97721912056,109.43956807554,110.00122332629,105.75300045793,110.36904844275,110.88275280079,111.04003619593,91.153772689511,110.02955344012,111.00445566092,110.22797552692,111.17965957125,111.86228287928,111.23421477583,95.873910814471,112.85549574075,107.56832410575,111.11896911009,111.32831650932,107.30738335168,108.8489251968,105.69883502285,110.51826129389,109.06182400421,110.91596220702,111.85581421214,109.78604681965,106.7634610085,106.39899836428,108.30453418432,109.65477861276,113.23285106452,110.66096912651,107.73598985188,111.83391938366,108.71476891278,109.72631800395,107.2314985823,107.9047115596,109.96986354999,112.98505264204,110.33843717346,107.09818482745,110.8651700391,104.2737023416,105.4728665708,105.26681653505,108.66534797759,110.2839254469,104.61185083249,109.06219977142,109.48636752855,106.08107954225,110.37799322855,108.13768411384,108.46221041386,109.60961937072,107.88660414507,109.87969831957,109.12291492714,107.57177951589,105.53011738952,107.33273710961,109.70462206046,108.45985311646,107.42515466843,106.80586135719,106.44520171358,108.34432716178,106.01701063197,107.04800412043,107.87779414716,106.84998206109,107.75495063307,96.747042529874,105.1876740621,108.34224705864,110.53068331661,108.18325933515,108.41623767685,106.65664623488,109.14806217729,109.97161070955,108.81424985177,109.3147054893,108.27862038208,107.74308377277,105.5366380238,105.57486998292,108.66183566067,107.50219146774,105.51858325055,110.36666459736,111.47696492321,113.46032665134,111.18187524714,107.41491294366,109.65745578015,108.30597589177,111.43638530276,109.34158020617,110.95964997937,107.83265565107,112.53580176849,107.77581205949,106.96303617742,108.32772448743,111.7717853465,107.69162999283,110.99464386402,108.18119321669,108.88352274518,112.14673528738,109.93102022417,112.70729123782,109.78879894942,108.93401518588,111.98945044216,112.15177350147,111.77799882109,111.49913129167,112.60056185345,112.25302335737,107.97435578534,108.42810031119,107.64057351216,111.31594448527,107.75341912369,110.0282682642,107.97140844925,111.68357238164,110.76978232131,110.72298349198,105.48215915973,106.08038588173,106.39694404007,106.79337143182,107.14540203348,112.0181203398,107.40790609881,108.33144965193,111.04308136626,113.68002585271,109.57302621725,107.9682450493,111.01847046231,106.46119260412,112.96045099607,105.29248687762,105.64510556623,111.47510086983,108.96218833794,107.72023010095,107.44950930954,107.7163815622,107.86365770172,107.35622025917,111.48394754937,112.44853683986,109.53599527084,111.1409286807,109.84197504781,107.51547573199,112.2236232605,112.12545241065,108.77123601952,113.37436582625,108.64912836469,110.66846008941,110.13401998077,81.186485762187,109.7677337288,111.75131445468,111.00651492367,110.15452067027,108.50020946669,107.75348508089,111.45415423027,109.93488640184,112.86547952596,110.15568378902,108.02623390069,107.62714771158,110.35041406329,112.68614636376,112.24542416039,110.68444324482,110.59173757899,110.17624041183,110.02351999437,111.75986160926,111.88073802617,108.92835464321,108.87600510769,105.6227306821,108.45433650085,107.90648507855,106.11952920665,105.84777246738,104.99815994364,109.43597229943,109.3702449033,107.6873979055,109.5627713427,105.71144005933,109.06516897029,103.3461411033,108.88996776286,103.55153317468,105.58236788034,106.12559265287,109.8659751351,104.51690337389,105.51311973161,105.95348152869,106.51213195793,103.95900713034,109.62759760679,103.92085483513,93.087990833072,108.4310309405,107.68211506345,108.40323240193,108.81202709113,106.55794582044,110.42380645928,107.85532106461,103.91837938851,107.13891832232,107.53493452297,108.85713973794,108.58817212829,108.89583854107,108.36053032157,109.92931065516,103.57248138677,104.92503217271,105.08614826133,104.81166553617,108.41872842598,103.58963461623,107.33221007604,109.2609310275,104.97433692514,108.09780890109,105.42657217886,107.1702895711,108.19030596427,110.14001460549,111.57997542153,108.85365575584,110.43451056324,111.40086619208,108.02995224437,107.93091720474,106.90657583468,110.33731900155,108.37659349966,105.30719987693,108.06021523459,110.20193853488,112.25372136061,110.92310110747,109.95878198708,106.40603206646,109.61721858763,105.49346401078,110.61904290334,109.57439320071,109.81123627609,111.42313451353,109.55452409815,102.56619219795,104.40798521703,107.55175233276,102.54986718707,108.52893687019,108.03168227575,106.00487060612,103.08763546042,92.133268310071,96.206461497743,90.343760068419,93.808018940555,93.143782124236,106.46089855673,102.75757607809,104.38797714248,107.12700010671,106.10114962329,105.55940425579,106.35811250209,105.67926004431,109.41196385706,111.70687291211,105.49026123419,107.26057740797,107.3955757939,106.30280636705,108.12989902751,111.40218450866,109.69997311418,112.15107488238,111.18465479747,109.99054075779,106.0583169911,110.41372812531,109.20568249923,107.36813169905,104.969661442,110.30109628475,109.52684190507,108.72696694389,104.58019300786,110.05432795271,108.69624262707,111.60084529539,112.68851822235,110.37021964975,114.11942143994,114.06918665321,112.64728029936,110.47938828949,111.94928912695,111.34719251162,111.10950371894,111.1514333078,113.08848167581,108.99216592349,110.58232880582,112.19818119231,113.04467723767,113.82790351655,111.41873341223,112.24410906174,112.88422273886,110.3537021332,113.62486632809,107.24785147088,113.19573657938,113.49562413654,111.32888469865,112.95699180049,112.8329354754,112.18664779404,108.98174886507,112.35190564764,113.62623188756,109.51322986116,111.45127838239,108.80055421984,110.48348252107,106.17529995134,106.49985045354,110.60638047947,111.45128931112,110.56951357698,109.52474606049,107.6196905486,105.34089525811,109.49093521012,111.15969039594,109.05283918177,107.33819051142,110.0612130803,112.39234557658,110.82366036123,111.01981928062,103.71088072758,112.23301781237,107.02891807985,110.61949622561,107.07544584969,106.42051493526,112.03283332254,110.48070765143,105.75707012058,108.57074227491,109.86320190347,109.86356553839,108.38925949959,111.45423458246,109.25516568625,110.6722378552,105.40635533276,107.91260804116,110.79241123174,104.19251136514,108.06228051746,107.26244406399,107.58359998101,108.87243548409,108.40108255545,108.01649068012,107.18344788594,107.57362479483,109.78564773897,105.76470428063,107.97147508447,109.41803104993,108.27104708998,109.619188468,110.84779080731,111.40146215398,106.88871444722,107.43815401951,111.78078543675,107.78307412377,109.81806504901,109.35851513139,110.89116280541,110.2417933917,109.08922739076,109.68980631662,108.18107358211,109.90367940634,108.6350185989,109.52512643441,107.92571165575,109.42691133011,104.96874620215,108.83913782743,109.74195757795,111.01085102653,109.37670192463,106.48756978469,110.3495571201,111.26714492358,109.19082916149,109.84399230394,111.58731761256,110.71284742043,111.05889626294,108.94571861896,104.8152895356,111.40326862974,112.01662459935,109.07303256728,106.87306374904,110.03198683517,108.38160515193,107.75605219967,110.92259531965,105.67082378676,107.77599653253,109.75198219525,107.7451214971,105.64183358203,108.89546822703,110.4236840885,107.66609010246,107.43562789749,108.17456854237,108.62200581631,110.18143157275,110.6467863198,109.48126788198,107.87329545705,108.76276587244,106.62668067894,108.68393528787,111.46109709084,111.69609810403,112.37151722319,107.70744339783,108.33502895225,106.62192327906,112.0010920451,109.10948951212,112.15287620736,110.2265726419,105.81595064833,104.38432724115,108.2052308317,108.55712963571,109.43884433168,110.90857272959,112.03565684758,106.59024401233,109.73141024469,109.1158074444,108.14327377937,110.85402656584,108.44822076968,104.68260604154,112.00746065149,107.83521935053,106.35829252044,108.05475517331,108.73994603231,109.39205966723,107.43459134851,108.8348946125,108.42233629073,110.47683029544,110.51843844547,109.82487219939,108.02163494301,110.19038770986,109.46020515665,110.06540320241,105.57291025657,107.68897083653,106.62774480078,108.69058460205,105.84096577757,108.47343208928,110.11012561936,106.00786074024,108.01380752673,109.89512310427,110.44809622327,111.37279325279,111.98583326712,106.24168268537,106.55801506845,111.24841599951,109.90744551639,109.60913984141,107.97330640113,108.3912985381,109.35544804725,109.4518833776,106.80396636304,107.05214809014,110.26680656173,111.89802365735,106.4089495026,112.99568421136,111.33714517529,109.97033888985,112.19795386904,108.90778537781,110.42039126485,110.7366600481,110.35638808374,109.91623595599,108.57112352817,108.11354312774,111.21631263164,109.91159238035,105.84451955539,108.83624896636,112.49904863199,108.04943052018,108.29813856168,107.38470949244,112.28579170164,112.48227594429,110.96308285977,110.24808942231,111.43378019203,113.39326830002,110.98231113819,111.87511274427,112.40909908498,112.83643130536,110.60721464,111.50357632947,112.62469446674,110.93282919719,110.7447645788,114.03967326812,110.47228356165,109.6153990452,112.98274406699,107.7710096802,108.13730668969,112.13412883796,109.18219542752,111.75515918478,114.38485281237,107.68649508842,111.16682293293,112.00769297165,108.03580496517,111.96320969648,111.15111403141,110.74621353177,110.21844085513,108.61462032219,108.11011317309,109.96092533593,110.41407854603,108.14305650474,110.29293207203,86.149839250651,82.289319039634,80.973256986218,84.33951818243,84.48022993106,85.274146844815,82.464907241928,112.7704596248,107.93529765182,112.91995288917,113.25727608086,113.34866099886,110.09889926053,111.46096351928,111.88183012148,110.55223927678,111.68193450493,106.73314292778,107.73567077649,109.79995476469,113.92132409085,110.55203845357,109.88296897766,112.98470288323,107.00629242998,108.85370700354,107.668068211,112.45047738563,108.51628815802,112.1897715412,110.11110398085,111.97358102316,110.42368325536,112.38977437929,114.88359915408,112.75322305381,108.37487751741,113.71198604399,113.17078895235,111.76075249565,110.70510337341,112.95099761527,111.22329208852,114.265016515,112.77452825519,114.06998712058,112.63851794266,110.54832466505,112.2757976681,110.99850520982,114.74849155983,115.58548176696,116.11031818714,110.59575223031,114.38285982598,109.81590946878,110.9795014303,110.87441493815,110.02108200163,115.54007132331,113.94309016168,111.64641041811,114.28535823566,109.91891661496,110.67660119767,114.35221423371,114.65135329217,111.25422759727,114.28907322058,115.71272290337,113.19769192618,114.99996049434,111.27037827997,112.17782966539,108.59951788634,108.63441162051,107.94283060248,113.40212408476,110.28481560846,111.18149954448,111.21641686476,112.08541032959,111.36180997889,111.49975864629,111.62255055762,111.43549328324,114.46518590503,112.6425106123,109.95342632266,110.8696424953,111.4356408272,111.49513302133,111.86722565049,109.74796537758,112.39127162574,115.53965580133,111.81345103863,115.00281817176,114.4639325004,109.08291567204,112.5580063067,112.73090535167,113.78874810199,115.58789890842,109.47747414533,113.08607507851,113.8755624009,115.31251828518,115.31886387024,110.72617842591,115.15893279074,112.05014483738,109.47450054681,113.99823035202,112.98707260156,107.62892976791,111.09134695151,110.12614046839,110.32666983285,109.26394858831,112.55341065538,112.59151114874,110.58423936012,113.0791138635,110.98977447822,111.25592645634,109.74414900034,109.87995559368,112.23654341835,113.15622389857,115.8704769513,112.23319457358,111.86082153969,113.05662129874,112.02687965925,108.07272966868,110.70197319627,112.81157862542,111.44024560732,110.20478849659,110.05544805395,112.23209504268,112.53870271683,113.05940464365,111.22918023094,111.94871784063,111.70233718041,112.51788752256,111.84294668116,111.23807225753,108.72503469457,113.98648854422,109.03712019799,109.4863652603,110.70429096085,113.17204543933,113.53587259659,112.73476836285,110.46487044219,112.21568703583,113.63210713502,110.41287353221,112.12466135631,112.54007356218,114.13938757201,110.35933475707,112.54224632441,108.99420032387,112.19553969915,113.5190200971,110.80450740679,114.51219391549,113.09216627592,112.47189166951,113.73676818209,113.25650645965,109.5275742486,112.64650804566,111.43301560641,111.31677007466,113.57692568818,111.55570490648,112.11723071845,112.32649909838,109.75008356489,112.58451745694,109.76972873278,111.45845929876,110.58019569734,112.92517631691,113.18342715533,111.0057055914,109.46259040535,114.83187640376,111.45552722324,111.49977582707,111.19523199583,111.20135254293,114.51715292657,114.32079523893,111.88482588001,110.06938107569,111.0417893555,112.20617913967,109.53055296139,112.36921875231,112.74976508723,107.60619943221,113.38821064129,111.26382178641,112.82647243669,111.62201329943,111.40026320359,113.35999707227,114.8224814509,108.2498884806,111.93692378932,108.8428096806,109.81910997293,108.47539420246,110.50901192968,110.42072354504,110.53720502527,111.21911908405,109.66140605796,110.40880077384,108.86909353856,110.01796347361,109.31562945568,109.41386368765,109.24014020146,104.56672221016,106.6989585203,111.79672471805,106.55034913116,112.55593767355,109.29165270061,109.09882615351,108.13954334625,111.7689563185,109.90651538771,110.35861421726,108.42370917861,110.76277505074,108.816255053,113.22146777841,112.18937067425,111.06350066348,113.28840932907,113.75562863147,109.89280909677,109.63156793416,113.25668835993,113.69234884739,111.80977743155,112.5890552585,110.1229080109,112.06680011274,108.02351486939,109.96763171847,111.7517588341,112.18275633104,108.11430395762,109.34220090154,113.16296882816,112.89627032507,113.31147401954,108.25880073394,109.43162835333,112.93416715565,110.21967787664,111.37394069865,109.5340221703,112.51423807815,112.46704258111,109.87637893629,108.33951363088,109.54638563548,108.48195622185,112.33553881493,110.98917028711,112.42403201316,113.20348470572,107.90137055584,108.43568778778,108.9991038625,109.01950284407,112.56845784554,113.74598380172,107.68239201025,107.15547408321,109.93533014499,108.79797637976,110.42861591953,112.5951675597,109.5087727398,113.07334007009,108.13642712615,110.528411617,112.74340374183,112.76449042148,112.78415258937,112.35267189302,110.49948987181,114.06440940705,113.59536673001,113.33952165435,113.1239321398,114.40128781144,111.88549609237,113.90338928634,113.2398336429,114.07839156903,113.94535663271,115.55145138009,115.4042343221,109.95646479133,112.21653314198,112.0606194813,113.82889170024,111.45577392735,111.20271879858,111.69764160034,111.93914134264,110.37739608251,113.66218088013,115.76931535304,108.99397809589,114.88177147639,112.55251055487,113.98448612093,113.06367370983,110.98759853296,111.00552163091,111.56602592373,115.24447711365,112.06111540298,113.15082844619,113.82091418927,109.19905345431,111.58553034097,112.42762212124,113.68850724962,109.55734398113,109.87399573949,114.98482129158,111.02205188508,112.27625684481,111.11050731447,110.84725166823,109.01258991493,113.92330986156,111.79262924378,113.44726423037,113.23248913392,115.64580923961,108.78335919627,115.39591439078,111.45369555835,108.17198400607,107.95964108421,108.53667554069,113.58740812268,104.57372621474,114.23058394974,108.757970608,112.71639895119,112.75584302446,114.74463989474,112.52800821881,111.7910479708,114.30445595157,108.51823010854,113.90344673717,110.06830612461,111.90008525786,108.35317106484,110.73621517555,112.30750672376,113.80109525697,110.17884923432,113.7298773507,110.26387571611,109.03582306209,109.81593731941,107.23403213694,106.29971011027,108.99315621328,106.24656565084,109.2174021113,103.08915456637,105.58958596527,106.1064424383,102.93504684401,108.7580618306,106.1333801108,105.92511279837,108.47032401161,113.74163887428,112.12665738541,112.78167160612,115.04907113069,115.02072820752,112.47515819751,110.6050964346,113.79675652764,113.34073739077,107.96093328817,113.52315271497,112.47714022771,109.90230097976,112.39391209774,111.24151971234,114.24153223977,113.4084735048,113.44632735741,106.05577550875,106.56800260803,110.87457361471,108.99036058609,110.31522683566,113.08193029573,114.27122141981,113.49510491802,110.75525456592,111.81234704314,112.63694689566,114.17466324463,110.07720848654,110.45347824657,113.31026153403,111.20745367155,110.57728480719,114.78133103925,111.74423357596,112.41848944357,114.34665937094,113.80754843564,113.60431837214,112.76095874626,113.35350951344,111.87765556917,113.59334622339,111.11755882476,114.12477665581,115.3266022893,109.78185517951,112.42296850377,110.33627913791,114.23347099117,112.33543827899,114.24245233275,111.18030370647,110.50274320387,111.48681865357,112.28174723024,109.96495297622,112.87340825651,111.63247278186,111.69847509775,112.72522637646,112.48991512948,113.44517244554,111.74457451343,111.88108978315,109.25822541157,109.35775533549,112.78311316897,113.59145570968,110.43883631663,112.95455842816,112.92972629198,112.52087021598,114.31381230469,114.43679371654,110.22974675319,110.61076291234,112.24907909914,114.13580666266,114.11283108142,107.71934306146,110.28117819606,109.63528030452,114.06666805269,113.85924921756,110.16178363272,111.21790934714,111.84970238566,113.10894776667,112.19217023909,107.89856499175,110.35681012713,109.5812724093,107.13970923135,111.48808277866,112.91608591114,106.4435684102,110.63317187478,108.97238915744,112.8200151534,113.35465547314,113.17065934237,107.91082708992,110.21955631005,111.12177190853,109.37278035106,110.47342835537,111.53802703771,112.38654200884,110.92375141748,109.61673114165,111.83549449337,111.76601663927,109.41431854925,105.83617108279,110.075960338,112.43756053763,111.07345566328,111.86720763186,107.78744049848,108.91589616909,110.91931190364,109.42346770303,112.01485773495,110.7271404311,110.71289194103,110.36265561215,114.10343969403,111.52053542372,112.29769363226,108.81800877682,111.52739705879,108.57179255732,108.4847838352,112.88521761504,112.36059934753,111.5421047717,109.73280777843,105.4643266568,112.13184057831,111.36671911677,110.55619765107,110.26747256531,111.36046200034,111.31532577455,107.70758609262,112.70839872049,110.07122755337,112.31446070407,110.20499011217,107.83911973443,106.6767845999,107.68822771522,106.58326780192,103.49543360781,106.38739686216,104.14831299331,108.72002510242,108.2092851845,106.90499626368,105.3793731537,107.85407428567,105.25615116908,107.76907388338,104.21013267069,105.32628950857,109.21716960366,103.20054209284,102.67579456119,105.65567478719,108.741840279,108.28251883704,109.42209944956,109.24062582169,108.11077442876,109.12597458704,109.37239919595,108.42617416847,105.42228978133,105.34497796319,102.75729759833,107.71336791774,105.33987539249,105.05619886674,105.30635894644,104.82438612865,108.19943429655,103.90385831358,109.30612729354,106.13298886951,103.19571583971,111.51284217089,111.50743443098,109.82740191685,111.07468175083,109.83586241128,106.96044538412,109.74248084378,109.74755804881,108.8399951094,110.27689587697,110.20772628168,109.73055832978,110.07632610931,112.03589027273,112.32890853461,113.39535782407,107.41699097908,107.44922160224,111.70902311549,108.04222378491,105.90392404836,110.98846719962,108.73392696497,111.98755216557,107.2321833421,110.21023551916,109.69309687317,108.75953050502,110.65034173691,111.88992791435,107.32613987742,112.04028359033,110.44871708468,108.61734301131,110.60496075273,107.07754371161,110.36184048117,109.21087744475,106.31469179991,112.2584461597,110.03595758449,112.58989197753,111.47382361748,110.59212219463,110.96316721827,109.21558557166,107.56835197203,113.61708821701,108.85426907559,108.70208019114,112.9180639701,110.32591402238,111.70762896752,111.52310840093,111.67263913649,110.59470850679,109.78422490944,109.5784916972,107.34124942578,112.1914610281,110.15001620449,107.57104745473,112.22639961666,111.1159447866,105.33022611476,112.21542626703,107.68900900082,108.28715143447,106.68235580058,107.54689363879,111.5184405101,112.4414208542,109.36305065493,108.69423534601,110.39591524442,109.78248824742,107.08745165432,111.5515445937,111.03339065055,111.99618607034,108.77779980842,111.23304530551,108.08755570572,110.77308219083,109.17370378348,112.46549167994,108.63557455279,107.77104453068,110.98181090053,110.74285232038,108.92050195875,112.62260595621,108.88254411298,111.7665885147,110.37480185596,111.71962391123,109.18802594985,110.87691009508,111.4304239013,112.07896842486,109.03086660053,109.1538217633,107.56812792153,109.96998803546,113.27476107793,109.40202632941,106.96935623347,112.59322518303,108.33776936263,108.23923170899,113.80420593255,111.36151575448,113.0177842826,111.88884861361,112.91360033345,113.20358250557,112.30810652915,109.42940800603,111.76116376321,110.85569206392,113.25514132485,113.66630560851,112.70307348123,111.99742587308,112.66258100257,113.39114316478,111.70529646572,109.66851580413,114.73506722874,112.96905437195,108.81456599867,110.16775006878,113.66435972804,112.8251111617,108.06848293421,109.91409009053,113.38555740446,111.67880821575,111.84010275506,111.45034682983,109.04987860709,112.41315439056,109.08545406942,113.09787604324,109.14694797393,110.55603927931,107.64140873892,107.99875030498,110.80233695573,108.98757812582,109.71432399578,110.78906982251,105.58995553564,108.33513451448,107.79558417009,108.05388209685,109.49727107951,108.75623454872,106.93127065792,106.57127079813,108.3487346916,106.4092682727,111.01676081868,110.43901241938,108.45766998045,108.52695555338,108.81741372744,106.66036212448,108.80290996254,110.1613786129,104.30867791117,111.34105212813,109.73138202696,109.56060419314,108.38190592254,106.21542419195,111.46815940443,104.52071033673,109.55487377568,109.35321391452,107.99533248825,109.26568952777,108.96625580736,108.79406305633,107.38925103563,109.87051181594,111.00430536167,106.38333580818,106.31029563658,111.50938566655,110.26895391381,107.77978368268,105.33156597912,106.06431096245,110.7771479286,108.19685004209,106.3783791376,108.94622790796,108.74086906326,105.96137837624,108.54506003574,109.81492937772,107.42149584462,109.78883395953,109.26099744509,108.03051279241,108.3708142043,111.53133290745,107.17742581364,106.3278897074,106.76782766536,107.64574813166,110.69241544746,109.06757796041,108.12542223881,107.94001626913,107.40114987673,110.5405582744,111.03699418665,110.84157349122,107.72281286566,109.54489433247,104.89495391122,109.52637883198,110.22590253519,109.1503140144,110.63840351495,109.8307633516,110.41543043397,106.49182664487,109.84437563493,110.86375423935,109.2052620553,110.27719764005,106.1701141912,108.78819063483,107.59387901699,107.4530795782,111.18958813066,108.68828766658,107.88560216144,110.45545218222,110.55998346555,109.67650720264,110.50591551963,109.43098780845,108.66505108146,107.82984777859,108.63842613375,109.58976557605,110.34395131923,111.4468433232,111.11060297344,106.21850476298,109.1151466855,110.35211757237,108.91512297545,106.34218849899,110.1116860634,111.33039541774,108.72557775239,110.04086779272,107.7878518772,110.4086718467,107.47501620495,109.52134897686,109.63457616369,111.04709152229,109.56026568278,111.95304347197,108.35654826581,110.45621179879,111.15634700212,112.06750708048,107.79674422589,111.95704749693,109.73648630098,108.96308962999,107.33809334534,107.69414961785,109.27888471038,109.28726680061,109.29657979777,109.27965004892,109.43282779287,109.91997134519,110.06911495273,107.72189191387,108.21302334909,109.50537110628,107.51939058256,104.67910189202,107.71280386561,108.54146620429,110.75540363214,110.30192841437,109.73585129699,112.10861136567,109.52224823886,111.56544766255,110.86357611132,105.05968979846,111.5930924628,109.66656761302,111.31367554683,111.95607143563,107.12951828663,108.85996945471,110.80702855698,109.51380263323,109.54346422096,111.97598239214,107.75391200218,110.09006780576,108.17087379765,109.87539454109,109.4493641885,108.93925358757,106.14472888859,108.66519754468,111.2601984331,108.03925426566,109.48509564288,110.31169292989,105.60491386475,109.67609718499,110.36340930783,110.77476606334,110.92909005948,109.33727533125,109.6273539889,108.9434044956,106.55912849205,109.59691077154,110.71761548134,112.39384969028,112.17534518007,113.14955571144,112.40344724186,105.46236819735,111.42879782583,109.54324928673,109.13862699799,110.19553967747,109.66739071325,107.04730855896,113.10723836136,112.04324977362,110.56397841886,112.49176134489,108.19338573531,110.35311496205,112.06628036907,111.69913184307,110.45517767995,105.77198737888,107.63524536787,107.47483548947,109.45570713974,108.26965851197,108.61091667233,106.19471662302,105.79047484809,108.99495037914,111.84769375753,109.60711994411,111.03411174055,107.46677511888,109.6087906574,106.87272922234,111.70240047701,112.13954958763,109.96059572896,110.13734381329,110.428273336,110.93667367446,110.23148490804,114.77359680875,109.78813390659,108.83674682108,111.13449178659,111.22210374323,107.25281088493,109.85245107668,112.31976621895,108.24439134008,109.56925152074,111.73185925947,110.58639663762,107.75391494453,111.2673837591,111.67214658145,107.03511997732,112.6406241352,109.24762636293,110.06441863519,110.62567833182,108.89227846855,112.05210822825,110.90073646599,110.88713434148,112.79949580292,110.02258614844,108.27163932086,110.092661302,112.25864680186,109.69211828564,111.49223132438,112.73317200964,113.10100153814,108.30025255593,112.96334323289,109.45863352139,110.58811223584,110.29759711646,112.51490066586,112.14774040468,111.04175995261,110.74012992723,109.25156704506,109.35631839977,109.3337242686,109.78660454726,110.23964378182,110.22516527332,109.69638409213,111.44484628415,112.04686290225,111.29077083025,109.588540624,110.34693616761,112.38857779469,113.16373595957,112.52667576062,109.25793335798,109.61551538907,112.30908952618,107.28500688067,111.73804270473,111.18508812348,112.42814877856,109.3120656546,109.39453861782,111.82459720529,110.28530295872,109.06258382836,112.10259618298,111.64366848109,110.92805047153,110.99192351934,112.00728645318,108.38328533882,113.00106408289,106.81516298812,110.82716411479,110.19182497992,112.0108889852,107.89807360605,111.29986428831,111.75471301086,106.4472568745,109.29337496892,108.45449840956,112.32481139742,112.9072235594,106.61920440419,111.50331710473,110.67259078774,111.16850167319,107.51513551203,112.99739798508,110.41447802621,108.85266463928,112.51206672515,108.23831532944,109.18913183626,110.19808839568,108.89310788879,111.47956851959,109.24181771364,111.86443697631,113.13652710654,112.03524439298,110.88122039112,113.4495510714,112.660123801,110.85780126424,113.17837476435,112.54414926638,110.41242898166,110.78294264478,109.51390174291,110.74804364465,109.43272791353,112.8037476105,112.711461252,109.10161025607,110.75928982665,111.45531164804,112.74400340871,111.32680735904,110.67494519366,108.61151805511,109.58080147571,110.97492452988,110.69976426889,110.19635894966,111.51733432672,108.59042018494,112.44820488902,110.26803481289,108.21214775166,108.83876509802,109.02960502862,111.63169171939,112.09040897471,108.44887026233,110.26689106662,111.1263007174,111.48819571608,109.24636040442,111.65585621473,113.46107830434,110.67588804662,110.25632476123,108.5911091021,112.00607707267,108.73160272268,109.18255563923,114.33030892801,112.16704510775,109.20763043961,111.32434329315,112.75644616344,109.45080490482,109.30877731252,110.23973275734,107.3176287538,112.41048410017,112.05196141802,108.09954512961,111.24789016778,110.92783226491,111.03207929403,110.01759317874,112.46188901852,111.25668666359,110.09812196843,111.0587511784,111.91064850521,105.93911286301,111.10218089976,108.41001060185,111.08544288384,110.85344011485,112.23771249938,113.15664168159,111.44836316442,107.10891854393,111.14072416373,107.77211922555,111.31871950378,107.51338822919,108.88895463254,110.87682223352,110.52993227405,111.90700773326,112.74703170892,113.10771645875,109.76803076867,112.30417101532,110.6957255338,111.47475246251,108.35972276063,113.24385643091,106.52845328979,110.94240536369,110.609657949,110.99746787409,106.06088423064,113.04084612449,108.46500535848,111.9265619691,113.74300976262,108.29648210926,112.91041347416,109.77048050734,109.34141716191,112.80014369416,108.65358907777,111.67687762103,109.20898358956,111.03942162392,109.57338675508,107.24379037463,110.52603294697,108.77994934359,111.66219028204,106.35506640752,106.07152872868,111.72405079442,109.08387819201,108.45179949391,107.76861201252,106.71188820376,108.83806936282,107.28054375314,110.24162321105,103.96278227775,107.6833874642,109.62931118643,105.04735498449,108.91016983405,110.61217336704,111.10819000434,110.86257897843,111.63417912099,110.9139220426,107.95085128064,111.23279698467,111.26433475851,107.24094000962,110.40872574808,107.97587206762,109.06157745002,111.60258087935,109.02069971085,106.36939975972,109.26790916514,111.87788958635,111.18048854878,108.61450557073,111.15074659602,109.74154318741,106.77732679578,111.78193175048,111.84307002277,107.45799184516,109.81441551272,105.44290532566,107.28510393117,110.7990109791,110.43226229446,108.90876819725,110.55043342105,110.16000837507,111.59882916343,110.4169327541,111.19466409307,110.73676592647,110.49945878247,111.83968712076,109.37980313241,110.94987971353,109.3270614151,107.88542780076,108.1180429595,106.78975328313,106.87498622616,109.62391076668,107.00214749026,109.53073423542,108.40328698867,108.99649128008,112.13361214767,107.27380938596,106.63351804709,108.09894348204,110.71798592567,111.41165448799,110.76915176283,109.00751349049,109.67733983949,109.75861175207,110.93555036397,108.4487004033,107.24350092104,108.19181185936,109.30431363496,112.2078842167,111.14873167593,106.70605212505,107.06172697034,108.05402557015,111.43115954638,109.42033924838,110.9624806778,111.23731845419,107.0569185269,111.75629774063,110.77718897992,108.3274809991,109.62562452664,106.35241047174,110.2817862272,109.02062468246,106.42137471587,108.80160077386,106.94497485588,109.08787016706,108.50111044983,107.21318033471,111.31979437513,110.99940705084,110.49028739336,111.23573235393,106.6404766357,108.46372937464,106.49973777502,108.48306678136,110.08444712808,107.59961548503,111.00309103321,107.89563994779,104.8729228894,109.93530029262,109.48314332173,107.95044752395,109.18841460138,106.90141806561,112.21699875105,106.12983829675,110.5977011242,110.75259895689,105.82475398854,109.25992505635,107.87781183919,110.41668769987,109.11733350655,105.16788884514,110.95072828831,111.07348607442,110.02903313822,109.4531288816,110.88395233243,111.69787945849,111.08565192695,112.48931401723,111.24582449361,112.53483364274,111.93912277528,106.95001311656,111.29957048179,107.50843696846,107.98426239621,111.5921152146,109.2937895663,107.11776500613,109.88166849742,111.58663234377,111.17840583042,108.74025511215,112.11476498954,110.92948486333,112.23087229249,109.07676433613,112.2961848945,109.57580353114,107.95016567278,110.52358763046,110.99846599628,111.50466087958,111.66871724157,109.04104050033,109.9581790821,113.46053517153,108.91564890627,108.61303012126,107.22685970511,107.5529649504,111.21949148459,111.18156160249,110.64189280064,108.34073515466,107.93042125928,109.3493811317,108.56803565322,109.92800466692,109.86351468913,113.17710346354,109.03664828082,107.23733170789,111.34235583563,111.40954684636,108.71878064448,109.54892371318,107.23965642315,111.1588188939,110.07956053491,110.09940394783,109.66670395691,107.74190990388,109.1273096927,111.62753986451,109.79383562304,108.17301611657,108.07972538395,106.98436616644,109.16599070534,110.80971701742,110.94869439062,109.36764417794,111.64371086718,109.63443057544,109.77459688357,107.8185971491,111.55528364916,110.64533844841,111.41067626445,110.64874065056,105.9284868877,107.24066576268,110.29309231253,110.71714363321,108.02327811608,110.49726436332,107.98255573351,111.595462277,111.5466395007,109.84994099173,111.13108883308,111.26980555782,107.77909743464,111.89387378737,107.82004627684,111.58456565232,110.8084112105,110.98374237236,109.99217310952,110.73713455171,109.4558407367,107.41170674826,111.29091572312,107.66216324313,110.30775369287,112.48312040207,112.03058210241,112.88625731555,113.71689477884,113.5780484565,115.0599707226,110.56853468837,111.66532755981,112.41730021595,115.66254986512,114.33856227915,116.07487590213,114.09026053299,112.71362460442,112.87773152823,109.91376417494,114.32792822133,112.35112500438,111.02392663219,113.79471839093,113.92880354916,107.93800444385,112.89282175199,108.95025819245,110.23276436936,113.15228084231,110.99179240646,109.51098608463,113.49386859298,114.41142768206,112.11715088518,113.48228286343,109.90817039254,110.8948752127,110.64833870975,113.68682989237,111.45078554313,114.20801290419,112.5083849364,113.71818930071,111.95813928248,109.94613722509,115.71924158354,113.74113337254,113.73546202429,112.38491934516,112.12809728202,112.61026649382,113.70034538763,109.47464548589,107.54834037071,113.43503459807,112.31618279827,114.04769193072,114.88345461929,112.12089099525,109.1863202745,108.52720215121,113.35528052053,113.58028820781,115.19684026194,112.17765922308,110.79473202146,113.38852210561,107.19259032157,112.88593599215,113.51294619598,112.82308190541,114.53637630893,112.34291264225,112.3583870372,110.82788872209,110.86131344163,112.92703815772,111.87356085135,114.35284454291,113.82635898166,114.19488791157,113.05014301233,111.85808103677,109.07485147839,108.40267309517,109.34968239628,112.85893784615,111.44518440826,111.34603784843,112.62688975008,108.59624075997,112.4462229737,114.36128398003,115.15302116972,115.45326118087,110.04367199457,107.03605285253,113.76307386566,113.65526810437,112.84519427864,114.91077404704,111.9743693047,110.28048939578,109.894050866,114.17193978597,113.53652449356,112.22663051261,114.52542257484,112.38665825455,112.10088818518,112.94145912906,113.83051980759,113.66975690192,113.59147537301,112.72628111011,114.31546295206,110.13749581881,113.6866476679,113.24633188619,114.46531709301,113.82419959417,115.07050637877,114.02135148354,109.52230256495,112.62417655842,110.18994166913,109.19221121775,114.51136236549,109.81952134024,112.53044811373,112.49069881459,115.10476736519,110.87249058065,114.38787162409,109.88938247708,111.75368678607,111.37697625174,111.44004658747,111.35595551159,110.76638787189,114.81106484508,113.6588902363,113.3091148264,113.38396747078,112.03804269807,109.2695813654,111.28855744552,113.08086051932,112.73018642316,110.37916430115,112.27425049761,113.68423951714,112.79287978174,113.99265400022,112.08882061178,113.76148927507,111.1259664835,114.37907413379,112.02534348375,114.06993190656,110.95334024279,111.04208582283,114.73555052212,110.93917272254,114.56691804765,112.73552515557,114.20820461481,113.20017274323,112.28594601074,113.15429470087,110.96672445891,114.50219675976,113.83699282592,107.29938904168,111.35346356798,116.12192391739,113.21435333786,112.15148448956,112.25231283544,112.76029050812,111.0070151195,111.91654884179,112.52310272368,107.53265679213,110.89618940601,112.47701600713,114.46508370214,112.23232630152,109.40870576498,115.63336480391,113.52357560284,111.05554993462,112.6702646312,114.18239416507,111.52273318501,115.55650398322,114.23497923812,113.73400624254,112.48577038516,113.3970521105,109.57896241536,111.34423612227,114.99014409087,111.45592039521,114.85276044672,114.23429632459,112.55123562831,111.98605743192,112.4267039816,109.73710706421,113.16804410079,111.39630097033,113.73990990613,109.22630005641,113.89899956427,114.31384261059,109.62118115398,113.81819524886,112.49016747458,108.8402278117,111.34135165455,113.18048989815,110.13397491211,107.4705810144,112.45993333058,112.41153776067,112.92999852169,107.91717825016,112.07784053741,111.97800380109,113.70126858677,110.31275190638,111.470575293,113.21759767823,111.20680755498,114.43713936917,113.30812024296,113.94507253081,114.8654495967,112.41712523473,113.72718987995,112.11959225154,111.70300172342,114.16875313007,109.38444637594,107.53740128436,112.79478171551,112.98995558607,111.6642915592,113.70433828044,112.6311163741,113.28423960978,114.78409195609,111.77586127483,111.30594288209,107.62594215613,113.47984902308,113.07190959247,110.73830267689,109.49952410322,113.6240539593,112.39316376771,111.03065730728,111.24262380814,110.15266481647,111.62567878472,112.08212279901,113.42025856229,112.57659961396,113.327620901,110.91802799621,108.60613981269,113.34856632784,108.84510890375,110.55662305743,111.46743696977,109.48142090922,111.21897698624,107.7895616209,113.03364114838,111.24721378682,110.10081811843,110.72253390589,109.49669052644,112.14687428406,114.72621326432,111.21090571331,113.55301314503,113.99829990045,109.91596334134,111.56656846336,108.87361666676,112.54855989452,111.83382278034,109.43065452417,113.06851958087,107.60207888469,111.65369994007,112.12179791723,111.08670020754,113.73120955032,110.91410780129,113.5640428415,111.65648794569,112.23925405068,109.97667900228,108.7090141918,109.84841399263,109.45004904022,110.45375127042,113.21795494175,112.62202471042,111.61689573657,113.77914033125,111.69499211842,113.35705450856,110.0843296693,114.10063275279,109.90587432915,111.06078292058,109.00877465985,110.97615963083,112.67426817782,112.75029257834,110.16549060001,110.08192636229,111.47843143291,111.48310852971,109.38045395724,113.05712822756,111.32503750718,111.72786456028,108.09760323418,111.98095264432,109.48561068836,110.28314147757,111.09019099464,111.07345248056,112.51330719601,111.59973504321,108.45902108837,114.45728040767,113.7521692782,107.4571460979,112.33944998943,113.16016639147,112.92687652834,110.62242331799,111.56607028321,112.89603684705,111.6678337427,108.84457441603,109.70848109675,112.27415168155,110.62040205077,107.08830556541,107.36234980205,114.2270306841,113.64796754191,109.71534917121,112.48323226716,111.44567673892,114.12797210988,108.99688096085,112.38420022485,112.44480114249,110.70400408576,113.5591447174,111.95782481006,113.76610095907,110.58992146841,111.55532824414,112.9492330217,112.36854943188,111.15209350943,113.47190110818,112.31620032919,109.23787233643,111.74268062263,110.73156450937,113.72325606895,113.30903981864,110.88692809933,112.42869469951,112.72450712744,112.63392840914,110.41988744397,108.07740298686,111.66047128029,111.03447160754,109.81886026751,112.98832211954,110.10725622401,111.70202456054,113.17609825471,108.53244715213,111.03036208456,112.06612631657,114.22175464271,112.11987820516,112.01703036546,109.15102069035,112.90440244286,112.69207284991,109.90810040894,106.58527180588,109.31446379254,111.91934651768,108.88528184498,111.56028232042,110.92909756331,112.44412368093,108.74952451409,109.76178220752,111.70908980768,113.4953346504,109.13655794404,110.25937035635,107.47268369154,109.35352879485,112.07564106222,113.63985191393,109.98712655259,111.42816188043,112.44236796187,110.18533202811,109.3712528669,108.67843950473,112.91900039266,113.23179335849,112.93662104528,109.94014415002,111.70364058992,113.91250506517,111.41254182855,113.338463045,108.03443727079,113.07889840704,110.30047955645,106.74245917159,111.4004762464,111.39231219624,111.97603367276,110.62176186174,111.86269113041,108.60432118656,109.88151749451,112.35057837723,112.79457414672,109.68459722491,111.72197062474,113.29238089057,112.0485268564,113.58167261383,110.90950086758,110.79485919762,108.05775418656,107.92298843229,109.88705377884,112.20306622547,111.71952520497,110.55548710267,109.43747030417,111.86590061667,110.67551306428,112.62075668186,112.89407739879,109.99407923645,112.35434265423,110.11905681293,113.62025144395,107.85177217845,113.86397140647,113.15407101286,113.67790052415,108.77934013417,109.39388985639,109.71724484927,109.0369561964,112.46652318798,113.04739449227,111.54251745469,113.5187506472,109.38320291894,112.84575806315,112.2384646206,112.13610849326,109.90334323411,106.88016440842,112.71547586515,110.07604291229,111.11081136719,109.07135505457,109.91463457836,112.09573287198,112.67753947397,108.3551375975,107.72525666978,109.18022469406,109.84421512485,110.76897770976,109.86742607784,107.01288163091,107.28374027124,108.98732418366,111.45445409894,114.04546787512,111.69749710412,112.46850303767,113.09685839822,110.04311791113,109.67987032004,111.75938469376,111.40849306725,112.36843221551,109.30681467937,107.76424932541,110.21448050123,107.81794030157,107.52073081697,109.87448278386,113.09108977009,109.66605707332,108.00783809425,111.51036665657,111.1719902725,107.56339942785,108.45110850055,110.52076799243,111.15367045455,110.99604492052,112.3821373635,107.12487768952,110.44327986202,108.42134889396,110.64167928388,111.34206143321,108.7113909286,110.91665374323,113.06531217744,112.86914362612,109.04264305727,112.18855945586,110.93965307797,108.93612350998,111.3799195253,110.74542882836,110.92560596686,111.72267135118,111.4504321651,107.08957042502,109.67431939904,111.68771512044,111.32551498819,109.83358143048,112.79177394517,110.24773230524,111.71010216972,110.26177217563,113.37062630076,108.90457283813,107.29325592263,111.35576819746,108.05283048155,110.91691975803,112.16886573238,113.26115864744,108.9874963601,111.14168527249,109.27298484797,110.34516879316,108.78193404853,110.63611235167,108.86159496888,110.34430185563,110.30977003059,109.69148888006,107.36091550098,110.90977283861,108.72414875266,109.96899175202,106.47115656105,110.62195576252,112.98946016234,111.74636884826,109.11370652689,111.17588164781,108.82981721203,108.12907663456,112.06382243644,113.5575644723,110.76630856948,111.09551208041,109.34612136691,109.89099619444,113.23694707867,111.66215920815,107.97423452674,111.33641205503,111.79256941002,106.78637760552,111.24393125,108.24491327609,111.72291452497,111.95988922757,108.3365020214,110.74612868982,113.92106059834,109.90479972775,111.83877202739,110.02051904429,115.05219692706,111.20150892068,109.7417142807,113.25773836233,110.99344804312,114.12914508126,115.11551574709,111.75399151794,111.65510473207,111.85954931621,109.91097863711,114.10524288519,114.40788877244,112.13004780934,112.2935683862,108.17394118508,108.81783861997,110.35009452284,110.78131784089,113.05960510316,113.86824501018,112.31117383334,109.44912495693,111.08123989105,112.41822005328,109.68722027826,110.16777720188,112.62054135777,110.53840169084,113.74612700038,110.24442526051,113.49439987583,113.45257451623,111.96281161052,112.91532937238,109.12374453716,112.73117572885,112.1546910652,113.31126082996,112.50953904657,112.80903242266,114.41910804594,113.25749016985,110.25146347602,113.14407667453,111.74602029176,113.70227802382,114.35317904749,110.59051556075,112.48180136353,110.33236583928,112.65325837142,110.58494575472,114.7034946663,112.42486194841,109.74246532973,109.55334180136,113.77667203215,111.0238316278,110.41527147228,111.95764233218,113.76914479523,113.10627729583,111.12793856059,113.29882795471,112.2224916703,112.92678438802,111.07867756803,111.35074984396,109.13776290195,113.09042945784,111.10586833961,115.03138595938,110.44108439343,111.66833497011,109.36296079945,112.94466196614,113.67176291054,113.60655027084,114.21600102848,115.20581654511,113.53486679409,111.12856947841,110.65159486353,112.81238978671,109.84040763616,113.06428113608,113.42372106999,110.7998286183,112.67365779597,110.93293111977,111.6110267612,112.99945357745,113.78180153912,110.75913407971,112.0367854534,113.08501743143,112.96585875136,109.43645558543,113.94022164137,113.46361762148,112.23335467905,114.58023347241,114.48055591443,112.50963727297,111.15558732168,112.33591795317,114.23327259474,110.03854894778,111.56551364507,108.18250104987,110.8184491936,109.502291663,112.57490651021,112.25236221991,107.04248262004,108.48542744535,110.66865095007,108.79166606249,110.75113632825,109.67497009712,111.26845902788,110.58340352227,109.53969313913,109.75642964595,108.50099991405,111.51858634957,107.19920338684,112.40771422368,112.39895023643,113.23843901256,108.52598525222,105.25996622906,108.22919688135,106.48025882264,112.68987613662,112.56841307238,106.31444151717,108.95447689321,109.32573690024,113.77577755158,109.09730775041,114.19872074123,109.7874243114,111.50894597674,112.85701366297,107.90577996562,113.77530171925,112.00094108346,108.37680969517,111.60198152985,113.38163375546,109.67636073212,111.85959106943,112.56389168031,112.84132346062,113.98287444888,109.95467424164,112.68481856072,115.12167173864,108.86885400104,113.41147899988,108.03432596561,111.46799359161,112.88383716051,106.66132133866,112.97800202891,111.04290213558,108.48602665552,114.87586838103,114.27714782733,112.06561143089,113.84618904271,109.49797075398,114.78608147414,112.68307474696,110.9677667885,113.26496079456,108.64033955152,110.54477438828,115.19094102122,113.44110750208,111.09765529241,110.95124868024,113.50974816302,112.57964475931,115.22277410416,112.71724233546,113.97280174354,113.49660192714,113.8057261496,111.98132759726,113.49076991101,114.63572337575,112.5531860546,111.46389762262,114.05231708959,112.59209917513,114.56332218993,109.32838444674,114.96208507657,111.17671446293,110.09243186695,113.50059160661,108.49590371955,112.81396682723,112.36862594556,113.27584671866,113.63377864218,111.76611958042,110.6540113593,108.9977116194,111.19154718318,110.73483893478,111.92279861028,112.77115535414,109.94525319882,111.09520536584,113.35986990618,111.21451195467,109.24141583053,112.30102496524,108.47498209847,112.4714062245,107.52049593642,112.63058005709,112.8519007695,111.21814579965,114.98032804725,114.21870624083,113.38192584136,111.54186944556,111.91220495379,114.10527939134,111.73461472074,113.46304936759,109.34453053083,109.01337862206,113.38529617927,110.04624758617,113.41869080793,114.80840931665,113.07595591423,113.47064176442,112.69114171926,112.62532512125,112.92589485089,110.01782305378,110.99076899021,109.18569141978,111.49941624684,112.04449626018,113.44809831991,113.03860504769,113.41654841791,114.2205007914,110.91522440861,113.79872429343,111.75372595879,111.44675779158,109.60833771055,112.32204134247,110.87302568357,113.65057023712,111.73077999572,114.39576518315,112.63242363608,113.06933730176,111.95011326398,111.32180313452,113.2625583728,113.55865454472,113.90595584896,113.6210766236,109.56750779851,113.5269789622,111.70388415544,111.96110047372,110.0337355006,112.23778176964,111.91563006512,109.2871447868,112.99200966079,112.88474071548,112.49931116263,112.02434003272,108.56332264968,110.66248560434,110.37721537965,114.09962184874,110.49036483591,110.09067496267,112.81938456577,110.74257423059,110.67991630532,112.4236874341,114.16453821678,111.27463635,111.4519504617,109.31336170395,108.34864530625,113.13471214977,110.97810387667,111.11567676549,112.04498849399,111.07651392845,112.63168761268,109.99929578655,110.36744888293,107.95081510861,111.29756731613,111.29271805639,106.40670290798,110.97072052433,110.26045061645,107.60844036332,110.27125503036,108.74811431944,107.74187646934,109.48506796343,107.02159237932,109.84240590497,110.85073691038,107.29853064707,108.29440543339,107.96505964455,110.70779560501,111.28583379058,109.42429471192,112.18612498362,111.95549066876,107.11130365476,109.99200532369,111.92654053553,108.36577839125,109.66208032279,108.17914801282,111.92773451479,111.27143022459,109.08501150399,109.98771791037,108.50727576147,109.8593077054,111.06311387026,111.23244285998,110.11721156161,108.7735716995,109.88724793214,109.7344126996,110.24658846774,111.02969434504,111.0887969772,107.56925834761,105.48603760848,109.6537925223,107.42462801179,105.62048895987,106.61573891666,106.0166784479,108.66761890768,105.84679009959,105.63776825964,106.65614056894,109.33792543227,109.37140409391,105.52125723544,107.39657202932,105.17087093076,108.98946414514,107.97131434424,104.13580144714,107.2075754675,104.52205002516,107.16643426847,107.08271603989,108.76613733121,104.48240585277,104.91095459107,108.02523733348,108.69122655587,108.98317651777,108.42384124855,110.29690745191,108.05433570231,105.04064340543,108.94826116678,107.26549475164,107.68921306071,110.90880878316,110.65238605523,108.64940396664,107.72363635825,105.76720925001,105.89061233041,108.39132746157 };
        d2 = new double[] { 372535296,382377984,391487488,372695040,384020480,388427776,385986560,401125376,389529600,394014720,383983616,383504384,388804608,391004160,379936768,390008832,392830976,385937408,386101248,391065600,383262720,383717376,405553152,411299840,395431936,400896000,411578368,413659136,393289728,397176832,415444992,383922176,380809216,402214912,420286464,408412160,392343552,406532096,405950464,405266432,405266432,399302656,392708096,403107840,396943360,413351936,394735616,414461952,391102464,415363072,394797056,394432512,399646720,393465856,395780096,395829248,393895936,420745216,401338368,396079104,407846912,401051648,396705792,396345344,401694720,395808768,408088576,393785344,400572416,392282112,397897728,393547776,394969088,398417920,409014272,394620928,401031168,402567168,399011840,398532608,410574848,391426048,419282944,415105024,400883712,399093760,398114816,408608768,408432640,394809344,410787840,394522624,392658944,394158080,409600000,415043584,396181504,413138944,392978432,390225920,396677120,405676032,403087360,405594112,399937536,395894784,394608640,398200832,406245376,392458240,418922496,401690624,389500928,411136000,386760704,413073408,385392640,394715136,389586944,395960320,392036352,387768320,381210624,405696512,389230592,384266240,392065024,397029376,408678400,385114112,388362240,395304960,383799296,384253952,383303680,396394496,402731008,401383424,394399744,410091520,393256960,408727552,390725632,388235264,395939840,390950912,385069056,387432448,399179776,390594560,391823360,408346624,394641408,403292160,395751424,381997056,396726272,386023424,402124800,382537728,391188480,398589952,382857216,403599360,390873088,394309632,397361152,383676416,380076032,385716224,401346560,399822848,399826944,381673472,398479360,385056768,377995264,376963072,391421952,376049664,392478720,399884288,376856576,393891840,376143872,384397312,395837440,392298496,374730752,383254528,385105920,400908288,387526656,377487360,388812800,381935616,385187840,377909248,376717312,391622656,376836096,384651264,377069568,379535360,382607360,378822656,395116544,378396672,380440576,399740928,381788160,397438976,390111232,386150400,379228160,375971840,385290240,383262720,377401344,398135296,386297856,384110592,386359296,383848448,385998848,376852480,384012288,384344064,399777792,396468224,376623104,375128064,387264512,375648256,392667136,389255168,373874688,386101248,382988288,390455296,397877248,389328896,404615168,383582208,386965504,384815104,381370368,384880640,383262720,383463424,374714368,385224704,376725504,387416064,384774144,383967232,386433024,386834432,392900608,383627264,383184896,375844864,388706304,387858432,394989568,385040384,379428864,376778752,388112384,382734336,402206720,376815616,377913344,380018688,382402560,381964288,382722048,396218368,386211840,380309504,381325312,390418432,383815680,398917632,384167936,377221120,384434176,385839104,395571200,379887616,376418304,380084224,385200128,378437632,378007552,391417856,391852032,376610816,390393856,380276736,386224128,376111104,384335872,377425920,379564032,400736256,376180736,382963712,385146880,385929216,377671680,379469824,398663680,381931520,379908096,375885824,383655936,384159744,379043840,380432384,380719104,377397248,377303040,381177856,396369920,377040896,379293696,382693376,390258688,377618432,381018112,379682816,386469888,385335296,376221696,378331136,388902912,376958976,370102272,377380864,370663424,385691648,378560512,397266944,370716672,377184256,392278016,377294848,369430528,370847744,378265600,370515968,387846144,371118080,375603200,375427072,370831360,377589760,371916800,368480256,369254400,399187968,394952704,371372032,369049600,370802688,370241536,370724864,376610816,379260928,387842048,375123968,369745920,371642368,366592000,374640640,366833664,368275456,367886336,392151040,374788096,382320640,399278080,394911744,370671616,383025152,379637760,372101120,379641856,378572800,381874176,401866752,391942144,388337664,397881344,381849600,381624320,388698112,393519104,400277504,384905216,392511488,383672320,384270336,393269248,390021120,383795200,391340032,390000640,400314368,398553088,392880128,391102464,392032256,394575872,387489792,385622016,388419584,394289152,384110592,393973760,392511488,385220608,387932160,386142208,399581184,391589888,385613824,384352256,421908480,395427840,390283264,389324800,392372224,385978368,389001216,389984256,392323072,386953216,387043328,412049408,385286144,401416192,385998848,386850816,389926912,387919872,390127616,385937408,385949696,402853888,385982464,387874816,391024640,389029888,385191936,399749120,392933376,388321280,390909952,385171456,395223040,405741568,386252800,392491008,405037056,411389952,385073152,385007616,392638464,391790592,385249280,391319552,405843968,416309248,392921088,409276416,386154496,385003520,401702912,395935744,386121728,400613376,394919936,387952640,383307776,385748992,399876096,402923520,393531392,396890112,393920512,385622016,385769472,393744384,384585728,392798208,394072064,384020480,386056192,386588672,390852608,401821696,395300864,388411392,389746688,387645440,392527872,385970176,386428928,403804160,385298432,391159808,394797056,398413824,386326528,385699840,400703488,384266240,399073280,385585152,380256256,393302016,389107712,400658432,399187968,401653760,400347136,391622656,404819968,405901312,384520192,385212416,390123520,410066944,385863680,390877184,388022272,386711552,392916992,387321856,393375744,394133504,386162688,388407296,401240064,405028864,385122304,384532480,383918080,391254016,384630784,388775936,393994240,389951488,402829312,390533120,404512768,406695936,388239360,402108416,387985408,388354048,392888320,388096000,401743872,396255232,385892352,405897216,404418560,386805760,396849152,394371072,388145152,387448832,386236416,409993216,401129472,387588096,394891264,387182592,386543616,388444160,386121728,386625536,397778944,387407872,395767808,398163968,397365248,407547904,386146304,409202688,395661312,395874304,395132928,399032320,395784192,403288064,410578944,389087232,393326592,388059136,401653760,395988992,396222464,403824640,387166208,390733824,393596928,402788352,403066880,405454848,402452480,389226496,391843840,407330816,387592192,397062144,389820416,391610368,398606336,391118848,410587136,387948544,391540736,403501056,410337280,392830976,390316032,398049280,397852672,399421440,401035264,400805888,393482240,401317888,390524928,386334720,394346496,389861376,385884160,384860160,386805760,386306048,388091904,387276800,393334784,391700480,386404352,398622720,401170432,384499712,397950976,396468224,393453568,404672512,400969728,393252864,392265728,392482816,387379200,390426624,385302528,383746048,386195456,384454656,398245888,385691648,390766592,399802368,400494592,385101824,384856064,402444288,404422656,391413760,402563072,384585728,383619072,401821696,401833984,384405504,389898240,402153472,402497536,391884800,393842688,390832128,385335296,384233472,398077952,389877760,405716992,399298560,400531456,391458816,386830336,384421888,392290304,403394560,391475200,383385600,392302592,397303808,394502144,393555968,397070336,392605696,392744960,406175744,392818688,384851968,385933312,384335872,386592768,383471616,387084288,391172096,412065792,402452480,385261568,390647808,390623232,389591040,392114176,394051584,392331264,384299008,408211456,393187328,392241152,398827520,383926272,383315968,383152128,389939200,397438976,392482816,390303744,384610304,384958464,384372736,399204352,385687552,391839744,385683456,403890176,389718016,383553536,399872000,385257472,389197824,390795264,384520192,400461824,384442368,382869504,393924608,391839744,384770048,389672960,407650304,390295552,391045120,385327104,384610304,392437760,409055232,384761856,403435520,400232448,386711552,385761280,386285568,384671744,399806464,394633216,398528512,391598080,405090304,399736832,406904832,393920512,388259840,403427328,398077952,391073792,388755456,390496256,397262848,386727936,395436032,389242880,396541952,392810496,390242304,397524992,390258688,403668992,396431360,398143488,395526144,397553664,400596992,397959168,403660800,388767744,403263488,399773696,394182656,388907008,397426688,396058624,397897728,404041728,397348864,390393856,389042176,397131776,399577088,397553664,391086080,388935680,402513920,401604608,397463552,402673664,401817600,394625024,404332544,408780800,393977856,393424896,399740928,395005952,399953920,396546048,392331264,409677824,400760832,402407424,393269248,396808192,410763264,394498048,393211904,399032320,407982080,394838016,413130752,403775488,403705856,397312000,399523840,395091968,407863296,389251072,415043584,395890688,387371008,395984896,393003008,397635584,411521024,386285568,389165056,393502720,415977472,411815936,396730368,403148800,394293248,403685376,399835136,398995456,408477696,398450688,398692352,412418048,408764416,412856320,397312000,392294400,391036928,391405568,407961600,397799424,400281600,401494016,413253632,401825792,401199104,399945728,405934080,395919360,408256512,395464704,398176256,415178752,396312576,396881920,414007296,397107200,399175680,391057408,399020032,397148160,398213120,404647936,399118336,394190848,401154048,400338944,401244160,397488128,391778304,402292736,399106048,399286272,417206272,400060416,400388096,401494016,423178240,397717504,398397440,392335360,401256448,399622144,400826368,411598848,407568384,401833984,399503360,391524352,416133120,391598080,421818368,392282112,409391104,391421952,393224192,396636160,390242304,396832768,397680640,400850944,405123072,398376960,398151680,393281536,399826944,397139968,399482880,410599424,394346496,399007744,400388096,391458816,398831616,400113664,397578240,410689536,398233600,418140160,400883712,402780160,405360640,394121216,393596928,407470080,391274496,398905344,416735232,402161664,407363584,390864896,395837440,393203712,392425472,395022336,392749056,389918720,389500928,389857280,394772480,390447104,398753792,390180864,398376960,397250560,388698112,396152832,394387456,389074944,387846144,391360512,383578112,384126976,400961536,391557120,390279168,390287360,390987776,383635456,390905856,400347136,391475200,384557056,391036928,386150400,400265216,388395008,393281536,397758464,400261120,404889600,398827520,389062656,392822784,384344064,391835648,397471744,395702272,384479232,394362880,383299584,382685184,383225856,385851392,403853312,407011328,385372160,398884864,396054528,405037056,393187328,405254144,406917120,388354048,397664256,386363392,401121280,386560000,393064448,390868992,404119552,390901760,394952704,389353472,397049856,390438912,404344832,392249344,404205568,393547776,398315520,389488640,392052736,414990336,389951488,391974912,392019968,415809536,391049216,391290880,385212416,391061504,399392768,399798272,399515648,400138240,389763072,391811072,386707456,400629760,389394432,383860736,406667264,417964032,390823936,386109440,382717952,407584768,383463424,393445376,398598144,392568832,391716864,389066752,393375744,393080832,405549056,387276800,390103040,406409216,401420288,387174400,385667072,391921664,407142400,408084480,394809344,394850304,399925248,408895488,401756160,393363456,395399168,395943936,405184512,396918784,410599424,403107840,397185024,409542656,407248896,396083200,415416320,396484608,402944000,400650240,393797632,392265728,393302016,386400256,408252416,395911168,400863232,387461120,402927616,398295040,404803584,390864896,388210688,395354112,391360512,394485760,406605824,405422080,395497472,394166272,394760192,387878912,386150400,394924032,394989568,387919872,413483008,390541312,387452928,392347648,396607488,393936896,394293248,405471232,392663040,410255360,391606272,392953856,402628608,397623296,400793600,400990208,405929984,398073856,395886592,391462912,386752512,395608064,407408640,385658880,387170304,385630208,393875456,406089728,388427776,391901184,392667136,413384704,392818688,408313856,403550208,402284544,399339520,405766144,420265984,401895424,412491776,399577088,404189184,385101824,404819968,385372160,291188736,296480768,297787392,299606016,298692608,293167104,298237952,296534016,293281792,292384768,294965248,295485440,298450944,304115712,294559744,289992704,294879232,297078784,290041856,297852928,298606592,295825408,291622912,293593088,298921984,297779200,295362560,298815488,298422272,289411072,295591936,290762752,295673856,289914880,297881600,294699008,294805504,294019072,287223808,295411712,292614144,288407552,385720320,383209472,391704576,389382144,383201280,407142400,386478080,393007104,385937408,377839616,389427200,385241088,400355328,378044416,384827392,388366336,383766528,387325952,392237056,388292608,402309120,396709888,384438272,400044032,386252800,395636736,403226624,393646080,385146880,402522112,385380352,384790528,400891904,392134656,391106560,386510848,387305472,388374528,385699840,396496896,406208512,382910464,385449984,385314816,381751296,388829184,385482752,386449408,390582272,393027584,382779392,395206656,383447040,384823296,393637888,384679936,388308992,382980096,386863104,392306688,383381504,388018176,384794624,384507904,386191360,385601536,386850816,391856128,384663552,384839680,391868416,383504384,385617920,395919360,387452928,400744448,389541888,388444160,403210240,399941632,395210752,414572544,401928192,381435904,393691136,386453504,386412544,386981888,384184320,395857920,393535488,399757312,381939712,386412544,395280384,382754816,386711552,383279104,383422464,390987776,399958016,378380288,392060928,392089600,383275008,384913408,407044096,404140032,401248256,381685760,384421888,395026432,395366400,384868352,407744512,388931584,398843904,391618560,394207232,405176320,387641344,380780544,400125952,399826944,386080768,393302016,410181632,390754304,385708032,400916480,388022272,401211392,385048576,381923328,392982528,404332544,400089088,390340608,398057472,394788864,387006464,406806528,398897152,388444160,389513216,389316608,395677696,385646592,395096064,397803520,397733888,399806464,387932160,400449536,404066304,407502848,386994176,386330624,402825216,385130496,386260992,387588096,382042112,384741376,389431296,402821120,385867776,385318912,395460608,387104768,384061440,386609152,384741376,392220672,386052096,384851968,388976640,387543040,381124608,385785856,386732032,386740224,384950272,387481600,392159232,387239936,388366336,402800640,394317824,388050944,384425984,392540160,403525632,387866624,380022784,408162304,380465152,385703936,387796992,404549632,399368192,403726336,405385216,398004224,395042816,382509056,398913536,387969024,405676032,394735616,402128896,397217792,392822784,409153536,385884160,403779584,401068032,382951424,392904704,378798080,384659456,371425280,390029312,375861248,376197120,396877824,379461632,401448960,374833152,390217728,395063296,385048576,375222272,377143296,376578048,378826752,379371520,377835520,396656640,394915840,403378176,389148672,375836672,388861952,392941568,378101760,394153984,379289600,380375040,376193024,368799744,376352768,390782976,381968384,391761920,374767616,391192576,376516608,379764736,386265088,377774080,384618496,372830208,386125824,390316032,374030336,380915712,372731904,400863232,393453568,375349248,377430016,376463360,373248000,376213504,386936832,373616640,387436544,375042048,371515392,388816896,390144000,372924416,376328192,376770560,373813248,376516608,375394304,376250368,382226432,396083200,377667584,373145600,370556928,371937280,389427200,388857856,391917568,382529536,374960128,392146944,388632576,383545344,386060288,380100608,399507456,376893440,385888256,372240384,383184896,376504320,377171968,373481472,380628992,376545280,390868992,375214080,379314176,393064448,375300096,373551104,375828480,392347648,378941440,375533568,375971840,389517312,386265088,390873088,391704576,387850240,375504896,386076672,384815104,381489152,384724992,376307712,377044992,374427648,374906880,392429568,382857216,382939136,375349248,385773568,375668736,384364544,379510784,374681600,396218368,377876480,379494400,387219456,375934976,377798656,386273280,369971200,400265216,377872384,373235712,374751232,374394880,377696256,390717440,376033280,384692224,376418304,379609088,388616192,378662912,389971968,399503360,391856128,375574528,393760768,377466880,377683968,375857152,380948480,387874816,379297792,387076096,375119872,375635968,393445376,393482240,392957952,378433536,370634752,374681600,373497856,382873600,374984704,373964800,389668864,375853056,389722112,372416512,378978304,374829056,377475072,374702080,381538304,376160256,372932608,376332288,397053952,375914496,375422976,390955008,371269632,377061376,375930880,373587968,380956672,388837376,375070720,392695808,376512512,376340480,375414784,396967936,391073792,373850112,379985920,375697408,379744256,381472768,372756480,374398976,382038016,390258688,373948416,376696832,376885248,382050304,382582784,372441088,375648256,380776448,393101312,389390336,377065472,380514304,383803392,373911552,390995968,372961280,374579200,375463936,375398400,389562368,372850688,392114176,382152704,379744256,377061376,394747904,375865344,373325824,376156160,374456320,373735424,378597376,373805056,375455744,376352768,375783424,391098368,402968576,368898048,376279040,373030912,379879424,376725504,381943808,394969088,377384960,379441152,371564544,386527232,399327232,373452800,381349888,377253888,372449280,371707904,371052544,373686272,376713216,386408448,372416512,374513664,375848960,371105792,378126336,384462848,379965440,372609024,389025792,382492672,387194880,385175552,384118784,370188288,378691584,372310016,374665216,371642368,372424704,391073792,373231616,369340416,383279104,379801600,373362688,388509696,373223424,386052096,380825600,389750784,386859008,370704384,372457472,382431232,373641216,372633600,373125120,371953664,373018624,370466816,372518912,380309504,372166656,377802752,384622592,372002816,379908096,371417088,372162560,374280192,386142208,380338176,372105216,371363840,373211136,373379072,371916800,372604928,372051968,373252096,391073792,387354624,372690944,381378560,371814400,374038528,387031040,372957184,377147392,371425280,379654144,374300672,380600320,373686272,385773568,369127424,380715008,368545792,372715520,379273216,374874112,383705088,387375104,374304768,378077184,371019776,380145664,381358080,371343360,376156160,377257984,374022144,380653568,368398336,385990656,383868928,373714944,390578176,372830208,372105216,382775296,379285504,375668736,370888704,373559296,374181888,394297344,375119872,373878784,370089984,382570496,372236288,373268480,381718528,375136256,370241536,373248000,375988224,387932160,372047872,375177216,374538240,379006976,372051968,369758208,374226944,373547008,386052096,378658816,384389120,369590272,370839552,380297216,384172032,371306496,365875200,373956608,369893376,373288960,370696192,384086016,372285440,372199424,373256192,369426432,380665856,390111232,373006336,387108864,388898816,383434752,371621888,374145024,390414336,389136384,376786944,373555200,370225152,383160320,376180736,371761152,373231616,379674624,377307136,373395456,369618944,374669312,386531328,385179648,371273728,374251520,368623616,370929664,373399552,373313536,372408320,382242816,372686848,372944896,368799744,370094080,372817920,373657600,372031488,373170176,395087872,370274304,373719040,371875840,371183616,372174848,370569216,371957760,369922048,387551232,378621952,395399168,372203520,386924544,384585728,369942528,371695616,372584448,388300800,393158656,391913472,371253248,369766400,384376832,372068352,390217728,384958464,372715520,372731904,371908608,368857088,372228096,384196608,370978816,375975936,368709632,368799744,370302976,370622464,374620160,387104768,383799296,371044352,387899392,370966528,385638400,372985856,371634176,368402432,384716800,371318784,374784000,370446336,373465088,370491392,389332992,373239808,366252032,369139712,380239872,372805632,374173696,373252096,374120448,374775808,370970624,373706752,373751808,375681024,384434176,385662976,376180736,372609024,380231680,389509120,375812096,371904512,364613632,373067776,379772928,372256768,374218752,370524160,391626752,375902208,381923328,377307136,370987008,392921088,378163200,370626560,382029824,365228032,384765952,382304256,374382592,375435264,372224000,372408320,375484416,371417088,373063680,371789824,382992384,374312960,374153216,379752448,373673984,388976640,370274304,381718528,372396032,372920320,373051392,372477952,369123328,372879360,372219904,392192000,372551680,368996352,370192384,379498496,370888704,374292480,370147328,370642944,380588032,370896896,371666944,370024448,370839552,370667520,378220544,371490816,373350400,371224576,372219904,372510720,375754752,388562944,373059584,378142720,372654080,370835456,386736128,372809728,373325824,373276672,372150272,387133440,372412416,382504960,378040320,372613120,368168960,375123968,375136256,384065536,371671040,380882944,382021632,371478528,377614336,372588544,387661824,370311168,370790400,387084288,381628416,384303104,382279680,380313600,379609088,371838976,371834880,367751168,370061312,386146304,368447488,371228672,372101120,373080064,368340992,383873024,395632640,374468608,379928576,381923328,370511872,369000448,373669888,382623744,371200000,389341184,382124032,378892288,373874688,369467392,373313536,378941440,376381440,377860096,370102272,370937856,395755520,385273856,378195968,381595648,382029824,372129792,370794496,378376192,373465088,369266688,368513024,369901568,371245056,373334016,362967040,373473280,376500224,371118080,367845376,368312320,368164864,377597952,379117568,371425280,366784512,370692096,368910336,370978816,372547584,370343936,372080640,371109888,366940160,379387904,368119808,371728384,372416512,369967104,369332224,377061376,385376256,369225728,369848320,389013504,372367360,378388480,377720832,387248128,370417664,381558784,370139136,382668800,367198208,371838976,367915008,367620096,371101696,369205248,373731328,369627136,369651712,381599744,390742016,371814400,371048448,362299392,368099328,379396096,369119232,367681536,366387200,366268416,366825472,368615424,380604416,376455168,367116288,364990464,368521216,366899200,366141440,369451008,379838464,373055488,369520640,393150464,384552960,366624768,368615424,368406528,366325760,365395968,380497920,377622528,379215872,379101184,370495488,373059584,371806208,371269632,387858432,385708032,371236864,370954240,379895808,381620224,372211712,394719232,369860608,372924416,371527680,370757632,368431104,390426624,369766400,379793408,369954816,387121152,387162112,381812736,379297792,382279680,371814400,372318208,371326976,371146752,373252096,369623040,385957888,380936192,385626112,384794624,366374912,389472256,380719104,380309504,368304128,371027968,388665344,370974720,384122880,379248640,372711424,389300224,393555968,387596288,371970048,391942144,372527104,369471488,385052672,385064960,394190848,385081344,372797440,369909760,372535296,369557504,383819776,372375552,375762944,369844224,384839680,388796416,371859456,375693312,372879360,370106368,379752448,373714944,370425856,372535296,378732544,372543488,371085312,371208192,370233344,385236992,378343424,385323008,374513664,391991296,371798016,375418880,382160896,393342976,397946880,395812864,374747136,376135680,373137408,393773056 };
    }

    public void runIteration() {
        // var @params = new double[] { 2.5, 500 };
        const double segmentCountWeight = 2.5;
        const int gridLength = 500;
        var data = new [] { d1, d2 };

        var promises = new List<int[]>();
        foreach (var d in data) {
            // _invokeSegmentationAlgorithm (string segmentationName, object[] parameters, object[] timeSeriesValues)
            // var args = [timeSeriesValues].concat(parameters || []);
            // var promise = Task.Run(() => {
            //    return Statistics.segmentTimeSeriesByMaximizingSchwarzCriterion(d, segmentCountWeight, gridLength);
            //});
            var result = Statistics.segmentTimeSeriesByMaximizingSchwarzCriterion(d, segmentCountWeight, gridLength);
            promises.Add(result);
        }

        var expectedResults = new[] {
            new int [] { 0, 97, 174, 211, 301, 1270, 1278, 2696, 3613, 3700 },
            new int [] { 0, 175, 1194, 1237, 2310 }
        };

        for (var i = 0; i < promises.Count; ++i) {
            var result = promises[i];
            if (result.ToString() != expectedResults[i].ToString())
                throw new Exception("Bad result");
        }
    }

    public void reset() {
    }
};
