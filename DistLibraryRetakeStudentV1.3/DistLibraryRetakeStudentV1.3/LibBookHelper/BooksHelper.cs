﻿using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using LibData;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace BookHelperSolution
{
    public struct Setting
    {
        public int BookHelperPortNumber { get; set; }
        public string BookHelperIPAddress { get; set; }
        public int ServerListeningQueue { get; set; }
    }

    abstract class AbsSequentialServerHelper
    {
        protected Setting settings;
        protected string booksDataFile;

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

            Console.Out.WriteLine("[Server Helper] {0} : {1}", type, msg);
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

                settings.BookHelperIPAddress = Config.GetSection("BookHelperIPAddress").Value;
                settings.BookHelperPortNumber = Int32.Parse(Config.GetSection("BookHelperPortNumber").Value);
                settings.ServerListeningQueue = Int32.Parse(Config.GetSection("ServerListeningQueue").Value);
            }
            catch (Exception e) { report("[Exception]", e.Message); }
        }

        protected abstract void loadDataFromJson();
        protected abstract void createSocket();
        public abstract void handelListening();
        protected abstract Message processMessage(Message message);

    }

    class SequentialServerHelper : AbsSequentialServerHelper
    {
        // check all the required parameters for the server. How are they initialized? 
        public Socket listener;
        public IPEndPoint listeningPoint;
        public IPAddress ipAddress;
        public List<BookData> booksList;

        bool isRunning = true;
        bool isRunning2 = true;


        public SequentialServerHelper() : base()
        {
            booksDataFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../") + "Books.json");
            GetConfigurationValue();
            loadDataFromJson();
        }

        /// <summary>
        /// This method loads data items provided in booksDataFile into booksList.
        /// </summary>
        protected override void loadDataFromJson()
        {
            //todo: To meet the assignment requirement, implement this method 
            try
            {
                string all_in_text = File.ReadAllText(booksDataFile);
                booksList = JsonSerializer.Deserialize<List<BookData>>(all_in_text);
            }
            catch 
            {
                Console.WriteLine("Error: could not serialize JSON file to list");
            }
        }

        /// <summary>
        /// This method establishes required socket: listener.
        /// </summary>
        protected override void createSocket()
        {
            //todo: To meet the assignment requirement, implement this method
            IPAddress ipAddress = IPAddress.Parse(this.settings.BookHelperIPAddress);
            listeningPoint = new IPEndPoint(ipAddress, this.settings.BookHelperPortNumber);

            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(listeningPoint);
                listener.Listen(this.settings.ServerListeningQueue);
            }
            catch
            {
                Console.WriteLine("Could not create listener on the Bookhelper.");
            }

        }

        /// <summary>
        /// This method is optional. It delays the execution for a short period of time.
        /// Note: Can be used only for testing purposes.
        /// </summary>
        void delay()
        {
            int m = 10;
            for (int i = 0; i <= m; i++)
            {
                Console.Out.Write("{0} .. ", i);
                Thread.Sleep(200);
            }
            Console.WriteLine("\n");
        }

        /// <summary>
        /// This method handles all the communications with the LibServer.
        /// </summary>
        public override void handelListening()
        {
            delay();
            delay();
            delay();
            delay();
            delay();
            delay();

            createSocket();
            //todo: To meet the assignment requirement, finish the implementation of this method 
            Socket SocketAgain = listener.Accept();

            while (SocketAgain.Connected)
            {
                int maxBuffSize = 1000;
                byte[] buffer = new byte[maxBuffSize];
                byte[] msg = new byte[maxBuffSize];
                string data = null;
                
                int receivingBytes = SocketAgain.Receive(buffer);
                data += Encoding.ASCII.GetString(buffer, 0, receivingBytes);
                
                if (data == "")
                {
                    
                }
                try
                {
                    Message message = JsonSerializer.Deserialize<Message>(data);
                    Message to_send_back = processMessage(message);
                    string json_ding = JsonSerializer.Serialize(to_send_back);
                    SocketAgain.Send(Encoding.ASCII.GetBytes(json_ding));
                }
                catch(System.Text.Json.JsonException e)
                {
                    e = new System.Text.Json.JsonException("Invalid JSON");
                    break;                   
                }
            }

        }

        /// <summary>
        /// Given the message received from the Server, this method processes the message and returns a reply.
        /// </summary>
        /// <param name="message">The message received from the LibServer.</param>
        /// <returns>The message that needs to be sent back as the reply.</returns>
        protected override Message processMessage(Message message)
        {
            Message reply = new Message();
            //todo: To meet the assignment requirement, finish the implementation of this method .
            try
            {
                foreach(BookData book in booksList)
                {
                    if (book.Title == message.Content)
                    {
                        reply.Type = MessageType.BookInquiryReply;
                        reply.Content = JsonSerializer.Serialize(book);
                        //Console.WriteLine(reply.Content);
                        break;
                    }
                    else
                    {
                        reply.Type = MessageType.NotFound;
                        reply.Content = message.Content;
                    }
                }
                //Console.WriteLine(reply.Type);
            }
            catch
            {
                Console.WriteLine("Message could not be prosessed in bookhelper");
            }
            return reply;
        }
    }
}
