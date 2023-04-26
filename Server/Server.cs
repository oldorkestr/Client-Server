using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

namespace Server
{
    public class GeometryShape
    {
        public int ID;
        public string Name;
        public double Area;
        public double Perimeter;

        public GeometryShape(int id, string name, double area, double perimeter)
        {
            ID = id;
            Name = name;
            Area = area;
            Perimeter = perimeter;
        }
    }


    class Program
    {
        //to do add object geometry shape
        static List<GeometryShape> shapes = new List<GeometryShape>(); // Stores shape name, area, perimeter

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
                            GeometryShape geometryShape = new GeometryShape(shapes.Count,shapeName, area,perimeter);
                            shapes.Add(geometryShape);
                            SendMessage(stream, "Shape added successfully");
                        }
                        else if (command == "search")
                        {
                            string shapeName = tokens[1];
                            List<GeometryShape> searchResults = shapes.Where(x => x.Name == shapeName).ToList();
                            if (searchResults.Count() >= 1)
                            {
                                foreach (var item in searchResults)
                                {
                                    SendMessage(stream, $"ID = {item.ID}, {item.Name} - Area: {item.Area}, Perimeter: {item.Perimeter} \n");
                                }
                            }
                            else
                            {
                                SendMessage(stream, $"Shape {shapeName} not found");
                            }
                        }
                        else if (command == "delete")
                        {
                            string shapeID = tokens[1];
                            int id;
                            int.TryParse(shapeID, out id);
                            if (id >= 0)
                            {
                                GeometryShape itemToDelete = shapes.Where(x => x.ID == id).First();
                                if (itemToDelete != null)
                                {
                                    shapes.Remove(itemToDelete);
                                    SendMessage(stream, $"{itemToDelete.Name} with ID {itemToDelete.ID} was removed successfully");
                                }
                                else
                                {
                                    SendMessage(stream, $"Shape with ID = {id} not found");
                                }
                            }
                            else SendMessage(stream, $"Wrong ID entered!");

                        }
                        else if (command == "edit")
                        {
                            string shapeID = tokens[1];
                            int id;
                            int.TryParse(shapeID, out id);
                            if (id >= 0)
                            {
                                GeometryShape itemToEdit = shapes.Where(x => x.ID == id).First();
                                if (itemToEdit != null)
                                {
                                    shapes.Remove(itemToEdit);
                                    double newArea = double.Parse(tokens[2]);
                                    double newPerimeter = double.Parse(tokens[3]);
                                    GeometryShape geometryShape = new GeometryShape(itemToEdit.ID, itemToEdit.Name, newArea, newPerimeter);
                                    shapes.Add(geometryShape);
                                    SendMessage(stream, $"Shape with ID {itemToEdit.ID} was edited successfully!\nNew values are: {itemToEdit.Name} - area {newArea}, perimeter {newPerimeter}");
                                }
                                else
                                {
                                    SendMessage(stream, $"Shape with ID {id} not found");
                                }
                            }
                            else SendMessage(stream, $"Wrong ID!");
                        }
                        else if (command == "sort")
                        {
                            string field = tokens[1];
                            List<GeometryShape> sortedShapes = null;
                            if (field == "id")
                            {
                                sortedShapes = new List<GeometryShape>(shapes);
                                sortedShapes.OrderBy(x => x.ID);
                            }
                            else if (field == "name")
                            {
                                sortedShapes = new List<GeometryShape>(shapes);
                                sortedShapes.OrderBy(x => x.Name);
                            }
                            else if (field == "area")
                            {
                                sortedShapes = new List<GeometryShape>(shapes);
                                sortedShapes.OrderBy(x => x.Area);
                            }
                            else if (field == "perimeter")
                            {
                                sortedShapes = new List<GeometryShape>(shapes);
                                sortedShapes.OrderBy(x => x.Perimeter);
                            }

                            if (sortedShapes != null)
                            {
                                SendMessage(stream, "Sorted Shapes:");
                                foreach (var item in sortedShapes)
                                {
                                    SendMessage(stream, $"{item.ID}, {item.Name} - Area: {item.Area}, Perimeter: {item.Perimeter}\n");
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