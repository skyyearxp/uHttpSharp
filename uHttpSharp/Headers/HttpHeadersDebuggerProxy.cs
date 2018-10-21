using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace uhttpsharp.Headers {
    internal class HttpHeadersDebuggerProxy {
        private readonly IHttpHeaders _real;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public HttpHeader[] Headers {
            get { return _real.Select(kvp => new HttpHeader(kvp)).ToArray(); }
        }

        public HttpHeadersDebuggerProxy(IHttpHeaders real) {
            _real = real;
        }

        [DebuggerDisplay("{Value,nq}", Name = "{Key,nq}")]
        internal class HttpHeader {
            private readonly KeyValuePair<string, string> _header;

            public string Value => _header.Value;

            public string Key => _header.Key;

            public HttpHeader(KeyValuePair<string, string> header) {
                _header = header;
            }
        }
    }
}