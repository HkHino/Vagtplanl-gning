using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Vagtplanlægning.Controllers;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Services;
using Xunit;

namespace Vagtplanlægning.Tests.Controllers
{
    public class ShiftPlansControllerTests
    {
        private ShiftPlansController CreateController(
            out Mock<IShiftPlanRepository> repoMock,
            out Mock<IShiftPlanService> serviceMock)
        {
            repoMock = new Mock<IShiftPlanRepository>();
            serviceMock = new Mock<IShiftPlanService>();

            return new ShiftPlansController(repoMock.Object, serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithSummaries()
        {
            // Arrange
            var controller = CreateController(out var repoMock, out var serviceMock);

            var plans = new List<ShiftPlan>
            {
                new ShiftPlan
                {
                    ShiftPlanId = Guid.NewGuid().ToString(),
                    Name = "Plan A",
                    StartDate = new DateTime(2025, 11, 1),
                    EndDate = new DateTime(2025, 11, 7),
                    Shifts = new List<Shift> { new Shift(), new Shift() }
                },
                new ShiftPlan
                {
                    ShiftPlanId = Guid.NewGuid().ToString(),
                    Name = "Plan B",
                    StartDate = new DateTime(2025, 11, 8),
                    EndDate = new DateTime(2025, 11, 14),
                    Shifts = new List<Shift>()
                }
            };

            repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(plans);

            // Act
            var result = await controller.GetAll(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsType<List<ShiftPlanSummaryDto>>(ok.Value);

            Assert.Equal(2, dtos.Count);
            Assert.Equal("Plan A", dtos[0].Name);
            Assert.Equal(2, dtos[0].ShiftCount);
            Assert.Equal("Plan B", dtos[1].Name);

            repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            // Arrange
            var controller = CreateController(out var repoMock, out var serviceMock);

            repoMock
                .Setup(r => r.GetByIdAsync("does-not-exist", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShiftPlan?)null);

            // Act
            var result = await controller.GetById("does-not-exist", CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetById_Found_ReturnsDetailDto()
        {
            // Arrange
            var controller = CreateController(out var repoMock, out var serviceMock);

            var plan = new ShiftPlan
            {
                ShiftPlanId = "123",
                Name = "Test Plan",
                StartDate = new DateTime(2025, 11, 1),
                EndDate = new DateTime(2025, 11, 7),
                Shifts = new List<Shift> { new Shift { ShiftId = 1 } }
            };

            repoMock
                .Setup(r => r.GetByIdAsync("123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            // Act
            var result = await controller.GetById("123", CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ShiftPlanDetailDto>(ok.Value);

            Assert.Equal("123", dto.ShiftPlanId);
            Assert.Equal("Test Plan", dto.Name);
            Assert.Single(dto.Shifts);
        }

        [Fact]
        public async Task UpdateName_EmptyName_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController(out var repoMock, out var serviceMock);

            var dto = new UpdateShiftPlanNameDto { Name = "   " };

            // Act
            var result = await controller.UpdateName("123", dto, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateName_Valid_UpdatesAndReturnsNoContent()
        {
            // Arrange
            var controller = CreateController(out var repoMock, out var serviceMock);

            var plan = new ShiftPlan
            {
                ShiftPlanId = "123",
                Name = "Old Name"
            };

            repoMock
                .Setup(r => r.GetByIdAsync("123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            var dto = new UpdateShiftPlanNameDto { Name = "New Name" };

            // Act
            var result = await controller.UpdateName("123", dto, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal("New Name", plan.Name);

            repoMock.Verify(r => r.UpdateAsync(plan, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
