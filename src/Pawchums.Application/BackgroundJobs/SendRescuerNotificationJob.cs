using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pawchums.Entities.RequestRescues;
using Pawchums.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Pawchums.BackgroundJobs;

/// <summary>
/// Background job for sending rescuer notifications
/// ABP will automatically use Hangfire as the provider
/// </summary>
public class SendRescuerNotificationsJob
    : AsyncBackgroundJob<SendRescuerNotificationsArgs>, ITransientDependency
{
    private readonly IRepository<RequestRescue, Guid> _requestRescueRepository;
    private readonly RescuerNotificationEmailService _notificationService;
    private readonly ILogger<SendRescuerNotificationsJob> _logger;

    public SendRescuerNotificationsJob(
        IRepository<RequestRescue, Guid> requestRescueRepository,
        RescuerNotificationEmailService notificationService,
        ILogger<SendRescuerNotificationsJob> logger)
    {
        _requestRescueRepository = requestRescueRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(SendRescuerNotificationsArgs args)
    {
        try
        {
            _logger.LogInformation("Hangfire Job: Starting notification job for request {RequestId}",
                args.RequestRescueId);

            // Fetch the request with fresh DbContext in new scope
            var request = await _requestRescueRepository.GetAsync(args.RequestRescueId);

            if (request == null)
            {
                _logger.LogWarning("Hangfire Job: Request {RequestId} not found", args.RequestRescueId);
                return;
            }

            // Send notifications to ALL rescuers
            _logger.LogInformation("Hangfire Job: Sending notifications to all rescuers for request {RequestId}",
                args.RequestRescueId);

            await _notificationService.NotifyAllRescuersAsync(request);

            _logger.LogInformation("Hangfire Job: Successfully completed notifications for request {RequestId}",
                args.RequestRescueId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hangfire Job: Error sending notifications for request {RequestId}",
                args.RequestRescueId);
            throw; // Hangfire will retry failed jobs automatically
        }
    }
}