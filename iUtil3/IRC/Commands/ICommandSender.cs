using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.IRC.Commands
{
    public interface ICommandSender
    {

        string Nick { get; set; }
        string Address { get; set; }
        string Identd { get; set; }

    }
}
