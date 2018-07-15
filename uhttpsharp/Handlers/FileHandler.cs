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
using System.IO;
using System.Threading.Tasks;
using uhttpsharp.Headers;

namespace uhttpsharp.Handlers {
    public class FileHandler : IHttpRequestHandler {
        public List<string> DefaultFiles = new List<string> {"index.html", "default.html"};
        public static string DefaultMimeType { get; set; }
        public static string HttpRootDirectory { get; set; }

        public static IDictionary<string, string> MimeTypes { get; }

        static FileHandler() {
            DefaultMimeType = "text/plain";
            MimeTypes = new Dictionary<string, string> {
                {".css", "text/css"},
                {".gif", "image/gif"},
                {".htm", "text/html"},
                {".html", "text/html"},
                {".jpg", "image/jpeg"},
                {".js", "application/javascript"},
                {".png", "image/png"},
                {".xml", "application/xml"}
            };
        }

        public async Task Handle(IHttpContext context, Func<Task> next) {
            var requestPath = context.Request.Uri.OriginalString.TrimStart('/');

            if (string.IsNullOrEmpty(requestPath)) requestPath = GetDefault();

            var httpRoot = Path.GetFullPath(HttpRootDirectory ?? ".");
            var path = Path.GetFullPath(Path.Combine(httpRoot, requestPath));

            if (!File.Exists(path)) {
                await next().ConfigureAwait(false);
                return;
            }

            context.Response = new HttpResponse(GetContentType(path), File.OpenRead(path), context.Request.Headers.KeepAliveConnection());
        }

        private string GetContentType(string path) {
            var extension = Path.GetExtension(path) ?? string.Empty;
            if (MimeTypes.ContainsKey(extension))
                return MimeTypes[extension];
            return DefaultMimeType;
        }

        private string GetDefault() {
            var httpRoot = Path.GetFullPath(HttpRootDirectory ?? ".");
            foreach (var @default in DefaultFiles) {
                var path = Path.GetFullPath(Path.Combine(httpRoot, @default));
                if (!File.Exists(path)) return @default;
            }

            return string.Empty;
        }
    }
}