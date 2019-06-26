#pragma once

namespace OpenCVBridge
{
	namespace Models
	{
		public ref struct HsvValue sealed
		{
		public:
			HsvValue(int hue, int saturation, int value)
			{
				Hue = hue;
				Saturation = saturation;
				Value = value;
			}
			property int Hue;
			property int Saturation;
			property int Value;
		};

		public ref class HsvRange sealed
		{
		public:
			HsvRange(HsvValue^ lower, HsvValue^ upper)
			{
				Lower = lower;
				Upper = upper;
			}
			property HsvValue^ Upper;
			property HsvValue^ Lower;
		};
	}	
}