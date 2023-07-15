using Xunit;
using SNA.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNA.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using NSubstitute;
using Shouldly;
using SNA.Services.Dtos;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SNA.Services.Tests
{
    public class DatasetAppServiceTests: ApplicationService
    {
        private readonly IRepository<Dataset, string> _datasetRepository;
        private readonly IRepository<DatasetItem, Guid> _datasetItemRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly DatasetManager _datasetManager;
        private readonly DatasetAppService _datasetAppService;
        private readonly IFormFile _validFile;
        public DatasetAppServiceTests()
        {
            _datasetRepository = Substitute.For<IRepository<Dataset, string>>();
            _datasetItemRepository = Substitute.For<IRepository<DatasetItem, Guid>>();
            _unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
            _datasetManager = Substitute.For<DatasetManager>();
            _validFile = Substitute.For<IFormFile>();
            _datasetAppService = new DatasetAppService(_datasetRepository, _datasetItemRepository, _unitOfWorkManager, _datasetManager);

            _validFile.FileName.Returns("valid.txt");
            _validFile.Length.Returns(100);
            string fileContent = "0 1\n0 2\n1 3";
            byte[] fileBytes = Encoding.UTF8.GetBytes(fileContent);
            _validFile.OpenReadStream().Returns(new MemoryStream(fileBytes));
        }

        [Fact()]
        public async Task Should_Return_DatasetDto()
        {
            //Arrange
            var name = "TestDataset";
            var dataset = new Dataset(name, 10, 100, "abc123");


            _datasetRepository.FindAsync(Arg.Any<Expression<Func<Dataset, bool>>>())
                       .Returns(dataset);
            //Act
            var datasetReturned = await _datasetAppService.GetDatasetAsync(name);
            //Assert
            datasetReturned.ShouldNotBeNull();
            datasetReturned.Name.ShouldBe(name);
            datasetReturned.AverageConnections.ShouldBe(dataset.AverageConnections);
            datasetReturned.UserCount.ShouldBe(dataset.UserCount);
            datasetReturned.HashedData.ShouldBe(dataset.HashedData);
            datasetReturned.ShouldBeOfType<DatasetDto>();

        }

        [Fact()]
        public async Task Should_Return_List_Of_DatasetDtos()
        {
            //Arrange
            var datasetList = new List<Dataset>() { 
                new Dataset("Dataset1", 1, 1, "123"), 
                new Dataset("Dataset2", 2, 2, "456") };

            _datasetRepository.GetListAsync().Returns(datasetList);

            //Act
            var datasetDtosList = await _datasetAppService.GetListAsync();

            //Assert
            datasetDtosList.ShouldNotBeNull();
            datasetDtosList.Count.ShouldBe(datasetList.Count);
            datasetDtosList.ShouldBeOfType<List<DatasetDto>>();

            for (int i = 0; i < datasetDtosList.Count; i++)
            {
                datasetDtosList[i].Name.ShouldBe(datasetList[i].Name);
                datasetDtosList[i].AverageConnections.ShouldBe(datasetList[i].AverageConnections);
                datasetDtosList[i].UserCount.ShouldBe(datasetList[i].UserCount);
                datasetDtosList[i].HashedData.ShouldBe(datasetList[i].HashedData);

            }

        }

        [Fact()]
        public async Task Should_Throw_Exception_If_File_Null()
        {
            //Arrange
            IFormFile file = null;
            var name = "Test";
            //Act & Assert
            var exception=await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _datasetAppService.PopulateDatasetAsync(file, name);

            });

            exception.Message.ShouldBe("Error loading file!");
        }
        [Fact()]
        public async Task Should_Throw_Exception_If_File_Has_Wrong_Extension()
        {
            //Arrange
            IFormFile invalidFile = Substitute.For<IFormFile>();
            invalidFile.FileName.Returns("file.png");
            var name = "Test";

            //Act & Assert
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _datasetAppService.PopulateDatasetAsync(invalidFile, name);

            });

            exception.Message.ShouldBe("Error loading file!");
        }
        [Fact()]
        public async Task Should_Throw_Exception_If_DatasetName_Empty()
        {
            //Arrange
            var name = "";

            //Act & Assert
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _datasetAppService.PopulateDatasetAsync(_validFile, name);

            });

            exception.Message.ShouldBe("Dataset name cannot be empty!");
        }

        [Fact()]
        public async Task Should_Throw_Exception_If_DatasetName_Exist_In_Db()
        {
            //Arrange          
            var name = "Duplicate name";
            var dataset = new Dataset(name, 10, 100, "abc123");

            _datasetRepository.FindAsync(Arg.Is<Expression<Func<Dataset, bool>>>(predicate=>predicate.Compile().Invoke(dataset))).Returns(dataset);
            
            //Act & Assert
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _datasetAppService.PopulateDatasetAsync(_validFile, name);

            });

            exception.Message.ShouldBe("Dataset with name '" + name + "' already exists!");
        }

        [Fact()]
        public async Task Should_Throw_Exception_If_Dataset_Exist_In_Db()
        {
            //Arrange
            var name = "Dataset name";
            var hashedData = "Hashed content";
            var dataset = new Dataset(name, 10, 100, "");
            var datasetWithDifferentNameButSameData = new Dataset("lol", 10, 100, hashedData);


            _datasetManager.HashDatasetContentAsync(Arg.Any<string>()).Returns(hashedData);
            _datasetRepository.FindAsync(Arg.Is<Expression<Func<Dataset, bool>>>(predicate=>predicate.Compile().Invoke(datasetWithDifferentNameButSameData))).Returns(dataset);

            //Act & Assert
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _datasetAppService.PopulateDatasetAsync(_validFile, name);

            });

            exception.Message.ShouldBe("Same dataset can't be uploaded twice!");
        }
        [Fact()]
        public async Task Should_Return_Dataset()
        {
            //Arrange
            var name = "Dataset name";
            var dataset = new Dataset(name, 2, 4, "Hashed content");

            _datasetRepository.FindAsync(Arg.Any<Expression<Func<Dataset, bool>>>()).Returns((Dataset)null);
            _datasetRepository.InsertAsync(Arg.Any<Dataset>()).Returns(dataset);

            //Act
           var result= await _datasetAppService.PopulateDatasetAsync(_validFile, name);

            //Assert
            result.ShouldNotBeNull();
            result.ShouldBeOfType<DatasetDto>();
            result.Name.ShouldBe(name);
            result.AverageConnections.ShouldBe(2);
            result.UserCount.ShouldBe(4);
            result.HashedData.ShouldBe("Hashed content");

        }


    }
}