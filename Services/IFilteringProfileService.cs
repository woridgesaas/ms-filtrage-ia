using ms_filtering_ia.DTOs;

namespace ms_filtering_ia.Services;

public interface IFilteringProfileService
{
    Task<CreateFilteringProfileResponse> CreateProfileAsync(
        CreateFilteringProfileRequest request, string userCin);
}