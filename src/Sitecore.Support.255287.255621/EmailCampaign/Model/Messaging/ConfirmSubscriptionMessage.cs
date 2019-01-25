namespace Sitecore.Support.EmailCampaign.Model.Messaging
{
  using ConfirmSubscriptionMessageOrigin = Sitecore.EmailCampaign.Model.Messaging.ConfirmSubscriptionMessage;

  public class ConfirmSubscriptionMessage : ConfirmSubscriptionMessageOrigin
  {
    public string MessageLanguage { get; set; }
  }
}