using System;
using NServiceBus;
using System.Data.Common;
using NServiceBus.Persistence.NHibernate;

public class OrderSubmittedHandler : IHandleMessages<OrderSubmitted>
{
    IBus bus;
    NHibernateStorageContext storageContext;

    public OrderSubmittedHandler(IBus bus ,NHibernateStorageContext storageContext)
    {
        this.bus = bus;
        this.storageContext = storageContext;
    }

    public void Handle(OrderSubmitted message)
    {
        Console.WriteLine("Order {0} worth {1} submitted", message.OrderId, message.Value);

        #region StoreUserData

        using (ReceiverDataContext ctx = new ReceiverDataContext(storageContext.Connection))
        {
            ctx.Database.UseTransaction((DbTransaction) storageContext.DatabaseTransaction);
            ctx.Orders.Add(new Order
                            {
                                OrderId = message.OrderId,
                                Value = message.Value
                            });
            ctx.SaveChanges();
        }

        #endregion

        #region Reply

        bus.Reply(new OrderAccepted
                    {
                        OrderId = message.OrderId,
                    });

        #endregion
    }
}
