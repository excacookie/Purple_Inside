using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterRA;

public interface IRaDisplayedStat
{
    public string Prefix { get; }
    public string Color { get; }

}
