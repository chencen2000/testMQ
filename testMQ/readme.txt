command line:
-playback=<path to folder for zip file>
-label=n, 1
-port=com<n>


iPhone and iPod touch Retina display: 120 x 120
iPhone and iPod touch standard resolution: 60 x 60

iPad Retina display: 152 x 152
iPad standard resolution: 76 x 76

row 1 and col 1:
(28,18) w:140 h:180

blue focus on home screen
76,127,206
68,144,220
141,206,212
37,89,139
60,132,216


iphone screen:
normal:
top margin: 26 (2.6%)
middle margin: 50 (5%)
botton margin: 130 (13%)
left margin: 30 (5%)
right margin: 30 (5%)
row=6
col=4
int w = (f1.Width - left_margin - left_margin) / total_col;
int h = (f1.Height - top_margin - botton_line - middle_margin) / total_row;


[2018-08-20T09:48:50.2494974-07:00]: 1_1: {X=30,Y=24,Width=111,Height=111}
[2018-08-20T09:48:50.2504968-07:00]: 1_2: {X=163,Y=24,Width=111,Height=111}
[2018-08-20T09:48:50.2504968-07:00]: 1_3: {X=296,Y=24,Width=111,Height=111}
[2018-08-20T09:48:50.2514978-07:00]: 1_4: {X=429,Y=24,Width=111,Height=111}
[2018-08-20T09:48:50.2514978-07:00]: 2_1: {X=30,Y=157,Width=111,Height=111}
[2018-08-20T09:48:50.2524973-07:00]: 2_2: {X=163,Y=157,Width=111,Height=111}
[2018-08-20T09:48:50.2524973-07:00]: 2_3: {X=296,Y=157,Width=111,Height=111}
[2018-08-20T09:48:50.2534974-07:00]: 2_4: {X=429,Y=157,Width=111,Height=111}
[2018-08-20T09:48:50.2534974-07:00]: 3_1: {X=30,Y=290,Width=111,Height=111}
[2018-08-20T09:48:50.2544987-07:00]: 3_2: {X=163,Y=290,Width=111,Height=111}
[2018-08-20T09:48:50.2544987-07:00]: 3_3: {X=296,Y=290,Width=111,Height=111}
[2018-08-20T09:48:50.2555069-07:00]: 3_4: {X=429,Y=290,Width=111,Height=111}
[2018-08-20T09:48:50.2564969-07:00]: 4_1: {X=30,Y=423,Width=111,Height=111}
[2018-08-20T09:48:50.2564969-07:00]: 4_2: {X=163,Y=423,Width=111,Height=111}
[2018-08-20T09:48:50.2574995-07:00]: 4_3: {X=296,Y=423,Width=111,Height=111}
[2018-08-20T09:48:50.2574995-07:00]: 4_4: {X=429,Y=423,Width=111,Height=111}
[2018-08-20T09:48:50.2584998-07:00]: 5_1: {X=30,Y=556,Width=111,Height=111}
[2018-08-20T09:48:50.2584998-07:00]: 5_2: {X=163,Y=556,Width=111,Height=111}
[2018-08-20T09:48:50.2594990-07:00]: 5_3: {X=296,Y=556,Width=111,Height=111}
[2018-08-20T09:48:50.2594990-07:00]: 5_4: {X=429,Y=556,Width=111,Height=111}
[2018-08-20T09:48:50.2604990-07:00]: 6_1: {X=30,Y=689,Width=111,Height=111}
[2018-08-20T09:48:50.2604990-07:00]: 6_2: {X=163,Y=689,Width=111,Height=111}
[2018-08-20T09:48:50.2614972-07:00]: 6_3: {X=296,Y=689,Width=111,Height=111}
[2018-08-20T09:48:50.2614972-07:00]: 6_4: {X=429,Y=689,Width=111,Height=111}
[2018-08-20T09:48:50.2624982-07:00]: 0_1: {X=30,Y=869,Width=111,Height=111}
[2018-08-20T09:48:50.2624982-07:00]: 0_2: {X=163,Y=869,Width=111,Height=111}
[2018-08-20T09:48:50.2624982-07:00]: 0_3: {X=296,Y=869,Width=111,Height=111}
[2018-08-20T09:48:50.2634970-07:00]: 0_4: {X=429,Y=869,Width=111,Height=111}


8MP camera:
blue on home screen:
Bgr c1 = new Bgr(80, 20, 10)
Bgr c2 = new Bgr(160, 100, 60)
Image<Gray, Byte> g1 = (img1.ToImage<Bgr, Byte>()).InRange(c1, c2);

2MP camera:
Bgr c3 = new Bgr(170, 130, 70); //new Bgr(160, 50, 30);
Bgr c4 = new Bgr(250, 230, 160); //new Bgr(250, 200, 100);

airplay mirroring
iphone 6s: 1334-by-750-pixel resolution at 326 ppi
homepage blue size
normal: 141x196; row=6
zoomed: 125x179; row=5,


http://ip:21173/getConnectedDevicesList
http://ip:21173/getScreen?deviceId={deviceId}


