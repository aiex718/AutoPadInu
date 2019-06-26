//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
// OpenCVHelper.cpp

#include "pch.h"
#include "OpenCVHelper.h"
#include "MemoryBuffer.h"
#include <iostream>
using namespace Microsoft::WRL;

using namespace OpenCVBridge;
using namespace Platform;


using namespace cv;

OpenCVHelper::OpenCVHelper()
{
    pMOG2 = createBackgroundSubtractorMOG2();
} 

bool OpenCVHelper::TryConvert(SoftwareBitmap^ from, Mat& convertedMat)
{
    unsigned char* pPixels = nullptr;
    unsigned int capacity = 0;
    if (!GetPointerToPixelData(from, &pPixels, &capacity))
    {
        return false;
    }

    Mat mat(from->PixelHeight,
        from->PixelWidth,
        CV_8UC4, // assume input SoftwareBitmap is BGRA8
        (void*)pPixels);

    // shallow copy because we want convertedMat.data = pPixels
    // don't use .copyTo or .clone
    convertedMat = mat;
    return true;
}

bool OpenCVHelper::GetPointerToPixelData(SoftwareBitmap^ bitmap, unsigned char** pPixelData, unsigned int* capacity)
{
    BitmapBuffer^ bmpBuffer = bitmap->LockBuffer(BitmapBufferAccessMode::ReadWrite);
    IMemoryBufferReference^ reference = bmpBuffer->CreateReference();

    ComPtr<IMemoryBufferByteAccess> pBufferByteAccess;
    if ((reinterpret_cast<IInspectable*>(reference)->QueryInterface(IID_PPV_ARGS(&pBufferByteAccess))) != S_OK)
    {
        return false;
    }

    if (pBufferByteAccess->GetBuffer(pPixelData, capacity) != S_OK)
    {
        return false;
    }
    return true;
}

bool OpenCVHelper::CropImage(Windows::Foundation::Point start, Windows::Foundation::Point end, SoftwareBitmap^ input, SoftwareBitmap^ output)
{
	Mat inputMat, outputMat;
	if (!(TryConvert(input, inputMat) && TryConvert(output, outputMat)))
	{
		return false;
	}

	cv::Point StartPoint(start.X, start.Y), EndPoint(end.X, end.Y);
	rectangle(inputMat, StartPoint, EndPoint, Scalar(255, 255, 255));

	//Crop ROI
	Mat InputMatROI(inputMat, cv::Rect(StartPoint, EndPoint));

	//Affine Transform
	Point2f SrcPoint[]{ Point2f(0,0),Point2f(InputMatROI.cols - 1,0),Point2f(InputMatROI.cols - 1,InputMatROI.rows - 1) };
	Point2f DstPoint[]{ Point2f(0,0),Point2f(outputMat.cols - 1,0),Point2f(outputMat.cols - 1,outputMat.rows - 1) };
	Mat AffineM = getAffineTransform(SrcPoint, DstPoint);

	Mat WarpedMat = Mat::zeros(outputMat.size(), InputMatROI.type());
	warpAffine(InputMatROI, WarpedMat, AffineM, WarpedMat.size());
	WarpedMat.copyTo(outputMat);

	return true;
}

bool OpenCVHelper::MaskAndDrawPanelArea(SoftwareBitmap^ input, SoftwareBitmap^ output)
{
	Mat inputMat, outputMat;
	if (!(TryConvert(input, inputMat) && TryConvert(output, outputMat)))
	{
		return false;
	}

	double Radius = MIN((inputMat.cols) / 12.0, (inputMat.rows) / 10.0);
	double Offset = 0;// Radius / 5.0;

	//Draw Mask
	Mat Mask = Mat::zeros(inputMat.size(),CV_8U);
	for (size_t x = 0; x < 6; x++)
	{
		for (size_t y = 0; y <5; y++)
		{
			cv::Point Center(Offset+(x*2+1)*Radius, Offset+(y * 2 + 1)*Radius);
			circle(inputMat, Center, Radius, Scalar(255, 255, 255, 255));

			circle(Mask,Center,Radius,Scalar(255),-1);
		}
	}

	//Crop ROI
	//Mat MaskedMat;
	inputMat.copyTo(outputMat, Mask);

	//Affine Transform
	/*Point2f SrcPoint[]{ Point2f(0,0),Point2f(MaskedMat.cols - 1,0),Point2f(MaskedMat.cols-1,MaskedMat.rows - 1) };
	Point2f DstPoint[]{ Point2f(0,0),Point2f(outputMat.cols - 1,0),Point2f(outputMat.cols-1,outputMat.rows - 1) };
	Mat AffineM = getAffineTransform(SrcPoint, DstPoint);

	Mat WarpedMat = Mat::zeros(outputMat.size(), MaskedMat.type());
	warpAffine(MaskedMat, WarpedMat, AffineM, WarpedMat.size());
	WarpedMat.copyTo(outputMat);*/

	//Fill Black
	Mat BackgroundMask;
	inRange(outputMat, Scalar(0, 0, 0, 0), Scalar(0, 0, 0, 255), BackgroundMask);
	outputMat.setTo(Scalar(0, 0, 0, 255), BackgroundMask);
	
	return true;
}


