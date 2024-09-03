#import "NativeTouchRecognizer.h"
#import "UnityAppController.h"
#import "UnityView.h"

@implementation NativeTouchRecognizer

NativeTouchRecognizer* gestureRecognizer;
UnityView* unityView;
CGRect screenSize;
CGFloat screenScale;

static double appStartTime; // Static variable to store the app start time

typedef void (*NativeTouchDelegate)(int x, int y, double elapsedTime, int state);
typedef void (*NativeTimestampDelegate)(const char* timestamp);

NativeTouchDelegate callback;
NativeTimestampDelegate timestampCallback;

+ (void)initialize {
    if (self == [NativeTouchRecognizer class]) {
        // Record the app start time when the class is initialized
        appStartTime = [NativeTouchRecognizer GetCurrentTimeInMilliseconds];

        NSLog(@"[Native] App Start Time recorded: %f", [NativeTouchRecognizer GetCurrentTimeInMilliseconds]);
        [NativeTouchRecognizer PrintIOSTimeStamp];
    }
}

+ (NativeTouchRecognizer*) GetInstance
{
    if (gestureRecognizer == nil)
    {
        gestureRecognizer = [[NativeTouchRecognizer alloc] init];
    }
    return gestureRecognizer;
}

+ (void)StopNativeTouch
{
    [unityView removeGestureRecognizer:gestureRecognizer];
}

+ (void)StartNativeTouchWithCallback:(NativeTouchDelegate)nativeTouchDelegate
{
    callback = nativeTouchDelegate;
    
    UnityAppController* uiApp = GetAppController();
    unityView = [uiApp unityView];
    screenScale = [[UIScreen mainScreen] scale];
    screenSize = [unityView bounds];
    
   NSLog(@"[Native] Starting native touch - Screen : %@ Scale : %f %@", NSStringFromCGRect(screenSize), screenScale, [NativeTouchRecognizer GetCurrentDateTimeAsString]);

    gestureRecognizer = [[NativeTouchRecognizer alloc] init];
    [unityView addGestureRecognizer:gestureRecognizer];
}

+(CGPoint)scaledCGPoint:(CGPoint)point
{
    return CGPointMake(point.x * screenScale, point.y * screenScale);
}

+(NSString *)elapsedTimeSinceAppStart
{
    // Calculate the timestamp since the app started
    double currentTime = [NativeTouchRecognizer GetCurrentTimeInMilliseconds];
    double elapsedTime = currentTime - appStartTime;

    // Calculate hours, minutes, seconds, and milliseconds
    int hours = (int)(elapsedTime / 3600);
    int minutes = ((int)(elapsedTime / 60)) % 60;
    int seconds = (int)elapsedTime % 60;
    int milliseconds = (int)((elapsedTime - floor(elapsedTime)) * 1000);

    // Format the string with the elapsed time
    NSString *elapsedTimeString = [NSString stringWithFormat:@"| Time: %02d:%02d:%02d.%03d", hours, minutes, seconds, milliseconds];

    return elapsedTimeString;
}

+ (void)PrintIOSTimeStamp
{
    NSString *currentTimeStamp = [NativeTouchRecognizer GetCurrentDateTimeAsString];
    NSLog(@"[Native] Current Time Stamp: %@", currentTimeStamp);
    NSLog(@"[Native] Current TimeElapsed: %@", [NativeTouchRecognizer elapsedTimeSinceAppStart]);

     if (timestampCallback != nil)
    {
        timestampCallback([currentTimeStamp UTF8String]);
    }
    else
    {
        NSLog(@"[Native] Timestamp callback is nil");
    }
}
+(void)PrintIOSTimeStampWithCallback:(NativeTimestampDelegate)nativeTimestampDelegate
{
    timestampCallback = nativeTimestampDelegate;

    NSString *currentTimeStamp = [NativeTouchRecognizer GetCurrentDateTimeAsString];
    
    NSLog(@"[Native] Current Time Stamp: %@", currentTimeStamp);
    NSLog(@"[Native] Current TimeElapsed: %@", [NativeTouchRecognizer elapsedTimeSinceAppStart]);

    if (timestampCallback != nil)
    {
        timestampCallback([currentTimeStamp UTF8String]);
    }
    else
    {
        NSLog(@"[Native] Timestamp callback is nil");
    }
}
+ (NSString *)GetCurrentDateTimeAsString
{
    NSDateFormatter *formatter = [[NSDateFormatter alloc] init];
    [formatter setDateFormat:@"yyyy-MM-dd HH:mm:ss.SSS"];
    return [formatter stringFromDate:[NSDate date]];
}
+ (double)GetCurrentTimeInMilliseconds
{
    return [[NSDate date] timeIntervalSince1970] * 1000;
}
+(void)sendTouchesToUnity:(NSSet<UITouch*> *)touches withPhase:(int)phase
{
    NSArray<UITouch *>* touchesArray = [touches allObjects];
    for (UITouch* touch in touchesArray)
    {
        CGPoint location = [NativeTouchRecognizer scaledCGPoint:[touch locationInView:nil]];
        
        double iosTimestampMS = [NativeTouchRecognizer GetCurrentTimeInMilliseconds];

        NSLog(@"[Native] Touch at position: (%.2f, %.2f) | Phase: %d | Time:  %@",
        location.x, location.y, phase, [NativeTouchRecognizer GetCurrentDateTimeAsString]);

    if (callback != nil){
            callback((int)location.x, (int)location.y, iosTimestampMS, phase);
        }
    }
}

