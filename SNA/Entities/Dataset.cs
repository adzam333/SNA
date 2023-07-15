using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace SNA.Entities
{
    public class Dataset : BasicAggregateRoot<string>
    {
        [Key]
        public string Name { get; set; }
        public int AverageConnections { get; set; }
        public int UserCount { get; set; }
        public string HashedData { get; set; }

        private Dataset()
        {

        }
        public Dataset(string name, int averageConnections, int userCount, string hashedData)
        {
            Name = name;
            AverageConnections = averageConnections;
            UserCount = userCount;
            HashedData = hashedData;

        }
    }
}
