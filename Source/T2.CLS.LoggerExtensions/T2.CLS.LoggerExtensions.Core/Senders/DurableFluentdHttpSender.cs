// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using T2.CLS.LoggerExtensions.Core.Buffer;
using T2.CLS.LoggerExtensions.Core.Formatters;

namespace T2.CLS.LoggerExtensions.Core.Senders
{
	public class DurableFluentdHttpSender : TransportBase
	{
		#region Static Fields and Constants

		private const string ContentType = "application/json";
		private const int ReadLimitDefault = 1024;
		private const int ReadLimitMin = 64;
		private const int ReadLimitMax = 4096;
		
		#endregion

		#region Fields

		private readonly Buffer.Buffer _buffer;
		private readonly HttpClient _client;
		private readonly string[] _requestUri;
		private bool _disposed;
		private int _iRequestUri;

		#endregion
		
		#region Ctors

		public DurableFluentdHttpSender(string[] requestUris,
			HttpClient client = null,
			string bufferPath = null,
			int? memoryBufferLimit = null,
			int? fileBufferLimit = null,
			int? fluentBufferLimit = null,
			double? flushTimeout = null,
			int workerCount = 1,
			Encoding encoding = null,
			ILoggerFactory internalLoggerFactory = null)
		{
			var readLimit = Math.Min(Math.Max(fluentBufferLimit ?? ReadLimitDefault, ReadLimitMin), ReadLimitMax);

			_client = client ?? new HttpClient();
			_requestUri = requestUris;
			_buffer = new FileBuffer(GetBufferPath(bufferPath ?? "LogBuffer"), JsonFormatter.Instance, HandleBuffer, readLimit, memoryBufferLimit, fileBufferLimit, flushTimeout, workerCount, encoding, internalLoggerFactory);
		}

		#endregion

		#region  Methods

		protected override void DisposeCore(bool disposing)
		{
			base.DisposeCore(disposing);

			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
				_buffer.Dispose();
		}

		private static string GetBufferPath(string bufferPath)
		{
			return Path.IsPathRooted(bufferPath) ? bufferPath : Path.Combine(AppContext.BaseDirectory, bufferPath);
		}

		protected virtual void HandleBuffer(IReadOnlyList<string> bufferBlock)
		{
			var stringBuilder = new StringBuilder();
			var textWriter = new StringWriter(stringBuilder);

			var c = '[';

			foreach (var line in bufferBlock)
			{
				textWriter.Write(c);
				textWriter.Write(line);
				c = ',';
			}

			textWriter.Write(']');
			textWriter.Close();

			var content = stringBuilder.ToString();
			using var stringContent = new StringContent(content, Encoding.UTF8, ContentType);

			var requestUriIndex = Interlocked.Increment(ref _iRequestUri) % _requestUri.Length;
			var requestUri = _requestUri[requestUriIndex];

			var postTask = _client.PostAsync(requestUri, stringContent);
			var result = postTask.Result;

			if (result.IsSuccessStatusCode == false)
				throw new Exception(result.Content.ReadAsStringAsync().Result);
		}

		protected override void SendCore(LogEvent logEvent)
		{
			_buffer.Write(logEvent);
		}

		#endregion
	}
}