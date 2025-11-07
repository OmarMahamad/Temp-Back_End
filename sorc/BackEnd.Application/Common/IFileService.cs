using BackEnd.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Common
{
     public interface IFileService
    {
        public record UploadResultDto(string Url, string PublicId, string Format);
        Task<Response<UploadResultDto>> UploadAsync(Stream? imageStream, string? fileName, DefaultImageType imageType, CancellationToken ct = default);
        Task<Response<UploadResultDto>> UpdateAsync(Stream imageStream, string fileName, string oldpuplicid, CancellationToken ct = default);
        Task<Response> DeleteAsync(string publicImageId, CancellationToken ct = default);
    }
}
