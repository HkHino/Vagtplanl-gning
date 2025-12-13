using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Vagtplanlægning.Authentication;
using Vagtplanlægning.Controllers.PublicControllers;
using Vagtplanlægning.Data;
using Vagtplanlægning.Mapping;
using Vagtplanlægning.Models;
using Vagtplanlægning.Models.ApiModels;
using Vagtplanlægning.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace Vagtplanlægning.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IEmployeeRepository> _employeeRepository;
        private readonly IMapper _mapper;
        private readonly JwtHelper _jwtHelper;
        private readonly ITestOutputHelper _output;

        public AuthControllerTests(ITestOutputHelper output)
        {
            _output = output;
            _userRepository = new Mock<IUserRepository>();
            _employeeRepository = new Mock<IEmployeeRepository>();

            _mapper = new Mapper(
                new MapperConfiguration(cfg =>
                    cfg.AddProfile(new AutoMapperProfile()))
            );

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Key", "ZNztR3p+MCOCLtOQe5yTJNJHC1JkiqNfLs6vhaNVzAw=" }
                })
                .Build();

            _jwtHelper = new JwtHelper(configuration);
            _controller = new AuthController(
                _mapper,
                _jwtHelper,
                _userRepository.Object,
                _employeeRepository.Object,
                null //Loggging not relevant for tests 
            );

        }

        // --------------------------
        // BVA: dto = null → BadRequest
        // --------------------------
        [Fact]
        public async Task Login_NullBody_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SignIn(null);
            
            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new SignInRequest
            {
                Username = "alice",
                Password = "bad"
            };

            var passwordHash = PasswordHelper.HashPassword("1234");

            var existingUser = new User
            {
                Username = "alice",
                Hash = passwordHash,
                Role = UserRole.Employee
            };

            _userRepository
                .Setup(r => r.GetByUsernameAsync("alice"))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _controller.SignIn(request);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid password", unauthorized.Value);

            _userRepository.Verify(
                r => r.GetByUsernameAsync("alice"),
                Times.Once
            );
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk_WithTokenLikePayload()
        {
            // Arrange
            var request = new SignInRequest
            {
                Username = "alice",
                Password = "1234"
            };

            var passwordHash = PasswordHelper.HashPassword("1234");

            var existingUser = new User
            {
                Username = "alice",
                Hash = passwordHash,
                Role = UserRole.Employee
            };

            _userRepository
                .Setup(r => r.GetByUsernameAsync("alice"))
                .ReturnsAsync(existingUser);

            // Hvis jeres AuthController forsøger at slå employee op,
            // så kan det være I skal "Setup" en metode på _employeeRepository her.
            // (Hvis testen fejler med NullReference eller Verify-fejl, så paste fejlen til mig,
            // og jeg tilpasser setup til præcis den metode jeres controller kalder.)

            // Act
            var result = await _controller.SignIn(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);

            // Tolerant check: enten string token eller objekt med token-property
            if (ok.Value is string tokenString)
            {
                Assert.False(string.IsNullOrWhiteSpace(tokenString));
                Assert.True(tokenString.Length >= 10); // “token-ish”
            }
            else
            {
                var tokenProp = ok.Value.GetType().GetProperty("token")
                              ?? ok.Value.GetType().GetProperty("Token");

                Assert.NotNull(tokenProp);

                var token = tokenProp!.GetValue(ok.Value)?.ToString();
                Assert.False(string.IsNullOrWhiteSpace(token));
                Assert.True(token!.Length >= 10);
            }

            _userRepository.Verify(r => r.GetByUsernameAsync("alice"), Times.Once);
        }

        [Fact]
        public async Task Login_UnknownUser_ReturnsNotFound()
        {
            // Arrange
            var request = new SignInRequest
            {
                Username = "ghost",
                Password = "whatever"
            };

            _userRepository
                .Setup(r => r.GetByUsernameAsync("ghost"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.SignIn(request);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);

            _userRepository.Verify(r => r.GetByUsernameAsync("ghost"), Times.Once);
        }




        /*

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
    */
    }
}
