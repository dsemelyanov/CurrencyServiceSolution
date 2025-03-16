using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using SharedModels.Data;
using SharedModels.Models;
using SharedModels.Settings;
using UserService.Controllers;
using UserService.Models.UserModels;

namespace UserService.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<AppDbContext> _dbContextMock;
    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    private readonly AuthController _controller;
    private readonly Mock<DbSet<User>> _userSetMock;

    public AuthControllerTests()
    {
        // Настройка мока для DbContext
        _userSetMock = new Mock<DbSet<User>>();
        _dbContextMock = new Mock<AppDbContext>();
        _dbContextMock.Setup(db => db.Users).Returns(_userSetMock.Object);

        // Настройка JwtSettings
        _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        _jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings
        {
            SecretKey = "K7mP9nLqR4tYvB2wNxZ8jQ3eF6hG5iO0pA1sD8uC9rE2vT3w",
            Issuer = "UserService",
            Audience = "CurrencyServiceApi",
            TokenExpiryMinutes = 60
        });

        _controller = new AuthController(_dbContextMock.Object, _jwtSettingsMock.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var user = new User { Id = 1, Name = "testuser", Password = "testpass" };
        var users = new List<User> { user }.AsQueryable();

        _userSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        _userSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        _userSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _userSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _userSetMock.As<IAsyncEnumerable<User>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<User>(users.GetEnumerator()));

        _dbContextMock.Setup(db => db.Users.FirstOrDefaultAsync(u => u.Name == "testuser" && u.Password == "testpass", default))
            .ReturnsAsync(user);

        var loginModel = new LoginModel { Username = "testuser", Password = "testpass" };

        // Act
        var result = await _controller.Login(loginModel) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
        var response = result.Value as dynamic;
        response.token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var users = new List<User>().AsQueryable();

        _userSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        _userSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        _userSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _userSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _userSetMock.As<IAsyncEnumerable<User>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<User>(users.GetEnumerator()));

        _dbContextMock.Setup(db => db.Users.FirstOrDefaultAsync(u => u.Name == "testuser" && u.Password == "wrongpass", default))
            .ReturnsAsync((User)null);

        var loginModel = new LoginModel { Username = "testuser", Password = "wrongpass" };

        // Act
        var result = await _controller.Login(loginModel) as UnauthorizedObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(401);
        result.Value.Should().Be("Invalid username or password.");
    }
}

// Вспомогательный класс для мока IAsyncEnumerable
public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return default;
    }
}