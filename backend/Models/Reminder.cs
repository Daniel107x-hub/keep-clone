public class Reminder {
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Deadline { get; set; }
    public bool IsActive { get; set; }
}