namespace ms_filtering_ia.DTOs;

public sealed class AcceptCandidateResponse
{
    public int    Id               { get; set; }
    public string CandidateName    { get; set; } = default!;
    public string Decision         { get; set; } = default!;
    public string Message          { get; set; } = default!;
}