using ms_filtering_ia.DTOs;
using ms_filtering_ia.Models;

namespace ms_filtering_ia.Mappers;

public static class FilteringResultMapper
{
    public static FilteringResultResponse ToDto(
        FilteringResult result,
        ExtractionData? data = null,
        CandidateMatch? candidate = null)
    {
        var techSkills = data is not null
            ? data.TechnicalSkills.Concat(data.Tools).Distinct().ToList()
            : result.ExtractedSkills
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

        var softSkills = data is not null
            ? data.SoftSkills
            : result.ExtractedSoftSkills
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

        var missingSkills = candidate is not null
            ? candidate.MissingSkills
            : result.MissingSkills
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

        return new FilteringResultResponse
        {
            Id              = result.Id,
            CandidateName   = result.CandidateName,
            CandidateEmail  = result.CandidateEmail,
            CandidatePhone  = result.CandidatePhone,
            Location        = result.CandidateLocation,
            ScoreIa         = result.GlobalScore,
            ExperienceYears = result.ExperienceYears,
            TechnicalSkills = techSkills,
            SoftSkills      = softSkills,
            MissingSkills   = missingSkills,
            Recommendation  = candidate?.Recommendation ?? result.Recommendation,
            MainDomain      = result.MainDomain,
            SeniorityLevel  = result.SeniorityLevel,
            CvStatus        = result.CvStatus,
            CvDownloadUrl   = $"minio://{result.CvMinioPath}",
            AnalysisDate    = result.AnalysisDate,
        };
    }

    public static List<FilteringResultResponse> ToDtoList(List<FilteringResult> results)
        => results.ConvertAll(r => ToDto(r));
}