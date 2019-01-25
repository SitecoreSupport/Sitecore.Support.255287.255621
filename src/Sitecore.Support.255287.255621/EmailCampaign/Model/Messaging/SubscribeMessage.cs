namespace Sitecore.Support.EmailCampaign.Model.Messaging
{
  using SubscribeMessageOrigin = Sitecore.EmailCampaign.Model.Messaging.SubscribeMessage;

  public class SubscribeMessage : SubscribeMessageOrigin
  {
    public string MessageLanguage { get; set; }
  }
}