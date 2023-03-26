using System.Data;
using MikuMikuPlugin;
using PianoPlayingMotionGenerator.HandModel;
using PianoPlayingMotionGenerator.Test;
using PianoPlayingMotionGenerator.Util;
using PianoPlayingMotionGenerator.Util.FingeringCalculator;

// ReSharper disable LoopCanBePartlyConvertedToQuery
// ReSharper disable InconsistentNaming

namespace PianoPlayingMotionGenerator {

public static class Program {

    /// <summary>
    /// 开始计算与保存关键帧
    /// </summary>
    public static void execute() {
        if(leftPath == null || rightPath == null) {
            MainForm.printer.println("未选择文件，请点击编辑框选择左右手文件");
            return;
        }
        MainForm.printer.clear();
        if(form.willRunTest) {
            MainForm.printer.println("运行测试……");
            new AddFrameTest(form).run();
        }
        MainForm.printer.println("计算左手……");
        execute(leftPath, leftHand);
        MainForm.printer.println("计算右手……");
        execute(rightPath, rightHand);
        MainForm.printer.println("执行完成");
    }

    private static void execute(string path, Hand hand) {
        DataTable data = CsvFileHelper.openCsv(path);
        var calculator = new FingeringCalculator(hand, data);
        calculator.loadNotes();
        hand.noteList = calculator.calculateWithoutSeq();
        foreach(Note n in hand.noteList) {
            if(form.willPrintRow)
                MainForm.printer.println(n);
            switch(hand.prefix) {
                case "左":
                    break;
                case "右":
                    hand.play(n);
                    break;
            }
        }
    }

    /// <summary>
    /// 打开插件窗口时要执行的方法
    /// 只有此方法执行完成，执行按钮才能被点击
    /// </summary>
    public static void loadData() {
        //当前选择的模型
        Model model = CommandPluginImpl.instance.Scene.ActiveModel;
        //左手
        leftHand = new Hand(model, "左");
        //右手
        rightHand = new Hand(model, "右");
    }

    public static void printData() {
        MainForm.printer.println("\r\n加载信息：\r\n");
        //左手
        leftHand.printData(MainForm.printer);
        //右手
        rightHand.printData(MainForm.printer);
    }

    public static void showDialog() {
        form = new MainForm();
        form.ShowDialog();
    }

    public static Hand leftHand, rightHand;
    
    //默认为null
    public static string rightPath, leftPath; 

    private static MainForm form;
}

}