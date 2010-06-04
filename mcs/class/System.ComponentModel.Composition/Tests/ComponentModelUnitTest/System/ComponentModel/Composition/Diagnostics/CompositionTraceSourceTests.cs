// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.UnitTesting;
using System.Collections.Generic;

namespace System.ComponentModel.Composition.Diagnostics
{
    [TestClass]
    public class ComposableTraceSourceTests
    {
#if !SILVERLIGHT
        [TestMethod]
        public void CanWriteInformation_ShouldReturnFalseByDefault()
        {
            Assert.IsFalse(CompositionTraceSource.CanWriteInformation);
        }

        [TestMethod]
        public void CanWriteWarning_ShouldReturnTrueByDefault()
        {
            Assert.IsTrue(CompositionTraceSource.CanWriteWarning);
        }

        [TestMethod]
        public void CanWriteError_ShouldReturnTrueByDefault()
        {
            Assert.IsTrue(CompositionTraceSource.CanWriteError);
        }

        [TestMethod]
        public void CanWriteInformation_WhenSwitchLevelLessThanInformation_ShouldReturnFalse()
        {
            var levels = GetSourceLevelsLessThan(SourceLevels.Information);
            foreach (var level in levels)
            {
                using (new TraceContext(level))
                {
                    Assert.IsFalse(CompositionTraceSource.CanWriteInformation);
                }
            }
        }

        [TestMethod]
        public void CanWriteInformation_WhenSwitchLevelGreaterThanOrEqualToInformation_ShouldReturnTrue()
        {
            var levels = GetSourceLevelsGreaterThanOrEqualTo(SourceLevels.Information);
            foreach (var level in levels)
            {
                using (new TraceContext(level))
                {
                    Assert.IsTrue(CompositionTraceSource.CanWriteInformation);
                }
            }
        }

        [TestMethod]
        public void CanWriteWarning_WhenSwitchLevelLessThanWarning_ShouldReturnFalse()
        {
            var levels = GetSourceLevelsLessThan(SourceLevels.Warning);
            foreach (var level in levels)
            {
                using (new TraceContext(level))
                {
                    Assert.IsFalse(CompositionTraceSource.CanWriteWarning);
                }
            }
        }

        [TestMethod]
        public void CanWriteWarning_WhenSwitchLevelGreaterThanOrEqualToWarning_ShouldReturnTrue()
        {
            var levels = GetSourceLevelsGreaterThanOrEqualTo(SourceLevels.Warning);
            foreach (var level in levels)
            {
                using (new TraceContext(level))
                {
                    Assert.IsTrue(CompositionTraceSource.CanWriteWarning);
                }
            }
        }

        [TestMethod]
        public void CanWriteError_WhenSwitchLevelLessThanError_ShouldReturnFalse()
        {
            var levels = GetSourceLevelsLessThan(SourceLevels.Error);
            foreach (var level in levels)
            {
                using (new TraceContext(level))
                {
                    Assert.IsFalse(CompositionTraceSource.CanWriteError);
                }
            }
        }

        [TestMethod]
        public void CanWriteError_WhenSwitchLevelGreaterThanOrEqualToError_ShouldReturnTrue()
        {
            var levels = GetSourceLevelsGreaterThanOrEqualTo(SourceLevels.Error);
            foreach (var level in levels)
            {
                using (new TraceContext(level))
                {
                    Assert.IsTrue(CompositionTraceSource.CanWriteError);
                }
            }
        }

        [TestMethod]
        public void WriteInformation_WhenSwitchLevelLessThanInformation_ShouldThrowInternalError()
        {
            var levels = GetSourceLevelsLessThan(SourceLevels.Information);
            foreach (var level in levels)
            {
                using (TraceContext context = new TraceContext(level))
                {
                    ThrowsInternalError(() =>
                    {
                        CompositionTraceSource.WriteInformation(0, "format", "arguments");
                    });
                }
            }
        }

