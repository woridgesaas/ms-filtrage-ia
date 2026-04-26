using ms_filtering_ia.DTOs;
using ms_filtering_ia.Models;
using ms_filtering_ia.Repositories;

namespace ms_filtering_ia.Services;

public class FilteringProfileService : IFilteringProfileService
{
    private readonly IFilteringProfileRepository _repository;
    private readonly ILogger<FilteringProfileService> _logger;

    public FilteringProfileService(
        IFilteringProfileRepository repository,
        ILogger<FilteringProfileService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CreateFilteringProfileResponse> CreateProfileAsync(
        CreateFilteringProfileRequest request, string userCin)
    {
        var user = await _repository.GetUserByCinAsync(userCin)
            ?? throw new KeyNotFoundException(
                $"Aucun utilisateur trouvé pour le CIN : {userCin}");

        var criteriaTypes = new List<CriteriaType>();
        foreach (var name in request.CriteriaNames)
        {
            var criteria = await _repository.GetOrCreateCriteriaAsync(name);
            if (criteria is not null)
                criteriaTypes.Add(criteria);
        }

        var profile = new FilteringProfile
        {
            ProfileName = request.ProfileName,
            OfferId = request.OfferId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            CriteriaTypes = criteriaTypes
        };

        await _repository.CreateAsync(profile);
        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "[FilteringProfileService] Profil créé : Id={Id}, UserId={UserId}",
            profile.Id, profile.UserId);

        return new CreateFilteringProfileResponse { Id = profile.Id };
    }
}