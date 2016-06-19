using iUtil3.IRC.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.IRC.IAL
{
    public class User : ICommandSender
    {

        public string Nick { get; set; }
        public string Address { get; set; }
        public string Identd { get; set; }
        public string FullAddress { get { return String.Format("{0}!{1}@{2}", this.Nick, this.Identd, this.Address); } }

        private string _userModes;
        public string UserModes { get { return this._userModes; } private set { this._userModes = value; } }

        public User(string nick, string identd, string address) {
            this.Nick = nick;
            this.Identd = identd;
            this.Address = address;
        }

        public void setUserModes(string modes)
        {
            this.UserModes = modes;
        }

        public void addUserMode(string mode)
        {
            if (this._userModes.IndexOf(mode) > -1) { return; }
            this._userModes += mode;
        }

        public void addUserModes(params string[] modes)
        {
            Contract.Requires(modes != null && modes.Any());
            foreach (string s in modes)
                addUserMode(s);
        }

        public void removeUserMode(string mode)
        {
            setUserModes(this._userModes.Replace(mode, ""));
        }

        public void removeUserModes(params string[] modes)
        {
            Contract.Requires(modes != null && modes.Any());
            string newModes = this._userModes;
            foreach (string s in modes)
                newModes = newModes.Replace(s, "");
            setUserModes(newModes);
        }

    }
}
