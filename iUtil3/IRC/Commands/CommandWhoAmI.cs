using iUtil3.IRC.IAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.IRC.Commands
{
    public class CommandWhoAmI : Command
    {

        public CommandWhoAmI()
            : base("WhoAmI")
        {
            registerAliases("whoami");
        }

        public override UserLevel requiredUserLevel() { return UserLevel.NONE; }
        public override bool protectOnLoad() { return true; }

        public override void run(CommandMessage commandMessage)
        {

            if (Engine.Instance.Protocol.NETWORK_SETTINGS["CHANTYPES"].ToString().IndexOf(commandMessage.Target.Substring(0, 1)) == -1)
            {
                commandMessage.ReplyMethod((commandMessage.PublicCommand ? commandMessage.Target : commandMessage.CommandSender.Nick), "Sorry, but this command must be called from a channel.");
                return;
            }

            List<User> user = Engine.Instance.Protocol.IAL.getUsersForChannel(commandMessage.Target.ToLower()).FindAll(a => a.Nick.Equals(commandMessage.CommandSender.Nick));
            foreach (User u in user)
            {
                commandMessage.ReplyMethod((commandMessage.PublicCommand ? commandMessage.Target : commandMessage.CommandSender.Nick), String.Format("You are {0} ({1}), your address is {2} and your UserLevel is {3}", commandMessage.CommandSender.Nick, u.Nick, u.FullAddress, commandMessage.UserLevel));
            }

        }

    }
}