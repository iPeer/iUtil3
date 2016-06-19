using iUtil3.IRC.Commands;
using iUtil3.IRC.IAL.Exceptions;
using iUtil3.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.IRC.IAL
{
    public class IAL
    {

        public static IAL Instance { get; private set; }
        public Logger logger;

        private Dictionary<string, List<User>> CHANNEL_LIST = new Dictionary<string, List<User>>();
        public string ME { get; private set; } // Bot's current nick

        public IAL()
        {

            Instance = this;
            this.logger = new Logger("IAL");

        }

        public void setBotNick(string me) { this.logger.Log("Bot changed nick to '{0}'", me); this.ME = me; }

        public void clearIAL()
        {
            this.logger.Log("Clearing all IAL entries");
            this.CHANNEL_LIST.Clear();
        }

        public void processNickChange(string old, string @new)
        {

            this.CHANNEL_LIST.Values.ToList().ForEach(l => l.FindAll(a => a.Nick.Equals(old)).ForEach((User user) => { user.Nick = @new; }));
            this.logger.Log("Updated IAL entry for '{0}' ({1})", @new, old);

        }

        public void processQuit(string nick)
        {
            this.CHANNEL_LIST.Values.ToList().ForEach(a => a.RemoveAll(b => b.Nick.Equals(nick)));
            this.logger.Log("Removed '{0}' from all IAL entries because they quit the server", nick);
        }

        public void processPart(string channel, string nick)
        {
            try
            {
                this.CHANNEL_LIST[channel.ToLower()].RemoveAll(u => u.Nick.Equals(nick));
                this.logger.Log("Unregistered nick '{0}' from channel '{1}' as the user parted, or was kicked from it", nick, channel);
            }
            catch 
            {
                this.logger.Log("Couldn't unregister nick '{0}' from channel '{1}'", nick, channel);
            }
        }

        public void updateUserEntries(string nick, string identd, string address)
        {
            int u = 0;
            this.CHANNEL_LIST.Values.ToList().ForEach(l => l.FindAll(a => a.Nick.Equals(nick)).ForEach((User user) => 
            {
                if (!user.Identd.Equals(identd) || !user.Address.Equals(address))
                {
                    u++;
                    if (!user.Identd.Equals(identd))
                    {
                        user.Identd = identd;
                    }
                    if (!user.Address.Equals(address))
                    {
                        user.Address = address;
                    }
                }
            }));
            if (u > 0)
                this.logger.Log("Updated all present entries for nick '{0}'", nick);
        }

        public void registerChannel(string channel)
        {

            string _channel = channel.ToLower();
            if (this.CHANNEL_LIST.ContainsKey(_channel))
            {
                this.logger.Log("Channel list already contains an entry for '{0}'", LogLevel.WARNING, _channel);
                return;
            }

            this.CHANNEL_LIST.Add(_channel, new List<User>());
            this.logger.Log("Channel '{0}' has been registered", channel);

        }

        public void registerChannelAndUsers(string channel, User[] users)
        {
            registerChannelAndUsers(channel, users.ToList());
        }

        public void registerChannelAndUsers(string channel, List<User> users)
        {
            registerChannel(channel);
            registerUsersInChannel(channel, users);
        }

        public void registerUsersInChannel(string channel, User[] users)
        {
            registerUsersInChannel(channel, users.ToList());
        }

        public void registerUsersInChannel(string channel, List<User> users)
        {

            if (!this.CHANNEL_LIST.ContainsKey(channel.ToLower()))
                throw new NoSuchChannelException();

            this.logger.Log("Registering {0} user(s) into channel '{1}'", users.Count, channel);
            foreach (User u in users)
            {
                if (this.CHANNEL_LIST[channel.ToLower()].Any(a => a.Nick.Equals(u.Nick)))
                {
                    this.logger.Log("Nick conflict: Channel '{0}' already contains a user with the nickname '{1}'; skipping.", LogLevel.WARNING, channel, u.Nick);
                    continue;
                }
                this.CHANNEL_LIST[channel.ToLower()].Add(u);
                this.logger.Log("User with nick '{0}' has been registered under '{1}'", u.Nick, channel);
            }

        }

        public void unregisterChannel(string channel)
        {
            if (this.CHANNEL_LIST.ContainsKey(channel.ToLower()))
            {
                this.CHANNEL_LIST.Remove(channel.ToLower());
                this.logger.Log("Unregistered channel '{0}'", channel);
            }
            else
            {
                this.logger.Log("Couldn't unregister channe '{0}' because it is not registered", LogLevel.WARNING, channel);
            }
        }

        public List<User> getUsersForChannel(string channel)
        {
            channel = channel.ToLower();
            if (!this.CHANNEL_LIST.ContainsKey(channel))
                throw new NoSuchChannelException();
            return this.CHANNEL_LIST[channel];
        }

        // :bipartite.ny.us.SwiftIRC.net 353 iUtil3 = #Peer.Dev :iUtil3 @iPeer @iUtil @Coder 
        // 0                             1   2      3 4         5-
        public void parseNAMESResponse(string names)
        {

            string[] data = names.Split(' ');

            string channel = data[4];
            string[] nicks = names.TrimEnd().Split(':')[2].Split(' ');

            string modePrefixes = Engine.Instance.Protocol.NETWORK_SETTINGS["PREFIX"].ToString().Split(')')[1];
            List<User> users = new List<User>();

            foreach (string nick in nicks)
            {
                // Extract mode(s) from username prefixes
                int x = 0;
                for (; modePrefixes.IndexOf(nick[x]) > -1; x++);
                string modes = nick.Substring(0, x);

                // Now we know where the modes end, we can easily extract the nick
                string _nick = nick.Substring(x);

                User user = new User(_nick, "", "");
                user.setUserModes(modes);

                users.Add(user);

            }

#if DEBUG
            this.logger.Log("NAMES input parsed: {0}", LogLevel.DEBUG, names);
            foreach (User a in users)
                this.logger.Log("Nick: {0}, IdentD: {1}, Address: {2} -- Modes: {3}", LogLevel.DEBUG, a.Nick, a.Identd, a.Address, a.UserModes);
#endif

            this.registerUsersInChannel(channel.ToLower(), users);


        }

        public UserLevel getUserLevel(string nick)
        {
            try
            {
                List<User> debugChannelNicks = this.getUsersForChannel("#peer.dev");
                if (debugChannelNicks.Any(u => u.Nick.Equals(nick)))
                {
                    User user = debugChannelNicks.First(u => u.Nick.Equals(nick)); // Don't need to check for non-existing nicks here because we do it in the if above
                    if (user.UserModes.IndexOf('@') > -1)
                        return (nick.Equals("iPeer") ? UserLevel.OWNER : UserLevel.ADMIN); // Temporary
                    else if (user.UserModes.IndexOf('%') > -1)
                        return UserLevel.HOP;
                    else if (user.UserModes.IndexOf('+') > -1)
                        return UserLevel.VOICE;
                    else
                        return UserLevel.NONE;
                }
                else
                    return UserLevel.NONE;
            }
            catch (NoSuchChannelException) { return UserLevel.NONE; }
        }

    }
}
