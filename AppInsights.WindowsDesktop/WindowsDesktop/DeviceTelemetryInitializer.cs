﻿namespace Microsoft.ApplicationInsights.WindowsDesktop
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.WindowsDesktop.Implementation;
    
    /// <summary>
    /// A telemetry context initializer that will gather device context information.
    /// </summary>
    public class DeviceTelemetryInitializer : ITelemetryInitializer
    {

#if !NET461
        private readonly string _operatingSystem = RuntimeInformation.OSDescription?.Replace("Microsoft ", ""); // Shorter description
        private readonly string _processArchitecture = RuntimeInformation.ProcessArchitecture.ToString();
        private readonly string _osArchitecture = RuntimeInformation.OSArchitecture.ToString();
#else
        private readonly string _operatingSystem =$"Windows {Environment.OSVersion.Version.ToString(3)}"; // Shorter description, to match the other platforms
        private readonly string _processArchitecture = Environment.Is64BitProcess ? "X64" : "X86";
        private readonly string _osArchitecture = Environment.Is64BitOperatingSystem ? "X64" : "X86";
#endif

        /// <summary>
        /// Populates device properties on a telemetry item.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null)
            {
                throw new ArgumentNullException(nameof(telemetry));
            }

            if (telemetry.Context != null && telemetry.Context.Device != null)
            {
                var reader = DeviceContextReader.Instance;
                telemetry.Context.Device.Type = reader.GetDeviceType();
                telemetry.Context.Device.OperatingSystem = _operatingSystem;

#if WINDOWS
                telemetry.Context.Device.Id = reader.GetDeviceUniqueId();
                telemetry.Context.Device.OemName = reader.GetOemName();
                telemetry.Context.Device.Model = reader.GetDeviceModel();
#endif

#if WINDOWS_UWP
                telemetry.Context.Device.Id = reader.GetDeviceUniqueId();
#endif

                // Overwrite capture of host name
                telemetry.Context.Cloud.RoleInstance = telemetry.Context.Device.Id;
                telemetry.Context.Cloud.RoleName = telemetry.Context.Device.Id;

                telemetry.Context.GlobalProperties["Network type"] = reader.GetNetworkType();
                telemetry.Context.GlobalProperties["Thread culture"] = reader.GetHostSystemLocale();
                telemetry.Context.GlobalProperties["UI culture"] = reader.GetDisplayLanguage();                
                telemetry.Context.GlobalProperties["Time zone"] = TimeZoneInfo.Local.Id;

                telemetry.Context.GlobalProperties["Process architecture"] = _processArchitecture;
                telemetry.Context.GlobalProperties["OS architecture"] = _osArchitecture;
            }
        }
    }
}
