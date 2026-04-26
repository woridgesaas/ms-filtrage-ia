namespace ms_filtering_ia.Messaging.Events;

public sealed class CandidateAcceptedEvent
{
    public int      OfferId        { get; set; }
    public string   CandidateName  { get; set; } = string.Empty;
    public string   CandidateEmail { get; set; } = string.Empty;
    public string   CandidatePhone { get; set; } = string.Empty;
    public string   Location       { get; set; } = string.Empty;
    public string   MainDomain     { get; set; } = string.Empty;
    public double   AiScore        { get; set; }
    public string   CvMinioUrl     { get; set; } = string.Empty;
    public DateTime AcceptedAt     { get; set; }
    public string   UserCin        { get; set; } = string.Empty;
}