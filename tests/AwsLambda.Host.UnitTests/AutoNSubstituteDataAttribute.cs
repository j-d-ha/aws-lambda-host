using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit3;

namespace AwsLambda.Host.UnitTests;

public class AutoNSubstituteDataAttribute()
    : AutoDataAttribute(() => new Fixture().Customize(new AutoNSubstituteCustomization()));

public class InlineAutoNSubstituteDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoNSubstituteDataAttribute(params object[] args)
        : base(new AutoNSubstituteDataAttribute(), args) { }
}
