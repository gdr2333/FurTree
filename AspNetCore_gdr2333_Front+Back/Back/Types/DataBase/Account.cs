﻿using System.ComponentModel.DataAnnotations;

namespace Back.Types.DataBase;

public class Account
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    public bool EmailConfired { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public bool Locked { get; set; } = false;
    public DateTime BannedTo { get; set; } = DateTime.MinValue;
    public DateTime LastGlobalMessageReadTime { get; set; } = DateTime.MinValue;

    public int ThisHourPostSend { get; set; } = 0;
    public int ThisDayPostSend { get; set; } = 0;
    public int ThisHourPostCommentSend { get; set; } = 0;
    public int ThisDayPostCommentSend { get; set; } = 0;
    public int ThisHourTreehollowSend { get; set; } = 0;
    public int ThisDayTreehollowSend { get; set; } = 0;
    public int ThisHourTreehollowCommentSend { get; set; } = 0;
    public int ThisDayTreehollowCommentSend { get; set; } = 0;
    public int ThisDayUnresivedPrivateMessageSend { get; set; } = 0;
    public DateTime LastConfirmEmailSendTime { get; set; } = DateTime.MinValue;

    public Account(string name, string email, byte[] passwordHash)
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
    }

    public Account()
    {

    }
}
