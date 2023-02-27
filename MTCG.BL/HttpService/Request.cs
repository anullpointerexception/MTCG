using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace MTCG.BL.HttpService
{
    internal class Request
    {
        private TcpClient _tcpClient;

        public Method Method { get; set; }

        public Dictionary<string, string> QueryParams = new();
        public string Path { get; private set; }

        public string ProtocolVersion { get; set; }

        public Dictionary<string, string> headers = new();

        public string Content { get; private set; }


        private StreamReader _streamReader;

        public Dictionary<string, string> BodyMessage
        {
            get;
            private set;
        }


        public Request(TcpClient sock, StreamReader reader)
        {

            this._tcpClient = sock;
            this._streamReader = reader;

            string line = reader.ReadLine();

            Console.WriteLine(line);
            var firstLineParts = line.Split(" ");
            Method = (Method)Enum.Parse(typeof(Method), firstLineParts[0]);


            var path = firstLineParts[1];
            var pathParts = path.Split('?');
            if (pathParts.Length == 2)
            {
                var queryParams = pathParts[1].Split('&');
                foreach (string queryParam in queryParams)
                {
                    var queryParamParts = queryParam.Split('=');
                    if (queryParamParts.Length == 2)
                        QueryParams.Add(queryParamParts[0], queryParamParts[1]);
                    else
                        QueryParams.Add(queryParamParts[0], null);
                }
            }
            Path = pathParts[0];

            ProtocolVersion = firstLineParts[2];

            // headers
            int contentLength = 0;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
                if (line.Length == 0)
                    break;

                var headerParts = line.Split(": ");
                headers[headerParts[0]] = headerParts[1];
                if (headerParts[0] == "Content-Length")
                    contentLength = int.Parse(headerParts[1]);
            }

            Content = "";
            if (contentLength > 0 && headers.ContainsKey("Content-Type"))
            {
                var data = new StringBuilder(200);
                char[] buffer = new char[1024];
                int bytesReadTotal = 0;
                while (bytesReadTotal < contentLength)
                {
                    try
                    {
                        var bytesRead = reader.Read(buffer, 0, 1024);
                        bytesReadTotal += bytesRead;
                        if (bytesRead == 0) break;
                        data.Append(buffer, 0, bytesRead);
                    }

                    catch (IOException) { break; }
                }
                Content = data.ToString();
                try
                {
                    BodyMessage = JsonConvert.DeserializeObject<Dictionary<string, string>>(Content);
                }
                catch (JsonReaderException)
                {
                    throw new JsonReaderException("Invalid Json-Format");
                }
                Console.WriteLine(Content);
            }
        }
    }
}