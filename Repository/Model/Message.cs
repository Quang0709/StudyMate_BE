using System;

namespace Repository.Model;

public partial class Message
{
    public Guid Id { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }

    public virtual User Sender { get; set; } = null!;

    public virtual User Receiver { get; set; } = null!;
}