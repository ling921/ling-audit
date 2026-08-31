using FluentAssertions;
using Ling.Audit.EntityFrameworkCore;
using Xunit;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class EntityOperationTypeTests
{
    [Theory]
    [InlineData(EntityOperationType.None, false, false, false)]
    [InlineData(EntityOperationType.Create, true, false, false)]
    [InlineData(EntityOperationType.Update, false, true, false)]
    [InlineData(EntityOperationType.Delete, false, false, true)]
    [InlineData(EntityOperationType.All, true, true, true)]
    public void EntityOperationType_ShouldHaveCorrectFlags(
        EntityOperationType operationType,
        bool isCreate,
        bool isUpdate,
        bool isDelete)
    {
        // Assert
        operationType.HasFlag(EntityOperationType.Create).Should().Be(isCreate);
        operationType.HasFlag(EntityOperationType.Update).Should().Be(isUpdate);
        operationType.HasFlag(EntityOperationType.Delete).Should().Be(isDelete);
    }

    [Fact]
    public void EntityOperationType_ShouldSupportCombinedOperations()
    {
        // Arrange
        var createAndUpdate = EntityOperationType.Create | EntityOperationType.Update;

        // Assert
        createAndUpdate.HasFlag(EntityOperationType.Create).Should().BeTrue();
        createAndUpdate.HasFlag(EntityOperationType.Update).Should().BeTrue();
        createAndUpdate.HasFlag(EntityOperationType.Delete).Should().BeFalse();
    }

    [Fact]
    public void EntityOperationType_All_ShouldIncludeAllOperations()
    {
        // Assert
        EntityOperationType.All.Should().Be(EntityOperationType.Create | EntityOperationType.Update | EntityOperationType.Delete);
    }
}
