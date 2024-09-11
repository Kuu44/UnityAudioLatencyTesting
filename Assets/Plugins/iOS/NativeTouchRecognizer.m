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

typedef void (*NativeTouchDelegate)(int x, int y, double elapsedTime, int state);
typedef void (*NativeTimestampDelegate)(const char* timestamp);

NativeTouchDelegate callback;
NativeTimestampDelegate timestampCallback;

// Declare the NativeAudio functions (C-Linkage) here

// extern int _Initialize();
// extern int _LoadAudio(char* soundUrl, int resamplingQuality);
// extern void _PrepareAudio(int bufferIndex, int nativeSourceIndex);
// extern void _PlayAudioWithNativeSourceIndex(int nativeSourceIndex, NativeAudioPlayAdjustment playAdjustment);
// extern int _GetNativeSource(int index);


+ (void)initialize {
    if (self == [NativeTouchRecognizer class]) {
        // Record the app start time when the class is initialized
        appStartTime = [NativeTouchRecognizer GetCurrentTimeInMilliseconds];


        NSLog(@"[Native] App Start Time recorded: %f", [NativeTouchRecognizer GetCurrentTimeInMilliseconds]);
        // [NativeTouchRecognizer PrintIOSTimeStamp];
        
        //Initialise Sound
        // _Initialize();//OpenAL and NativeAudio

        //Load audio buffer and get index
        // char *soundFile = "A1.wav";
        // audioBufferIndex = _LoadAudio(soundFile, 0);

        // if(audioBufferIndex != -1){
            //get a source for playback
            // audioSourceIndex = _GetNativeSource(-1); //gets a round robin audio source 
        // }

        // if(audioSourceIndex != -1){
            //prepare the buffer to the source
            // _PrepareAudio(audioBufferIndex, audioSourceIndex);
        // }
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
// + (void)PlayTouchSound {
//     NSLog(@"[Native] Started Playing touch sound at %@", [NativeTouchRecognizer GetCurrentDateTimeAsString]);
    
//     if(audioSourceIndex != -1){
//         NativeAudioPlayAdjustment playAdjustment;
//         playAdjustment.volume = 1.0f;
//         playAdjustment.pan = 0.0f; // Set pan (0 is center, -1 if left, 1 is right)
//         playAdjustment.offsetSeconds = 0.0f; // Start at the beginnning of the sound
//         playAdjustment.trackLoop = false;
        
//         //Play the prepared sound
//         _PlayAudioWithNativeSourceIndex(audioSourceIndex, playAdjustment);
        
//         // if (audioPlayer) {
//         //     [audioPlayer play];
        
//         NSLog(@"[Native] begun Playing touch sound at %@", [NativeTouchRecognizer GetCurrentDateTimeAsString]);
//         // }
//     }
// }

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
    return CGPointMake(point.x * screenScale, (screenSize.size.height - point.y) * screenScale);
}

// +(NSString *)elapsedTimeSinceAppStart
// {
//     // Calculate the timestamp since the app started
//     double currentTime = [NativeTouchRecognizer GetCurrentTimeInMilliseconds];
//     double elapsedTime = currentTime - appStartTime;

//     // Calculate hours, minutes, seconds, and milliseconds
//     int hours = (int)(elapsedTime / 3600);
//     int minutes = ((int)(elapsedTime / 60)) % 60;
//     int seconds = (int)elapsedTime % 60;
//     int milliseconds = (int)((elapsedTime - floor(elapsedTime)) * 1000);

//     // Format the string with the elapsed time
//     NSString *elapsedTimeString = [NSString stringWithFormat:@"| Time: %02d:%02d:%02d.%03d", hours, minutes, seconds, milliseconds];

//     return elapsedTimeString;
// }

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
    NSArray<UITouch *>* touchesArray = [touches allObjects];
    for (UITouch* touch in touchesArray)
    {
        // Get the touch phase directly from the UITouch object
        UITouchPhase phase = touch.phase;

        CGPoint location = [NativeTouchRecognizer scaledCGPoint:[touch locationInView:nil]];
        double iosTimestampMS = [NativeTouchRecognizer GetCurrentTimeInMilliseconds];

        NSLog(@"[Native] Touch at position: (%.2f, %.2f) | Phase: %ld | Time: %@",
              location.x, location.y, (long)phase, [NativeTouchRecognizer GetCurrentDateTimeAsString]);

        if (callback != nil) {
            callback((int)location.x, (int)location.y, iosTimestampMS, (int)phase);
        }
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
}

-(void)touchesMoved:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event
{
    [NativeTouchRecognizer sendTouchesToUnity:touches];
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
