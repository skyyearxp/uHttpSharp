/*
 * Copyright (C) 2011 uhttpsharp project - http://github.com/raistlinthewiz/uhttpsharp
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.

 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using uhttpsharp.Demo.Controllers;
using uhttpsharp.Demo.Handlers;
using uhttpsharp.Demo.Logging;
using uhttpsharp.Handlers;
using uhttpsharp.Handlers.Compression;
using uhttpsharp.Listeners;
using uhttpsharp.ModelBinders;
using uhttpsharp.RequestProviders;

namespace uhttpsharp.Demo {
    internal static class Program {
        private static void Main() {
            var logger = new NLogger(nameof(Program));

            //var serverCertificate = X509Certificate.CreateFromCertFile(@"TempCert.cer");

            using (var httpServer = new HttpServer(new HttpRequestProvider(), logger)) {

                var httpPort = 8088;
                var ipAddr = LocalMachineIpAddress ?? IPAddress.Loopback;
                
                httpServer.Use(new TcpListenerAdapter(new TcpListener(ipAddr, httpPort)));

                //httpServer.Use(new ListenerSslDecorator(new TcpListenerAdapter(new TcpListener(IPAddress.Loopback, 443)), serverCertificate));

                httpServer.Use(new ExceptionHandler());
                httpServer.Use(new SessionHandler<SimpleSession>(() => new SimpleSession(), int.MaxValue, logger));
                httpServer.Use(new ControllerHandler(new DerivedController(), new ModelBinder(new ObjectActivator()), new JsonView()));

                httpServer.Use(new CompressionHandler(DeflateCompressor.Default, GZipCompressor.Default));
                httpServer.Use(new ControllerHandler(new BaseController(), new JsonModelBinder(), new JsonView()));
                httpServer.Use(new HttpRouter().With(string.Empty, new IndexHandler())
                    .With("about", new AboutHandler())
                    .With("Assets", new AboutHandler())
                    .With("strings", new RestHandler<string>(new StringsRestController(), JsonResponseProvider.Default)));

                httpServer.Use(new ClassRouter(new MySuperHandler()));
                httpServer.Use(new TimingHandler(logger));

                httpServer.Use(new MyHandler());
                httpServer.Use(new FileHandler());
                httpServer.Use(new ErrorHandler());
                httpServer.Use((context, next) => {
                    Console.WriteLine("Got Request!");
                    return next();
                });

                httpServer.Start();
                logger.Info($"Started server : http://{ipAddr}:{httpPort}");
                logger.Info($"Press any key to stop");
                
                Console.ReadLine();
                
                logger.Warn($"Stopping server...");
            }
        }
        
        /// <summary>
        /// get preferred outbound IP address of local machine or by default 127.0.0.1
        /// </summary>
        private static IPAddress LocalMachineIpAddress {
            get {
                // 
                var localIp = IPAddress.Loopback;
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                    socket.Connect("8.8.8.8", 65530);
                    if (socket.LocalEndPoint is IPEndPoint endPoint) {
                        localIp = endPoint.Address;
                    }
                }
    
                return localIp;
            }
        }
    }

    public class MySuperHandler : IHttpRequestHandler {
        private int _index;

        public MySuperHandler Child {
            get {
                _index++;
                return this;
            }
        }

        public Task Handle(IHttpContext context, Func<Task> next) {
            context.Response = HttpResponse.CreateWithMessage(HttpResponseCode.Ok, "Hello!" + _index, true);
            return Task.Factory.GetCompleted();
        }

        [Indexer]
        public Task<IHttpRequestHandler> GetChild(IHttpContext context, int index) {
            _index += index;
            return Task.FromResult<IHttpRequestHandler>(this);
        }
    }

    internal class MyModel {
        public int MyProperty { get; set; }

        public MyModel Model { get; set; }
    }

    internal class MyHandler : IHttpRequestHandler {
        public Task Handle(IHttpContext context, Func<Task> next) {
            var model = new ModelBinder(new ObjectActivator()).Get<MyModel>(context.Request.QueryString);
            return Task.Factory.GetCompleted();
        }
    }
}