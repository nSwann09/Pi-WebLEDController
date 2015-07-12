using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Newtonsoft.Json;

namespace WebLEDController.HttpServer
{
    public sealed class HttpServer : IDisposable
    {
        const uint BufferSize = 8192;
        int port = 8000;
        readonly StreamSocketListener listener;
       Func<string, string> ApiProccessor;

        public string AllowOriginsHeader { get; set; }
        public string BasePath { get; set; }
        public HttpServer(int serverPort, Func<string, string> apiProcessor , string basePath = "", string[] allowOrigins = null)
        {
            listener = new StreamSocketListener();
            port = serverPort;
            ApiProccessor = apiProcessor;

            if (allowOrigins != null)
            {
                var allowOriginsHeadersBuilder = new StringBuilder();
                foreach (var url in allowOrigins)
                {
                    allowOriginsHeadersBuilder.AppendFormat("Access-Control-Allow-Origin: {0}\r\n",url );  
                }

                allowOriginsHeadersBuilder.Append("Access-Control-Allow-Methods: GET\r\n");
                AllowOriginsHeader = allowOriginsHeadersBuilder.ToString();
            }
            else
            {
                AllowOriginsHeader = "Access-Control-Allow-Origin: *\r\n";
            }
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                BasePath = basePath.Replace("\\", "/");
            }
            listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
        }

        public async void StartServer()
        {
            await listener.BindServiceNameAsync(port.ToString());
        }

        public void Dispose()
        {
            listener.Dispose();
        }

        async void ProcessRequestAsync(StreamSocket socket)
        {
            // this works for text only
            var request = new StringBuilder();
            using (IInputStream input = socket.InputStream)
            {
                //var bobo = new StreamReader(input.AsStreamForRead());
                //request.Append(bobo.ReadToEnd());
                //int dataRead = request.Length;
                byte[] data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = BufferSize;
                while (dataRead == BufferSize)
                {

                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            using (IOutputStream output = socket.OutputStream)
            {
                string requestMethod = request.ToString().Split('\n')[0];
                string[] requestParts = requestMethod.Split(' ');

                if (requestParts[0] == "GET")
                {
                    await WriteResponseAsync(requestParts[1], output);
                }
                else
                {
                    //throw new InvalidDataException("HTTP method not supported: " + requestParts[0]);
                    await WriteResponseAsync("error - HTTP method not supported: " + requestParts[0], output);
                }
            }
        }

        async Task WriteResponseAsync(string request, IOutputStream os)
        {
            

            var RtnString = "Unable to processRequest";
            if (!request.StartsWith("error -", StringComparison.CurrentCulture))
            {
                var apiString = "/api/";
                if (request.ToLower().Contains(apiString))
                {
                    request = request.Substring(request.IndexOf(apiString, StringComparison.CurrentCulture) + apiString.Length);
                    RtnString = ApiProccessor(request);
                }
                else
                {
                    try
                    {
                        var file = await MapPath(request);
                        RtnString = await ReadFile(file);
                    }
                    catch (Exception ex)
                    {
                        var header = "<!DOCTYPE html><html><body>";
                        var footer = "</body></html>";

                        RtnString = header + "Path: " + request + "<br />" + "Error: " + ex.Message + "<br />" + "Stacktrace: " + ex.StackTrace.Replace("\r\n", "<br/>") + footer;
                    }
                }
            }
            else
            {
                RtnString = request;
            }
            // Show the html 
            using (Stream resp = os.AsStreamForWrite())
            {
                // Look in the Data subdirectory of the app package
                byte[] bodyArray = Encoding.UTF8.GetBytes(RtnString);
                var stream = new MemoryStream(bodyArray);
                string header = string.Format("HTTP/1.1 200 OK\r\n" +                                    
                                  "Content-Length: {0}\r\n" +
                                  AllowOriginsHeader +
                                  "Connection: close\r\n\r\n",
                                  stream.Length);
                byte[] headerArray = Encoding.UTF8.GetBytes(header);
                await resp.WriteAsync(headerArray, 0, headerArray.Length);
                await stream.CopyToAsync(resp);
                await resp.FlushAsync();
            }

        }

        async Task<Windows.Storage.StorageFile> MapPath(string request)
        {
            request = BasePath + request;
            Windows.ApplicationModel.Package package = Windows.ApplicationModel.Package.Current;
            Windows.Storage.StorageFolder installedLocation = package.InstalledLocation;
            return await GetFile(request, installedLocation);
        }

        async Task<Windows.Storage.StorageFile> GetFile(string Path, Windows.Storage.StorageFolder Parent)
        {
            if (Path.TrimStart('/').Contains("/"))
            {
                var pts = Path.TrimStart('/').Split('/');
                var ptsLength = pts[0].Length;
                var newPath = Path.TrimStart('/').Substring(ptsLength);
                return await GetFile(newPath, await Parent.GetFolderAsync(pts[0]));
            }
            return await Parent.GetFileAsync(Path.TrimStart('/'));
        }

        async Task<string> ReadFile(Windows.Storage.StorageFile file)
        {
            if (file != null)
            {
                // Read the data.
                using (StreamReader streamReader = new StreamReader(await file.OpenStreamForReadAsync()))
                {
                    var rtn = streamReader.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(rtn))
                    {
                        return rtn;
                    }
                    else
                    {
                        return "unable to read file";
                    }
                }
            }

            return "unable to find file";

        }
    }
}
