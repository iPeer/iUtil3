using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.IRC.Commands
{
    public class CommandQuit : Command
    {

        public CommandQuit()
            : base("Quit")
        {
            registerAliases("3quit", "3qqq");
        }

        public override UserLevel requiredUserLevel() { return UserLevel.ADMIN; }
        public override bool protectOnLoad() { return true; }

        public override void run(CommandMessage commandMessage)
        {

            Engine.Instance.Protocol.sendLine("QUIT :Quit from {0}", commandMessage.CommandSender.Nick);
            Engine.Instance.Protocol.quitWasRequested = true;
            Engine.Instance.prepareExit();

        }

    }
}