using Microsoft.Win32;
using System.Collections.Generic;

namespace LawinServer.Core
{
    internal static class Extensions
    {
        public static void SetValues(this RegistryKey registryKey, List<RegistryInfo> registryInfos) => registryInfos
                .ForEach(x => registryKey
                .SetValue(x.Name, x.Value, x.RegistryValueKind));
    }
}