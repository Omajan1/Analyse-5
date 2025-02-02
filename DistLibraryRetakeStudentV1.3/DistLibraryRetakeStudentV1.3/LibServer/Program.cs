﻿using System;
// using LibServer;
using LibServerSolution;
using System.Text;
using System.Diagnostics.Tracing;

// NOTE: THIS FILE MUST NOT CHANGE

namespace Server
{
    public class ServerSimulator
    {
        public Setting settings;

        public ServerSimulator()
        { }

        /// <summary>
        /// initiates the server
        /// </summary>
        public void sequentialRun()
        {
            SequentialServer server = new SequentialServer();
            server.handelListening();
        }
    }

    public class Program
    {
        /// <summary>
        /// Starts the simulation for a set of clients and produces the output results.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // using var listener = new SocketEventListener();
            Console.Clear();
            new ServerSimulator().sequentialRun();

        }

    //     internal sealed class SocketEventListener : EventListener
    //     {
    //         // Constant necessary for attaching ActivityId to the events.
    //         public const EventKeywords TasksFlowActivityIds = (EventKeywords)0x80;

    //         protected override void OnEventSourceCreated(EventSource eventSource)
    //         {
    //             // List of event source names provided by networking in .NET 5.
    //             if (eventSource.Name == "System.Net.Sockets" ||
    //                 eventSource.Name == "System.Net.Security" ||
    //                 eventSource.Name == "System.Net.NameResolution")
    //             {
    //                 EnableEvents(eventSource, EventLevel.LogAlways);
    //             }
    //             // Turn on ActivityId.
    //             else if (eventSource.Name == "System.Threading.Tasks.TplEventSource")
    //             {
    //                 // Attach ActivityId to the events.
    //                 EnableEvents(eventSource, EventLevel.LogAlways, TasksFlowActivityIds);
    //             }
    //         }

    //         protected override void OnEventWritten(EventWrittenEventArgs eventData)
    //         {
    //             var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.ff}  {eventData.ActivityId}.{eventData.RelatedActivityId} {eventData.EventSource.Name}.{eventData.EventName} {eventData.Task}.{eventData.Message}(");
    //             for (int i = 0; i < eventData.Payload?.Count; i++)
    //             {
    //                 sb.Append(eventData.PayloadNames?[i]).Append(": ").Append(eventData.Payload[i]);
    //                 if (i < eventData.Payload?.Count - 1)
    //                 {
    //                     sb.Append(", ");
    //                 }
    //             }

    //             sb.Append(")");
    //             Console.ForegroundColor = ConsoleColor.Yellow;
    //             Console.WriteLine(sb.ToString());
    //             Console.ForegroundColor = ConsoleColor.Blue;
    //         }
    //     }
    }
}

