namespace AwsLambda.Host;

internal interface IOnInitBuilderFactory
{
    ILambdaOnInitBuilder CreateBuilder();
}
