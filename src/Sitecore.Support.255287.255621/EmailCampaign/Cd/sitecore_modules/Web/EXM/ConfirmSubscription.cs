namespace Sitecore.Support.EmailCampaign.Cd.sitecore_modules.Web.EXM
{
  using Microsoft.Extensions.DependencyInjection;
  using Sitecore.DependencyInjection;
  using Sitecore.EmailCampaign.Cd.Services;
  using Sitecore.EmailCampaign.Model.Web.Settings;
  using Sitecore.ExM.Framework.Diagnostics;
  using Sitecore.Modules.EmailCampaign.Core;
  using System;
  using ConfirmSubscriptionOrigin = Sitecore.EmailCampaign.Cd.sitecore_modules.Web.EXM.ConfirmSubscription;
  using ConfirmSubscriptionMessage = Sitecore.Support.EmailCampaign.Model.Messaging.ConfirmSubscriptionMessage;
  using Sitecore.Configuration;

  public class ConfirmSubscription : ConfirmSubscriptionOrigin
  {
    private readonly IClientApiService _clientApiService;
    private readonly ILogger _logger;

    public ConfirmSubscription() : base()
    {
      _clientApiService = ServiceProviderServiceExtensions.GetService<IClientApiService>(ServiceLocator.ServiceProvider);
      _logger = ServiceProviderServiceExtensions.GetService<ILogger>(ServiceLocator.ServiceProvider);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!ExmContext.IsEcmIdVerified)
      {
        this._logger.LogError("Attempt to access ConfirmSubscription with unverified url: " + base.Request.RawUrl);
      }
      else
      {
        string str = base.Request.QueryString[GlobalSettings.ConfirmSubscriptionQueryStringKey];
        string language = string.IsNullOrEmpty(ExmContext.Message.TargetLanguage.Name) ? Settings.DefaultLanguage : ExmContext.Message.TargetLanguage.Name;

        if (!string.IsNullOrEmpty(str))
        {
          ConfirmSubscriptionMessage confirmSubscriptionMessage = new ConfirmSubscriptionMessage
          {
            ConfirmationKey = str,
            MessageLanguage = language
          };

          this._clientApiService.ConfirmSubscription(confirmSubscriptionMessage);
        }

        string confirmativePageUrl = ExmContext.Message.ManagerRoot.GetConfirmativePageUrl();
        base.Response.Redirect(string.IsNullOrWhiteSpace(confirmativePageUrl) ? "/" : confirmativePageUrl, false);
      }
    }


  }
}