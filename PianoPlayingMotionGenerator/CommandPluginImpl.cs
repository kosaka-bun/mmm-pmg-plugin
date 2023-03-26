using System;
using System.Drawing;
using System.Windows.Forms;
using MikuMikuPlugin;

// ReSharper disable ClassNeverInstantiated.Global

namespace PianoPlayingMotionGenerator {

public class CommandPluginImpl : ICommandPlugin {

    //此对象的实例
    public static ICommandPlugin instance;

    public Guid GUID {
        get {
            var ass = System.Reflection.Assembly.GetExecutingAssembly();
            object[] attrs = ass.GetCustomAttributes(typeof(System.Runtime
                .InteropServices.GuidAttribute), false);
            return new Guid(((System.Runtime.InteropServices
                .GuidAttribute)attrs[0]).Value);
        }
    }

    public string Description => "钢琴演奏动作生成器";

    //MMM本体窗口句柄
    public IWin32Window ApplicationForm { get; set; }

    //日语环境下文本
    public string Text => "钢琴演奏动作生成器";

    //英语环境下文本
    public string EnglishText => "PianoPlayingMotionGenerator";

    //插件图标
    public Image Image => null;

    //插件命令栏图标
    public Image SmallImage => null;

    //获取场景中的成员
    public Scene Scene { get; set; }

    /// <summary>
    /// 插件被点击后要执行的内容
    /// </summary>
    /// <param name="e"></param>
    public void Run(CommandArgs e) {
        instance = this;
        Program.showDialog();
    }

    /// <summary>
    /// 插件关闭时需要执行的内容（释放资源）
    /// </summary>
    public void Dispose() {
        instance = null;
    }
}

}