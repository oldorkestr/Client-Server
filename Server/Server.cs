using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static Dictionary<string, (double, double)> shapes = new Dictionary<string, (double, double)>(); // Stores shape name, area, perimeter

        static void Main(string[] args)
        {
            int port = 8080;
            TcpListener server = null;

            try
            {
                // Set the TcpListener on port 8080.
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                byte[] bytes = new byte[1024];
                string data;

                // Enter the listening loop.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        string[] tokens = data.Split(' ');
                        string command = tokens[0];

                        if (command == "add")
                        {
                            string shapeName = tokens[1];
                            double area = double.Parse(tokens[2]);
                            double perimeter = double.Parse(tokens[3]);
                            shapes[shapeName] = (area, perimeter);
                            SendMessage(stream, "Shape added successfully");
                        }
                        else if (command == "search")
                        {
                            string shapeName = tokens[1];
                            if (shapes.ContainsKey(shapeName))
                            {
                                (double area, double perimeter) = shapes[shapeName];
                                SendMessage(stream, $"{shapeName} - Area: {area}, Perimeter: {perimeter}");
                            }
                            else
                            {
                                SendMessage(stream, $"Shape {shapeName} not found");
                            }
                        }
                        else if (command == "delete")
                        {
                            string shapeName = tokens[1];
                            if (shapes.ContainsKey(shapeName))
                            {
                                shapes.Remove(shapeName);
                                SendMessage(stream, $"{shapeName} removed successfully");
                            }
                            else
                            {
                                SendMessage(stream, $"Shape {shapeName} not found");
                            }
                        }
                        else if (command == "edit")
                        {
                            string shapeName = tokens[1];
                            if (shapes.ContainsKey(shapeName))
                            {
                                double newArea = double.Parse(tokens[2]);
                                double newPerimeter = double.Parse(tokens[3]);
                                shapes[shapeName] = (newArea, newPerimeter);
                                SendMessage(stream, $"{shapeName} edited successfully");
                            }
                            else
                            {
                                SendMessage(stream, $"Shape {shapeName} not found");
                            }
                        }
                        else if (command == "sort")
                        {
                            string field = tokens[1];
                            List<KeyValuePair<string, (double, double)>> sortedShapes = null;
                            if (field == "name")
                            {
                                sortedShapes = new List<KeyValuePair<string, (double, double)>>(shapes);
                                sortedShapes.Sort(delegate (KeyValuePair<string, (double, double)> x, KeyValuePair<string, (double, double)> y)
                                {
                                    return x.Key.CompareTo(y.Key);
                                });
                            }
                            else if (field == "area")
                            {
                                sortedShapes = new List<KeyValuePair<string, (double, double)>>(shapes);
                                sortedShapes.Sort(delegate (KeyValuePair<string, (double, double)> x, KeyValuePair<string, (double, double)> y)
                                {
                                    return x.Value.Item1.CompareTo(y.Value.Item1);
                                });
                            }
                            else if (field == "perimeter")
                            {
                                sortedShapes = new List<KeyValuePair<string, (double, double)>>(shapes);
                                sortedShapes.Sort(delegate (KeyValuePair<string, (double, double)> x, KeyValuePair<string, (double, double)> y)
                                {
                                    return x.Value.Item2.CompareTo(y.Value.Item2);
                                });
                            }

                            if (sortedShapes != null)
                            {
                                SendMessage(stream, "Sorted Shapes:");
                                foreach (KeyValuePair<string, (double, double)> kvp in sortedShapes)
                                {
                                    SendMessage(stream, $"{kvp.Key} - Area: {kvp.Value.Item1}, Perimeter: {kvp.Value.Item2}");
                                }
                            }
                            else
                            {
                                SendMessage(stream, $"Invalid sort field: {field}");
                            }
                        }
                        else
                        {
                            SendMessage(stream, "Invalid command");
                        }
                    }

                    // Shutdown and end connection
                    client.Close();
                    Console.WriteLine("Connection closed");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        static void SendMessage(NetworkStream stream, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }
}