using IdentityUserRegistration.DTO;
using AutoMapper;
using IdentityUserRegistration.Entities;

namespace IdentityUserRegistration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserForRegistrationDto, User>()
            .ForMember(u => u.UserName, opt => opt.MapFrom(dto => dto.Email));
    }
}