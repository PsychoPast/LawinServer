using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace LawinServer.Core
{
    internal class AppRegistry : IDisposable
    {
        public List<RegistryInfos> RegistryInfo { private get; set; }

        private readonly RegistryKey _registryKey;

        private const string AppKey = @"SOFTWARE\LawinServer";

        private readonly RegistryKey currentUser = Registry.CurrentUser;

        private RegistryKey OpenKey => currentUser.OpenSubKey(AppKey, RegistryKeyPermissionCheck.ReadWriteSubTree);

        public AppRegistry() => _registryKey = OpenKey switch
        {
            null => currentUser.CreateSubKey(AppKey, RegistryKeyPermissionCheck.ReadWriteSubTree),
            _ => currentUser.OpenSubKey(AppKey, RegistryKeyPermissionCheck.ReadWriteSubTree)
        };

        public void UpdateRegistry() => _registryKey.SetValues(RegistryInfo);

        public void Dispose()
        {
            _registryKey.Close();
            _registryKey.Dispose();
            GC.SuppressFinalize(this);
        }

        public T GetRegistryValue<T>(string name) => (T)_registryKey.GetValue(name, null);
    }

    internal class RegistryInfos
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public RegistryValueKind RegistryValueKind { get; set; }
    }
}