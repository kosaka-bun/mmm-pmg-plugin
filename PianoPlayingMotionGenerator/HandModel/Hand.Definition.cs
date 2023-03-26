using DxMath;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace PianoPlayingMotionGenerator.HandModel {

/// <summary>
/// 手掌模型类，移动数据定义与初始化部分
/// </summary>
public partial class Hand {

    //白键间向量间隔
    public Vector3 whiteKeySpacing;

    //弹下白键和黑键的向量间隔
    public Vector3 pressWhiteKeySpacing, pressBlackKeySpacing;

    //每根手指到它右边的黑键的向量间隔
    public Vector3[] toBlackKeyRightSpacing = new Vector3[6];

    //每根手指到它左边的黑键的向量间隔
    public Vector3[] toBlackKeyLeftSpacing = new Vector3[6];

    //手腕到右边黑键的向量间隔
    public Vector3 wristToBlackKeyRightSpacing;
    
    //手指从标准位置到白键较深处位置（Z轴反方向）的向量间隔
    public Vector3[] toWhiteKeyDeepPositionSpacing = new Vector3[6];

    /// <summary>
    /// 加载定义的的移动数据
    /// </summary>
    private void loadData() {
        //计算白键间向量间隔
        whiteKeySpacing = (getPosOfFinger(1, 2) -
            getPosOfFinger(1, 0)) / 2;
        //弹下白键的向量间隔
        pressWhiteKeySpacing = getPosOfFinger(1, 1) -
            getPosOfFinger(1, 0);
        //弹下黑键的向量间隔
        pressBlackKeySpacing = getPosOfFinger(1, 5) -
            getPosOfFinger(1, 4);
        //手腕到右边黑键的向量间隔
        wristToBlackKeyRightSpacing = getPosOfWrist(3) -
            getPosOfWrist(0);
        //初始化五指数据
        for(var finger = 1; finger <= 5; finger++) {
            //计算每根手指到黑键的向量间隔
            toBlackKeyRightSpacing[finger] = getPosOfFinger(finger, 3) -
                getPosOfFinger(finger, 0);
            toBlackKeyLeftSpacing[finger] = getPosOfFinger(finger, 4) -
                getPosOfFinger(finger, 0);
            //手指从标准位置到白键较深处位置（Z轴反方向）的向量间隔
            toWhiteKeyDeepPositionSpacing[finger] = getPosOfFinger(finger, 6) -
                getPosOfFinger(finger, 0);
        }
    }

    /// <summary>
    /// 输出加载的数据
    /// </summary>
    public void printData(Printable printer) {
        printer.println(prefix + "手：");
        printer.println("白键间向量间隔：" + whiteKeySpacing);
        printer.println("弹下白键与黑键的向量间隔：" + pressWhiteKeySpacing +
            "（白），" + pressBlackKeySpacing + "（黑）");
        printer.println("手腕到右边黑键的向量间隔：" + wristToBlackKeyRightSpacing);
        printer.println();
    }
}

}