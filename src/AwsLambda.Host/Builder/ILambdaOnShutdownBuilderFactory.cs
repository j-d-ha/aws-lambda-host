namespace AwsLambda.Host;

internal interface ILambdaOnShutdownBuilderFactory
{
    ILambdaOnShutdownBuilder CreateBuilder();
}