        [TestMethod]
        public void WriteInformation_WhenSwitchLevelGreaterThanOrEqualToInformation_ShouldWriteToTraceListener()
        {
            var levels = GetSourceLevelsGreaterThanOrEqualTo(SourceLevels.Information);
            foreach (var level in levels)
            {
                using (TraceContext context = new TraceContext(level))
                {
                    CompositionTraceSource.WriteInformation(0, "format", "arguments");

                    Assert.IsNotNull(context.LastTraceEvent);
                }
            }
        }

        [TestMethod]
        public void WriteWarning_WhenSwitchLevelLessThanWarning_ShouldThrowInternalError()
        {
            var levels = GetSourceLevelsLessThan(SourceLevels.Warning);
            foreach (var level in levels)
            {
                using (TraceContext context = new TraceContext(level))
                {
                    ThrowsInternalError(() =>
                    {
                        CompositionTraceSource.WriteWarning(0, "format", "arguments");
                    });
                }
            }
        }

        [TestMethod]
        public void WriteWarning_WhenSwitchLevelGreaterThanOrEqualToWarning_ShouldWriteToTraceListener()
        {
            var levels = GetSourceLevelsGreaterThanOrEqualTo(SourceLevels.Information);
            foreach (var level in levels)
            {
                using (TraceContext context = new TraceContext(level))
                {
                    CompositionTraceSource.WriteWarning(0, "format", "arguments");

                    Assert.IsNotNull(context.LastTraceEvent);
                }
            }
        }

        [TestMethod]
        public void WriteError_WhenSwitchLevelLessThanError_ShouldThrowInternalError()
        {
            var levels = GetSourceLevelsLessThan(SourceLevels.Error);
            foreach (var level in levels)
            {
                using (TraceContext context = new TraceContext(level))
                {
                    ThrowsInternalError(() =>
                    {
                        CompositionTraceSource.WriteError(0, "format", "arguments");
                    });
                }
            }
        }

        [TestMethod]
        public void WriteError_WhenSwitchLevelGreaterThanOrEqualToError_ShouldWriteToTraceListener()
        {
            var levels = GetSourceLevelsGreaterThanOrEqualTo(SourceLevels.Error);
            foreach (var level in levels)
            {
                using (TraceContext context = new TraceContext(level))
                {
                    CompositionTraceSource.WriteError(0, "format", "arguments");

                    Assert.IsNotNull(context.LastTraceEvent);
                }
            }
        }

