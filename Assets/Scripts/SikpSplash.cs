using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Rendering;

public class SikpSplash : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void Run()
    {
        Task.Run(() =>
        {
            SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
        });
    }
}
