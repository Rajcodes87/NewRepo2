using System;

namespace Pawchums.BackgroundJobs;

/// <summary>
/// Arguments for sending rescuer notifications background job
/// </summary>
public class SendRescuerNotificationsArgs
{
    public Guid RequestRescueId { get; set; }

    public SendRescuerNotificationsArgs()
    {
    }

    public SendRescuerNotificationsArgs(Guid requestRescueId)
    {
        RequestRescueId = requestRescueId;
    }
}