using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace LawinServer.Net
{
    public class PrivateKeyDeleters
    {
        private readonly IDictionary<Type, Action<AsymmetricAlgorithm>> privateKeyDeleters = new Dictionary<Type, Action<AsymmetricAlgorithm>>();

        public PrivateKeyDeleters()
        {
            AddPrivateKeyDeleter<RSACng>(DefaultRSACngPrivateKeyDeleter);
            AddPrivateKeyDeleter<RSACryptoServiceProvider>(DefaultRSACryptoServiceProviderPrivateKeyDeleter);
        }

        private void AddPrivateKeyDeleter<T>(Action<T> keyDeleter) where T : AsymmetricAlgorithm => privateKeyDeleters[typeof(T)] = (a) => keyDeleter((T)a);

        public void DeletePrivateKey(AsymmetricAlgorithm asymmetricAlgorithm)
        {
            for (Type type = asymmetricAlgorithm.GetType(); type != null; type = type.BaseType)
            {
                if (privateKeyDeleters.TryGetValue(type, out Action<AsymmetricAlgorithm> deleter))
                {
                    deleter(asymmetricAlgorithm);
                    return;
                }
            }
        }

        private void DefaultRSACryptoServiceProviderPrivateKeyDeleter(RSACryptoServiceProvider rsaCryptoServiceProvider)
        {
            rsaCryptoServiceProvider.PersistKeyInCsp = false;
            rsaCryptoServiceProvider.Clear();
        }

        private void DefaultRSACngPrivateKeyDeleter(RSACng rsaCng)
        {
            rsaCng.Key.Delete();
            rsaCng.Clear();
        }
    }
}