using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uhttpsharp.Headers {
    internal class EmptyHttpPost : IHttpPost {
        private static readonly byte[] _emptyBytes = new byte[0];

        public static readonly IHttpPost Empty = new EmptyHttpPost();

        private EmptyHttpPost() { }

        #region IHttpPost implementation

        public byte[] Raw => _emptyBytes;

        public IHttpHeaders Parsed => EmptyHttpHeaders.Empty;

        #endregion
    }

    internal class HttpPost : IHttpPost {
        private readonly Lazy<IHttpHeaders> _parsed;

        private readonly int _readBytes;

        public HttpPost(byte[] raw, int readBytes) {
            Raw = raw;
            _readBytes = readBytes;
            _parsed = new Lazy<IHttpHeaders>(Parse);
        }

        public static async Task<IHttpPost> Create(StreamReader reader, int postContentLength) {
            var rawEncoded = new char[postContentLength];

            var readBytes = await reader.ReadAsync(rawEncoded, 0, rawEncoded.Length).ConfigureAwait(false);

            var raw = Encoding.UTF8.GetBytes(rawEncoded, 0, readBytes);

            return new HttpPost(raw, readBytes);
        }

        private IHttpHeaders Parse() {
            var body = Encoding.UTF8.GetString(Raw, 0, _readBytes);
            var parsed = new QueryStringHttpHeaders(body);

            return parsed;
        }

        #region IHttpPost implementation

        public byte[] Raw { get; }

        public IHttpHeaders Parsed => _parsed.Value;

        #endregion
    }

    [DebuggerDisplay("{Count} Headers")]
    [DebuggerTypeProxy(typeof(HttpHeadersDebuggerProxy))]
    public class ListHttpHeaders : IHttpHeaders {
        private readonly IList<KeyValuePair<string, string>> _values;

        internal int Count => _values.Count;

        public ListHttpHeaders(IList<KeyValuePair<string, string>> values) {
            _values = values;
        }

        public string GetByName(string name) {
            return _values.Where(kvp => kvp.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(kvp => kvp.Value).First();
        }

        public bool TryGetByName(string name, out string value) {
            value = _values.Where(kvp => kvp.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(kvp => kvp.Value).FirstOrDefault();

            return value != default(string);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    [DebuggerDisplay("{Count} Headers")]
    [DebuggerTypeProxy(typeof(HttpHeadersDebuggerProxy))]
    public class HttpHeaders : IHttpHeaders {
        private readonly IDictionary<string, string> _values;

        internal int Count => _values.Count;

        public HttpHeaders(IDictionary<string, string> values) {
            _values = values;
        }

        public string GetByName(string name) {
            return _values[name];
        }

        public bool TryGetByName(string name, out string value) {
            return _values.TryGetValue(name, out value);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}