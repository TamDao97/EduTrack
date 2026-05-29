using AutoMapper;

namespace TD.Lib.AutoMapper
{
    public static class AutoMapperGeneric
    {
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                // Lấy kiểu phần tử nếu là list
                var sourceType = typeof(TSource);
                var destinationType = typeof(TDestination);

                if (sourceType.IsGenericType && destinationType.IsGenericType)
                {
                    var sourceItemType = sourceType.GetGenericArguments()[0];
                    var destinationItemType = destinationType.GetGenericArguments()[0];

                    cfg.CreateMap(sourceItemType, destinationItemType).ReverseMap();
                }
                else
                {
                    cfg.CreateMap<TSource, TDestination>().ReverseMap();
                }
            });

            var mapper = new Mapper(config);
            return mapper.Map<TDestination>(source);
        }
    }
}