        [TestMethod]
        public void WriteInformation_ShouldWriteTraceEventTypeInformationToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Information))
            {
                CompositionTraceSource.WriteInformation(0, "format", "arguments");

                Assert.AreEqual(TraceEventType.Information, context.LastTraceEvent.EventType);
            }
        }

        [TestMethod]
        public void WriteWarning_ShouldWriteTraceEventTypeWarningToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Warning))
            {
                CompositionTraceSource.WriteWarning(0, "format", "arguments");

                Assert.AreEqual(TraceEventType.Warning, context.LastTraceEvent.EventType);
            }
        }

        [TestMethod]
        public void WriteError_ShouldWriteTraceEventTypeWarningToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Error))
            {
                CompositionTraceSource.WriteError(0, "format", "arguments");

                Assert.AreEqual(TraceEventType.Error, context.LastTraceEvent.EventType);
            }
        }

        [TestMethod]
        public void WriteInformation_ShouldWriteCorrectSourceToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Information))
            {
                CompositionTraceSource.WriteInformation(0, "format", "arguments");

                Assert.AreEqual("System.ComponentModel.Composition", context.LastTraceEvent.Source);
            }
        }

        [TestMethod]
        public void WriteWarning_ShouldWriteCorrectSourceToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Warning))
            {
                CompositionTraceSource.WriteWarning(0, "format", "arguments");

                Assert.AreEqual("System.ComponentModel.Composition", context.LastTraceEvent.Source);
            }
        }

        [TestMethod]
        public void WriteError_ShouldWriteCorrectSourceToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Error))
            {
                CompositionTraceSource.WriteError(0, "format", "arguments");

                Assert.AreEqual("System.ComponentModel.Composition", context.LastTraceEvent.Source);
            }
        }

        [TestMethod]
        public void WriteInformation_ValueAsTraceId_ShouldWriteIdToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Information))
            {
                var expectations = Expectations.GetEnumValues<CompositionTraceId>();
                foreach (var e in expectations)
                {
                    CompositionTraceSource.WriteInformation(e, "format", "arguments");

                    Assert.AreEqual(e, (CompositionTraceId)context.LastTraceEvent.Id);
                }
            }
        }

        [TestMethod]
        public void WriteWarning_ValueAsTraceId_ShouldWriteIdToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Warning))
            {
                var expectations = Expectations.GetEnumValues<CompositionTraceId>();
                foreach (var e in expectations)
                {
                    CompositionTraceSource.WriteWarning(e, "format", "arguments");

                    Assert.AreEqual(e, (CompositionTraceId)context.LastTraceEvent.Id);
                }
            }
        }

        [TestMethod]
        public void WriteError_ValueAsTraceId_ShouldWriteIdToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Error))
            {
                var expectations = Expectations.GetEnumValues<CompositionTraceId>();
                foreach (var e in expectations)
                {
                    CompositionTraceSource.WriteError(e, "format", "arguments");

                    Assert.AreEqual(e, (CompositionTraceId)context.LastTraceEvent.Id);
                }
            }
        }

        [TestMethod]
        public void WriteInformation_ValueAsFormat_ShouldWriteFormatToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Information))
            {
                var expectations = Expectations.GetExceptionMessages();
                foreach (var e in expectations)
                {
                    CompositionTraceSource.WriteInformation(0, e, "arguments");

                    Assert.AreEqual(e, context.LastTraceEvent.Format);
                }
            }
        }

        [TestMethod]
        public void WriteWarning_ValueAsFormat_ShouldWriteFormatToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Warning))
            {
                var expectations = Expectations.GetExceptionMessages();
                foreach (var e in expectations)
                {
                    CompositionTraceSource.WriteWarning(0, e, "arguments");

                    Assert.AreEqual(e, context.LastTraceEvent.Format);
                }
            }
        }

        [TestMethod]
        public void WriteError_ValueAsFormat_ShouldWriteFormatToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Error))
            {
                var expectations = Expectations.GetExceptionMessages();
                foreach (var e in expectations)
                {
                    CompositionTraceSource.WriteError(0, e, "arguments");

                    Assert.AreEqual(e, context.LastTraceEvent.Format);
                }
            }
        }

        [TestMethod]
        public void WriteInformation_ValueAsArgs_ShouldWriteArgsToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Information))
            {
                var expectations = Expectations.GetObjectArraysWithNull();
                foreach (var e in expectations)
                {
                    CompositionTraceSource.WriteInformation(0, "format", e);

                    Assert.AreSame(e, context.LastTraceEvent.Args);
                }
            }
        }

        [TestMethod]
        public void WriteWarning_ValueAsArgs_ShouldWriteArgsToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Warning))
            {
                var expectations = Expectations.GetObjectArraysWithNull();
                foreach (var e in expectations)
                {
                    CompositionTraceSource.WriteWarning(0, "format", e);

                    Assert.AreSame(e, context.LastTraceEvent.Args);
                }
            }
        }

        [TestMethod]
        public void WriteError_ValueAsArgs_ShouldWriteArgsToTraceListener()
        {
            using (var context = new TraceContext(SourceLevels.Error))
            {
                var expectations = Expectations.GetObjectArraysWithNull();
                foreach (var e in expectations)
                {
                    CompositionTraceSource.WriteError(0, "format", e);

                    Assert.AreSame(e, context.LastTraceEvent.Args);
                }
            }
        }

        private static IEnumerable<SourceLevels> GetSourceLevelsLessThan(SourceLevels level)
        {
            return GetOnSourceLevels(level, false);
        }

        private static IEnumerable<SourceLevels> GetSourceLevelsGreaterThanOrEqualTo(SourceLevels level)
        {
            return GetOnSourceLevels(level, true);
        }

        private static IEnumerable<SourceLevels> GetOnSourceLevels(SourceLevels sourceLevel, bool on)
        {
            // SourceSwitch determines if a particular level gets traced based on whether its bit is
            // set in the current level. For example, if the current level was Warning (0000 0111),
            // then Warning (0000 0111), Error (0000 0011), and Critical (0000 0001) all get traced.

            var levels = TestServices.GetEnumValues<SourceLevels>();

            foreach (var level in levels)
            {
                if (level == 0)
                    continue;
                
                if (((level & sourceLevel) == sourceLevel) == on)
                {
                    yield return level;
                }
            }

            if (!on)
            {   
                yield return SourceLevels.Off;
            }
        }

        private static void ThrowsInternalError(Action action)
        {
            try
            {
                action();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Type exceptionType = ex.GetType();

                // The exception should not be a 
                // publicily catchable exception
                Assert.IsFalse(exceptionType.IsVisible);
            }
        }
