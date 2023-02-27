using System.Net.Sockets;

namespace MTCG.BL.HttpService
{
    public class Server
    {
        private TcpListener tcpListener;
        private Handler handler;

        public Server(string uri)
        {
            tcpListener = new TcpListener(System.Net.IPAddress.Any, 10001);
            handler = new Handler();
        }

        public void Start()
        {
            tcpListener.Start(5);
            while (true)
            {
                Console.WriteLine("Listening...");

                TcpClient socket = tcpListener.AcceptTcpClient();

                Task.Run(() =>
                {
                    handler.HandleREQ(socket);
                });
            }
        }

    }
}
