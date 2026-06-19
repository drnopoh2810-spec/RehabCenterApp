using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using RehabCenterApp.Models;

namespace RehabCenterApp.Services;

public class NotificationService
{
    private readonly DatabaseService _dbService;
    private System.Timers.Timer? _timer;
    private List<Reminder> _pendingReminders = new();

    public event EventHandler<Reminder>? ReminderTriggered;

    public NotificationService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public void Start()
    {
        _timer = new System.Timers.Timer(60000); // Check every minute
        _timer.Elapsed += async (s, e) => await CheckRemindersAsync();
        _timer.Start();
    }

    public void Stop()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }

    private async Task CheckRemindersAsync()
    {
        try
        {
            var reminders = await _dbService.GetUpcomingRemindersAsync(1);
            var now = DateTime.Now;

            foreach (var reminder in reminders)
            {
                if (reminder.DateTime <= now && !_pendingReminders.Contains(reminder))
                {
                    _pendingReminders.Add(reminder);
                    ReminderTriggered?.Invoke(this, reminder);
                }
            }
        }
        catch { /* Ignore errors in background timer */ }
    }
}