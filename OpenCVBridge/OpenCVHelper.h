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
// OpenCVHelper.h

#pragma once
#include <opencv2\core\core.hpp>
#include <opencv2\imgproc\imgproc.hpp>
#include <opencv2\video.hpp>

#include "HsvValueRange.h"

using namespace Windows::Graphics::Imaging;
using namespace Windows::Storage::Streams;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace OpenCVBridge::Models;

namespace OpenCVBridge
{
    public ref class OpenCVHelper sealed
    {
    public:
        OpenCVHelper();

		bool CropImage(Windows::Foundation::Point start,
			Windows::Foundation::Point end,
			SoftwareBitmap^ input,
			SoftwareBitmap^ output);

		bool MaskAndDrawPanelArea(
			SoftwareBitmap^ input,
			SoftwareBitmap^ output);

		bool RotateImage(
			SoftwareBitmap ^ input, 
			SoftwareBitmap ^ output, 
			int deg);

		bool InRange(SoftwareBitmap ^ input, SoftwareBitmap ^ output);

		bool ParsePanelByHsv(SoftwareBitmap ^ input, SoftwareBitmap ^ output, IIterable<HsvRange^>^ ranges , Windows::Foundation::Size PanelSize, IVector<Windows::Foundation::Point>^ ParseResult);

		bool GetAvgBrightness(SoftwareBitmap ^ input, double* AvgBrightness);

    private:
        // used only for the background subtraction operation
        cv::Mat fgMaskMOG2;
        cv::Ptr<cv::BackgroundSubtractor> pMOG2;

        // helper functions for getting a cv::Mat from SoftwareBitmap
        bool GetPointerToPixelData(SoftwareBitmap^ bitmap,
            unsigned char** pPixelData, unsigned int* capacity);

        bool TryConvert(SoftwareBitmap^ from, cv::Mat& convertedMat);
		
	};
}
