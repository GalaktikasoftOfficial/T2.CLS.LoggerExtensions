// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Layouts;
using Serilog;
using Serilog.Debugging;
using T2.CLS.LoggerExtensions.NLog;
using T2.CLS.LoggerExtensions.Serilog;

namespace T2.CLS.LoggerExtensions.DevApp
{
	class Program
	{
		#region  Methods

		private static void ConfigureNLog()
		{
			var config = new LoggingConfiguration();
			var target = new DurableFluentdTarget
			{
				Name = "Fluentd",
				RequestUri = new List<string>
				{
					"http://by01-vm45:9880/T2.Cls.LogViewer.LoggerExtension"
				},
				MemoryBufferLimit = 256,
				FileBufferLimit = 16384,
				WorkerCount = 1,
				BufferPath = "LogBuffer",
				FlushTimeout = 5000,
				FluentBufferLimit = 512,
				Attributes =
				{
					new FluentTargetAttribute{ Name = "Attribute", Layout = new SimpleLayout{ Text = "Value"}}
				}
			};

			config.AddTarget(target);
			config.AddRuleForAllLevels(target);

			LogManager.Configuration = config;
		}

		private static void ConfigureSerilog()
		{
			SelfLog.Enable(new StreamWriter("d:\\serilog.log"));

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()
				.WriteTo.DurableFluentd(new[]{"http://by01-app21:9881/T2.Cls.LogViewer.LoggerExtension",})
				.CreateLogger();

			//Log.CloseAndFlush();
		}

		private static async Task Main(string[] args)
		{
			//ConfigureSerilog();
			//ConfigureNLog();
			//ConfigureSerilog();

			NLogParallelTest();
			//SerilogParallelTest();

			//LogManager.GetLogger("ASd").Info("ASD");

			Thread.Sleep(new TimeSpan(0, 1, 0));

			LogManager.Shutdown();
			//await NLogSimpleTest();
		}

		private static void NLogParallelTest()
		{
			Console.WriteLine(DateTime.Now);
			var logger = LogManager.GetLogger("NLogLogger");
			var count = 0;
			var logCount = 100;
			//var logCount = 1000000;

			Parallel.For(0, 10, i =>
			{
				while (true)
				{
					if (Interlocked.Increment(ref count) > logCount)
						break;

					logger.Info($"Привет от Nlog на русском языке. ThreadId: {Thread.CurrentThread.ManagedThreadId}");

					//Thread.Sleep(5);
				}
			});

			Console.WriteLine(DateTime.Now);


			LogManager.Flush();
		}

		private static void SerilogParallelTest()
		{
			Console.WriteLine(DateTime.Now);
			var logger = Log.Logger;
			var count = 0;
			var logCount = 200000;

			Parallel.For(0, 10, i =>
			{
				while (true)
				{
					if (Interlocked.Increment(ref count) > logCount)
						break;

					logger.Information($"Hi from Serilog. ThreadId: {Thread.CurrentThread.ManagedThreadId}");

					//Thread.Sleep(5);
				}
			});


			//Log.CloseAndFlush();
		}

		private static async Task NLogSimpleTest()
		{
			var logger = LogManager.GetLogger("NLogLogger");

			logger.Log(LogLevel.Info, "NLog Message");
			logger.Log(LogLevel.Info, "NLog Message");
			logger.Log(LogLevel.Info, "NLog Message");

			LogManager.Flush();
			LogManager.Shutdown();

			await Task.Delay(TimeSpan.FromSeconds(10));
		}

		#endregion
	}
}