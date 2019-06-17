using Shopanizer.DataObjects;

namespace Shopanizer.Services
{
    public interface ICredentialManager
    {
        StoredCredentials GetTokenByResource(string resource);
        void StoreCredentials(string resource, StoredCredentials storedCredentials);
    }
}