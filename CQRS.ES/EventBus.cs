using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.Common;
using System.Threading;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;
using Amazon;
using System.Configuration;

namespace CQRS.ES
{
    public class EventBus : IEventPublisher
    {
        public readonly Dictionary<Type, List<Action<Message>>> _routes = new Dictionary<Type, List<Action<Message>>>();

        public void RegisterHandler<T>(Action<T> handler) where T : Message
        {
            List<Action<Message>> handlers;

            if (!_routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Action<Message>>();
                _routes.Add(typeof(T), handlers);
            }

            handlers.Add((x => handler((T)x)));
        }

        public void Publish<T>(T @event) where T : Event
        {
            List<Action<Message>> handlers;

            if (!_routes.TryGetValue(@event.GetType(), out handlers)) return;

            //foreach (var handler in handlers)
            //{
                //dispatch on thread pool for added awesomeness
                //var handler1 = handler;
                var sns = new AmazonSimpleNotificationServiceClient(ConfigurationManager.AppSettings["AWSAccessKey"], ConfigurationManager.AppSettings["AWSSecretKey"], RegionEndpoint.USWest2);
                var res = sns.Publish(new PublishRequest(ConfigurationManager.AppSettings["TopicArn"], JsonConvert.SerializeObject(@event)));
                //ThreadPool.QueueUserWorkItem(x => handler1(@event));
            //}
        }
    }
}
