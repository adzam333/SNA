namespace SNA.Services.Dtos
{
    public class DatasetDto
    {
        public string Name { get; set; }
        public int AverageConnections { get; set; }
        public int UserCount { get; set; }
        public string HashedData { get; set; }

        private DatasetDto()
        {

        }
        public DatasetDto(string name, int averageConnections, int userCount, string hashedData)
        {
            Name = name;
            AverageConnections = averageConnections;
            UserCount = userCount;
            HashedData = hashedData;

        }
    }
}
