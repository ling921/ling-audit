namespace Ling.Audit.EntityFrameworkCore.Tests;

public class AuditFieldChangeLogTests
{
    [Fact]
    public void AuditFieldChangeLog_ShouldInitializeWithDefaultValues()
    {
        // Act
        var log = new AuditFieldChangeLog();

        // Assert
        log.FieldName.Should().BeNull();
        log.ValueType.Should().BeNull();
        log.OriginalValue.Should().BeNull();
        log.NewValue.Should().BeNull();
        log.EntityLogId.Should().Be(0);
    }

    [Fact]
    public void AuditFieldChangeLog_ShouldTrackPropertyChanges()
    {
        // Arrange
        var log = new AuditFieldChangeLog
        {
            FieldName = "Name",
            ValueType = "string",
            OriginalValue = "OldName",
            NewValue = "NewName"
        };

        // Assert
        log.FieldName.Should().Be("Name");
        log.ValueType.Should().Be("string");
        log.OriginalValue.Should().Be("OldName");
        log.NewValue.Should().Be("NewName");
    }

    [Fact]
    public void AuditFieldChangeLog_ShouldHandleNullValues()
    {
        // Arrange
        var log = new AuditFieldChangeLog
        {
            FieldName = "Name",
            ValueType = "string",
            OriginalValue = null,
            NewValue = null
        };

        // Assert
        log.OriginalValue.Should().BeNull();
        log.NewValue.Should().BeNull();
    }

    [Fact]
    public void AuditFieldChangeLog_ShouldHandleDifferentValueTypes()
    {
        // Arrange
        var log = new AuditFieldChangeLog
        {
            FieldName = "Age",
            ValueType = "int",
            OriginalValue = "20",
            NewValue = "21"
        };

        // Assert
        log.ValueType.Should().Be("int");
        log.OriginalValue.Should().Be("20");
        log.NewValue.Should().Be("21");
    }
}
