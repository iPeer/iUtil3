using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.IRC.Commands
{
    public class CommandMessage
    {

        public string Command { get; private set; }
        public string CommandCharacter { get; private set; }
        public bool PublicCommand { get; private set; }
        public string[] CommandParameters { get; private set; }
        public ICommandSender CommandSender { get; private set; }
        public Action<string, string> ReplyMethod { get; private set; }
        public UserLevel UserLevel { get; private set; }
        public string Target { get; private set; }

        public CommandMessage(string command, string commandChar, bool publicCom, string target, Action<string, string> replyMethod, string[] param, ICommandSender sender, UserLevel userLevel)
        {
            this.Command = command;
            this.CommandSender = sender;
            this.CommandCharacter = commandChar;
            this.CommandParameters = param;
            this.PublicCommand = publicCom;
            this.ReplyMethod = replyMethod;
            this.UserLevel = userLevel;
            this.Target = target;
        }

    }
}
