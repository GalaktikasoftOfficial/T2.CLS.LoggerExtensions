using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using T2.CLS.LoggerExtensions.Core.Properties;

namespace T2.CLS.LoggerExtensions.Core.Extensions
{
    public static class LoggerExtensions
    {
		//private static readonly Action<ILogger, object, object, object, object, object, object, object, object, Exception> _LEC00001_Debug_GenericFileBuffer_path_path_readLimit_readLimit_memoryBufferLimit_memoryBufferLimit_fileBufferLimit_fileBufferLimit_flushTimeout_flushTimeout_resendTimeout_resendTimeout_workerCount_workerCount_encoding_encoding;
		private static readonly Action<ILogger, object, Exception> _LEC00002_Debug_Checked_WorkerCount_value_workerCount;
		private static readonly Action<ILogger, object, Exception> _LEC00003_Debug_Checked_path_value_value;
		private static readonly Action<ILogger, object, Exception> _LEC00004_Trace_FileBufferWorker_workerIndex_workerIndex;
		private static readonly Action<ILogger, Exception> _LEC00005_Debug_GenericFileBuffer;
		private static readonly Action<ILogger, object, Exception> _LEC00006_Trace_InputParameter_path_path;
		private static readonly Action<ILogger, object, Exception> _LEC00007_Trace_InputParameter_readLimit_readLimit;
		private static readonly Action<ILogger, object, Exception> _LEC00008_Trace_InputParameter_memoryBufferLimit_memoryBufferLimit;
		private static readonly Action<ILogger, object, Exception> _LEC00009_Trace_InputParameter_fileBufferLimit_fileBufferLimit;
		private static readonly Action<ILogger, object, Exception> _LEC00010_Trace_InputParameter_flushTimeout_flushTimeout;
		private static readonly Action<ILogger, object, Exception> _LEC00011_Trace_InputParameter_resendTimeout_resendTimeout;
		private static readonly Action<ILogger, object, Exception> _LEC00012_Trace_InputParameter_workerCount_workerCount;

        static LoggerExtensions()
        {
           /* _LEC00001_Debug_GenericFileBuffer_path_path_readLimit_readLimit_memoryBufferLimit_memoryBufferLimit_fileBufferLimit_fileBufferLimit_flushTimeout_flushTimeout_resendTimeout_resendTimeout_workerCount_workerCount_encoding_encoding = LoggerMessage.Define<object, object, object, object, object, object, object, object>(
                LogLevel.Debug,
                new EventId(1, nameof(Logs.LEC00001)),
                Logs.LEC00001);*/
            _LEC00002_Debug_Checked_WorkerCount_value_workerCount = LoggerMessage.Define<object>(
                LogLevel.Debug,
                new EventId(2, nameof(Logs.LEC00002)),
                Logs.LEC00002);
            _LEC00003_Debug_Checked_path_value_value = LoggerMessage.Define<object>(
                LogLevel.Debug,
                new EventId(3, nameof(Logs.LEC00003)),
                Logs.LEC00003);
            _LEC00004_Trace_FileBufferWorker_workerIndex_workerIndex = LoggerMessage.Define<object>(
                LogLevel.Trace,
                new EventId(4, nameof(Logs.LEC00004)),
                Logs.LEC00004);
            _LEC00005_Debug_GenericFileBuffer = LoggerMessage.Define(
                LogLevel.Debug,
                new EventId(5, nameof(Logs.LEC00005)),
                Logs.LEC00005);
            _LEC00006_Trace_InputParameter_path_path = LoggerMessage.Define<object>(
                LogLevel.Trace,
                new EventId(6, nameof(Logs.LEC00006)),
                Logs.LEC00006);
            _LEC00007_Trace_InputParameter_readLimit_readLimit = LoggerMessage.Define<object>(
                LogLevel.Trace,
                new EventId(7, nameof(Logs.LEC00007)),
                Logs.LEC00007);
            _LEC00008_Trace_InputParameter_memoryBufferLimit_memoryBufferLimit = LoggerMessage.Define<object>(
                LogLevel.Trace,
                new EventId(8, nameof(Logs.LEC00008)),
                Logs.LEC00008);
            _LEC00009_Trace_InputParameter_fileBufferLimit_fileBufferLimit = LoggerMessage.Define<object>(
                LogLevel.Trace,
                new EventId(9, nameof(Logs.LEC00009)),
                Logs.LEC00009);
            _LEC00010_Trace_InputParameter_flushTimeout_flushTimeout = LoggerMessage.Define<object>(
                LogLevel.Trace,
                new EventId(10, nameof(Logs.LEC00010)),
                Logs.LEC00010);
            _LEC00011_Trace_InputParameter_resendTimeout_resendTimeout = LoggerMessage.Define<object>(
                LogLevel.Trace,
                new EventId(11, nameof(Logs.LEC00011)),
                Logs.LEC00011);
            _LEC00012_Trace_InputParameter_workerCount_workerCount = LoggerMessage.Define<object>(
                LogLevel.Trace,
                new EventId(12, nameof(Logs.LEC00012)),
                Logs.LEC00012);
        }
        
