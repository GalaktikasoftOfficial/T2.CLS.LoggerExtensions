using System;
using System.Collections.Generic;
using System.Text;

namespace T2.CLS.LoggerExtensions.Core.Config
{
	public class BufferSettingsConfig
	{
		#region properties
		
		public string? BufferPath { get; set; }

		public int? ReadLimit { get; set; } = 4 * 1024;

		public int? WorkerCount { get; set; } = 4;

		public int? MemoryBufferLimit { get; set; } = 512;

		public int? FileBufferLimit { get; set; } = 512 * 16;

		public double? FlushTimeout { get; set; } = 3000;

		public double? ResendTimeout { get; set; } = 5000;

		public double[] ResendIntervals { get; set; } = null;

		public Encoding Encoding { get; set; } = null;

		#endregion // properties

		#region Override methods 

		public override string ToString()
		{
			return
				$"{{  {Environment.NewLine} " +
				$"	BufferPath:{BufferPath}; {Environment.NewLine}" +
				$"	ReadLimit: {ReadLimit};  {Environment.NewLine}" +
				$"	WorkerCount: {WorkerCount};  {Environment.NewLine} " +
				$"	MemoryBufferLimit:{MemoryBufferLimit};  {Environment.NewLine} " +
				$"	FileBufferLimit: {FileBufferLimit};  {Environment.NewLine} " +
				$"	FlushTimeout: {FlushTimeout};  {Environment.NewLine} " +
				$"	ResendTimeout: {ResendTimeout};  {Environment.NewLine} " +
				$"	Encoding: {Encoding}}}";
		}
		 
		#endregion

		#region static methods

		public void JoinWithParentBufferConfig(BufferSettingsConfig parentConfig)
		{
			JoinBufferConfig(parentConfig, this);
		}

		public static void JoinBufferConfig(BufferSettingsConfig parentConfig, BufferSettingsConfig childConfig)
		{
			if (parentConfig == null) return;
			if (childConfig == null)
			{
				childConfig = parentConfig;
				return;
			}

			childConfig.BufferPath = childConfig.BufferPath ?? parentConfig.BufferPath;
			childConfig.ReadLimit = childConfig.ReadLimit ?? parentConfig.ReadLimit;
			childConfig.WorkerCount = childConfig.WorkerCount ?? parentConfig.WorkerCount;
			childConfig.MemoryBufferLimit = childConfig.MemoryBufferLimit ?? parentConfig.MemoryBufferLimit;
			childConfig.FileBufferLimit = childConfig.FileBufferLimit ?? parentConfig.FileBufferLimit;
			childConfig.FlushTimeout = childConfig.FlushTimeout ?? parentConfig.FlushTimeout;
			childConfig.ResendTimeout = childConfig.ResendTimeout ?? parentConfig.ResendTimeout;
			childConfig.ResendIntervals = childConfig.ResendIntervals ?? parentConfig.ResendIntervals;
			childConfig.Encoding = childConfig.Encoding ?? parentConfig.Encoding;
		}
		
		#endregion //static methods
	}
}
