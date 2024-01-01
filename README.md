# MMM PMG Plugin

[![License](https://img.shields.io/github/license/kosaka-bun/mmm-pmg-plugin?label=License&color=blue&logo=GitHub)](./LICENSE)
![GitHub Stars](https://img.shields.io/github/stars/kosaka-bun/mmm-pmg-plugin?label=Stars&logo=GitHub&style=flat)
![GitHub Forks](https://img.shields.io/github/forks/kosaka-bun/mmm-pmg-plugin?label=Forks&logo=GitHub&style=flat)

## 简介
本项目全称为**M**iku**M**iku**M**oving **P**iano **M**otion **G**enerator Plugin，是一款基于MikuMikuMoving平台开发的，用于为那些能在MikuMikuMoving中加载的人物模型，生成钢琴演奏动作的插件。

能生成的动作包括人在演奏钢琴时，在手指、手腕、手臂、上半身、踏板、目光等部分上的动作。

本项目采用GPL-3.0 License，其要求：

- 本项目的衍生项目需采用GPL-3.0 License。
- 必须在修改的文件中附有明确的说明，须包含所修改的部分及具体的修改日期。
- 通过任何形式发布衍生项目的可执行程序时，必须同时附带或公布衍生项目的源代码。

## 前言
本项目开始于2020年上半年，旨在解决[原有的插件](https://bowlroll.net/file/73817)存在的一些譬如生成的动作不自然的问题。由于本项目工作量庞大，内容复杂困难，且作者没有足够多的空闲时间来投入本项目的研究，故现将此尚未完成的项目代码开源，供有相关兴趣和研究方向的开发人员参考和使用。

若有相关开发人员愿意参与到此项目的开发中来，或此项目对相关开发人员的研究有帮助，作者将不胜荣幸和感激。

## 细节
### 原插件
可在[此处](https://www.nicovideo.jp/watch/sm28798666)查看原插件的使用教程，原插件在本项目中备份在[此处](./files/)。

其原理大致为：
1. 改造模型，使手指和手腕关节支持按坐标轴进行平移。
2. 在MikuMikuMoving中结合钢琴模型，移动人物模型的各个关节，注册十余帧的关键帧，以确定手指、手腕的原始位置、琴键的间距与方向等数据。
3. 插件读取MIDI数据，根据音符序列以及注册的关键帧数据，计算出每个音符对应的手指手腕动作、踏板动作等数据。插件将自动把这些数据，注册到后续的帧当中。

原插件在生成动作数据时，还会附带创建与指定的MIDI文件相关的csv文件，其中记录了每个音符的开始帧、结束帧、音符号等数据，可在[此处](./docs/original_plugin/generated_csv/)查看。

### 基本原则
本项目不解析MIDI文件，而是解析原插件所生成的csv文件，来读取音符序列数据。与原插件类似，本项目也需要注册相应的关键帧，但相关细节会更多一些。

譬如，原插件在处理大跨度复音，如八度双音时，并不会把手掌放平，以及略微向小指方向旋转手掌，这使得在这样的情况下，手指会被拉得很长。

此外，原插件对音符的相关指法的计算也不够准确，本项目对指法进行了重新计算。

### 进度
现目前已完成指法计算算法，和初步的动作生成逻辑。

通过[开发备忘](./docs/memorandum/)，可查看到当前项目的详细进度，和正在研究处理的问题。

### 笔记
通过[策略笔记](./docs/strategy/)和[思考笔记](./docs/thinking/)，可查看在开发过程中，对某些较有难度的问题所使用的策略和思路。

### 相关资料
- [MikuMikuMoving插件类库文档](https://sites.google.com/site/mikumikumoving/%E3%83%9E%E3%83%8B%E3%83%A5%E3%82%A2%E3%83%AB/17-%E3%83%97%E3%83%A9%E3%82%B0%E3%82%A4%E3%83%B3)
- [MikuMikuMoving插件开发基础教程](https://mmdybk.gitee.io/md/MikuMikuMoving插件编写介绍/)
