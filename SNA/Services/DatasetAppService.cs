using SNA.Entities;
using SNA.Services.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace SNA.Services
{
    public class DatasetAppService : ApplicationService
    {
        private readonly IRepository<Dataset, string> _datasetRepository;
        private readonly IRepository<DatasetItem, Guid> _datasetItemRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly DatasetManager _datasetManager;
        public DatasetAppService(
            IRepository<Dataset,
                string> datasetRepository,
            IRepository<DatasetItem, Guid> datasetItemRepository,
            IUnitOfWorkManager unitOfWorkManager,
            DatasetManager datasetManager)
        {
            _datasetRepository = datasetRepository;
            _datasetItemRepository = datasetItemRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _datasetManager = datasetManager;
        }

        public async Task<List<DatasetDto>> GetListAsync()
        {
            var items = await _datasetRepository.GetListAsync();
            var itemsList = items
                .Select(dataset => new DatasetDto(
                dataset.Name,
                dataset.AverageConnections,
                dataset.UserCount,
                dataset.HashedData))
                .ToList();


            return itemsList;
        }

        public async Task<DatasetDto> GetDatasetAsync(string name)
        {
            var dataset = await _datasetRepository.FindAsync(dataset => dataset.Name == name);

            return new DatasetDto(
                dataset.Name,
                dataset.AverageConnections,
                dataset.UserCount
                , dataset.HashedData);
        }

        public async Task<DatasetDto> PopulateDatasetAsync(IFormFile file, string datasetName)
        {

            if (file == null || !file.FileName.EndsWith("txt"))
                throw new UserFriendlyException("Error loading file!");
            if (string.IsNullOrEmpty(datasetName))
                throw new UserFriendlyException("Dataset name cannot be empty!");
            if (await _datasetRepository.FindAsync(x => x.Name == datasetName) != null)
                throw new UserFriendlyException("Dataset with name '" + datasetName + "' already exists!");


            string contentString;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                contentString = await reader.ReadToEndAsync();
            }

            string[] lines = contentString.Split("\n");


            var hashedDatasetData = await _datasetManager.HashDatasetContentAsync(contentString);
            if (await _datasetRepository.FindAsync(x => x.HashedData == hashedDatasetData) != null)
                throw new UserFriendlyException("Same dataset can't be uploaded twice!");

            var datasetItemsDtoList = new List<DatasetItemDto>();
            int userOne;
            int userTwo;

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                string[] columns = line.Split(' ');
                try
                {
                    userOne = Convert.ToInt32(columns[0]);
                    userTwo = Convert.ToInt32(columns[1]);
                }
                catch (Exception)
                {
                    throw new UserFriendlyException("There was an error parsing data!");
                }
                datasetItemsDtoList.Add(new DatasetItemDto(

                        datasetName,
                        userOne,
                        userTwo
                    ));
            }


            var datasetItems = await CreateManyDatasetItemsAsync(datasetItemsDtoList);

            var averageFriendships = GetAverageFriendships(datasetItems);
            var userCount = GetNumberOfUsersInDatasetAsync(datasetItems);

            var newDatasetDto = await CreateDatasetAsync(new DatasetDto(
                datasetName,
                averageFriendships,
                userCount,
                hashedDatasetData));


            return newDatasetDto;
        }

        protected virtual async Task<DatasetDto> CreateDatasetAsync(DatasetDto datasetDto)
        {
            Dataset newDataset = await _datasetRepository.InsertAsync(new Dataset(
                datasetDto.Name,
                datasetDto.AverageConnections,
                datasetDto.UserCount,
                datasetDto.HashedData));

            return new DatasetDto(
                newDataset.Name,
                newDataset.AverageConnections,
                newDataset.UserCount,
                newDataset.HashedData);
        }

        protected virtual async Task<List<DatasetItemDto>> CreateManyDatasetItemsAsync(List<DatasetItemDto> datasetItemsDtoList)
        {
            var datasetItemsList = datasetItemsDtoList
                .Select(datasetDto => new DatasetItem(
                    datasetDto.Name,
                    datasetDto.UserOne,
                    datasetDto.UserTwo
                    )).ToList();


            const int batchSize = 1000;

            for (int i = 0; i < datasetItemsList.Count; i += batchSize)
            {
                var batchEntities = datasetItemsList.Skip(i).Take(batchSize).ToList();

                using (var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true))
                {

                    await _datasetItemRepository.InsertManyAsync(batchEntities);
                    await unitOfWork.SaveChangesAsync();
                    await unitOfWork.CompleteAsync();
                    unitOfWork.Dispose();
                }

            }
            return datasetItemsList
                .Select(datasetItem => new DatasetItemDto(
                datasetItem.Name,
                datasetItem.UserOne,
                datasetItem.UserTwo)
                {
                    Id = datasetItem.Id
                }).ToList();
        }

        protected virtual int GetNumberOfUsersInDatasetAsync(List<DatasetItemDto> datasetItemsDto)
        {
            return datasetItemsDto
                .Select(x => x.UserOne)
                .Union(datasetItemsDto
                .Select(x => x.UserTwo))
                .ToList()
                .Count;
        }

        protected virtual int GetAverageFriendships(List<DatasetItemDto> datasetItemsDto)
        {
            var friendshipsGrouped = datasetItemsDto
                .SelectMany(item => new[] { item.UserOne, item.UserTwo })
                .GroupBy(userId => userId);

            var averageFriendships = Convert.ToInt32(friendshipsGrouped
                .Average(group => group.Count()));

            return averageFriendships;
        }

    }
}
