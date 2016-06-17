using iUtil3.Logging;
using iUtil3.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iUtil3.IRC
{
    public class Protocol
    {

        public const string DEFAULT_NICK = "iUtil3";
        public const string DEFAULT_SERVER = "irc.swiftirc.net";
        public const int DEFAULT_PORT = 6697;
        public const bool DEFAULT_SSL = true;
        public const string DEFAULT_CHANNELS = "#QuestHelp,#Peer.Dev";
        public const int RETRY_ATTEMPTS = 5; // Max number of times to rety connection if the bots is unexpectedly disconnected before terminating
        public const int RETRY_DELAY = 10; // Time (in seconds) to wait between each retry attempt (prevents some IRCd types from throttling us if we connect too fast

        public const string COMMAND_CHARS = "@#.!"; // What characters count as command characters when a message is started with them?

        public string Server { get; private set; }
        public string Nick { get; private set; }
        public string Channels { get; private set; }
        public int Port { get; private set; }
        public bool SSL { get; private set; }

        public bool RegisteredWithServer { get; private set; }
        public bool ReadyToParseMessages { get; private set; }

        public string CURRENT_SERVER { get; private set; }
        public Dictionary<string, object> NETWORK_SETTINGS;

        public Logger Logger { get; private set; }

        public bool Connected { get; private set; }

        public static Protocol Instance { get; private set; }
        public Thread Thread { get; private set; }
        public bool IsRunning { get; private set; }

        private Int64 _bytesSent = 0, _bytesRecieved = 0;
        private int currentRetryCount = 0;

        public Int64 BytesSent
        {
            get
            {
                return _bytesSent;
            }
        }

        public Int64 BytesReceived
        {
            get
            {
                return _bytesRecieved;
            }
        }

        private bool quitWasRequested = false; // Was the disconnect we just experienced requested? (t = yes, f = no)

        private TcpClient connection;
        private StreamWriter _out;
        private StreamReader _in;

        public Protocol()
        {
            Instance = this;
            this.Logger = new Logger("IRC");
            this.Server = DEFAULT_SERVER;
            this.Port = DEFAULT_PORT;
            this.Nick = DEFAULT_NICK;
            this.Channels = DEFAULT_CHANNELS;
            this.SSL = DEFAULT_SSL;
            createThread();
        }

        public Protocol setServer(string server)
        {
            // TODO: Sanity checking
            this.Server = server;
            return this;
        }

        public Protocol setPort(int port)
        {
            this.Port = port;
            return this;
        }

        public Protocol setNick(string nick)
        {
            if (this.Connected) { /* TODO: Server-verified nick changes */ }
            else // Not connected, no need to verify with the server
            {
                this.Nick = nick;
            }
            return this;
        }

        public Protocol setSLL(bool b)
        {
            this.SSL = b;
            return this;
        }

        public Protocol setChannels(string c)
        {
            this.Channels = c;
            return this;
        }

        public Protocol setChannels(string[] c)
        {
            return setChannels(String.Join(",", c));
        }

        public void startIfNotRunning()
        {
            if (this.IsRunning) return;
            this.start();
        }

        public void start()
        {
            this.IsRunning = true;
            this.Thread.Start();
        }

        public void stop()
        {
            this.Thread.Abort();
            this.IsRunning = false;
        }

        public void createThread()
        {
            this.Thread = new Thread(new ThreadStart(this.run));
            this.Thread.Name = "iUtil3 main IRC Protocol Thread";
            this.Thread.IsBackground = false; // DO NOT terminate the EXE before this thread is finished
        }

        public void run()
        {

            // Try to create the necessary connections for the IRC server

            try
            {
                TcpClient t = new TcpClient();
                t.NoDelay = true;
                t.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

                t.ReceiveTimeout = 60000;   // 60 seconds
                t.SendTimeout = 60000;      // ^

                t.Connect(this.Server, this.Port);

                this.connection = t;

                Stream stream = t.GetStream();

                if (this.SSL)
                {
                    // TODO?: Verify SSL certificates
                    RemoteCertificateValidationCallback sslCallback = delegate { return true; };
                    RemoteCertificateValidationCallback certValidationWithIrcAsSender = delegate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                        {
                            return sslCallback(this, cert, chain, sslPolicyErrors);
                        };
                    LocalCertificateSelectionCallback selectionCallback = delegate(object sender, string targetHost, X509CertificateCollection lCerts, X509Certificate rCert, string[] acceptableIssuers)
                    {
                        if (lCerts == null || lCerts.Count == 0)
                        {
                            return null;
                        }
                        return lCerts[0];
                    };

                    SslStream sslStream = new SslStream(stream, false, certValidationWithIrcAsSender, selectionCallback);

                    try
                    {
                        sslStream.AuthenticateAsClient(this.Server);
                    }
                    catch (IOException e)
                    {
                        this.Logger.LogToEngineAndModule("Couldn't authenticate with SSL: {0}", LogLevel.ERROR, e.Message);
                        // Bail
                        this.IsRunning = false;
                        this.Thread.Abort();
                        return;
                    }

                    stream = sslStream;
                }

                this._in = new StreamReader(stream, new UTF8Encoding());
                this._out = new StreamWriter(stream, new UTF8Encoding());

                if (new UTF8Encoding().GetPreamble().Length > 0) // Stop the IRCd getting confused by encoding with preambles.
                {
                    _out.WriteLine();
                    _out.Flush();
                }



            }
            catch (Exception e)
            {
                this.Logger.LogToEngineAndModule("Unable to initialise IRC connection object: {0} // {1}", LogLevel.ERROR, e.GetType().ToString(), e.Message);
                reconnect();
            }

            changeNick(this.Nick);

            String @in = String.Empty;
            while ((@in = this._in.ReadLine()) != null && this.IsRunning/* && this.Connected*/)
            {
                parseIRCLine(@in);
            }

        }

        private void parseIRCLine(string line)
        {
            this._bytesRecieved += Encoding.UTF8.GetByteCount(line);
            string[] line_split = line.Split(' ');
            if (line.StartsWith("PING "))
            {
                sendLine(line.Replace("PING", "PONG"));
            }
            else // We don't log pings because they're really spammy
                this.Logger.Log("[RECV] {0}", LogLevel.INFO, line);

            if (line_split[1] == "001") // Welcone to the ____ [IRC] network _____
            {
                this.Connected = true;
                this.CURRENT_SERVER = line_split[0].Substring(1);
                this.currentRetryCount = 0;
                if (this.NETWORK_SETTINGS != null && this.NETWORK_SETTINGS.Count > 0)
                {
                    this.Logger.Log("Network settings are present from a previous connection on this instance, clearing to avoid conflicts", LogLevel.WARNING);
                    this.NETWORK_SETTINGS.Clear();
                }
            }
            else if (line_split[1] == "004") // Identify and set usermodes here
            {
                //TODO: Identify
                sendLine(String.Format("MODE {0} {1}", this.Nick, "+Bp"));
            }
            else if (line_split[1] == "005") // Network settings
            {
                if (this.NETWORK_SETTINGS == null)
                    this.NETWORK_SETTINGS = new Dictionary<string, object>();

                for (int x = 3; x < line_split.Length - 5 /* Don't parse "are supported by this server" */; x++)
                {
                    if (line_split[x].Contains("="))
                    {
                        string[] split = line_split[x].Split('=');
                        this.NETWORK_SETTINGS.Add(split[0], split[1]);
                    }
                    else
                    {
                        this.NETWORK_SETTINGS.Add(line_split[x], true);
                    }
                }
#if DEBUG
                foreach (KeyValuePair<string, object> kv in this.NETWORK_SETTINGS)
                {
                    this.Logger.Log("\t{0} = {1}", LogLevel.DEBUG, kv.Key, kv.Value);
                }
#endif
            }
            else if (line_split[1] == "010") // please use this server instead
            {
                // TODO
            }
            else if (line_split[1] == "251") // End of /MOTD command
            {
                this.ReadyToParseMessages = true;
                this.Logger.Log("Connected to the {0} network on the server {1}", (this.NETWORK_SETTINGS["NETWORK"] ?? "UNKNOWN"), this.CURRENT_SERVER);
                string[] channels = this.Channels.Split(',');
                this.Logger.Log("Joining {0} channel(s)", channels.Length);
                foreach (string s in channels)
                {
                    joinChannel(s);
                }
            }
            else if (line_split[1] == "433") // Nick already taken
            {
                // TODO
            }

            /* Actual IRC messages and stuff */

            if (line.StartsWith("ERROR :")) // Disconnected
            {
                this.Connected = false;
                this.Logger.LogToEngineAndModule("Disconnected from server!", LogLevel.WARNING);
                if (!this.quitWasRequested) reconnect();
                else
                {
                    this.IsRunning = false;
                    Engine.Instance.prepareExit();
                }
            }

            //if (!this.ReadyToParseMessages) return;

            if (Array.IndexOf(new string[] { "PRIVMSG", "NOTICE" }, line_split[1]) > -1)
            {
                // get the nick, address (for debug), channel, mode (PRIVMSG or NOTICE) and message that was sent
                // <- :Bond!~Bond@Swift-C8F0EEAD.range86-143.btcentralplus.com PRIVMSG #QuestHelp :lol

                string[] userData = line_split[0].Substring(1).Split('!');
                string nick = userData[0];
                string address = (userData.Length == 2 ? userData[1] : nick);
                string mode = line_split[1];
                string channel = line_split[2];
                string message = line_split[3].Substring(1);

                for (int x = 4; x < line_split.Length; x++)
                {
                    message += String.Format(" {0}", line_split[x]);
                }

                if (nick.EqualsIgnoreCase("ipeer"))
                {
                    messageChannel("#QuestHelp", String.Format("Char 1 is CTCP Char?: {0}, CTCP Type (safed): {1}", message.ToCharArray()[0].Equals('\x01'), message.Split(' ')[0].Substring(1).Replace('\x01', '@')));
                }

                if (message.ToCharArray()[0].Equals('\x01') && mode.Equals("PRIVMSG")) // CTCPs
                {
                    string ctcpType = message.Split(' ')[0].Substring(1).Replace('\x01'.ToString(), "");
                    if (ctcpType.Equals("PING"))
                        sendLine("NOTICE {0} :{1}{2} {3}", nick, '\x01', ctcpType, message.Substring(6));
                    else if (ctcpType.Equals("TIME"))
                        sendLine("NOTICE {0} :{1}{2} {3}", nick, '\x01', ctcpType, DateTime.Now.ToString("o"));
                    else if (ctcpType.Equals("VERSION"))
                        sendLine("NOTICE {0} :{1}{2} {3}", nick, '\x01', ctcpType, "I don't have a version number yet :("); // TODO
                    // else ignore the CTCP
                }
                int charIndex = -1;
                if ((charIndex = COMMAND_CHARS.IndexOf(message.Substring(0, 1))) > -1) // We have been given a command for the bot to evaluate
                {

                    Action<string, string> replyMethod;
                    bool @public = false;
                    if (charIndex < 2) // "Public" (PRIVMSG) commands
                    {
                        replyMethod = messageChannel;
                        @public = true;
                    }
                    else // "Private" (NOTICE) commands
                        replyMethod = noticeUser;

                    string command = message.Substring(1).Split(' ')[0];

                    if (command.EqualsIgnoreCase("3quit") && nick.Equals("iPeer"))
                    {
                        sendLine("QUIT :Quit command from {0}", nick);
                        this.quitWasRequested = true;
                    }
                    else if (command.EqualsIgnoreCase("test"))
                    {
                        replyMethod((@public ? channel : nick), "Test reply!");
                    }


                }

            }

        }

        public void reconnect()
        {
            if (currentRetryCount++ < RETRY_ATTEMPTS)
            {
                this.Logger.LogToEngineAndModule("Waiting {0} seconds before attempting to reconnect...", RETRY_DELAY.ToString());
                Thread.Sleep(RETRY_DELAY * 1000);
                this.Logger.LogToEngineAndModule("Attempting to reconnect to network '{0}'", (this.NETWORK_SETTINGS == null ? "???" : this.NETWORK_SETTINGS["NETWORK"] ?? "???"));
                this.Logger.LogToEngineAndModule("Retry attempt #{0}/{1}", currentRetryCount, RETRY_ATTEMPTS);
                this.IsRunning = false;
                createThread();
                this.Thread.Start();
            }
            else
            {
                this.Logger.LogToEngineAndModule("Attempt retries have exceeded the maximum that can be attempted, terminating for safety purposes...", LogLevel.ERROR);
                this.IsRunning = false;
                Engine.Instance.prepareExit();
            }
            /*this.Thread.Abort();
            this.IsRunning = false;
            createThread();
            this.Thread.Start();*/
        }

        public void sendLine(string line, params object[] fillers)
        {
            sendLine(line, true, fillers);
        }

        public void sendLine(string line, bool log = true, params object[] fillers)
        {
            line = String.Format(line, fillers);

            this._bytesSent += Encoding.UTF8.GetByteCount(line);
            this.Logger.Log("[SEND] {0}", LogLevel.INFO, line);
            _out.WriteLine(line);
            _out.Flush();
        }

        public void messageChannel(string channel, string message)
        {
            sendLine("PRIVMSG {0} :{1}", channel, message);
        }

        public void noticeUser(string user, string message)
        {
            sendLine("NOTICE {0} :{1}", user, message);
        }

        public void joinChannel(string c)
        {
            string validPrefixes = this.NETWORK_SETTINGS["CHANTYPES"].ToString();
            if (!validPrefixes.Contains(c.Substring(0, 1)))
            {
                this.Logger.Log("Specified channel '{1}' does not start with a valid CHANTYPES character for this network ({0}), aborting join attempt", LogLevel.WARNING, this.NETWORK_SETTINGS["CHANTYPES"], c);
                return;
            }
            sendLine("JOIN {0}", c);
        }

        public void changeNick(string newNick)
        {
            sendLine("NICK {0}", newNick);
            if (!this.RegisteredWithServer)
            {
                sendLine("USER {0} ipeer.auron.co.uk {0}: iPeer's Java Utility Bot", newNick);
                this.RegisteredWithServer = true;
            }
        }

    }
}
