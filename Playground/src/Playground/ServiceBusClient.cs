using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Amqp;
using Amqp.Framing;
using Playground.Exe.Interfaces;

namespace Playground.Exe
{
    public class ServiceBusClient : IPublisher, IServiceBusListener
    {
        private Address _address;
        private Connection _connection;
        private Session _session;
        private readonly ISubscriberLocator _locator;

        public ServiceBusClient(ServiceBusSettings settings, ISubscriberLocator locator)
        {
            ConnectionString = $"amqps://{settings.PolicyName}:{settings.Key}@{settings.NamespaceUrl}/";
            _locator = locator;
        }

        protected string ConnectionString { get; }

        protected Address Address => _address ?? (_address = new Address(ConnectionString));

        protected Connection Connection => _connection ?? (_connection = new Connection(Address));

        protected Session Session => _session ?? (_session = new Session(Connection));

        private void OnMessage(ReceiverLink receiver, Message message)
        {
            var messageTypeStr = message.ApplicationProperties["Message.Type.FullName"].ToString();
            var messageType = Type.GetType(messageTypeStr);
            var subscribers = _locator.FindSubscribers(Type.GetType(messageTypeStr)).ToArray();

            if (subscribers != null)
            {
                var messageBody = GetMessageBody(message, messageType);

                foreach (var subscriber in subscribers)
                {
                    var onMessageReceived = typeof(Subscriber).GetMethod(nameof(ISubscriber<string>.OnMessageReceivedAsync));
                    onMessageReceived.Invoke(subscriber, new[] {messageBody});
                }

                receiver.Accept(message);
            }
        }

        private static object GetMessageBody(Message message, Type messageType)
        {
            var rawBody = message.GetBody<object>();

            byte[] bytes;

            bytes = rawBody as byte[];

            if (bytes != null)
            {
                return GetObjectFromByteArray(bytes, messageType);
            }

            var ampqValue = rawBody as AmqpValue;

            if (ampqValue != null)
            {
                bytes = ampqValue.Value as byte[];

                if (bytes != null)
                {
                    return GetObjectFromByteArray(bytes, messageType);
                }

                return ampqValue.Value;
            }

            var strBody = rawBody.ToString();

            return strBody;
        }

        private static object GetObjectFromByteArray(byte[] bytes, Type messageType)
        {
            if (bytes.Length == 0)
            {
                return null;
            }

            using (var ms = new MemoryStream(bytes))
            {
                using (var reader = XmlDictionaryReader.CreateBinaryReader(ms, null, XmlDictionaryReaderQuotas.Max))
                {
                    var serializer = new DataContractSerializer(messageType);
                    var objBody = serializer.ReadObject(reader);
                    return objBody;
                }
            }
        }

        public static Message GetMessage<T>(T model)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = XmlDictionaryWriter.CreateBinaryWriter(ms))
                {
                    var serializer = new DataContractSerializer(model.GetType());
                    serializer.WriteObject(writer, model);
                    writer.Flush();
                    var array = ms.ToArray();
                    var message = new Message(new AmqpValue
                                              {
                                                  Value = array
                                              })
                    {
                        Properties = new Properties
                        {
                            MessageId = Guid.NewGuid().ToString()
                        },
                        ApplicationProperties = new ApplicationProperties
                        {
                            ["Message.Type.FullName"] = typeof(T).FullName
                        }
                    };
                    return message;
                }
            }
        }

        public async Task PublishAsync<T>(T model, string topic)
        {

            var sender = new SenderLink(Session, $"{GetType().FullName}.sender", topic);

            var message = GetMessage(model);
            await sender.SendAsync(message);
            await sender.CloseAsync();
        }

        public Task StartAsync(string topic, string subscription)
        {
            var consumer = new ReceiverLink(Session, $"{GetType().FullName}.reciever",
                                            $"{topic}/Subscriptions/{subscription}");
            consumer.Start(5, OnMessage);
            return Task.CompletedTask;
        }
    }
}