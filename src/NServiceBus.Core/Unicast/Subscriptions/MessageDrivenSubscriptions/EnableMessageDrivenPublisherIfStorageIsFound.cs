﻿namespace NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions
{
    using Features;
    using Logging;

    /// <summary>
    /// This class handles backwards compatibility. If there is a ISubscription storage registered by the user we should use
    /// the message drive´n subscription manager
    /// </summary>
    public class EnableMessageDrivenPublisherIfStorageIsFound : Configurator
    {
        public override void BeforeFinalizingConfiguration()
        {
            if (IsRegistered<ISubscriptionStorage>())
            {
                Feature.Enable<StorageDrivenPublisher>();
                Logger.InfoFormat("ISubscriptionStorage found in the container. The message driven publisher feature will be activeated");
            }
                
        }

        static readonly ILog Logger = LogManager.GetLogger(typeof(EnableMessageDrivenPublisherIfStorageIsFound));
    }
}