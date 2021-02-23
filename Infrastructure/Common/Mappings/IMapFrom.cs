using AutoMapper;

namespace PasswordPoliciesDemo.API.Infrastructure.Common.Mappings
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
    }
}
