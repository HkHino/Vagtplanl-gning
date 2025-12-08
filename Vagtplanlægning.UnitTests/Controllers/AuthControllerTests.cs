/*using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Vagtplanlægning.Controllers;
using Vagtplanlægning.Controllers.PublicControllers;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Services;
using Xunit;

namespace Vagtplanlægning.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _serviceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _serviceMock = new Mock<IAuthService>();
            _controller = new AuthController(_serviceMock.Object);
        }

        // --------------------------
        // BVA: dto = null → BadRequest
        // --------------------------
        [Fact]
        public async Task Login_NullBody_ReturnsBadRequest()
        {
            var result = await _controller.Login(null);

            var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("missing", badReq.Value!.ToString().ToLower());

            _serviceMock.Verify(
                s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        // --------------------------
        // EP: Invalid credentials → Unauthorized
        // --------------------------
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var dto = new LoginRequestDto
            {
                Username = "wrong",
                Password = "bad"
            };

            _serviceMock
                .Setup(s => s.LoginAsync("wrong", "bad"))
                .ReturnsAsync((LoginResponseDto)null);

            var result = await _controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        // --------------------------
        // EP: Valid credentials → Ok + JWT
        // --------------------------
        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var dto = new LoginRequestDto
            {
                Username = "admin",
                Password = "1234"
            };

            var expected = new LoginResponseDto
            {
                Token = "FAKE-JWT-TOKEN"
            };

            _serviceMock
                .Setup(s => s.LoginAsync("admin", "1234"))
                .ReturnsAsync(expected);

            var result = await _controller.Login(dto);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsType<LoginResponseDto>(ok.Value);

            Assert.Equal("FAKE-JWT-TOKEN", model.Token);
        }

        // --------------------------
        // Edge case: tomt username/password
        // --------------------------
        [Fact]
        public async Task Login_EmptyCredentials_ReturnsBadRequest()
        {
            var dto = new LoginRequestDto
            {
                Username = "",
                Password = ""
            };

            var result = await _controller.Login(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);

            _serviceMock.Verify(
                s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}
*/