bool OpenCVBridge::OpenCVHelper::RotateImage(SoftwareBitmap ^ input, SoftwareBitmap ^ output, int deg)
{
	Mat inputMat, outputMat;
	if (!(TryConvert(input, inputMat) && TryConvert(output, outputMat)))
	{
		return false;
	}

	Mat temp;
	inputMat.copyTo(temp);

	for (size_t i = 0; i < deg; i+=90)
	{
		transpose(temp, temp);
		flip(temp, temp,1);
	}
	
	temp.copyTo(outputMat);
	return true;
}


bool OpenCVBridge::OpenCVHelper::InRange(SoftwareBitmap ^ input, SoftwareBitmap ^ output)
{
	Mat inputMat, outputMat;
	if (!(TryConvert(input, inputMat) && TryConvert(output, outputMat)))
	{
		return false;
	}
	
	Mat Mask, MaskBgra;
	inRange(inputMat, Scalar(100, 100, 100,0), Scalar(200, 200, 200,255), Mask);

	cvtColor(Mask, MaskBgra, CV_GRAY2BGRA);

	MaskBgra.copyTo(outputMat);

	return true;
}


bool OpenCVBridge::OpenCVHelper::ParsePanelByHsv(SoftwareBitmap ^ input, SoftwareBitmap ^ output, IIterable<HsvRange^>^ ranges, Windows::Foundation::Size PanelSize, IVector<Windows::Foundation::Point>^ ParseResult)
{
	Mat inputMat, outputMat;
	if (!(TryConvert(input, inputMat) && TryConvert(output, outputMat)))
	{
		return false;
	}

	Mat InputMatBgr, InputMatHsv;
	cvtColor(inputMat, InputMatBgr, CV_BGRA2BGR);
	cvtColor(InputMatBgr, InputMatHsv, CV_BGR2HSV);

	Mat InRangeMask= Mat::zeros(InputMatHsv.size(),CV_8U);
	auto RangeIterator = ranges->First();

	while (RangeIterator->HasCurrent)
	{
		Mat dst;
		auto LowerHsv = RangeIterator->Current->Lower;
		auto UpperHsv = RangeIterator->Current->Upper;

		inRange(InputMatHsv, Scalar(LowerHsv->Hue, LowerHsv->Saturation, LowerHsv->Value), Scalar(UpperHsv->Hue, UpperHsv->Saturation, UpperHsv->Value),dst);
		InRangeMask += dst;

		RangeIterator->MoveNext();
	}

	inputMat.copyTo(outputMat,InRangeMask);
	
	int BeadImageSize = MIN(input->PixelWidth / PanelSize.Width, input->PixelHeight / PanelSize.Height);
	int ValueThreshold = (BeadImageSize*BeadImageSize)*0.785*0.5;//0.785 is circle area, 0.5 is confidence

	for (size_t x = 0; x < PanelSize.Width; x++)
	{
		for (size_t y = 0; y < PanelSize.Height; y++)
		{
			Mat RoiInRangeMask(InRangeMask, cv::Rect(x*BeadImageSize, y*BeadImageSize, BeadImageSize, BeadImageSize));
			int WhitePixelCount = 0;

			for (int height = 0; height < RoiInRangeMask.rows; height++) 
			{
				uchar *data = RoiInRangeMask.ptr<uchar>(height);
				for (int width = 0; width < RoiInRangeMask.cols; width++)
					data[width] ? WhitePixelCount++ : NULL ;
			}

			if (WhitePixelCount>= ValueThreshold)
				ParseResult->Append(Windows::Foundation::Point(x, y));
		}
	}
	

	return true;
}

bool OpenCVBridge::OpenCVHelper::GetAvgBrightness(SoftwareBitmap ^ input, double* AvgBrightness)
{
	Mat inputMat;
	if (!(TryConvert(input, inputMat)))
	{
		return false;
	}

	Mat gray;
	cvtColor(inputMat, gray, CV_BGRA2GRAY);
	*AvgBrightness = mean(gray).val[0];

	return true;
}