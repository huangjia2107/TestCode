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
#define VIDEO_TIME_SCALE 90000		// ��Ƶÿ���ticks��
#define AUDIO_TIME_SCALE 8000		// ��Ƶÿ���ticks��
#define VIDEO_WIDTH 1280			// ��Ƶ���
#define VIDEO_HEIGHT 720			// ��Ƶ�߶�

//NALU��Ԫ
typedef struct _MP4ENC_NaluUnit
{
	int type;
	int size;
	unsigned char* data;
}MP4ENC_NaluUnit;


//���
MP4FileHandle mFile;
MP4TrackId mVideoId;			// viodeo��trackID

MP4FileHandle CreateMP4File(string sfilePath);
int WriteH264Data(MP4FileHandle MP4File, const unsigned char* pData, int size, long dwDataType);    // ��H264����д��MP4�ļ�
int ReadOneNaluFromBuf(const unsigned char* pBuffer, unsigned int nBufferSize, unsigned int offSet, MP4ENC_NaluUnit& nalu); // ��H264���ݻ�������ȡһ��nalu

EXPORTDLL bool GenerateMP4File(string sfilePath, BYTE* pBuffer, DWORD dwBufSize, long dwDataType);							// ����H264����ΪMP4�ļ��Ľӿ�
EXPORTDLL void CloseMP4File();																				// �ر�MP4�ļ��Ľӿ