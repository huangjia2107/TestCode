#include "pch.h"
#include "MP4Generator.h"

MP4FileHandle CreateMP4File(TCHAR *sfilePath)
{
	if (mFile == NULL)
	{
		const char* fileName = TCHAR2char(sfilePath);
		mFile = MP4Create(fileName);

		if (mFile == MP4_INVALID_FILE_HANDLE)
			return NULL;

		if (mFile == NULL)
			return NULL;

		// ����ʱ��Ƭ
		MP4SetTimeScale(mFile, VIDEO_TIME_SCALE);
	}

	return mFile;
}

int WriteH264Data(MP4FileHandle pMP4File, const unsigned char* pData, int size, long dwDataType)
{
	if (pMP4File == NULL)
		return -1;

	if (pData == NULL)
		return -1;

	MP4ENC_NaluUnit nalu;
	int pos = 0;
	int	len = 0;
	while (len = ReadOneNaluFromBuf(pData, size, pos, nalu))
	{
		if (nalu.type == 0x07) // sps
		{
			if (mVideoId == MP4_INVALID_TRACK_ID)
			{	// ���h264 track 
				mVideoId = MP4AddH264VideoTrack(
					pMP4File,
					VIDEO_TIME_SCALE,       // ��Ƶÿ���ticks������90000��
					VIDEO_TIME_SCALE / 25,  // ��Ƶ�Ĺ̶�����Ƶ֡����ʾʱ��,��ʽΪtimeScale��90000��/fps(��������20f)
					VIDEO_WIDTH,			// ��Ƶ�Ŀ��
					VIDEO_HEIGHT,			// ��Ƶ�ĸ߶�
					nalu.data[1],			// sps[1] AVCProfileIndication
					nalu.data[2],			// sps[2] profile_compat
					nalu.data[3],			// sps[3] AVCLevelIndication
					3						// 4 bytes length before each NAL unit
				);

				if (mVideoId == MP4_INVALID_TRACK_ID)
				{
					printf("add video track failed.\n");
					return 0;
				}
				MP4SetVideoProfileLevel(pMP4File, 0x01); //  Simple Profile @ Level 3
				MP4AddH264SequenceParameterSet(pMP4File, mVideoId, nalu.data, nalu.size);
			}
		}
		else if (nalu.type == 0x08) // pps
		{
			MP4AddH264PictureParameterSet(pMP4File, mVideoId, nalu.data, nalu.size);
		}
		else
		{
			int datalen = nalu.size + 4;
			unsigned char* data = new unsigned char[datalen];

			// MP4 Naluǰ�ĸ��ֽڱ�ʾNalu����
			data[0] = nalu.size >> 24;
			data[1] = nalu.size >> 16;
			data[2] = nalu.size >> 8;
			data[3] = nalu.size & 0xff;
			memcpy(data + 4, nalu.data, nalu.size);
			if (dwDataType == VIDEO_I_FRAME)
			{
				MP4WriteSample(pMP4File, mVideoId, data, datalen, MP4_INVALID_DURATION, 0, 1);
			}

			if (dwDataType == VIDEO_P_FRAME)
			{
				MP4WriteSample(pMP4File, mVideoId, data, datalen, MP4_INVALID_DURATION, 0, 0);
			}
			delete[] data;
		}

		pos += len;
	}
	return pos;
}

int ReadOneNaluFromBuf(const unsigned char* pBuffer, unsigned int nBufferSize, unsigned int offSet, MP4ENC_NaluUnit& nalu)
{
	unsigned int i = offSet;
	while (i < nBufferSize)
	{//Ѱ�ҵ�һ��00 00 00 01
		if (pBuffer[i++] == 0x00 &&
			pBuffer[i++] == 0x00 &&
			pBuffer[i++] == 0x00 &&
			pBuffer[i++] == 0x01
			)
		{
			unsigned int pos = i;
			while (pos < nBufferSize)
			{//Ѱ�����һ��00 00 00 01
				if (pBuffer[pos++] == 0x00 &&
					pBuffer[pos++] == 0x00 &&
					pBuffer[pos++] == 0x00 &&
					pBuffer[pos++] == 0x01
					)
				{
					break;
				}
			}

			if (pos == nBufferSize)
			{
				nalu.size = pos - i;
			}

			else
			{
				nalu.size = (pos - 4) - i;
			}

			nalu.type = pBuffer[i] & 0x1f;
			nalu.data = (unsigned char*)&pBuffer[i];
			return (nalu.size + i - offSet);
		}
	}
	return 0;
}

bool GenerateMP4File(TCHAR *sfilePath, BYTE* pBuffer, DWORD dwBufSize, long dwDataType)
{
	CreateMP4File(sfilePath);
	WriteH264Data(mFile, pBuffer, dwBufSize, dwDataType);

	return true;
}

void CloseMP4File()
{
	MP4Close(mFile);
}

char* TCHAR2char(const TCHAR* STR)
{
	//�����ַ����ĳ���
	int size = WideCharToMultiByte(CP_ACP, 0, STR, -1, NULL, 0, NULL, FALSE);

	//����һ�����ֽڵ��ַ�������
	char* str = new char[sizeof(char) * size];

	//��STRת��str
	WideCharToMultiByte(CP_ACP, 0, STR, -1, str, size, NULL, FALSE);

	return str;
}

TCHAR* char2TCAHR(const char* str)
{
	int size = MultiByteToWideChar(CP_ACP, 0, str, -1, NULL, 0);
	TCHAR* retStr = new TCHAR[size * sizeof(TCHAR)];
	MultiByteToWideChar(CP_ACP, 0, str, -1, retStr, size);
	return retStr;
}
