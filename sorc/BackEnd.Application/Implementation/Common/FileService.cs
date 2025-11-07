using BackEnd.Application.Common;
using BackEnd.Application.DTOs.Common;
using BackEnd.Application.ModelOptions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static BackEnd.Application.Common.IFileService;

namespace BackEnd.Application.Implementation.Common
{
    public class FileService : IFileService
    {
        private static string DefaultUserImageUrl = "";
        private static string DefaultBranchImageUrl = "";
        private readonly Cloudinary _cloudinary;
        private readonly CloudinaryOptions _options;
        public FileService(IOptions<CloudinaryOptions> options)
        {

            _options = options.Value;
            var account = new Account(_options.CloudName, _options.ApiKey, _options.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<Response> DeleteAsync(string publicImageId, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrEmpty(publicImageId))
                    return Response.Failure("Public Image ID is required");

                // مثال: حذف الصورة من Cloudinary
                var deletionParams = new DeletionParams(publicImageId);
                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Result == "ok" || result.Result == "not found")
                {
                    return Response.Success("Image deleted successfully.");
                }

                return Response.Failure("Failed to delete the image: " + result.Result);
            }
            catch (Exception ex)
            {
                return Response.Failure("Error deleting image: " + ex.Message);
            }
        }
        public async Task<Response<UploadResultDto>> UpdateAsync(Stream imageStream, string fileName, string oldPublicId, CancellationToken ct = default)
        {
            try
            {
                if (imageStream == null || string.IsNullOrEmpty(fileName))
                    return Response<UploadResultDto>.Failure("Image stream and file name are required.");

                //  حذف الصورة القديمة إذا موجودة
                if (!string.IsNullOrEmpty(oldPublicId))
                {
                    var deleteResult = await DeleteAsync(oldPublicId, ct);
                    if (!deleteResult.IsSuccess)
                    {
                        // يمكن تجاهل الفشل إذا الصورة غير موجودة أو تسجيله فقط
                        Console.WriteLine($"Warning: Failed to delete old image: {deleteResult.Message}");
                    }
                }

                //  رفع الصورة الجديدة
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, imageStream),
                    PublicId = Guid.NewGuid().ToString() // يمكن توليد PublicId جديد أو استخدام اسم الملف
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    return Response<UploadResultDto>.Failure("Failed to upload image.");

                return Response<UploadResultDto>.Success(
                    new UploadResultDto(
                        Url: uploadResult.SecureUrl?.ToString() ?? string.Empty,
                        PublicId: uploadResult.PublicId ?? string.Empty,
                        Format: uploadResult.Format ?? _options.DeliveryFormat ?? string.Empty
                    ),
                    "Image uploaded successfully"
                );
            }
            catch (Exception ex)
            {
                return Response<UploadResultDto>.Failure("Error updating image: " + ex.Message);
            }
        }

        private Response<UploadResultDto> UploadDefaultImage(DefaultImageType imageFor)
        {
            try
            {
                return imageFor switch
                {
                    DefaultImageType.User => Response<UploadResultDto>.Success(
                        new UploadResultDto(DefaultUserImageUrl, "default_user", "png"),
                        "Default user image assigned"
                    ),
                    DefaultImageType.Branch => Response<UploadResultDto>.Success(
                        new UploadResultDto(DefaultBranchImageUrl, "default_branch", "jpg"),
                        "Default branch image assigned"
                    ),
                    _ => Response<UploadResultDto>.Success(
                        new UploadResultDto("/images/default.png", "default_general", "png"),
                        "Default image assigned"
                    ),
                };
            }
            catch (Exception ex)
            {
                return Response<UploadResultDto>.Failure("Error assigning default image: " + ex.Message);
            }
        }



        public async Task<Response<UploadResultDto>> UploadAsync(Stream? imageStream, string? fileName, DefaultImageType imageType, CancellationToken ct = default)
        {
            try
            {
                _options.Folder = imageType switch
                {
                    DefaultImageType.User => "UserImages",
                    DefaultImageType.Branch => "BrancheLogos",
                    _ => "General"
                };

                //  حالة: المستخدم ما رفعش صورة
                if (imageStream == null || string.IsNullOrEmpty(fileName))
                    return UploadDefaultImage(imageType);

                if (!imageStream.CanRead)
                    return Response<UploadResultDto>.Failure("Invalid image stream.");

                ct.ThrowIfCancellationRequested();

                if (imageStream.CanSeek)
                    imageStream.Position = 0;

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, imageStream),
                    Folder = _options.Folder,
                    Overwrite = false,
                    Invalidate = false,
                    Format = string.IsNullOrWhiteSpace(_options.DeliveryFormat)
                        ? null
                        : _options.DeliveryFormat
                };

                var result = await _cloudinary.UploadAsync(uploadParams, ct).ConfigureAwait(false);

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    var errorMsg = result.Error?.Message ?? "Unknown error";
                    return Response<UploadResultDto>.Failure($"Cloudinary upload failed: {errorMsg}");
                }

                return Response<UploadResultDto>.Success(
                    new UploadResultDto(
                        Url: result.SecureUrl?.ToString() ?? string.Empty,
                        PublicId: result.PublicId ?? string.Empty,
                        Format: result.Format ?? _options.DeliveryFormat ?? string.Empty
                    ),
                    "Image uploaded successfully"
                );
            }
            catch (Exception ex)
            {
                return Response<UploadResultDto>.Failure($"Exception: {ex.Message}");
            }
        }

       
    }
}
