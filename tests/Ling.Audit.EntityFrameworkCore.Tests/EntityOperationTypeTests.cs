namespace Ling.Audit.EntityFrameworkCore.Tests;

public class EntityOperationTypeTests
{
    [Fact]
    public void EntityOperationType_Should_BeFlags_Enum()
    {
        // Assert
        typeof(DataOperation).Should().BeDecoratedWith<FlagsAttribute>();
    }

    [Fact]
    public void EntityOperationType_All_Should_Include_All_Operations()
    {
        // Arrange
        var create = DataOperation.Create;
        var update = DataOperation.Modify;
        var delete = DataOperation.Delete;
        var all = DataOperation.All;

        // Assert
        all.Should().HaveFlag(create);
        all.Should().HaveFlag(update);
        all.Should().HaveFlag(delete);
        all.Should().Be(create | update | delete);
    }

    [Fact]
    public void EntityOperationType_Should_Support_FlagOperations()
    {
        // Arrange
        var createAndUpdate = DataOperation.Create | DataOperation.Modify;

        // Assert
        createAndUpdate.Should().HaveFlag(DataOperation.Create);
        createAndUpdate.Should().HaveFlag(DataOperation.Modify);
        createAndUpdate.Should().NotHaveFlag(DataOperation.Delete);

        // Test removing flag
        var onlyCreate = createAndUpdate & ~DataOperation.Modify;
        onlyCreate.Should().Be(DataOperation.Create);
        onlyCreate.Should().NotHaveFlag(DataOperation.Modify);
    }

    [Theory]
    [InlineData(DataOperation.Create, true, false, false)]
    [InlineData(DataOperation.Modify, false, true, false)]
    [InlineData(DataOperation.Delete, false, false, true)]
    [InlineData(DataOperation.Create | DataOperation.Modify, true, true, false)]
    [InlineData(DataOperation.Create | DataOperation.Delete, true, false, true)]
    [InlineData(DataOperation.Modify | DataOperation.Delete, false, true, true)]
    [InlineData(DataOperation.All, true, true, true)]
    public void EntityOperationType_HasFlag_Should_Work_Correctly(
        DataOperation value,
        bool hasCreate,
        bool hasUpdate,
        bool hasDelete)
    {
        // Assert
        value.HasFlag(DataOperation.Create).Should().Be(hasCreate);
        value.HasFlag(DataOperation.Modify).Should().Be(hasUpdate);
        value.HasFlag(DataOperation.Delete).Should().Be(hasDelete);
    }
}
