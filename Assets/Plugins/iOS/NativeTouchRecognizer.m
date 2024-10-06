#import "NativeTouchRecognizer.h"
#import "UnityAppController.h"
#import "UnityView.h"

@implementation NativeTouchRecognizer

NativeTouchRecognizer* gestureRecognizer;
UnityView* unityView;
CGRect screenSize;
CGFloat screenScale;

static double appStartTime; // Static variable to store the app start time

// static int audioBufferIndex = -1;
// static int audioSourceIndex = -1;

// typedef void (*NativeTouchesDelegate)(int x, int y, double elapsedTime, int state);
typedef void (*NativeTouchesDelegate)(const char* touchDataJson);
typedef void (*NativeTimestampDelegate)(const char* timestamp);

NativeTouchesDelegate callback;
NativeTimestampDelegate timestampCallback;

// Mapping between UITouch and touch IDs
static NSMutableDictionary<UITouch*, NSNumber*> *touchIdMapping;
static int nextTouchId = 0;

+ (void)initialize {
    if (self == [NativeTouchRecognizer class]) {
        // Record the app start time when the class is initialized
        appStartTime = [NativeTouchRecognizer GetCurrentTimeInMilliseconds];

        NSLog(@"[Native] App Start Time recorded: %f", appStartTime);
        // [NativeTouchRecognizer PrintIOSTimeStamp];
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

+ (void)StartNativeTouchWithCallback:(NativeTouchesDelegate)nativeTouchDelegate
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
    return CGPointMake(point.x * screenScale, (screenSize.size.height - point.y) * screenScale);
}

+(void)PrintIOSTimeStampWithCallback:(NativeTimestampDelegate)nativeTimestampDelegate
{
    timestampCallback = nativeTimestampDelegate;

    NSString *currentTimeStamp = [NativeTouchRecognizer GetCurrentDateTimeAsString];
    
    NSLog(@"[Native] Current Time Stamp: %@", currentTimeStamp);

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
+(void)sendTouchesToUnity:(NSSet<UITouch*> *)touches
{
    NSMutableArray *touchesDataArray = [NSMutableArray array];

    for (UITouch* touch in touches)
    {
         // Assign a unique touch ID if necessary
        NSNumber *touchId = touchIdMapping[touch];
        if (touchId == nil)
        {
            touchId = @(nextTouchId++);
            touchIdMapping[touch] = touchId;
        }

        CGPoint location = [NativeTouchRecognizer scaledCGPoint:[touch locationInView:nil]];
        double iosTimestampMS = [NativeTouchRecognizer GetCurrentTimeInMilliseconds];
        int phase = (int)touch.phase;

        NSDictionary *touchData = @{
            @"id": touchId,
            @"x": @(location.x),
            @"y": @(location.y),
            @"timestamp": @(iosTimestampMS),
            @"phase": @(phase)
        };

        [touchesDataArray addObject:touchData];
        
         NSLog(@"[Native] Touch ID: %@ at position: (%.2f, %.2f) | Phase: %ld | Time: %@",
              touchId, location.x, location.y, (long)touch.phase, [NativeTouchRecognizer GetCurrentDateTimeAsString]);

        // if (callback != nil) {
        //     callback((int)location.x, (int)location.y, iosTimestampMS, (int)phase);
        // }
    }

     // Convert the touchesDataArray to JSON string
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:touchesDataArray options:0 error:&error];
    if (!jsonData) {
        NSLog(@"[Native] Error serializing touch data to JSON: %@", error);
        return;
    }
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];

    // Send the JSON string to Unity
    if (callback != nil) {
        callback([jsonString UTF8String]);
    }
}

-(void)touchesBegan:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{

    // NSLog(@"[Native] Detected phase 0 and playing sound | Time: %@", [NativeTouchRecognizer GetCurrentDateTimeAsString]);
    // [NativeTouchRecognizer PlayTouchSound];

    [NativeTouchRecognizer sendTouchesToUnity:touches];
}

-(void)touchesEnded:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{
    [NativeTouchRecognizer sendTouchesToUnity:touches];

    // Remove touch IDs for ended touches
    for (UITouch *touch in touches)
    {
        [touchIdMapping removeObjectForKey:touch];
    }
}

-(void)touchesMoved:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{
    [NativeTouchRecognizer sendTouchesToUnity:touches];
}

-(void)touchesCancelled:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{
    [NativeTouchRecognizer sendTouchesToUnity:touches];

    // Remove touch IDs for cancelled touches
    for (UITouch *touch in touches)
    {
        [touchIdMapping removeObjectForKey:touch];
    }
}
@end

extern void _StopNativeTouch()
{
    [NativeTouchRecognizer StopNativeTouch];
}

extern void _StartNativeTouch(NativeTouchesDelegate nativeTouchDelegate)
{
    [NativeTouchRecognizer StartNativeTouchWithCallback:nativeTouchDelegate];
}

extern void _PrintIOSTimeStamp(NativeTimestampDelegate timestampCallback)
{
    [NativeTouchRecognizer PrintIOSTimeStampWithCallback:timestampCallback];
}