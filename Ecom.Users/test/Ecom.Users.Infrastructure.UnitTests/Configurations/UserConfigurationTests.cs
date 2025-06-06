using Ecom.Users.Infrastructure.Persistence.EfCore.Configurations;
using Ecom.Users.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Users.Infrastructure.UnitTests.Configurations;

public class UserConfigurationTests
{
    private readonly ModelBuilder _modelBuilder;
    private readonly UserConfiguration _configuration;

    public UserConfigurationTests()
    {
        _modelBuilder = new ModelBuilder();
        _configuration = new UserConfiguration();
    }

    [Fact]
    public void Configure_ShouldSetupUserEntityCorrectly()
    {
        // Act
        _configuration.Configure(_modelBuilder.Entity<User>());
        var model = _modelBuilder.FinalizeModel();

        // Assert
        var userEntityType = model.FindEntityType(typeof(User));
        userEntityType.Should().NotBeNull();

        // Check primary key
        var primaryKey = userEntityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be(nameof(User.Id));

        // Check required properties
        var emailProperty = userEntityType.FindProperty(nameof(User.Email));
        emailProperty.Should().NotBeNull();
        emailProperty.IsColumnNullable().Should().BeFalse();

        var passwordHashProperty = userEntityType.FindProperty(nameof(User.PasswordHash));
        passwordHashProperty.Should().NotBeNull();
        passwordHashProperty.IsColumnNullable().Should().BeFalse();

        // Check indexes
        var indexes = userEntityType.GetIndexes();
        indexes.Should().NotBeEmpty();
        
        // Should have unique index on Email
        var emailIndex = indexes.FirstOrDefault(i => 
            i.Properties.Count == 1 && 
            i.Properties[0].Name == nameof(User.Email));
        emailIndex.Should().NotBeNull();
        emailIndex!.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void Configure_ShouldSetupUserNavigationProperties()
    {
        // Act
        _configuration.Configure(_modelBuilder.Entity<User>());
        var model = _modelBuilder.FinalizeModel();

        // Assert
        var userEntityType = model.FindEntityType(typeof(User));
        userEntityType.Should().NotBeNull();

        // Check UserRoles navigation
        var userRolesNavigation = userEntityType!.FindNavigation(nameof(User.UserRoles));
        userRolesNavigation.Should().NotBeNull();
        userRolesNavigation!.IsCollection.Should().BeTrue();

        // Check RefreshTokens navigation
        var refreshTokensNavigation = userEntityType.FindNavigation(nameof(User.RefreshTokens));
        refreshTokensNavigation.Should().NotBeNull();
        refreshTokensNavigation!.IsCollection.Should().BeTrue();
    }

    [Fact]
    public void Configure_ShouldSetupTableName()
    {
        // Act
        _configuration.Configure(_modelBuilder.Entity<User>());
        var model = _modelBuilder.FinalizeModel();

        // Assert
        var userEntityType = model.FindEntityType(typeof(User));
        userEntityType.Should().NotBeNull();
        
        var tableName = userEntityType!.GetTableName();
        tableName.Should().Be(nameof(User));
    }

    [Fact]
    public void Configure_ShouldSetupColumnConstraints()
    {
        // Act
        _configuration.Configure(_modelBuilder.Entity<User>());
        var model = _modelBuilder.FinalizeModel();

        // Assert
        var userEntityType = model.FindEntityType(typeof(User));
        userEntityType.Should().NotBeNull();

        // Check Email constraints
        var emailProperty = userEntityType!.FindProperty(nameof(User.Email));
        emailProperty.Should().NotBeNull();
        emailProperty!.GetMaxLength().Should().Be(256);

        // Check UserName constraints
        var userNameProperty = userEntityType.FindProperty(nameof(User.UserName));
        userNameProperty.Should().NotBeNull();
        userNameProperty!.GetMaxLength().Should().Be(256);

        // Check FirstName constraints
        var firstNameProperty = userEntityType.FindProperty(nameof(User.FirstName));
        firstNameProperty.Should().NotBeNull();
        firstNameProperty!.GetMaxLength().Should().Be(100);

        // Check LastName constraints
        var lastNameProperty = userEntityType.FindProperty(nameof(User.LastName));
        lastNameProperty.Should().NotBeNull();
        lastNameProperty!.GetMaxLength().Should().Be(100);
    }
}
