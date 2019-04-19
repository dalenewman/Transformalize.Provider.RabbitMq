using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace Test.Integration {
   [TestClass]
   public class HelloWorld {

      private static readonly string QueueName = "hello";

      [TestMethod]
      public void Send() {
         var factory = new ConnectionFactory() { HostName = "localhost" };
         using (var connection = factory.CreateConnection()) {
            using (var channel = connection.CreateModel()) {
               channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
               for (int i = 1; i < 4; i++) {
                  var message = "Hello World " + i;
                  var body = Encoding.UTF8.GetBytes(message);
                  channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: null, body: body);
               }
            }
         }
      }

      [TestMethod]
      public void Receive() {
         var factory = new ConnectionFactory() { HostName = "localhost" };
         using (var connection = factory.CreateConnection()) {

            var channel = connection.CreateModel();
            
            channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            // setup a receive event
            consumer.Received += (model, ea) => {
               var body = ea.Body;
               var message = Encoding.UTF8.GetString(body);
               channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
               System.Diagnostics.Trace.WriteLine(message);
            };

            while (channel.MessageCount(queue: QueueName) > 0) {
               channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            }

            Thread.Sleep(1000);

            channel.Close();

         }
      }
   }
}
