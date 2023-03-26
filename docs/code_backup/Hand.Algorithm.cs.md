```csharp
// if(prev != null && note.type == Note.PlayType.NORMAL) {
//     fixedFingers.UnionWith(prev.fingers);
// }
//判断固定手指数量
//如果1、5指间间距不合适，则进行调整
void arrangeIntervalOfFinger1And5(int lowFinger, int? highFinger) {
    if(fixedFingers.Contains(1) && fixedFingers.Contains(5))
        return;
    for(var i = 0; i < 100; i++) {
        int nowNote1 = hand.fingers[1].nowNoteNo,
            nowNoteOfLowFinger = hand.fingers[lowFinger].nowNoteNo,
            nowNoteOfHighFinger = highFinger != null ? hand.fingers
                [highFinger.Value].nowNoteNo : nowNoteOfLowFinger,
            nowNote5 = hand.fingers[5].nowNoteNo;
        if(nowNote5 - nowNote1 > getMaxInterval(1, 5)) {
            if(nowNote1 + 2 < nowNoteOfLowFinger &&
               !fixedFingers.Contains(1)) {
                hand.moveFingerToNote(1, nowNote1 + 2);
            }
            if(nowNote5 - 2 > nowNoteOfHighFinger &&
               !fixedFingers.Contains(5)) {
                hand.moveFingerToNote(5, nowNote5 - 2);
            }
            continue;
        }
        if(nowNote5 - nowNote1 < getMinInterval(1, 5)) {
            if(!fixedFingers.Contains(1)) {
                hand.moveFingerToNote(1, nowNote1 - 2);
            }
            if(!fixedFingers.Contains(5)) {
                hand.moveFingerToNote(5, nowNote5 + 2);
            }
            continue;
        }
        return;
    }
    //输出错误信息
    MainForm.printer.println("算法可能存在错误：fixedFingers：" +
        $"{string.Join(",", fixedFingers)}，low：{lowFinger}，" +
        $"high：{highFinger}");
    for(var i = 1; i <= 5; i++) {
        MainForm.printer.println($@"finger-{i}: {hand.fingers[i]
            .nowNoteNo}");
    }
}
if(fixedFingers.Count == 1) {
    int finger = fixedFingers.First();
    switch(finger) {
        case 1: {
            arrangeInterval(1, 5, false);
            int distance = hand.fingers[5].nowNoteNo - hand.fingers[1].nowNoteNo;
            int step = distance / 4;
            for(var i = 2; i <= 4; i++) {
                hand.moveFingerToNote(i, hand.fingers[i - 1]
                    .nowNoteNo + step);
            }
            break;
        }
        case 5: {
            arrangeInterval(1, 5, true);
            int distance = hand.fingers[5].nowNoteNo - hand.fingers[1].nowNoteNo;
            int step = distance / 4;
            for(var i = 2; i <= 4; i++) {
                hand.moveFingerToNote(i, hand.fingers[i - 1]
                    .nowNoteNo + step);
            }
            break;
        }
        default: {
            //首先移动1、5指到合适的位置
            arrangeInterval(1, finger, true);
            arrangeInterval(finger, 5, false);
            arrangeIntervalOfFinger1And5(finger, null);
            //左
            int leftDistance = hand.fingers[finger].nowNoteNo -
                hand.fingers[1].nowNoteNo;
            int leftStep = leftDistance / (finger - 1);
            for(var i = 2; i < finger; i++) {
                hand.moveFingerToNote(i, hand.fingers[i - 1]
                    .nowNoteNo + leftStep);
            }
            //右
            int rightDistance = hand.fingers[5].nowNoteNo -
                hand.fingers[finger].nowNoteNo;
            int rightStep = rightDistance / (5 - finger);
            for(var i = 4; i > finger; i--) {
                hand.moveFingerToNote(i, hand.fingers[i + 1]
                    .nowNoteNo - rightStep);
            }
            break;
        }
    }
} else {
    List<int> fixedFingersList = fixedFingers.ToList();
    fixedFingersList.Sort();
    if(fixedFingersList.First() != 1) {
        for(int i = fixedFingersList.First() - 1; i > 0; i--) {
            hand.moveFingerToNote(i, hand.fingers[i + 1]
                .nowNoteNo - 2);
        }
    }
    if(fixedFingersList.Last() != 5) {
        for(int i = fixedFingersList.Last() + 1; i <= 5; i++) {
            hand.moveFingerToNote(i, hand.fingers[i - 1]
                .nowNoteNo + 2);
        }
    }
    for(var i = 0; i < fixedFingersList.Count - 1; i++) {
        int low = fixedFingersList[i], high = fixedFingersList[i + 1];
        if(low != 1 && hand.fingers[low].nowNoteNo <= 
           hand.fingers[1].nowNoteNo) {
            if(!fixedFingers.Contains(1)) {
                hand.moveFingerToNote(1, hand.fingers[low]
                    .nowNoteNo - getMaxInterval(1, low));
            }
        }
        if(high != 5 && hand.fingers[high].nowNoteNo >=
           hand.fingers[5].nowNoteNo) {
            if(!fixedFingers.Contains(5)) {
                hand.moveFingerToNote(5, hand.fingers[high]
                    .nowNoteNo + getMaxInterval(high, 5));
            }
        }
        if(low == 1) {
            if(high != 5) {
                arrangeIntervalOfFinger1And5(high, null);
            }
        } else if(high == 5) {
            arrangeIntervalOfFinger1And5(low, null);
        } else {
            arrangeIntervalOfFinger1And5(low, high);
        }
        int step = (hand.fingers[high].nowNoteNo - hand.fingers[low]
            .nowNoteNo) / (high - low);
        for(int j = low + 1; j < high; j++) {
            hand.moveFingerToNote(j, hand.fingers[j - 1]
                .nowNoteNo + step);
        }
    }
}
//优化手指所在的位置
```