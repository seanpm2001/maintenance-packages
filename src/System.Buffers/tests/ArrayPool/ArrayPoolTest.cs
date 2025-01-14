﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.RemoteExecutor;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;

namespace System.Buffers.ArrayPool.Tests
{
    public abstract class ArrayPoolTest
    {
        protected const string TrimSwitchName = "DOTNET_SYSTEM_BUFFERS_ARRAYPOOL_TRIMSHARED";

        protected static class EventIds
        {
            public const int BufferRented = 1;
            public const int BufferAllocated = 2;
            public const int BufferReturned = 3;
            public const int BufferTrimmed = 4;
            public const int BufferTrimPoll = 5;
        }

        protected static int RunWithListener(Action body, EventLevel level, Action<EventWrittenEventArgs> callback)
        {
            using (TestEventListener listener = new TestEventListener("System.Buffers.ArrayPoolEventSource", level))
            {
                int count = 0;
                listener.RunWithCallback(e =>
                {
                    Interlocked.Increment(ref count);
                    callback(e);
                }, body);
                return count;
            }
        }

        protected static void RemoteInvokeWithTrimming(Action method, int timeout = RemoteExecutor.FailWaitTimeoutMilliseconds)
        {
            RemoteInvokeOptions options = new RemoteInvokeOptions
            {
                TimeOut = timeout
            };

            options.StartInfo.UseShellExecute = false;
            options.StartInfo.EnvironmentVariables.Add(TrimSwitchName, true.ToString());

            RemoteExecutor.Invoke(method, options).Dispose();
        }
    }
}
