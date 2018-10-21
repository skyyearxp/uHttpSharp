using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Timers;

namespace uhttpsharp.Demo.Handlers {
    
    internal interface Session {
        string ID { get; set; }
        DateTime LastAccessTime { get; set; }
        IPEndPoint EndPoint { get; set; }
        void CloseSession();
    }

    internal class SimpleSession : Session {
        public string ID { get; set; }
        public DateTime LastAccessTime { get; set; }
        public IPEndPoint EndPoint { get; set; }

        public void CloseSession() { }
    }

    internal class SessionHandler<TSession> : IHttpRequestHandler
        where TSession : Session {
        private readonly Func<TSession> _sessionFactory;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, TSession> _sessions = new ConcurrentDictionary<string, TSession>();

        private readonly RNGCryptoServiceProvider random
            = new RNGCryptoServiceProvider();

        private Timer cleanupTimer;

        public SessionHandler(Func<TSession> sessionFactory, int sessionExpireMinutes, ILogger logger) {
            _sessionFactory = sessionFactory;
            _logger = logger;
            cleanupTimer = new Timer();
            cleanupTimer.AutoReset = true;
            cleanupTimer.Interval = 60 * 1000;
            cleanupTimer.Elapsed += (s, e) => {
                lock (this) {
                    var expiredSessions = _sessions.Where(a => (DateTime.Now - a.Value.LastAccessTime).TotalMinutes >= sessionExpireMinutes).ToArray();

                    foreach (var i in expiredSessions) {
                        TSession l;
                        if (_sessions.TryRemove(i.Key, out l)) l.CloseSession();
                    }
                }
            };
            cleanupTimer.Start();
        }

        public Task Handle(IHttpContext context, Func<Task> next) {
            string sessId = null;
            string userAgent = null;

            lock (this) {
                if (context.Request.Headers.TryGetByName("User-Agent", out userAgent))
                    if (!context.Cookies.TryGetByName("simple-id", out sessId)
                        || sessId != null && !_sessions.ContainsKey(sessId)) {
                        var key = new byte[24];

                        do {
                            random.GetBytes(key);
                            sessId = Convert.ToBase64String(key).ToLower().Replace("/", "").Replace("=", "");
                        } while (_sessions.ContainsKey(sessId));

                        context.Cookies.Upsert("simple-id", sessId);
                        _logger?.Info($"Generated new session for {((IPEndPoint) context.RemoteEndPoint).Address}");
                    }

                if (sessId != null) {
                    var session = _sessions.GetOrAdd(sessId, CreateSession);
                    session.LastAccessTime = DateTime.Now;
                    session.ID = sessId;
                    session.EndPoint = (IPEndPoint) context.RemoteEndPoint;
                    context.State.Session = session;
                } else {
                    context.State.Session = null;
                }
            }

            return next();
        }

        private TSession CreateSession(string sessionId) {
            return _sessionFactory();
        }
    }
}