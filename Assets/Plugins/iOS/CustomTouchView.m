// #import <UIKit/UIKit.h>
// #import "UnityAppController.h"
// #import "UnityView.h"

// @implementation CustomTouchView

// NativeTouchRecognizer* gestureRecognizer;
// UnityView* unityView;
// CGRect screenSize;
// CGFloat screenScale;

// typedef void (*NativeTouchDelegate)(int x, int y, int state);
// NativeTouchDelegate callback;

// + (CustomTouchView*) GetInstance
// {
//     static CustomTouchView* instance = nil;
//     if (instance == nil)
//     {
//         instance = [[CustomTouchView alloc] init];
//     }
//     return instance;
// }

// + (void) StopNativeTouch
// {
//     [unityView removeGestureRecognizer:gestureRecognizer];
// }

// + (void) StartNativeTouchWithCallback:(NativeTouchDelegate)nativeTouchDelegate
// {
//     callback = nativeTouchDelegate;
    
//     UnityAppController* uiApp = GetAppController();
//     unityView = [uiApp unityView];

//     gestureRecognizer = [[CustomTouchView alloc] init];
//     [unityView addGestureRecognizer:gestureRecognizer];

//     screenScale = [[UIScreen mainScreen] scale];
//     screenSize = [unityView bounds];
    
//     NSLog(@"Starting native touch - Screen : %@ Scale : %f", NSStringFromCGRect(screenSize), screenScale);
// }

// +(CGPoint) scaledCGPoint:(CGPoint)point
// {
//     return CGPointMake(point.x * screenScale, point.y * screenScale);
// }

// +(void) sendTouchesToUnity:(NSSet<UITouch*> *)touches withPhase:(int)phase
// {
//     NSArray<UITouch *>* touchesArray = [touches allObjects];
//     for (UITouch* touch in touchesArray)
//     {
//         CGPoint location = [CustomTouchView scaledCGPoint:[touch locationInView:nil]];
//         callback((int)location.x, (int)location.y, phase);
//     }
// }

// -(void) touchesBegan:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
// {
//     [CustomTouchView sendTouchesToUnity:touches withPhase:0];
// }

// -(void) touchesEnded:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
// {
//     [CustomTouchView sendTouchesToUnity:touches withPhase:1];
// }

// -(void) touchesMoved:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
// {
//     [CustomTouchView sendTouchesToUnity:touches withPhase:2];
// }

// @end

// extern "C" {
//     void _StopNativeTouch() {
//         [CustomTouchView StopNativeTouch];
//     }
    
//     void _StartNativeTouch(NativeTouchDelegate nativeTouchDelegate) {
//         [CustomTouchView StartNativeTouchWithCallback:nativeTouchDelegate];
//     }
// }
