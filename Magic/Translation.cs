using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic;

public class Translation
{
    // Temps singleton le temps que l'on crée la config
    public static Translation Singleton = new Translation();

    public string CoolDown { get; set; }
}
