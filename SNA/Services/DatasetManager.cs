using System.Security.Cryptography;
using Volo.Abp.Domain.Services;

namespace SNA.Services
{
    public class DatasetManager : DomainService
    {

        public virtual async Task<string> HashDatasetContentAsync(string datasetContent)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = new MemoryStream(datasetContent.GetBytes()))
                {
                    byte[] hashBytes = await sha256.ComputeHashAsync(stream);
                    string hashString = BitConverter.ToString(hashBytes);
                    return hashString;
                }
            }
        }
    }
}
