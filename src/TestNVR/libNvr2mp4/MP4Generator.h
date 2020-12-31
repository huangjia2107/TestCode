#ifdef LIBNVR2MP4_EXPORTS
#define EXPORTDLL extern "C" __declspec(dllexport)
#else
#define EXPORTDLL extern "C" __declspec(dllimport)
#endif

#include <string>
#include "mp4v2\mp4v2.h"
#include "HCNetSDK.h"

//#pragma comment(lib,"../../depends/lib/libmp4v2.lib")

using namespace std;

#define BUFFER_SIZE  (1024*1024)
#define VIDEO_TIME_SCALE 90000		// 视频每秒的ticks数
#define AUDIO_TIME_SCALE 8000		// 音频每秒的ticks数
#define VIDEO_WIDTH 1280			// 视频宽度
#define VIDEO_HEIGHT 720			// 视频高度

//NALU单元
typedef struct _MP4ENC_NaluUnit
{
	int type;
	int size;
	unsigned char* data;
}MP4ENC_NaluUnit;


//句柄
MP4FileHandle mFile;
MP4TrackId mVideoId;			// viodeo的trackID

MP4FileHandle CreateMP4File(TCHAR* sfilePath);
int WriteH264Data(MP4FileHandle MP4File, const unsigned char* pData, int size, long dwDataType);    // 把H264裸码写入MP4文件
int ReadOneNaluFromBuf(const unsigned char* pBuffer, unsigned int nBufferSize, unsigned int offSet, MP4ENC_NaluUnit& nalu); // 从H264数据缓冲区读取一个nalu

TCHAR* char2TCAHR(const char* str);
char* TCHAR2char(const TCHAR* STR);

EXPORTDLL bool GenerateMP4File(TCHAR* sfilePath, BYTE* pBuffer, DWORD dwBufSize, long dwDataType);							// 保存H264裸码为MP4文件的接口
EXPORTDLL void CloseMP4File();																				// 关闭MP4文件的接口