using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Vagtplanlægning.UnitTests.Controllers
{
    public class ShiftPlansControllerTests
    {
        private readonly Mock<IShiftPlanRepository> _repoMock;
        private readonly Mock<IShiftPlanService> _serviceMock;
        private readonly ShiftPlansController _controller;

        public ShiftPlansControllerTests()
        {
            _repoMock = new Mock<IShiftPlanRepository>();
            _serviceMock = new Mock<IShiftPlanService>();
            _controller = new ShiftPlansController(_repoMock.Object, _serviceMock.Object);
        }

        // ------------------------------------------------------------
        // GET: api/shiftplans  (positiv test + “black box” på output)
        // ------------------------------------------------------------
        [Fact]
        public async Task GetAll_ReturnsOkWithSummaryDtos()
        {
            // Arrange
            var plan = new ShiftPlan
            {
                ShiftPlanId = "plan-1",
                Name = "Testplan",
                StartDate = new DateTime(2025, 11, 1),
                EndDate = new DateTime(2025, 12, 31),
                Shifts = new List<Shift>
                {
                    new Shift { ShiftId = 1 },
                    new Shift { ShiftId = 2 }
                }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShiftPlan> { plan });

            // Act
            var result = await _controller.GetAll(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<ShiftPlanSummaryDto>>(ok.Value);
            var dto = Assert.Single(dtos);

            Assert.Equal("plan-1", dto.ShiftPlanId);
            Assert.Equal("Testplan", dto.Name);
            Assert.Equal(2, dto.ShiftCount);

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // ------------------------------------------------------------
        // GET: api/shiftplans/{id} – NotFound når repo returnerer null
        // ------------------------------------------------------------
        [Fact]
        public async Task GetById_UnknownId_ReturnsNotFound()
        {
            // Arrange
            _repoMock
                .Setup(r => r.GetByIdAsync("does-not-exist", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShiftPlan?)null);

            // Act
            var result = await _controller.GetById("does-not-exist", CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            _repoMock.Verify(r => r.GetByIdAsync("does-not-exist", It.IsAny<CancellationToken>()), Times.Once);
        }

        // ------------------------------------------------------------
        // GET: api/shiftplans/{id} – positiv test
        // ------------------------------------------------------------
        [Fact]
        public async Task GetById_ExistingPlan_ReturnsDetailDto()
        {
            // Arrange
            var plan = new ShiftPlan
            {
                ShiftPlanId = "plan-42",
                Name = "Answer plan",
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 2, 1),
                Shifts = new List<Shift>
                {
                    new Shift { ShiftId = 10, EmployeeId = 1 },
                    new Shift { ShiftId = 11, EmployeeId = 2 }
                }
            };

            _repoMock
                .Setup(r => r.GetByIdAsync("plan-42", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            // Act
            var result = await _controller.GetById("plan-42", CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ShiftPlanDetailDto>(ok.Value);

            Assert.Equal("plan-42", dto.ShiftPlanId);
            Assert.Equal("Answer plan", dto.Name);
            Assert.Equal(2, dto.Shifts.Count);

            _repoMock.Verify(r => r.GetByIdAsync("plan-42", It.IsAny<CancellationToken>()), Times.Once);
        }

        // ------------------------------------------------------------
        // POST: api/shiftplans/generate-6weeks – positiv test
        // ------------------------------------------------------------
        [Fact]
        public async Task Generate6Weeks_ValidRequest_ReturnsCreatedWithDetailDto()
        {
            // Arrange
            var start = new DateTime(2025, 11, 25);
            var generated = new ShiftPlan
            {
                ShiftPlanId = "generated-1",
                Name = "Auto generated",
                StartDate = start,
                EndDate = start.AddDays(41), // ca. 6 uger
                Shifts = new List<Shift>()
            };

            _serviceMock
                .Setup(s => s.Generate6WeekPlanAsync(start.Date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(generated);

            var request = new GenerateShiftPlanRequestDto { StartDate = start };

            // Act
            var result = await _controller.Generate6Weeks(request, CancellationToken.None);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(ShiftPlansController.GetById), created.ActionName);

            var dto = Assert.IsType<ShiftPlanDetailDto>(created.Value);
            Assert.Equal("generated-1", dto.ShiftPlanId);

            _serviceMock.Verify(
                s => s.Generate6WeekPlanAsync(start.Date, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ------------------------------------------------------------
        // DELETE: api/shiftplans/{id} – NotFound/NoContent
        // ------------------------------------------------------------
        [Fact]
        public async Task Delete_ExistingPlan_ReturnsNoContent()
        {
            _repoMock
                .Setup(r => r.DeleteAsync("plan-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.Delete("plan-1", CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
            _repoMock.Verify(r => r.DeleteAsync("plan-1", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_MissingPlan_ReturnsNotFound()
        {
            _repoMock
                .Setup(r => r.DeleteAsync("missing", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.Delete("missing", CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);
            _repoMock.Verify(r => r.DeleteAsync("missing", It.IsAny<CancellationToken>()), Times.Once);
        }

        // ------------------------------------------------------------
        // PUT: api/shiftplans/{id}/name – BadRequest vs NoContent
        // ------------------------------------------------------------
        [Fact]
        public async Task UpdateName_EmptyName_ReturnsBadRequest()
        {
            var dto = new UpdateShiftPlanNameDto { Name = "   " };

            var result = await _controller.UpdateName("plan-1", dto, CancellationToken.None);

            var badReq = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Name is required", badReq.Value!.ToString());
            _repoMock.Verify(
                r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateName_ValidName_UpdatesAndReturnsNoContent()
        {
            var existing = new ShiftPlan
            {
                ShiftPlanId = "plan-1",
                Name = "Old name",
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 1, 31)
            };

            _repoMock
                .Setup(r => r.GetByIdAsync("plan-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var dto = new UpdateShiftPlanNameDto { Name = " New Name " };

            var result = await _controller.UpdateName("plan-1", dto, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);

            _repoMock.Verify(r => r.GetByIdAsync("plan-1", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(
                It.Is<ShiftPlan>(p => p.Name == "New Name"),  // Trimmet
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
