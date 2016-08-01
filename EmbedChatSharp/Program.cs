using System;
using Quobject.SocketIoClientDotNet.Client;

namespace EmbedChatSharp
{
    class Program
    {
        public static Socket socket;
        public static string serverUrl = "http://bakechat.com:666/";
        public static string username = "Hacker";
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Initializing socket.io server connection");
            Console.ResetColor();

            socket = IO.Socket(serverUrl);
            socket.Close(); //close any orphaned connections
            socket = IO.Socket(serverUrl);
            socket.On(Socket.EVENT_DISCONNECT, () => SocketClosed());
            socket.On(Socket.EVENT_ERROR,e => SocketError(e));

            // register for 'chat message' events
            socket.On("chat message", (data) =>
            {
                Console.WriteLine(" Message:   {0}\r\n", data);
            });

            // make the socket.io connection
            socket.Connect();
            SocketOpened();

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Enter a username");
            Console.ResetColor();
            username = Console.ReadLine();

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Username set to {0}",username);
            Console.ResetColor();

            while (true) //main execution loop
            {
                string message = Console.ReadLine();
                if (message.ToLower() == "exit")
                {
                    socket.Emit("chat message", "CMD user " + Environment.MachineName + " left the chat");
                    socket.Close();
                    break;
                }
                socket.Emit("chat message", username + ": " + message);
            }
        }

        public static void SocketOpened()
        {
            Console.WriteLine("Connected to {0}", serverUrl);
            socket.Emit("chat message", "CMD Client Connected from " + Environment.MachineName);
        }

        public static void SocketError(object e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            dynamic args = e;
            Console.WriteLine("Socket error: {0}",args.Message);
            Console.ResetColor();
        }

        public static void SocketClosed()
        {
            Console.WriteLine("Socket closed");
        }
    }
}
