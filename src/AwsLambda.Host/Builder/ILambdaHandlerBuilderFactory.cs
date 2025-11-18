namespace AwsLambda.Host;

internal interface ILambdaHandlerBuilderFactory
{
    ILambdaHandlerBuilder CreateBuilder();
}
