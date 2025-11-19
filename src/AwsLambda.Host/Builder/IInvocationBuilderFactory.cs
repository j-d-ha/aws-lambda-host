namespace AwsLambda.Host;

internal interface IInvocationBuilderFactory
{
    ILambdaInvocationBuilder CreateBuilder();
}
