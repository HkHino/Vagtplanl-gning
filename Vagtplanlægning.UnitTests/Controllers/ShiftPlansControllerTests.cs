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
        // ------------------------------------------------------------
        // PUT: api/shiftplans/{id}/shifts/{index} – ugyldigt index
        // ------------------------------------------------------------
        [Fact]
        public async Task UpdateShiftInPlan_InvalidIndex_ReturnsBadRequest()
        {
            var plan = new ShiftPlan
            {
                ShiftPlanId = "plan-1",
                Shifts = new List<Shift>
                {
                    new Shift { ShiftId = 1 }
                }
            };

            _repoMock
                .Setup(r => r.GetByIdAsync("plan-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            var dto = new UpdateShiftInPlanDto
            {
                DateOfShift = new DateTime(2025, 1, 1),
                EmployeeId = 1,
                BicycleId = 1,
                RouteId = 1,
                SubstitutedId = 0
            };

            // index = 5 er out-of-range (vi har kun 1 shift)
            var result = await _controller.UpdateShiftInPlan(
                "plan-1", 5, dto, CancellationToken.None);

            var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("out of range", badReq.Value!.ToString());

            _repoMock.Verify(r => r.GetByIdAsync("plan-1", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ShiftPlan>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ------------------------------------------------------------
        // PUT: api/shiftplans/{id}/shifts/{index} – gyldigt index
        // ------------------------------------------------------------
        [Fact]
        public async Task UpdateShiftInPlan_ValidIndex_UpdatesAndReturnsDetail()
        {
            var plan = new ShiftPlan
            {
                ShiftPlanId = "plan-1",
                Shifts = new List<Shift>
                {
                    new Shift
                    {
                        ShiftId = 10,
                        DateOfShift = new DateTime(2025, 1, 1),
                        EmployeeId = 1,
                        BicycleId = 1,
                        RouteId = 1,
                        SubstitutedId = 0
                    }
                }
            };

            _repoMock
                .Setup(r => r.GetByIdAsync("plan-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            var dto = new UpdateShiftInPlanDto
            {
                DateOfShift = new DateTime(2025, 2, 2),
                EmployeeId = 2,
                BicycleId = 3,
                RouteId = 4,
                SubstitutedId = 5
            };

            var result = await _controller.UpdateShiftInPlan(
                "plan-1", 0, dto, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var detail = Assert.IsType<ShiftPlanDetailDto>(ok.Value);

            // vi forventer at første shift i planen er opdateret
            var updated = Assert.Single(detail.Shifts);
            Assert.Equal(new DateTime(2025, 2, 2), updated.DateOfShift);
            Assert.Equal(2, updated.EmployeeId);
            Assert.Equal(3, updated.BicycleId);
            Assert.Equal(4, updated.RouteId);
            Assert.Equal(5, updated.SubstitutedId);

            _repoMock.Verify(r => r.UpdateAsync(
                It.Is<ShiftPlan>(p =>
                    p.Shifts![0].EmployeeId == 2 &&
                    p.Shifts[0].BicycleId == 3),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ------------------------------------------------------------
        // POST: api/shiftplans/generate-6weeks – negative tests (EP/BVA)
        // ------------------------------------------------------------
        [Fact]
        public async Task Generate6Weeks_NullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Generate6Weeks(null!, CancellationToken.None);

            // Assert
            var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("StartDate is required", badReq.Value!.ToString());

            _serviceMock.Verify(
                s => s.Generate6WeekPlanAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Generate6Weeks_DefaultStartDate_ReturnsBadRequest()
        {
            var request = new GenerateShiftPlanRequestDto
            {
                StartDate = default // 0001-01-01
            };

            var result = await _controller.Generate6Weeks(request, CancellationToken.None);

            var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("StartDate is required", badReq.Value!.ToString());

            _serviceMock.Verify(
                s => s.Generate6WeekPlanAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ------------------------------------------------------------
        // PUT: api/shiftplans/{id}/shifts/{index} – dto == null
        // ------------------------------------------------------------
        [Fact]
        public async Task UpdateShiftInPlan_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.UpdateShiftInPlan(
                "plan-1",
                0,
                null!,
                CancellationToken.None);

            var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Request body is missing or invalid", badReq.Value!.ToString());

            _repoMock.Verify(
                r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Generate6Weeks_StartDateMinValue_ReturnsBadRequest()
        {
            var dto = new GenerateShiftPlanRequestDto
            {
                StartDate = default
            };

            var result = await _controller.Generate6Weeks(dto, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result.Result);

            _serviceMock.Verify(
                s => s.Generate6WeekPlanAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        [Fact]
        public async Task UpdateShiftInPlan_MissingDate_ReturnsBadRequest()
        {
            var plan = new ShiftPlan
            {
                ShiftPlanId = "plan-1",
                Shifts = new List<Shift> { new Shift { ShiftId = 10 } }
            };

            _repoMock.Setup(r => r.GetByIdAsync("plan-1", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(plan);

            var dto = new UpdateShiftInPlanDto
            {
                DateOfShift = default,
                EmployeeId = 1,
                BicycleId = 1,
                RouteId = 1,
                SubstitutedId = 0
            };

            var result = await _controller.UpdateShiftInPlan("plan-1", 0, dto, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }


        // ------------------------------------------------------------
        // PUT: api/shiftplans/{id}/shifts/{index} – BVA på index
        // ------------------------------------------------------------
        [Theory]
        [InlineData(-1)] // lige under nedre grænse
        [InlineData(1)]  // lige over øvre grænse, når der kun er 1 shift (index 0)
        public async Task UpdateShiftInPlan_IndexOutOfRange_ReturnsBadRequest(int index)
        {
            var plan = new ShiftPlan
            {
                ShiftPlanId = "plan-idx",
                Shifts = new List<Shift>
                {
                    new Shift { ShiftId = 10 }
                }
            };

            _repoMock
                .Setup(r => r.GetByIdAsync("plan-idx", It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            var dto = new UpdateShiftInPlanDto
            {
                DateOfShift = new DateTime(2025, 1, 1),
                EmployeeId = 1,
                BicycleId = 1,
                RouteId = 1,
                SubstitutedId = 0
            };

            var result = await _controller.UpdateShiftInPlan(
                "plan-idx", index, dto, CancellationToken.None);

            var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("out of range", badReq.Value!.ToString());

            _repoMock.Verify(
                r => r.GetByIdAsync("plan-idx", It.IsAny<CancellationToken>()),
                Times.Once);

            _repoMock.Verify(
                r => r.UpdateAsync(It.IsAny<ShiftPlan>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateName_PlanNotFound_ReturnsNotFound()
        {
            _repoMock
                .Setup(r => r.GetByIdAsync("missing-plan", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShiftPlan?)null);

            var dto = new UpdateShiftPlanNameDto { Name = "New name" };

            var result = await _controller.UpdateName("missing-plan", dto, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);

            _repoMock.Verify(r => r.GetByIdAsync("missing-plan", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ShiftPlan>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateShiftInPlan_PlanNotFound_ReturnsNotFound()
        {
            _repoMock
                .Setup(r => r.GetByIdAsync("missing-plan", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShiftPlan?)null);

            var dto = new UpdateShiftInPlanDto
            {
                DateOfShift = new DateTime(2025, 1, 1),
                EmployeeId = 1,
                BicycleId = 1,
                RouteId = 1,
                SubstitutedId = 0
            };

            var result = await _controller.UpdateShiftInPlan("missing-plan", 0, dto, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result.Result);

            _repoMock.Verify(r => r.GetByIdAsync("missing-plan", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ShiftPlan>(), It.IsAny<CancellationToken>()), Times.Never);
        }



    }
}
