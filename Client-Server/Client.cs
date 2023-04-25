using System;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = "127.0.0.1";
            int port = 8080;
            try
            {
                TcpClient client = new TcpClient(server, port);

                Console.WriteLine("Connected to server");

                NetworkStream stream = client.GetStream();
                string message;

                while (true)
                {
                    Console.Write("> ");
                    message = Console.ReadLine();

                    if (message == "exit")
                    {
                        break;
                    }

                    byte[] data = Encoding.ASCII.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    // Buffer to store the response bytes.
                    data = new byte[1024];

                    // String to store the response ASCII representation.
                    string responseData = string.Empty;

                    // Read the first batch of the TcpServer response bytes.
                    int bytes = stream.Read(data, 0, data.Length);
                    responseData = Encoding.ASCII.GetString(data, 0, bytes);
                    Console.WriteLine(responseData);
                }

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}