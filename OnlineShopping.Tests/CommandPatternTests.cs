using OnlineShopping.Utilities;

namespace OnlineShopping.Tests;

public sealed class CommandPatternTests
{
    [Fact]
    public void DelegateCommand_ShouldExecuteAndReturnResult()
    {
        var invoked = false;
        var command = new DelegateCommand("Sample", () =>
        {
            invoked = true;
            return CommandResult.Ok("done");
        });

        var result = command.Execute();

        Assert.True(invoked);
        Assert.True(result.Success);
        Assert.Equal("done", result.Message);
    }
}
