namespace AwsLambda.Host;

internal interface IOnShutdownBuilderFactory
{
    ILambdaOnShutdownBuilder CreateBuilder();
}
