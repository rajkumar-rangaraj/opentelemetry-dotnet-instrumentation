// <copyright file="OtelSdkPlugin.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Examples.AspNetCoreMvc;

public class OtelSdkPlugin
{
    public TracerProviderBuilder ConfigureTracerProvider(TracerProviderBuilder builder)
    {
        var typeName = this.ToString();
        Console.WriteLine($"Hello from Tracer Plugin - {typeName}");
        return builder;
    }

    public MeterProviderBuilder ConfigureMeterProvider(MeterProviderBuilder builder)
    {
        var typeName = this.ToString();
        Console.WriteLine($"Hello from Meter Plugin - {typeName}");
        return builder;
    }

    public OpenTelemetryLoggerOptions ConfigureLoggerOptions(OpenTelemetryLoggerOptions builder)
    {
        var typeName = this.ToString();
        Console.WriteLine($"Hello from Logs Plugin - {typeName}");
        return builder;
    }
}