-(void)touchesBegan:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{
    [NativeTouchRecognizer sendTouchesToUnity:touches withPhase:0];
}

-(void)touchesEnded:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{
    [NativeTouchRecognizer sendTouchesToUnity:touches withPhase:1];
}

-(void)touchesMoved:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{
    [NativeTouchRecognizer sendTouchesToUnity:touches withPhase:2];
}

@end

extern void _StopNativeTouch()
{
    [NativeTouchRecognizer StopNativeTouch];
}

extern void _StartNativeTouch(NativeTouchDelegate nativeTouchDelegate)
{
    [NativeTouchRecognizer StartNativeTouchWithCallback:nativeTouchDelegate];
}

extern void _PrintIOSTimeStamp(NativeTimestampDelegate timestampCallback)
{
    [NativeTouchRecognizer PrintIOSTimeStampWithCallback:timestampCallback];
}

/*
//
//  NativeTouchRecognizer.m
#define LOG_UNITY_MESSAGE

#import "NativeTouchRecognizer.h"
#import "UnityAppController.h"
#import "UnityView.h"

@implementation NativeTouchRecognizer

NativeTouchRecognizer* gestureRecognizer;
UnityView* unityView;
CGRect screenSize;
CGFloat screenScale;

const char* _gameObjectName = "N";
const char* _methodName = "T";

typedef void (*NativeTouchDelegate)(int x, int y, int state);
NativeTouchDelegate callback;

+ (NativeTouchRecognizer*) GetInstance
{
    if(gestureRecognizer == nil)
    {
        gestureRecognizer = [[NativeTouchRecognizer alloc] init];
    }
    return gestureRecognizer;
}

+ (void) StopNativeTouch
{
    [unityView removeGestureRecognizer:gestureRecognizer];
}

+ (void) StartNativeTouch
{
    UnityAppController* uiApp = GetAppController();
    
    unityView = [uiApp unityView];
    screenScale = [[UIScreen mainScreen]scale];
    screenSize = [unityView bounds];
    
    NSLog(@"Starting native touch - Screen : %@ Scale : %f",NSStringFromCGRect(screenSize),screenScale);
    
    gestureRecognizer = [[NativeTouchRecognizer alloc] init];
    [unityView addGestureRecognizer:gestureRecognizer];
    
}

+(CGPoint) scaledCGPoint:(CGPoint)point
{
    //Retina display have /2 scale and have a smallest unit of pixel as 0.5.
    //This will multiply it back and eliminate the floating point
    
    //0,0 is at the top left of portrait orientation.
    
    return CGPointMake(point.x*screenScale, point.y*screenScale);
}

#ifdef LOG_TOUCH
+(void) logTouches:(NSSet<UITouch*> *) touches
{
    NSArray<UITouch *>* touchesArray = [touches allObjects];
    for(int i = 0; i < [touchesArray count]; i++) {
        UITouch* touch = touchesArray[i];
        NSLog(@"#%d Loc:%@ Prev:%@ Radius:%f Phase:%d",
              i,
              NSStringFromCGPoint([NativeTouchRecognizer scaledCGPoint:[touch locationInView:nil]]),
              NSStringFromCGPoint([NativeTouchRecognizer scaledCGPoint:[touch previousLocationInView:nil]]),
              [touch majorRadius],
              [touch phase]);
    }
}
#endif

 +(const char*) encodeTouch: (UITouch*) touch
 {
     CGPoint location = [NativeTouchRecognizer scaledCGPoint:[touch locationInView:nil]];
     return [[NSString stringWithFormat:@"%d-%d-%d", (int)location.x, (int)location.y,[touch phase]] UTF8String];
 }

+(void) sendTouchesToUnity:(NSSet<UITouch*> *) touches
{
    NSArray<UITouch *>* touchesArray = [touches allObjects];
    for(UITouch* touch in touchesArray)
    {
#ifdef LOG_UNITY_MESSAGE
        NSLog(@"To Unity : %@",[NSString stringWithCString:[NativeTouchRecognizer encodeTouch:touch] encoding:NSUTF8StringEncoding]);
#endif
        CGPoint location = [NativeTouchRecognizer scaledCGPoint:[touch locationInView:nil]];

        callback( (int)location.x, (int) location.y, (int)[touch phase]);

        // if([touch phase] == UITouchPhaseBegan)
        // {
        //     [[IosNativeAudio GetInstance] PlaySoundIos];
        // }
    }
}

-(void) touchesBegan:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{
#ifdef LOG_TOUCH
    [NativeTouchRecognizer logTouches:touches];
#endif
    [NativeTouchRecognizer sendTouchesToUnity:touches];
}

-(void) touchesEnded:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{
#ifdef LOG_TOUCH
    [NativeTouchRecognizer logTouches:touches];
#endif
    [NativeTouchRecognizer sendTouchesToUnity:touches];
}

-(void) touchesMoved:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{
#ifdef LOG_TOUCH
    [NativeTouchRecognizer logTouches:touches];
#endif
    [NativeTouchRecognizer sendTouchesToUnity:touches];
}

@end

extern "C" {

    void _StopNativeTouch() {
        [NativeTouchRecognizer StopNativeTouch];
    }
    
    void _StartNativeTouch(NativeTouchDelegate nativeTouchDelegate) {
        callback = nativeTouchDelegate;
        [NativeTouchRecognizer StartNativeTouch];
    }
    
}
*/