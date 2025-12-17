namespace EventManager.Models
{
    public class EventRevenueViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal TicketPrice { get; set; }
        public int CurrentParticipants { get; set; }
        // ✅ Luôn đúng vì tính dựa trên 2 trường còn lại
        public decimal TotalRevenue => TicketPrice * CurrentParticipants;

    }

}
