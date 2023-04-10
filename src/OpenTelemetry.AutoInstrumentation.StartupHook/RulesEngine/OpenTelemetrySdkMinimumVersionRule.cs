// <copyright file="OpenTelemetrySdkMinimumVersionRule.cs" company="OpenTelemetry Authors">
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

using System.Diagnostics;
using System.Reflection;
using OpenTelemetry.AutoInstrumentation.Logging;

namespace OpenTelemetry.AutoInstrumentation.RulesEngine;

internal class OpenTelemetrySdkMinimumVersionRule : Rule
{
    private static IOtelLogger logger = OtelLogging.GetLogger("StartupHook");

    public OpenTelemetrySdkMinimumVersionRule()
    {
        Name = "OpenTelemetry SDK Validator";
        Description = "Ensure that the OpenTelemetry SDK version is not older than the version used by the Auto-Instrumentation";
    }

    // This constructor is used for test purpose.
    protected OpenTelemetrySdkMinimumVersionRule(IOtelLogger otelLogger)
        : this()
    {
        logger = otelLogger;
    }

    internal override bool Evaluate()
    {
        string? oTelPackageVersion = null;

        try
        {
            var loadedOTelFileVersion = GetVersionFromApp();
            if (loadedOTelFileVersion != null)
            {
                var autoInstrumentationOTelFileVersion = GetVersionFromAutoInstrumentation();
                if (loadedOTelFileVersion < autoInstrumentationOTelFileVersion)
                {
                    oTelPackageVersion = loadedOTelFileVersion.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            // Exception in evaluation should not throw or crash the process.
            logger.Information($"Rule Engine: Couldn't evaluate reference to OpenTelemetry Sdk in an app. Exception: {ex}");
            return true;
        }

        if (oTelPackageVersion != null)
        {
            logger.Error($"Rule Engine: Application has direct or indirect reference to older version of OpenTelemetry package {oTelPackageVersion}.");
            return false;
        }

        logger.Information($"Rule Engine: OpenTelemetrySdkMinimumVersionRule evaluation success.");
        return true;
    }

    protected virtual Version? GetVersionFromApp()
    {
        var openTelemetryType = Type.GetType("OpenTelemetry.Sdk, OpenTelemetry");
        if (openTelemetryType != null)
        {
            var loadedOTelAssembly = Assembly.GetAssembly(openTelemetryType);
            var loadedOTelFileVersionInfo = FileVersionInfo.GetVersionInfo(loadedOTelAssembly?.Location);
            var loadedOTelFileVersion = new Version(loadedOTelFileVersionInfo.FileVersion);

            return loadedOTelFileVersion;
        }

        return null;
    }

    protected virtual Version? GetVersionFromAutoInstrumentation()
    {
        var autoInstrumentationOTelLocation = Path.Combine(StartupHook.LoaderAssemblyLocation ?? string.Empty, "OpenTelemetry.dll");
        var autoInstrumentationOTelFileVersionInfo = FileVersionInfo.GetVersionInfo(autoInstrumentationOTelLocation);
        var autoInstrumentationOTelFileVersion = new Version(autoInstrumentationOTelFileVersionInfo.FileVersion);

        return autoInstrumentationOTelFileVersion;
    }
}
