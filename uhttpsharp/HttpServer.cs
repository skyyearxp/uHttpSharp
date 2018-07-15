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
using System.Collections.Generic;
using System.Threading.Tasks;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace uhttpsharp {
    public sealed class HttpServer : IDisposable {
        private readonly IList<IHttpRequestHandler> _handlers = new List<IHttpRequestHandler>();
        private readonly IList<IHttpListener> _listeners = new List<IHttpListener>();
        private readonly IHttpRequestProvider _requestProvider;

        private bool _isActive;
        private ILogger _logger;

        public HttpServer(IHttpRequestProvider requestProvider, ILogger log = null) {
            _requestProvider = requestProvider;
            _logger = log;
        }

        public void Dispose() {
            _isActive = false;
        }

        public void Use(IHttpRequestHandler handler) {
            _handlers.Add(handler);
        }

        public void Use(IHttpListener listener) {
            _listeners.Add(listener);
        }

        public void Start() {
            _isActive = true;

            foreach (var listener in _listeners) {
                var tempListener = listener;

                Task.Factory.StartNew(() => Listen(tempListener));
            }

            _logger?.Info("Embedded uhttpserver started.");
        }

        private async void Listen(IHttpListener listener) {
            var aggregatedHandler = _handlers.Aggregate();

            while (_isActive)
                try {
                    new HttpClientHandler(await listener.GetClient().ConfigureAwait(false), aggregatedHandler, _requestProvider, _logger);
                } catch (Exception e) {
                    _logger?.Warn($"Error while getting client", e);
                }

            _logger?.Info("Embedded uhttpserver stopped.");
        }
    }
}