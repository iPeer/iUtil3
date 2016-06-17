using iUtil3.IRC;
using iUtil3.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iUtil3
{
    public class Engine
    {

       public static Engine Instance { get; private set; }
       public Logger Logger { get; private set; }
       public DateTime STARTUP_TIME;

       public Protocol Protocol { get; private set; }

       private bool keepRunning = true;
       private Thread thread;

       public static void Main(string[] args)
       {
           new Engine();
       }

       public Engine()
       {
           Instance = this;
           this.STARTUP_TIME = DateTime.Now;
           this.thread = new Thread(new ThreadStart(this.run));
           this.thread.Name = "iUtil3 Main Keep-Alive Thread";
           this.thread.IsBackground = false;
           this.thread.Start();


       }

       private void run()
       {
           // Initialise logging
           this.Logger = new Logger("Engine");

           // Startup IRC handler

           this.Protocol = new Protocol();
           this.Protocol.setNick("iUtil3").setServer("irc.swiftirc.net").setChannels("#QuestHelp").setSLL(false).setPort(6667);
           this.Protocol.startIfNotRunning();

           while (keepRunning);

           //TODO cleanup
       }

       public void prepareExit()
       {
           this.keepRunning = false;
       }

       public TimeSpan getUptime()
       {
           return (TimeSpan)(DateTime.Now - this.STARTUP_TIME);
       }
    }
}
