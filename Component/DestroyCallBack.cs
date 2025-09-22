using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic.Component;

public class DestroyCallBack : MonoBehaviour
{
    public Action? CallBack;

    private void OnDestroy()
    {
        CallBack?.Invoke();
    }
}
