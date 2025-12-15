namespace HomeWifiQR.Services;

public interface IObjectMapper
{
    TDestination Map<TDestination>(object source);
}