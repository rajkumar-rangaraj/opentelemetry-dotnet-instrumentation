// <copyright file="MetricSettings.cs" company="OpenTelemetry Authors">
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
using OpenTelemetry.Exporter;

namespace OpenTelemetry.AutoInstrumentation.Configuration;
// TODO Move settings to more suitable place?

/// <summary>
/// Settings
/// </summary>
public class MetricSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetricSettings"/> class
    /// using the specified <see cref="IConfigurationSource"/> to initialize values.
    /// </summary>
    /// <param name="source">The <see cref="IConfigurationSource"/> to use when retrieving configuration values.</param>
    private MetricSettings(IConfigurationSource source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        MetricExporter = ParseMetricExporter(source);
        OtlpExportProtocol = GetExporterOtlpProtocol(source);

        ConsoleExporterEnabled = source.GetBool(ConfigurationKeys.ConsoleExporterEnabled) ?? false;

        var providerPlugins = source.GetString(ConfigurationKeys.ProviderPlugins);
        if (providerPlugins != null)
        {
            foreach (var pluginAssemblyQualifiedName in providerPlugins.Split(':'))
            {
                MetricPlugins.Add(pluginAssemblyQualifiedName);
            }
        }

        MetricsEnabled = source.GetBool(ConfigurationKeys.MetricEnabled) ?? true;
        LoadMetricsAtStartup = source.GetBool(ConfigurationKeys.LoadMetricAtStartup) ?? true;

        Integrations = new IntegrationSettingsCollection(source);

        Http2UnencryptedSupportEnabled = source.GetBool(ConfigurationKeys.Http2UnencryptedSupportEnabled) ?? false;
    }

    /// <summary>
    /// Gets a value indicating whether tracing is enabled.
    /// Default is <c>true</c>.
    /// </summary>
    /// <seealso cref="ConfigurationKeys.TraceEnabled"/>
    public bool MetricsEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether the tracer should be loaded by the profiler. Default is true.
    /// </summary>
    public bool LoadMetricsAtStartup { get; }

    /// <summary>
    /// Gets the traces exporter.
    /// </summary>
    public MetricExporter MetricExporter { get; }

    /// <summary>
    /// Gets the the OTLP transport protocol. Supported values: Grpc and HttpProtobuf.
    /// </summary>
    public OtlpExportProtocol? OtlpExportProtocol { get; }

    /// <summary>
    /// Gets a value indicating whether the console exporter is enabled.
    /// </summary>
    public bool ConsoleExporterEnabled { get; }

    /// <summary>
    /// Gets the list of plugins represented by <see cref="Type.AssemblyQualifiedName"/>.
    /// </summary>
    public IList<string> MetricPlugins { get; } = new List<string>();

     /// <summary>
    /// Gets a collection of <see cref="Integrations"/> keyed by integration name.
    /// </summary>
    public IntegrationSettingsCollection Integrations { get; }

    /// <summary>
    /// Gets a value indicating whether the `System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport`
    /// should be enabled.
    /// It is required by OTLP gRPC exporter on .NET Core 3.x.
    /// Default is <c>false</c>.
    /// </summary>
    public bool Http2UnencryptedSupportEnabled { get; }

    /// <summary>
    /// Gets the list of enabled meters.
    /// </summary>
    public IList<Meter> EnabledMeters { get; }

    internal static MetricSettings FromDefaultSources()
    {
        var configurationSource = new CompositeConfigurationSource
        {
            new EnvironmentConfigurationSource(),

#if NETFRAMEWORK
            // on .NET Framework only, also read from app.config/web.config
            new NameValueConfigurationSource(System.Configuration.ConfigurationManager.AppSettings)
#endif
        };

        return new MetricSettings(configurationSource);
    }

    private static OtlpExportProtocol? GetExporterOtlpProtocol(IConfigurationSource source)
    {
        // the default in SDK is grpc. http/protobuf should be default for our purposes
        var exporterOtlpProtocol = source.GetString(ConfigurationKeys.ExporterOtlpProtocol);

        if (string.IsNullOrEmpty(exporterOtlpProtocol))
        {
            // override settings only for http/protobuf
            return OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        }

        // null value here means that it will be handled by OTEL .NET SDK
        return null;
    }

    private static MetricExporter ParseMetricExporter(IConfigurationSource source)
    {
        var tracesExporterEnvVar = source.GetString(ConfigurationKeys.MetricsExporter) ?? "otlp";
        switch (tracesExporterEnvVar)
        {
            case null:
            case "":
            case "otlp":
                return MetricExporter.Otlp;
            case "zipkin":
                return MetricExporter.Zipkin;
            case "jaeger":
                return MetricExporter.Jaeger;
            case "none":
                return MetricExporter.None;
            default:
                throw new FormatException($"Metric exporter '{tracesExporterEnvVar}' is not supported");
        }
    }
}
