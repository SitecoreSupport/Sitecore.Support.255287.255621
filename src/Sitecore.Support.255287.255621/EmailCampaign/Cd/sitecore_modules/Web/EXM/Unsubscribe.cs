namespace Sitecore.Support.EmailCampaign.Cd.sitecore_modules.Web.EXM
{
  using Sitecore.Modules.EmailCampaign.Core;
  using Sitecore.XConnect;
  using System;
  using UnsubscribeOrigin = Sitecore.EmailCampaign.Cd.sitecore_modules.Web.EXM.Unsubscribe;
  using SupportUnsubscribeMessage = Model.Messaging.UnsubscribeMessage;

  public class Unsubscribe : UnsubscribeOrigin
  {
    protected override string UnsubscribeContact(ContactIdentifier contactIdentifier, Guid messageID)
    {
      SupportUnsubscribeMessage unsubscribeMessage = new SupportUnsubscribeMessage
      {
        AddToGlobalOptOutList = false,
        ContactIdentifier = contactIdentifier,
        MessageId = messageID,
        MessageLanguage = LanguageName
      };
      base.ClientApiService.Unsubscribe(unsubscribeMessage);
      return (ExmContext.Message.ManagerRoot.GetConfirmativePageUrl() ?? "/");

    }
    protected override string LanguageName
    {
      get
      {
        return string.IsNullOrEmpty(ExmContext.Message.TargetLanguage.Name) ? base.LanguageName : ExmContext.Message.TargetLanguage.Name;
      }
    }
  }
}