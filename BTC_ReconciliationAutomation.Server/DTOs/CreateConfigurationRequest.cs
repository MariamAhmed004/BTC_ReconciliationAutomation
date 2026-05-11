namespace BTC_ReconciliationAutomation.Server.DTOs
{
    public class CreateConfigurationRequest
    {
        public string? EMAIL_RECIPIENTS { get; set; }
        public string? RUN_TIME { get; set; }
        public string? FREQUENCY { get; set; }
        public string? DAY_OF_MONTH { get; set; }
        public decimal? DAYS_TO_DELETE_AUDITLOGS { get; set; }
        public string? DEFAULT_FILE_PATH { get; set; }
        public string? IGNORE_CONDITIONS { get; set; }
        public string? ADDED_BY { get; set; }
    }
}
