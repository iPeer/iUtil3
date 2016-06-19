using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.IRC.Commands
{
    public class CommandTest : Command
    {

        public CommandTest() : base("Test")
        {
            registerAliases("test", "test2", "test3");
        }

        public override UserLevel requiredUserLevel() { return UserLevel.ADMIN; }
        public override bool protectOnLoad() { return true; }

        public override void run(CommandMessage commandMessage)
        {

            string _target = (commandMessage.PublicCommand ? commandMessage.Target : commandMessage.CommandSender.Nick);
            commandMessage.ReplyMethod(_target, "Test reply!");
            
        }

    }
}
