namespace ThAmCo.Staffs.DTOs
{
    public class DispatchedDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public bool IsDispatched { get; set; }
        public DateTime? DispatchedAt { get; set; }
    }
}
