// <copyright file="EnvironmentConfigurationMetricHelper.cs" company="OpenTelemetry Authors">
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
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace OpenTelemetry.AutoInstrumentation.Configuration;

internal static class EnvironmentConfigurationMetricHelper
{
    private static readonly Dictionary<Meter, Action<MeterProviderBuilder>> AddMeters = new();

    public static MeterProviderBuilder UseEnvironmentVariables(this MeterProviderBuilder builder, MetricSettings settings)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault();

        builder
            .SetResourceBuilder(resourceBuilder)
            .SetExporter(settings);

        foreach (var enabledMeter in settings.EnabledMeters)
        {
            if (AddMeters.TryGetValue(enabledMeter, out var addMeter))
            {
                addMeter(builder);
            }
        }

        return builder;
    }

    private static MeterProviderBuilder SetExporter(this MeterProviderBuilder builder, MetricSettings settings)
    {
        builder.AddConsoleExporter();
        return builder;
    }
}
