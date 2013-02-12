namespace NServiceBus.Core.Tests.Persistence.RavenDB.SagaPersister
{
    using System;
    using NServiceBus.Persistence.Raven;
    using NUnit.Framework;
    using Raven.Client;
    using Raven.Client.Embedded;
    using NServiceBus.Saga;
    using SagaPersisters.Raven;

    public abstract class Raven_saga_persistence_concern
    {
        protected IDocumentStore store;

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            store = new EmbeddableDocumentStore { RunInMemory = true, DataDirectory = Guid.NewGuid().ToString() };
            //store = new DocumentStore
            //            {
            //                Url = "http://localhost:8080",
            //            };

            var conventions = new RavenConventions();

            store.Conventions.FindTypeTagName = conventions.FindTypeTagName;
            store.Initialize();
        }
        [TestFixtureTearDown]
        public virtual void Teardown()
        {
            store.Dispose();
        }
        public void WithASagaPersistenceUnitOfWork(Action<RavenSagaPersister> action)
        {
            var sessionFactory = new RavenSessionFactory(store);

            try
            {
                var sagaPersister = new RavenSagaPersister(sessionFactory);
                action(sagaPersister);

                sessionFactory.SaveChanges();
            }
            finally 
            {
                sessionFactory.ReleaseSession();
                
            }           
        }

        protected void SaveSaga<T>(T saga) where T : ISagaEntity
        {
            WithASagaPersistenceUnitOfWork(p => p.Save(saga));
        }

        protected void CompleteSaga<T>(Guid sagaId) where T : ISagaEntity
        {
            WithASagaPersistenceUnitOfWork(p =>
                                           {
                                               var saga = p.Get<T>(sagaId);
                                               Assert.NotNull(saga, "Could not complete saga. Saga not found");
                                               p.Complete(saga);
                                           });
        }

        protected void UpdateSaga<T>(Guid sagaId, Action<T> update) where T : ISagaEntity
        {
            WithASagaPersistenceUnitOfWork(p =>
            {
                var saga = p.Get<T>(sagaId);
                Assert.NotNull(saga, "Could not update saga. Saga not found");
                update(saga);
                p.Update(saga);
            });
        }
    }
}