using AutoMapper;
using EnterpriseDocumentManagementSystem.Api.Models;

namespace EnterpriseDocumentManagementSystem.Api.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, UserPayload>()
            .ConstructUsing(src => new UserPayload(
                src.Id,
                src.Email,
                src.Role,
                src.Name
            ));

        // Map for listing users without sensitive data
        CreateMap<User, User>()
            .ForMember(dest => dest.Password, opt => opt.Ignore());
    }
}