#endif

#if SILVERLIGHT
        [TestMethod]
        public void CanWriteInformation_ShouldReturnFalse()
        {
            Assert.IsFalse(CompositionTraceSource.CanWriteInformation);
        }

        [TestMethod]
        public void CanWriteWarning_ShouldReturnDebuggerLogging()
        {
            Assert.AreEqual(CompositionTraceSource.CanWriteWarning, Debugger.IsLogging());
        }

        [TestMethod]
        public void CanWriteError_ShouldReturnDebuggerLogging()
        {
            Assert.AreEqual(CompositionTraceSource.CanWriteError, Debugger.IsLogging());
        }

        [TestMethod]
        public void CreateLogMessage_ContainsTraceEventType()
        {
            IEnumerable<SilverlightTraceWriter.TraceEventType> eventTypes = Expectations.GetEnumValues<SilverlightTraceWriter.TraceEventType>();
            
            foreach(var eventType in eventTypes)
            {
                string message = SilverlightTraceWriter.CreateLogMessage(eventType, CompositionTraceId.Discovery_AssemblyLoadFailed, "Format");
                Assert.IsTrue(message.Contains(eventType.ToString()), "Should contain enum string of EventType");
            }            
        }

        [TestMethod]
        public void CreateLogMessage_ContainsTraceIdAsInt()
        {
            IEnumerable<CompositionTraceId> traceIds = Expectations.GetEnumValues<CompositionTraceId>();
            
            foreach(var traceId in traceIds)
            {
                string message = SilverlightTraceWriter.CreateLogMessage(SilverlightTraceWriter.TraceEventType.Information, traceId, "Format");
                Assert.IsTrue(message.Contains(((int)traceId).ToString()), "Should contain int version of TraceId");
            }            
        }

        [TestMethod]
        public void CreateLogMessage_FormatNull_ThrowsArugmentNull()
        {
            ExceptionAssert.ThrowsArgumentNull("format", () =>
                SilverlightTraceWriter.CreateLogMessage(SilverlightTraceWriter.TraceEventType.Information, CompositionTraceId.Discovery_AssemblyLoadFailed, null));
        }

        [TestMethod]
        public void CreateLogMessage_ArgumentsNull_ShouldCreateValidString()
        {
            string message = SilverlightTraceWriter.CreateLogMessage(SilverlightTraceWriter.TraceEventType.Information, CompositionTraceId.Discovery_AssemblyLoadFailed, "Format", null);

            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        [TestMethod]
        public void CreateLogMessage_ArgumentsPassed_ShouldCreateValidString()
        {
            string message = SilverlightTraceWriter.CreateLogMessage(SilverlightTraceWriter.TraceEventType.Information, CompositionTraceId.Discovery_AssemblyLoadFailed, "{0}", 9999);

            Assert.IsTrue(message.Contains("9999"));
        }
#endif

    }
}
