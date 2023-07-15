using Volo.Abp.Domain.Entities;

namespace SNA.Entities
{
    public class DatasetItem : Entity<Guid>
    {
        public string Name { get; set; }
        public int UserOne { get; set; }
        public int UserTwo { get; set; }

        private DatasetItem()
        {

        }
        public DatasetItem(string name, int userOne, int userTwo)
        {
            Name = name;
            UserOne = userOne;
            UserTwo = userTwo;
        }


    }
}
