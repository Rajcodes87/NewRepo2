using AnimalRescueSystem.Constants;
using AnimalRescueSystem.Entities.RequestRescues;
using AnimalRescueSystem.RequestRescues;
using AnimalRescueSystem.RescueCompletions;
using AnimalRescueSystem.RescueInitiations;
using AnimalRescueSystem.RescuerApplications;
using AnimalRescueSystem.RescuerProfiles;
using AutoMapper;
using Pawchums.Entities.RequestRescues;
using Pawchums.Entities.RescuerApplication;
using Pawchums.Entities.RescuerProfile;
using Pawchums.RescueCompletions;
using System;

namespace Pawchums;

public class PawchumsApplicationAutoMapperProfile : Profile
{
    public PawchumsApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        // RequestRescue mappings
        CreateMap<RequestRescue, RequestRescueDto>()
            .ForMember(dest => dest.InitiationsCount, opt => opt.MapFrom(src => src.RescueInitiations != null ? src.RescueInitiations.Count : 0))
            .ForMember(dest => dest.HasCompletion, opt => opt.MapFrom(src => src.RescueCompletion != null));
        CreateMap<CreateUpdateRequestRescueDto, RequestRescue>()
            .ForMember(dest => dest.RequestDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => RequestRescueConsts.RequestStatus.NotInitiated));

        // RescueInitiation mappings
        CreateMap<RescueInitiation, RescueInitiationDto>();
        CreateMap<CreateRescueInitiationDto, RescueInitiation>()
            .ForMember(dest => dest.InitiatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => RescueInitiationConsts.InitiationStatus.Pending))
            .ForMember(dest => dest.IsSelected, opt => opt.MapFrom(src => false));
        CreateMap<UpdateRescueInitiationDto, RescueInitiation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RequestRescueId, opt => opt.Ignore())
            .ForMember(dest => dest.RescuerId, opt => opt.Ignore())
            .ForMember(dest => dest.InitiatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsSelected, opt => opt.Ignore())
            .ForMember(dest => dest.AcceptedDate, opt => opt.Ignore())
            .ForMember(dest => dest.AcceptedByUserId, opt => opt.Ignore());

        // RescueCompletion mappings
        CreateMap<RescueCompletion, RescueCompletionDto>();
        CreateMap<CreateRescueCompletionDto, RescueCompletion>()
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => false));
        CreateMap<UpdateRescueCompletionDto, RescueCompletion>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RequestRescueId, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedByRescuerId, opt => opt.Ignore())
            .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
            .ForMember(dest => dest.VerifiedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.VerifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.VerificationNotes, opt => opt.Ignore());

        // RescuerNotification mappings
        CreateMap<RescuerNotification, RescuerNotificationDto>()
            .ForMember(dest => dest.RequestTitle, opt => opt.MapFrom(src => src.RequestRescue.Title))
            .ForMember(dest => dest.RequestLocation, opt => opt.MapFrom(src => src.RequestRescue.Location));
        CreateMap<RescuerApplication, RescuerApplicationDto>();
        CreateMap<CreateRescuerApplicationDto, RescuerApplication>();

        CreateMap<RescuerProfile, RescuerProfileDto>();
        CreateMap<CreateUpdateRescuerProfileDto, RescuerProfile>()
            .ForMember(dest => dest.LocationUpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}