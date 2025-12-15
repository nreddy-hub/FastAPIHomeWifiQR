using AgileObjects.AgileMapper;
using AgileObjects.AgileMapper.Extensions;

namespace HomeWifiQR.Services;

public class AgileMapperAdapter : IObjectMapper
{
    public TDestination Map<TDestination>(object source) =>
        source.Map().ToANew<TDestination>();
}