       /* /// <summary>
        /// ~GenericFileBuffer( path:'{path}', readLimit: '{readLimit}', memoryBufferLimit: '{memoryBufferLimit}, fileBufferLimit: '{fileBufferLimit}', flushTimeout: '{flushTimeout}', resendTimeout: '{resendTimeout}',  workerCount: '{workerCount}', encoding: '{encoding}'
        /// </summary>
        public static void LEC00001_Debug_GenericFileBuffer_path_path_readLimit_readLimit_memoryBufferLimit_memoryBufferLimit_fileBufferLimit_fileBufferLimit_flushTimeout_flushTimeout_resendTimeout_resendTimeout_workerCount_workerCount_encoding_encoding(this ILogger logger, object path, object readLimit, object memoryBufferLimit, object fileBufferLimit, object flushTimeout, object resendTimeout, object workerCount, object encoding)
        {
            _LEC00001_Debug_GenericFileBuffer_path_path_readLimit_readLimit_memoryBufferLimit_memoryBufferLimit_fileBufferLimit_fileBufferLimit_flushTimeout_flushTimeout_resendTimeout_resendTimeout_workerCount_workerCount_encoding_encoding(logger, path, readLimit, memoryBufferLimit, fileBufferLimit, flushTimeout, resendTimeout, workerCount, encoding, null);
        }
		*/

        /// <summary>
        /// Checked WorkerCount value: {workerCount}
        /// </summary>
        public static void LEC00002_Debug_Checked_WorkerCount_value_workerCount(this ILogger logger, object workerCount)
        {
            _LEC00002_Debug_Checked_WorkerCount_value_workerCount(logger, workerCount, null);
        }

        /// <summary>
        /// Checked path value: {value}
        /// </summary>
        public static void LEC00003_Debug_Checked_path_value_value(this ILogger logger, object value)
        {
            _LEC00003_Debug_Checked_path_value_value(logger, value, null);
        }

        /// <summary>
        /// ~FileBufferWorker(workerIndex: {workerIndex})
        /// </summary>
        public static void LEC00004_Trace_FileBufferWorker_workerIndex_workerIndex(this ILogger logger, object workerIndex)
        {
            _LEC00004_Trace_FileBufferWorker_workerIndex_workerIndex(logger, workerIndex, null);
        }

        /// <summary>
        /// ~GenericFileBuffer
        /// </summary>
        public static void LEC00005_Debug_GenericFileBuffer(this ILogger logger)
        {
            _LEC00005_Debug_GenericFileBuffer(logger, null);
        }

        /// <summary>
        /// InputParameter: path='{path}'
        /// </summary>
        public static void LEC00006_Trace_InputParameter_path_path(this ILogger logger, object path)
        {
            _LEC00006_Trace_InputParameter_path_path(logger, path, null);
        }

        /// <summary>
        /// InputParameter: readLimit='{readLimit}'
        /// </summary>
        public static void LEC00007_Trace_InputParameter_readLimit_readLimit(this ILogger logger, object readLimit)
        {
            _LEC00007_Trace_InputParameter_readLimit_readLimit(logger, readLimit, null);
        }

        /// <summary>
        /// InputParameter: memoryBufferLimit='{memoryBufferLimit}'
        /// </summary>
        public static void LEC00008_Trace_InputParameter_memoryBufferLimit_memoryBufferLimit(this ILogger logger, object memoryBufferLimit)
        {
            _LEC00008_Trace_InputParameter_memoryBufferLimit_memoryBufferLimit(logger, memoryBufferLimit, null);
        }

        /// <summary>
        /// InputParameter: fileBufferLimit='{fileBufferLimit}'
        /// </summary>
        public static void LEC00009_Trace_InputParameter_fileBufferLimit_fileBufferLimit(this ILogger logger, object fileBufferLimit)
        {
            _LEC00009_Trace_InputParameter_fileBufferLimit_fileBufferLimit(logger, fileBufferLimit, null);
        }

        /// <summary>
        /// InputParameter: flushTimeout='{flushTimeout}'
        /// </summary>
        public static void LEC00010_Trace_InputParameter_flushTimeout_flushTimeout(this ILogger logger, object flushTimeout)
        {
            _LEC00010_Trace_InputParameter_flushTimeout_flushTimeout(logger, flushTimeout, null);
        }

        /// <summary>
        /// InputParameter: resendTimeout='{resendTimeout}'
        /// </summary>
        public static void LEC00011_Trace_InputParameter_resendTimeout_resendTimeout(this ILogger logger, object resendTimeout)
        {
            _LEC00011_Trace_InputParameter_resendTimeout_resendTimeout(logger, resendTimeout, null);
        }

        /// <summary>
        /// InputParameter: workerCount='{workerCount}'
        /// </summary>
        public static void LEC00012_Trace_InputParameter_workerCount_workerCount(this ILogger logger, object workerCount)
        {
            _LEC00012_Trace_InputParameter_workerCount_workerCount(logger, workerCount, null);
        }
    }
}