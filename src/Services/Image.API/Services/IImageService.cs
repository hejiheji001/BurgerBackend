using Microsoft.AspNetCore.WebUtilities;

namespace Image.API.Services;

public interface IImageService
{
    bool ImageValidation();
    Task<bool> ListingImageUpload(MultipartReader multipartReader,int listingId);
    Task<bool> ReviewImageUpload(MultipartReader multipartReader, int reviewId);
    bool ImageDelete();
    bool ImageUpdate();
    Task<bool> ImageResize(string listingName, string fileName);
}