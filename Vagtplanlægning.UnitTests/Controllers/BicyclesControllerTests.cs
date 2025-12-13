using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vagtplanlægning.Controllers;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Xunit;

namespace Vagtplanlægning.UnitTests.Controllers
{
    public class BicyclesControllerTests
    {
        private readonly Mock<IBicycleRepository> _repo;
        private readonly BicyclesController _controller;

        public BicyclesControllerTests()
        {
            _repo = new Mock<IBicycleRepository>();
            _controller = new BicyclesController(_repo.Object);
        }

        // ------------------------------------------------------------
        // GET /api/bicycles
        // ------------------------------------------------------------
        [Fact]
        public async Task GetAll_ReturnsOk_WithMappedDtos()
        {
            // Arrange
            var bicycles = new List<Bicycle>
            {
                new Bicycle { BicycleId = 1, BicycleNumber = 100, InOperate = true },
                new Bicycle { BicycleId = 2, BicycleNumber = 200, InOperate = false }
            };

            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(bicycles);

            // Act
            var result = await _controller.GetAll(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<BicycleDto>>(ok.Value);
            var list = dtos.ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal(100, list[0].BicycleNumber);
            Assert.False(list[1].InOperate);
        }

        // ------------------------------------------------------------
        // GET /api/bicycles/{id}
        // ------------------------------------------------------------
        [Fact]
        public async Task GetById_ExistingBicycle_ReturnsOk()
        {
            _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Bicycle
                 {
                     BicycleId = 1,
                     BicycleNumber = 123,
                     InOperate = true
                 });

            var result = await _controller.GetById(1, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<BicycleDto>(ok.Value);

            Assert.Equal(123, dto.BicycleNumber);
        }

        [Fact]
        public async Task GetById_UnknownBicycle_ReturnsNotFound()
        {
            _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Bicycle?)null);

            var result = await _controller.GetById(99, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // ------------------------------------------------------------
        // POST /api/bicycles
        // ------------------------------------------------------------
        [Fact]
        public async Task Create_ValidDto_ReturnsCreated()
        {
            Bicycle? captured = null;

            _repo.Setup(r => r.AddAsync(It.IsAny<Bicycle>(), It.IsAny<CancellationToken>()))
                 .Callback<Bicycle, CancellationToken>((b, _) =>
                 {
                     b.BicycleId = 10; // simuler DB id
                     captured = b;
                 })
                 .Returns(Task.CompletedTask);

            var dto = new CreateBicycleDto
            {
                BicycleNumber = 777,
                InOperate = true
            };

            var result = await _controller.Create(dto, CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<BicycleDto>(created.Value);

            Assert.Equal(777, response.BicycleNumber);
            Assert.True(response.InOperate);
            Assert.Equal(10, response.BicycleId);

            Assert.NotNull(captured);
            Assert.Equal(777, captured!.BicycleNumber);
        }

        // ------------------------------------------------------------
        // PUT /api/bicycles/{id}
        // ------------------------------------------------------------
        [Fact]
        public async Task Update_ExistingBicycle_ReturnsNoContent()
        {
            var existing = new Bicycle
            {
                BicycleId = 5,
                BicycleNumber = 100,
                InOperate = true
            };

            _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            var dto = new Vagtplanlægning.DTOs.UpdateBicycleDto
            {
                BicycleNumber = 900,
                InOperate = false
            };

            var result = await _controller.Update(5, dto, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);

            _repo.Verify(r => r.UpdateAsync(
                It.Is<Bicycle>(b =>
                    b.BicycleNumber == 900 &&
                    b.InOperate == false
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task Update_UnknownBicycle_ReturnsNotFound()
        {
            _repo.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Bicycle?)null);

            var dto = new Vagtplanlægning.DTOs.UpdateBicycleDto
            {
                BicycleNumber = 111,
                InOperate = true
            };

            var result = await _controller.Update(42, dto, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);
            _repo.Verify(r => r.UpdateAsync(It.IsAny<Bicycle>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ------------------------------------------------------------
        // DELETE /api/bicycles/{id}
        // ------------------------------------------------------------
        [Fact]
        public async Task Delete_ExistingBicycle_ReturnsNoContent()
        {
            _repo.Setup(r => r.DeleteAsync(3, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

            var result = await _controller.Delete(3, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_UnknownBicycle_ReturnsNotFound()
        {
            _repo.Setup(r => r.DeleteAsync(99, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

            var result = await _controller.Delete(99, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
