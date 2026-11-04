using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Rendering;

public class SikpSplash : MonoBehaviour
{
    /// <summary>
    /// 在启动加载阶段异步停止 Unity 闪屏显示。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void Run()
    {
        // 将闪屏停止操作放到后台任务中执行。
        Task.Run(() =>
        {
            SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
        });
    }
}
