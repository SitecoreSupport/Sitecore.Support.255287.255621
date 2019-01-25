namespace Sitecore.Support.EmailCampaign.Cm.UI.Handlers
{
  using System.Threading.Tasks;
  using Sitecore.EmailCampaign.Cm;
  using Sitecore.EmailCampaign.Model.Messaging;
  using Sitecore.EmailCampaign.Model.Messaging.Buses;
  using Sitecore.Framework.Conditions;
  using Sitecore.Framework.Messaging;
  using Sitecore.Framework.Messaging.DeferStrategies;
  using Sitecore.Globalization;
  using Sitecore.Modules.EmailCampaign.Core.Contacts;
  using static System.FormattableString;
  using ILogger = Sitecore.ExM.Framework.Diagnostics.ILogger;
  using SupportConfigrmationSubscribeMessage = Sitecore.Support.EmailCampaign.Model.Messaging.ConfirmSubscriptionMessage;

  public class ConfirmSubscriptionMessageHandler : IMessageHandler<ConfirmSubscriptionMessage>
  {
    private readonly ILogger _logger;
    private readonly ISubscriptionManager _subscriptionManager;
    private readonly IDeferStrategy<DeferDetectionByResultBase<HandlerResult>> _deferStrategy;
    private readonly IMessageBus<ConfirmSubscriptionMessagesBus> _bus;

    public ConfirmSubscriptionMessageHandler([NotNull] ILogger logger, [NotNull] ISubscriptionManager subscriptionManager, IDeferStrategy<DeferDetectionByResultBase<HandlerResult>> deferStrategy, IMessageBus<ConfirmSubscriptionMessagesBus> bus)
    {
      Condition.Requires(logger, nameof(logger)).IsNotNull();
      Condition.Requires(subscriptionManager, nameof(subscriptionManager)).IsNotNull();
      Condition.Requires(deferStrategy, nameof(deferStrategy)).IsNotNull();
      Condition.Requires(bus, nameof(bus)).IsNotNull();

      _logger = logger;
      _subscriptionManager = subscriptionManager;
      _deferStrategy = deferStrategy;
      _bus = bus;
    }

    public async Task Handle([NotNull] ConfirmSubscriptionMessage message, IMessageReceiveContext receiveContext, IMessageReplyContext replyContext)
    {
      Condition.Requires(message, nameof(message)).IsNotNull();
      Condition.Requires(receiveContext, nameof(receiveContext)).IsNotNull();
      Condition.Requires(replyContext, nameof(replyContext)).IsNotNull();

      DeferResult<HandlerResult> result = await _deferStrategy.ExecuteAsync(
          _bus,
          message,
          receiveContext,
          () => ConfirmSubscription(message)).ConfigureAwait(false);

      if (result.Deferred)
      {
        _logger.LogDebug($"[{nameof(ConfirmSubscriptionMessageHandler)}] defered message.");
      }
      else
      {
        _logger.LogDebug($"[{nameof(ConfirmSubscriptionMessageHandler)}] processed message.'");
      }
    }

    protected HandlerResult ConfirmSubscription(ConfirmSubscriptionMessage message)
    {
      _logger.LogDebug(Invariant($"[{nameof(ConfirmSubscriptionMessageHandler)}] Confirming subscription. Key '{message.ConfirmationKey}'."));

      Language language = null;
      bool success = false;

      if (ParseModel(message, out language))
      {
        using (new LanguageSwitcher(language))
        {
          success = _subscriptionManager.ConfirmSubscription(message.ConfirmationKey);
        }
      }
      else
      {
        success = _subscriptionManager.ConfirmSubscription(message.ConfirmationKey);
      }

      if (success)
      {
        return new HandlerResult(HandlerResultType.Successful);
      }

      _logger.LogError(Invariant($"[{nameof(ConfirmSubscriptionMessageHandler)}] Failed to confirm subscription. Key '{message.ConfirmationKey}'."));

      return new HandlerResult(HandlerResultType.Error);
    }

    private bool ParseModel(ConfirmSubscriptionMessage message, out Language language)
    {
      bool result = false;
      language = null;

      SupportConfigrmationSubscribeMessage supportMessage = message as SupportConfigrmationSubscribeMessage;

      if (supportMessage == null)
      {
        return false;
      }

      if (!string.IsNullOrEmpty(supportMessage.MessageLanguage))
      {
        result = Language.TryParse(supportMessage.MessageLanguage, out language);
      }

      return result;
    }
  }
}