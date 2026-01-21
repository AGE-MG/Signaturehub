using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Domain.Entities;
using AutoMapper;

namespace AGE.SignatureHub.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Document, DocumentDto>()
                .ForMember(dest => dest.SignatureFlows, opt => opt.MapFrom(src => src.SignatureFlows));
            
            CreateMap<CreateDocumentDto, Document>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FileName, opt => opt.Ignore())
                .ForMember(dest => dest.OriginalFileName, opt => opt.Ignore())
                .ForMember(dest => dest.FileExtension, opt => opt.Ignore())
                .ForMember(dest => dest.FileSizeInBytes, opt => opt.Ignore())
                .ForMember(dest => dest.StoragePath, opt => opt.Ignore())
                .ForMember(dest => dest.ContentHash, opt => opt.Ignore())
                .ForMember(dest => dest.MimeType, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.SignatureFlows, opt => opt.Ignore())
                .ForMember(dest => dest.Versions, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }
    }
}