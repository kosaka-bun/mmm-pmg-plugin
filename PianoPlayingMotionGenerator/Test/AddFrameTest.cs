// ReSharper disable InconsistentNaming
namespace PianoPlayingMotionGenerator.Test {

/// <summary>
/// 本类用于在计算关键帧之前，注册几个测试性的关键帧，测试计算结果的正确性
/// </summary>
public class AddFrameTest {

    //在窗口处勾选运行测试时才会被调用
    public void run() {
        //wristMoveToBlackKeyTest(30);
    }

    /*private void wristMoveToBlackKeyTest(int frameNo) {
        leftHand.wristMoveTo(MovingData.CENTER_C - 12 + 8, frameNo);    //#G
        rightHand.wristMoveTo(MovingData.CENTER_C + 8, frameNo);
        leftHand.wristMoveTo(MovingData.CENTER_C - 12 + 1, frameNo + 1);
        rightHand.wristMoveTo(MovingData.CENTER_C + 1, frameNo + 1);
    }*/

    public AddFrameTest(Printable form) {
        this.form = form;
    }

    private readonly HandModel.Hand 
        leftHand = Program.leftHand, 
        rightHand = Program.rightHand;

    private Printable form;
}

}