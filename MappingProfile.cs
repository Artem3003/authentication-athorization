using IdentityUserRegistration.DTO;
using AutoMapper;
using IdentityUserRegistration.Entities;
using IdentityUserRegistration.VM;

namespace IdentityUserRegistration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserForRegistrationDto, User>()
            .ForMember(u => u.UserName, opt => opt.MapFrom(dto => dto.Email));

        CreateMap<RegisterViewModel, User>()
            .ForMember(u => u.UserName, opt => opt.MapFrom(vm => vm.Email));
    }
}