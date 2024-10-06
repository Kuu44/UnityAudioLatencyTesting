#ifndef NativeTouchRecognizer_h
#define NativeTouchRecognizer_h

#import <UIKit/UIKit.h>
#import <UIKit/UIGestureRecognizerSubclass.h>

// Typedef for the callback function pointer
typedef void (*NativeTouchesDelegate)(const char* touchDataJson);
typedef void (*NativeTimestampDelegate)(const char* timestamp);

@interface NativeTouchRecognizer : UIGestureRecognizer

// Class methods
+ (void)StartNativeTouchWithCallback:(NativeTouchesDelegate)nativeTouchDelegate;
+ (void)StopNativeTouch;
+ (NativeTouchRecognizer*)GetInstance;
+ (void)PrintIOSTimeStampWithCallback:(NativeTimestampDelegate)timestampCallback;
+ (void)PrintIOSTimeStamp;

//Audio methods
// + (void)PlayTouchSound;

// Public methods
+ (CGPoint)scaledCGPoint:(CGPoint)point;
+ (NSString *)GetCurrentDateTimeAsString;
+ (double)GetCurrentTimeInMilliseconds;

@end

#endif /* NativeTouchRecognizer_h */

