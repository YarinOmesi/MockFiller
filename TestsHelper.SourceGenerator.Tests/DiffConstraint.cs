using NUnit.Framework.Constraints;

namespace TestsHelper.SourceGenerator.Tests;

public class DiffConstraint : Constraint
{
    private readonly string _expected;

    public DiffConstraint(string expected)
    {
        _expected = expected;
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        if (actual is not string actualValue)
        {
            return new Result(this, actual, ConstraintStatus.Failure, "Expected is not string");
        }

        return Diff.IsDifferent(_expected, actualValue, out string? message)
            ? new Result(this, actual, ConstraintStatus.Failure, message)
            : new Result(this, actual, ConstraintStatus.Success, string.Empty);
    }

    private sealed class Result : ConstraintResult
    {
        private readonly string _diffDisplay;

        public Result(IConstraint constraint, object actualValue, ConstraintStatus status, string diffDisplay) : base(constraint, actualValue, status)
        {
            _diffDisplay = diffDisplay;
        }

        public override void WriteMessageTo(MessageWriter writer) => writer.WriteMessageLine(_diffDisplay);
    }
}