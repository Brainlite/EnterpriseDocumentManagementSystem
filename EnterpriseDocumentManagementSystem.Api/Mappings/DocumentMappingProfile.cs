using AutoMapper;
using EnterpriseDocumentManagementSystem.Api.Models.DTOs;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Mappings;

public class DocumentMappingProfile : Profile
{
    public DocumentMappingProfile()
    {
        // Document to DocumentResponse
        CreateMap<Document, DocumentResponse>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.DocumentTags.Select(dt => dt.Tag)))
            .ForMember(dest => dest.Shares, opt => opt.MapFrom(src => src.DocumentShares))
            .ForMember(dest => dest.CanEdit, opt => opt.Ignore())
            .ForMember(dest => dest.CanDelete, opt => opt.Ignore())
            .ForMember(dest => dest.CanShare, opt => opt.Ignore());

        // Document to DocumentListResponse
        CreateMap<Document, DocumentListResponse>()
            .ForMember(dest => dest.TagNames, opt => opt.MapFrom(src => src.DocumentTags.Select(dt => dt.Tag.Name)))
            .ForMember(dest => dest.IsShared, opt => opt.Ignore());

        // Tag to TagResponse
        CreateMap<Tag, TagResponse>();

        // DocumentShare to DocumentShareResponse
        CreateMap<DocumentShare, DocumentShareResponse>();

        // DocumentUploadRequest to Document
        CreateMap<DocumentUploadRequest, Document>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FileName, opt => opt.Ignore())
            .ForMember(dest => dest.FileSize, opt => opt.Ignore())
            .ForMember(dest => dest.ContentType, opt => opt.Ignore())
            .ForMember(dest => dest.FilePath, opt => opt.Ignore())
            .ForMember(dest => dest.UploadedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
            .ForMember(dest => dest.DocumentShares, opt => opt.Ignore())
            .ForMember(dest => dest.DocumentTags, opt => opt.Ignore())
            .ForMember(dest => dest.AuditLogs, opt => opt.Ignore());
    }
}
