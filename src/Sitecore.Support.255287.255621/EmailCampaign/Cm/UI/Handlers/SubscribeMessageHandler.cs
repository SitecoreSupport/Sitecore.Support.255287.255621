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
  using SupportSubscribeMessage = Sitecore.Support.EmailCampaign.Model.Messaging.SubscribeMessage;

  public class SubscribeMessageHandler : IMessageHandler<SubscribeMessage>
  {
    private readonly ILogger _logger;
    private readonly ISubscriptionManager _subscriptionManager;
    private readonly IDeferStrategy<DeferDetectionByResultBase<HandlerResult>> _deferStrategy;
    private readonly IMessageBus<SubscribeMessagesBus> _bus;

    public SubscribeMessageHandler([NotNull] ILogger logger, [NotNull] ISubscriptionManager subscriptionManager, IDeferStrategy<DeferDetectionByResultBase<HandlerResult>> deferStrategy, IMessageBus<SubscribeMessagesBus> bus)
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

    public async Task Handle([NotNull] SubscribeMessage message, IMessageReceiveContext receiveContext, IMessageReplyContext replyContext)
    {
      Condition.Requires(message, nameof(message)).IsNotNull();
      Condition.Requires(receiveContext, nameof(receiveContext)).IsNotNull();
      Condition.Requires(replyContext, nameof(replyContext)).IsNotNull();

      DeferResult<HandlerResult> result = await _deferStrategy.ExecuteAsync(
          _bus,
          message,
          receiveContext,
          () => Subscribe(message)).ConfigureAwait(false);

      if (result.Deferred)
      {
        _logger.LogDebug($"[{nameof(SubscribeMessageHandler)}] defered message.");
      }
      else
      {
        _logger.LogDebug($"[{nameof(SubscribeMessageHandler)}] processed message.'");
      }
    }

    protected HandlerResult Subscribe(SubscribeMessage message)
    {
      _logger.LogDebug(Invariant($"[{nameof(SubscribeMessageHandler)}] Subscribing '{message.ContactIdentifier.ToLogFile()}' from '{message.MessageId}'. Require subscription confirmation: '{message.RequireSubscriptionConfirmation}'"));
      Language language = null;
      bool subscribed = false;

      if (ParseModel(message, out language))
      {
        using (new LanguageSwitcher(language))
        {
          subscribed = _subscriptionManager.Subscribe(message.ContactIdentifier, message.MessageId, message.RequireSubscriptionConfirmation);
        }
      }
      else
      {
        subscribed = _subscriptionManager.Subscribe(message.ContactIdentifier, message.MessageId, message.RequireSubscriptionConfirmation);
      }

      if (subscribed)
      {
        return new HandlerResult(HandlerResultType.Successful);
      }

      _logger.LogError(Invariant($"[{nameof(SubscribeMessageHandler)}] Failed to subscribe '{message.ContactIdentifier.ToLogFile()}' from '{message.MessageId}'. Require subscription confirmation: '{message.RequireSubscriptionConfirmation}'"));
      return new HandlerResult(HandlerResultType.Error);
    }

    private bool ParseModel(SubscribeMessage message, out Language language)
    {
      bool result = false;
      language = null;
      SupportSubscribeMessage supportMessage = message as SupportSubscribeMessage;
      
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