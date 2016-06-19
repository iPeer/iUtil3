using iUtil3.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.IRC.Commands
{
    public class CommandManager
    {

        public static CommandManager Instance { get; private set; }

        public Logger logger;

        private List<ICommand> _commands = new List<ICommand>();
        public List<ICommand> RegisteredCommands 
        { 
            get 
            { 
                return _commands; 
            } 
        }

        public CommandManager()
        {
            this.logger = new Logger("CommandManager");
            Instance = this;
            this.registerCommand(new CommandTest());
            this.registerCommand(new CommandQuit());
            this.registerCommand(new CommandWhoAmI());
        }

        public void registerCommand(ICommand c)
        {
            if (this._commands.Contains(c))
            {
                this.logger.LogToEngineAndModule("Command registry already contains the command '{0}'", LogLevel.WARNING, c.Name);
                return;
            }
            this._commands.Add(c);
            this.logger.LogToEngineAndModule("Registered command '{0}' with aliases [{1}]", LogLevel.INFO, c.Name, String.Join(",", c.Aliases));
        }

    }
}
