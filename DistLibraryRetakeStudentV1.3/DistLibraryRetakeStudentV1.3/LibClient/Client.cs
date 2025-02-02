﻿using System.Linq;
using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Threading;
// using LibData;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace LibClient
{
    public struct Setting
    {
        public int ServerPortNumber { get; set; }
        public string ServerIPAddress { get; set; }

    }

    public class Output
    {
        public string Client_id { get; set; } // the id of the client that requests the book
        public string BookName { get; set; } // the name of the book to be reqyested
        public string Status { get; set; } // final status received from the server
        public string Error { get; set; } // True if errors received from the server
        public string BorrowerName { get; set; } // the name of the borrower in case the status is borrowed, otherwise null
        public string ReturnDate { get; set; } // the email of the borrower in case the status is borrowed, otherwise null
    }

    abstract class AbsSequentialClient
    {
        protected Setting settings;

        /// <summary>
        /// Report method can be used to print message to console in standaard formaat. 
        /// It is not mandatory to use it, but highly recommended.
        /// </summary>
        /// <param name="type">For example: [Exception], [Error], [Info] etc</param>
        /// <param name="msg"> In case of [Exception] the message of the exection can be passed. Same is valud for other types</param>

        protected void report(string type, string msg)
        {
            // Console.Clear();
            Console.Out.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>");
            if (!String.IsNullOrEmpty(msg))
            {
                msg = msg.Replace(@"\u0022", " ");
            }

            Console.Out.WriteLine("[Client] {0} : {1}", type, msg);
        }

        /// <summary>
        /// This methid loads required settings.
        /// </summary>
        protected void GetConfigurationValue()
        {
            settings = new Setting();
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                IConfiguration Config = new ConfigurationBuilder()
                    .SetBasePath(Path.GetFullPath(Path.Combine(path, @"../../../../")))
                    .AddJsonFile("appsettings.json")
                    .Build();

                settings.ServerIPAddress = Config.GetSection("ServerIPAddress").Value;
                settings.ServerPortNumber = Int32.Parse(Config.GetSection("ServerPortNumber").Value);
                // settings.ServerListeningQueue = Int32.Parse(Config.GetSection("ServerListeningQueue").Value);
            }
            catch (Exception e) { report("[Exception]", e.Message); }
        }

        protected abstract void createSocketAndConnect();
        public abstract Output handleConntectionAndMessagesToServer();
        protected abstract Message processMessage(Message message);

    }




    class SequentialClient : AbsSequentialClient
    {
        public Output result;
        public Socket clientSocket;
        public IPEndPoint serverEndPoint;
        public IPAddress ipAddress;

        public string client_id;
        private string bookName;

        //extra fields
        byte[] buffer;
        Message msg;

        bool error = false;

        //This field is optional to use. 
        private int delayTime;
        /// <summary>
        /// Initializes the client based on the given parameters and seeting file.
        /// </summary>
        /// <param name="id">id of the clients provided by the simulator</param>
        /// <param name="bookName">name of the book to be requested from the server, provided by the simulator</param>
        public SequentialClient(int id, string bookName)
        {
            GetConfigurationValue();

            // this.delayTime = 100;
            this.bookName = bookName;
            this.client_id = "Client " + id.ToString();
            this.result = new Output();
            result.Client_id = this.client_id;

            this.ipAddress = IPAddress.Parse(settings.ServerIPAddress);
            this.serverEndPoint = new IPEndPoint(ipAddress, settings.ServerPortNumber);
        }


        /// <summary>
        /// Optional method. Can be used for testing to delay the output time.
        /// </summary>
        public void delay()
        {
            int m = 10;
            for (int i = 0; i <= m; i++)
            {
                Console.Out.Write("{0} .. ", i);
                Thread.Sleep(delayTime);
            }
            Console.WriteLine("\n");
        }

        /// <summary>
        /// Connect socket settings and connect to the helpers.
        /// </summary>
        protected override void createSocketAndConnect()
        {
            //todo: To meet the assignment requirement, finish the implementation of this method.
  
            try
            {
                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(serverEndPoint);
            }
            catch
            {
                Console.WriteLine("Error: Could not connect to server!");
                error = true;
            }

        }


        /// <summary>
        /// This method starts the socketserver after initializion and handles all the communications with the server. 
        /// Note: The signature of this method must not change.
        /// </summary>
        /// <returns>The final result of the request that will be written to output file</returns>
        public override Output handleConntectionAndMessagesToServer()
        {
            Message response;
            bool Welcome_recieved = false;
            bool Diffirent_user = !Welcome_recieved;
            this.report("starting:", this.client_id + " ; " + this.bookName);
            createSocketAndConnect();

            buffer = new byte[1000];
            msg = new Message();

            //todo: To meet the assignment requirement, finish the implementation of this method.

            if (error)
            {
                result.Client_id = client_id;
                result.BookName = null;
                result.Status = null;
                result.Error = "true";
                result.BorrowerName = null;
                result.ReturnDate = null;

                return this.result;
            }

            //hello and welcome part of client
            if (this.client_id == "Client 0") {
                try
                {
                    msg.Type = MessageType.Hello;
                    msg.Content = this.client_id;
                    //clientSocket.Connect(serverEndPoint); //connect to server
                    response = processMessage(msg); //send hello to the server
                    clientSocket.Close();
                    if (response.Type == MessageType.Welcome)
                    {
                        Welcome_recieved = true;
                    }
                    else
                    {
                        result.Client_id = client_id;
                        result.BookName = null;
                        result.Status = null;
                        result.Error = "true";
                        result.BorrowerName = null;
                        result.ReturnDate = null;

                        return this.result;
                    }
                }
                catch 
                {
                    Console.WriteLine("Error: Hello message could not me send, or no welcome was recieved.");
                }
            }
            
            clientSocket.Close();
            if (Welcome_recieved == true || Diffirent_user)
            {
                //make the bookReq message
                createSocketAndConnect();
                //clientSocket.Connect(serverEndPoint);
                msg.Type = MessageType.BookInquiry;
                msg.Content = this.bookName.ToString();

                //send the bookreq message
                //Console.WriteLine(msg.Content);
                response = processMessage(msg);
                //Console.WriteLine(response.Content);
                
                if (response.Type == MessageType.BookInquiryReply)
                {
                    BookData book_data_recieved = JsonSerializer.Deserialize<BookData>(response.Content);
                    
                    result.Client_id = client_id;
                    result.BookName = book_data_recieved.Title;
                    result.Status = book_data_recieved.Status;
                    result.Error = null;
                    result.BorrowerName = book_data_recieved.BorrowedBy;
                    result.ReturnDate = book_data_recieved.ReturnDate;
                }
                else if (response.Type == MessageType.NotFound)
                {
                    result.Client_id = client_id;
                    result.BookName = response.Content;
                    result.Status = "Not Found";
                    result.Error = null;
                    result.BorrowerName = null;
                    result.ReturnDate = null;
                }
                else if (response.Type == MessageType.Error)
                {
                    result.Client_id = client_id;
                    result.BookName = null;
                    result.Status = null;
                    result.Error = "true";
                    result.BorrowerName = null;
                    result.ReturnDate = null;
                }
            }

            return this.result;
        }

       

        /// <summary>
        /// Process the messages of the server. Depending on the logic, type and content of a message the client may return different message values.
        /// </summary>
        /// <param name="message">Received message to be processed</param>
        /// <returns>The message that needs to be sent back as the reply.</returns>
        protected override Message processMessage(Message message)
        {
            Message processedMsgResult = new Message();
            //todo: To meet the assignment requirement, finish the implementation of this method.
            try
            {
                string json = JsonSerializer.Serialize(message);
                byte[] data = Encoding.ASCII.GetBytes(json);

                //Console.WriteLine("Sending");
                this.clientSocket.Send(data);

                //Console.WriteLine("Send");
                int recieved = this.clientSocket.Receive(buffer);

                //Console.WriteLine("Recieved back");
                string response = Encoding.ASCII.GetString(buffer, 0, recieved);
                processedMsgResult = JsonSerializer.Deserialize<Message>(response);
                //Console.WriteLine(processedMsgResult.Type);
                //Console.ReadLine();
            }
            catch 
            {
                Console.WriteLine("Could not send message to server.");
            }

            return processedMsgResult;
        }
    }
}

