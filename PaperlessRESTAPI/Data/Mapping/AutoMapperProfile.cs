using AutoMapper;
using PaperlessRESTAPI.Data.Entities;
using PaperlessRESTAPI.Models.DTOs;

namespace PaperlessRESTAPI.Data.Mapping;

/// <summary>
/// AutoMapper profile for entity-DTO mappings
/// </summary>
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Document mappings
        CreateMap<Document, DocumentDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

        CreateMap<DocumentDto, Document>()
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.AccessLogs, opt => opt.Ignore());

        // Tag mappings
        CreateMap<Tag, TagDto>();
        CreateMap<TagDto, Tag>()
            .ForMember(dest => dest.Documents, opt => opt.Ignore());

        // Upload DTO mapping
        CreateMap<UploadDocumentDto, Document>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.File.FileName))
            .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.File.Length))
            .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => Path.GetExtension(src.File.FileName)))
            .ForMember(dest => dest.FilePath, opt => opt.Ignore())
            .ForMember(dest => dest.UploadDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastModified, opt => opt.Ignore())
            .ForMember(dest => dest.OcrText, opt => opt.Ignore())
            .ForMember(dest => dest.Summary, opt => opt.Ignore())
            .ForMember(dest => dest.IsProcessed, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsIndexed, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.AccessLogs, opt => opt.Ignore());
    }
}
