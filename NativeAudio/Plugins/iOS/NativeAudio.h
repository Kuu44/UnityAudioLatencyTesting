// Native Audio
// 5argon - Exceed7 Experiments
// Problems/suggestions : 5argon@exceed7.com

#import <AVFoundation/AVFoundation.h>
#import <OpenAl/al.h>
#import <OpenAl/alc.h>
#import "libsamplerate-0.1.9/src/samplerate.h"
#include <AudioToolbox/AudioToolbox.h>

@interface NativeAudio : NSObject
{
}

typedef struct
{
    ALuint left;
    ALuint right;
    int channels;
    int bitDepth;
    float lengthSeconds;
} NativeAudioBufferIdPair;

typedef struct
{
    ALuint left;
    ALuint right;
} NativeAudioSourceIdPair;

typedef struct
{
    float volume;
    float pan;
    float offsetSeconds;
    bool trackLoop;
} NativeAudioPlayAdjustment;

+ (int)Initialize;
+ (int)LoadAudio:(char *)soundUrl resamplingQuality:(int)resamplingQuality;
+ (int)SendByteArray:(char *)audioData audioSize:(int)audioSize channels:(int)channel samplingRate:(int)samplingRate resamplingQuality:(int)resamplingQuality;
+ (int)PrepareAudio:(int)alBufferIndex IntoNativeSourceIndex:(int)nativeSourceIndex;
// + (int)PlayAudio:(int)alBufferIndex NativeSourceIndex:(int)nativeSourceIndex Adjustment:(NativeAudioPlayAdjustment)playAdjustment;
+ (void)PlayAudioWithNativeSourceIndex:(int)nativeSourceIndex Adjustment:(NativeAudioPlayAdjustment)playAdjustment;
+ (void)UnloadAudio:(int)index;
+ (float)LengthByAudioBuffer:(int)index;
+ (void)StopAudio:(int)nativeSourceIndex;

+ (void)SetVolume:(float)volume OnNativeSourceIndex:(int)nativeSourceIndex;
+ (void)SetPitch:(float)pitchFactor OnNativeSourceIndex:(int)nativeSourceIndex;
+ (void)SetPan:(float)pan OnNativeSourceIndex:(int)nativeSourceIndex;

+ (void)GetDeviceAudioInformation: (double*)interopArray OutputDeviceEnumArray:(int*) outputDeviceEnumArray;

+ (int)GetNativeSource:(int)index;

+ (float)GetPlaybackTimeOfNativeSourceIndex:(int)nativeSourceIndex;
+ (void)SetPlaybackTimeOfNativeSourceIndex:(int)nativeSourceIndex Offset:(float)offsetSeconds;
+ (void)Pause:(int)nativeSourceIndex;
+ (void)Resume:(int)nativeSourceIndex;

@end