
#ifndef NativeTouchRecognizer_h
#define NativeTouchRecognizer_h

#import <UIKit/UIKit.h>
#import <UIKit/UIGestureRecognizerSubclass.h>

// Typedef for the callback function pointer
typedef void (*NativeTouchDelegate)(int x, int y, double iosTimeInMilliseconds, int state);
typedef void (*NativeTimestampDelegate)(const char* timestamp);

@interface NativeTouchRecognizer : UIGestureRecognizer

// Class methods
+ (void)StartNativeTouchWithCallback:(NativeTouchDelegate)nativeTouchDelegate;
+ (void)StopNativeTouch;
+ (NativeTouchRecognizer*)GetInstance;
+ (void)PrintIOSTimeStampWithCallback:(NativeTimestampDelegate)timestampCallback;
+ (void)PrintIOSTimeStamp;

// Public methods
+ (CGPoint)scaledCGPoint:(CGPoint)point;
+ (NSString *)GetCurrentDateTimeAsString;
+ (double)GetCurrentTimeInMilliseconds;

@end

#endif /* NativeTouchRecognizer_h */

