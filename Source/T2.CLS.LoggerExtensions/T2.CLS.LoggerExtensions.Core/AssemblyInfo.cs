// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System.Runtime.CompilerServices;
using T2.CLS.LoggerExtensions.Core;

#if TEST

[assembly: InternalsVisibleTo("T2.CLS.AllSystemTests, PublicKey=" + AssemblyConstants.PublicKey)]

#endif

[assembly: InternalsVisibleTo("T2.CLS.LoggerExtensions.Clickhouse, PublicKey=" + AssemblyConstants.PublicKey)]