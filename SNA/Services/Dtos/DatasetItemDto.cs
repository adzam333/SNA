namespace SNA.Services.Dtos
{
    public class DatasetItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int UserOne { get; set; }
        public int UserTwo { get; set; }

        private DatasetItemDto()
        {

        }
        public DatasetItemDto(string name, int userOne, int userTwo)
        {
            Name = name;
            UserOne = userOne;
            UserTwo = userTwo;
        }
    }
}
