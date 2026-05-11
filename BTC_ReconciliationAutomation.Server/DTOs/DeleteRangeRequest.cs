namespace BTC_ReconciliationAutomation.Server.DTOs
{
    public class DeleteRangeRequest
    {
        public System.DateTime? From { get; set; }
        public System.DateTime? To { get; set; }
        public int? Days { get; set; }
    }
}
