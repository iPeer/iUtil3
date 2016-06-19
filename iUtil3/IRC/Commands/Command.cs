using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.IRC.Commands
{

    public enum UserLevel
    {
        NONE,
        VOICE,
        HOP,
        OP,
        ADMIN,
        OWNER
    }

    public interface ICommand 
    {
    
        string Name { get;  set; }
        List<string> Aliases { get; }

        void run(CommandMessage commandMessage);
        bool protectOnLoad();

        UserLevel requiredUserLevel();

    }

    public abstract class Command : ICommand
    {

        private List<string> _aliases = new List<string>();

        public string Name { get; set; }
        public List<string> Aliases { get { return this._aliases; } }

        public virtual void run(CommandMessage commandMessage) { }
        public virtual bool protectOnLoad() { return false; }

        public Command(string name/*, params string[] aliases*/)
        {
            Name = name;
            /*registerAlias(name);
            registerAliases(aliases);*/
        }

        public void registerAlias(string alias)
        {
            CommandManager.Instance.logger.Log("Registering alias '{0}' for command '{1}'", alias, this.Name);
            this._aliases.Add(alias);
            CommandManager.Instance.logger.Log("Aliases for command '{0}' are now [\"{1}\"]", this.Name, String.Join("\",\"", this._aliases.ToArray()));
        }

        public void registerAliases(params string[] aliases)
        {
            Contract.Requires(aliases != null && aliases.Any());
            CommandManager.Instance.logger.Log("Registering aliases '[\"{0}\"]' for command '{1}'", String.Join("\",\"", aliases), this.Name);
            this._aliases.AddRange(aliases);
            CommandManager.Instance.logger.Log("Aliases for command '{0}' are now [\"{1}\"]", this.Name, String.Join("\",\"", this._aliases.ToArray()));
        }

        public void registerAliases(List<string> aliases)
        {
            registerAliases(aliases.ToArray());
        }

        public virtual UserLevel requiredUserLevel() { return UserLevel.NONE; }

    }
}
